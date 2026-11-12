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

		private PairedDeviceStore DeviceStore { get; set; } = new();
		private DateTime? PairingDeadline { get; set; }

		private static readonly TimeSpan PAIRING_WINDOW = TimeSpan.FromSeconds(60);
		private static readonly TimeSpan AUTHENTICATION_TIMEOUT = TimeSpan.FromSeconds(5);
		private const int MAX_DEVICE_NAME_LENGTH = 64;
		private const string UNKNOWN_DEVICE_NAME = "Unknown device";

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			DeviceStore.Load();
			AttachToEvents();
			RestartCommunicationServer();
		}

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			SteelSeriesCompanionExtensionMenuItem menuItem = new("Remote App");
			menuItem.AddSubMenuItem(new SteelSeriesCompanionExtensionMenuItem("Pair New Device", StartPairingMode));
			menuItem.AddSubMenuItem(new SteelSeriesCompanionExtensionMenuItem("Forget Paired Devices", ForgetPairedDevices));
			menuItem.AddSubMenuItem(new SteelSeriesCompanionExtensionMenuItem("Restart Communication Server", RestartCommunicationServer));
			return menuItem;
		}

		private void StartPairingMode ()
		{
			PairingDeadline = DateTime.UtcNow + PAIRING_WINDOW;
			CompanionCore!.ShowNotification($"Pairing mode on for {PAIRING_WINDOW.TotalSeconds} seconds. Open the remote app to pair it.");
		}

		private void ForgetPairedDevices ()
		{
			DeviceStore.Clear();
			Writer = null;
			Client?.Dispose();
			Client = null;
			CompanionCore!.ShowNotification("All paired devices forgotten.");
		}

		private bool IsPairingModeActive ()
		{
			return PairingDeadline != null && DateTime.UtcNow < PairingDeadline;
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
			StreamWriter writer = new(stream)
			{
				AutoFlush = true
			};

			try
			{
				if (await Authenticate(reader, writer, token) == false)
				{
					return;
				}

				Client = client;
				Writer = writer;

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

		private async Task<bool> Authenticate (StreamReader reader, StreamWriter writer, CancellationToken token)
		{
			using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
			timeout.CancelAfter(AUTHENTICATION_TIMEOUT);

			AuthenticateCommand? command = null;

			try
			{
				string? message = await reader.ReadLineAsync(timeout.Token);
				command = message == null ? null : ExternalCommunicationCommandConverter.ConvertFromJson(message) as AuthenticateCommand;
			}
			catch (Exception e) when (token.IsCancellationRequested == false)
			{
				Trace.WriteLine($"Authentication failed: {e.Message}");
			}

			if (command == null)
			{
				return false;
			}

			if (DeviceStore.Contains(command.token) == true)
			{
				await writer.WriteLineAsync(ExternalCommunicationEventConverter.ConvertToJson(new AuthenticatedEvent(null)));
				return true;
			}

			if (IsPairingModeActive() == true)
			{
				string deviceName = TrimDeviceName(command.deviceName);
				string newToken = DeviceStore.Add(deviceName);
				PairingDeadline = null;
				await writer.WriteLineAsync(ExternalCommunicationEventConverter.ConvertToJson(new AuthenticatedEvent(newToken)));
				CompanionCore!.ShowNotification($"Paired: {deviceName}");
				return true;
			}

			Trace.WriteLine($"Rejected unpaired device '{TrimDeviceName(command.deviceName)}'");
			return false;
		}

		private string TrimDeviceName (string? deviceName)
		{
			if (string.IsNullOrWhiteSpace(deviceName) == true)
			{
				return UNKNOWN_DEVICE_NAME;
			}

			deviceName = deviceName.Trim();
			return deviceName.Length > MAX_DEVICE_NAME_LENGTH ? deviceName[..MAX_DEVICE_NAME_LENGTH] : deviceName;
		}

		private void ExecuteMessage (string message)
		{
			try
			{
				BaseExternalCommunicationCommand? command = ExternalCommunicationCommandConverter.ConvertFromJson(message);

				if (command is AuthenticateCommand)
				{
					return;
				}

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
