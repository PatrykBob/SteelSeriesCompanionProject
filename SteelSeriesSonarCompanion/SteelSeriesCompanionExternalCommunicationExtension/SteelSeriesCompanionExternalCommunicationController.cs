using SteelSeriesCompanion.SharedCore;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Text;
using SteelSeriesCompanion.ExternalCommunication.Shared;
using SteelSeriesCompanion.ExternalCommunication.Shared.Command;

namespace SteelSeriesCompanionExternalCommunicationExtension
{
	public class SteelSeriesCompanionExternalCommunicationController : BaseSteelSeriesCompanionExtension
	{
		private TcpClient? Client { get; set; }
		private TcpListener? Listener { get; set; }
		private IPEndPoint? LocalEndPoint { get; set; }

		private StreamReader? Reader { get; set; }
		private StreamWriter? Writer { get; set; }

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			RestartCommunicationServer();
		}

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			return new SteelSeriesCompanionExtensionMenuItem("Restart Communication Server", RestartCommunicationServer);
		}

		private void RestartCommunicationServer ()
		{
			Client?.Dispose();
			Listener?.Dispose();

			Listener = new(IPAddress.Any, 0);
			Task.Run(StartListeningLoop);
			Task.Run(RespondToServerRequest);
		}

		private async Task StartListeningLoop ()
		{
			Listener!.Start();
			CacheLocalEndpoint();

			while (true)
			{
				await CacheNetworkStreams(Listener);

				while (Reader != null)
				{
					string? message = await Reader.ReadLineAsync();

					if (message != null)
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
				}
			}
		}

		private void CacheLocalEndpoint ()
		{
			if (Listener!.LocalEndpoint is IPEndPoint ipEndPoint)
			{
				LocalEndPoint = GetLocalIPAddress(ipEndPoint.Port);
			}
		}

		private async Task CacheNetworkStreams (TcpListener listener)
		{
			TcpClient client = await listener.AcceptTcpClientAsync();
			NetworkStream stream = client.GetStream();

			Reader = new StreamReader(stream);
			Writer = new StreamWriter(stream)
			{
				AutoFlush = true
			};
		}

		private async Task RespondToServerRequest ()
		{
			UdpClient udpClient = new(ExternalCommunicationConstProvider.BROADCAST_PORT);

			while (true)
			{
				UdpReceiveResult result = await udpClient.ReceiveAsync();
				string message = Encoding.UTF8.GetString(result.Buffer);

				if (message == ExternalCommunicationConstProvider.SERVER_SEARCH_REQUEST)
				{
					byte[] response = Encoding.UTF8.GetBytes(LocalEndPoint!.ToString());
					await udpClient.SendAsync(response, response.Length, result.RemoteEndPoint);
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
