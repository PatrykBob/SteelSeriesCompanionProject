using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Text;
using SteelSeriesSonarCompanion.Shared.Core;
using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event;
using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared;
using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunication
{
	public class SteelSeriesCompanionExternalCommunicationController : BaseSteelSeriesCompanionExtension
	{
		private TcpClient? Client { get; set; }
		private TcpListener? Listener { get; set; }
		private UdpClient? DiscoveryClient { get; set; }
		private CancellationTokenSource? ServerCancellation { get; set; }
		private IPEndPoint? LocalEndPoint { get; set; }

		private StreamWriter? Writer { get; set; }

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			AttachToEvents();
			RestartCommunicationServer();
		}

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			return new SteelSeriesCompanionExtensionMenuItem("Restart Communication Server", RestartCommunicationServer);
		}

		private void AttachToEvents ()
		{
			CompanionCore!.VolumeSetupChanged += OnVolumeSetupChanged;
		}

		private void OnVolumeSetupChanged (object? sender, List<VolumeData> volumeDataCollection)
		{
			VolumeSetupEvent volumeSetupEvent = new(volumeDataCollection);
			string json = ExternalCommunicationEventConverter.ConvertToJson(volumeSetupEvent);

			try
			{
				Writer?.WriteLine(json);
			}
			catch (Exception e) when (e is IOException or ObjectDisposedException)
			{
				Trace.WriteLine($"Failed to send volume setup: {e.Message}");
			}
		}

		private void RestartCommunicationServer ()
		{
			StopCommunicationServer();

			ServerCancellation = new CancellationTokenSource();
			CancellationToken token = ServerCancellation.Token;

			try
			{
				Listener = new(IPAddress.Any, 0);
				Listener.Start();
				CacheLocalEndpoint();
				DiscoveryClient = new UdpClient(ExternalCommunicationConstProvider.BROADCAST_PORT);
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to start communication server: {e}");
				StopCommunicationServer();
				return;
			}

			_ = RunListeningLoop(Listener, token);
			_ = RunDiscoveryLoop(DiscoveryClient, token);
		}

		private void StopCommunicationServer ()
		{
			ServerCancellation?.Cancel();
			ServerCancellation?.Dispose();
			ServerCancellation = null;

			Writer = null;
			Client?.Dispose();
			Client = null;

			Listener?.Dispose();
			Listener = null;

			DiscoveryClient?.Dispose();
			DiscoveryClient = null;
		}

		private async Task RunListeningLoop (TcpListener listener, CancellationToken token)
		{
			while (token.IsCancellationRequested == false)
			{
				try
				{
					using TcpClient client = await listener.AcceptTcpClientAsync(token);
					await HandleClient(client, token);
				}
				catch (Exception) when (token.IsCancellationRequested)
				{
					break;
				}
				catch (Exception e)
				{
					Trace.WriteLine($"Communication error: {e}");
				}
			}
		}

		private async Task HandleClient (TcpClient client, CancellationToken token)
		{
			NetworkStream stream = client.GetStream();
			using StreamReader reader = new(stream);

			Client = client;
			Writer = new StreamWriter(stream)
			{
				AutoFlush = true
			};

			try
			{
				while (token.IsCancellationRequested == false)
				{
					string? message = await reader.ReadLineAsync(token);

					if (message == null)
					{
						break;
					}

					ExecuteMessage(message);
				}
			}
			catch (IOException e)
			{
				Trace.WriteLine($"Client connection lost: {e.Message}");
			}
			finally
			{
				if (Client == client)
				{
					Writer = null;
					Client = null;
				}
			}
		}

		private void ExecuteMessage (string message)
		{
			try
			{
				BaseExternalCommunicationCommand? command = ExternalCommunicationCommandConverter.ConvertFromJson(message);

				if (command != null)
				{
					command.ExecuteCommand(this, CompanionCore!);
				}
				else
				{
					Trace.WriteLine($"Invalid command: {message}");
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to execute command '{message}': {e.Message}");
			}
		}

		private void CacheLocalEndpoint ()
		{
			if (Listener!.LocalEndpoint is IPEndPoint ipEndPoint)
			{
				LocalEndPoint = GetLocalIPAddress(ipEndPoint.Port);
			}
		}

		private async Task RunDiscoveryLoop (UdpClient udpClient, CancellationToken token)
		{
			while (token.IsCancellationRequested == false)
			{
				try
				{
					UdpReceiveResult result = await udpClient.ReceiveAsync(token);
					string message = Encoding.UTF8.GetString(result.Buffer);

					if (message == ExternalCommunicationConstProvider.SERVER_SEARCH_REQUEST && LocalEndPoint != null)
					{
						byte[] response = Encoding.UTF8.GetBytes(LocalEndPoint.ToString());
						await udpClient.SendAsync(response, result.RemoteEndPoint, token);
					}
				}
				catch (Exception) when (token.IsCancellationRequested)
				{
					break;
				}
				catch (SocketException e)
				{
					Trace.WriteLine($"Discovery error: {e.Message}");
				}
			}
		}

		private IPEndPoint GetLocalIPAddress (int port)
		{
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return new IPEndPoint(ip, port);
				}
			}

			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}
