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
		private TcpListener? Listener { get; set; }
		private UdpClient? DiscoveryClient { get; set; }
		private CancellationTokenSource? ServerCancellation { get; set; }
		private int? ListenerPort { get; set; }

		private List<RemoteClientConnection> Connections { get; } = new();
		private object ConnectionsLock { get; } = new();

		private PairedDeviceStore DeviceStore { get; set; } = new();
		private ServerIdentityStore Identity { get; set; } = new();
		private DateTime? PairingDeadline { get; set; }

		private static readonly TimeSpan PAIRING_WINDOW = TimeSpan.FromSeconds(60);
		private static readonly TimeSpan AUTHENTICATION_TIMEOUT = TimeSpan.FromSeconds(5);
		private const int MAX_DEVICE_NAME_LENGTH = 64;
		private const string UNKNOWN_DEVICE_NAME = "Unknown device";
		private const int MAX_IMPORT_TEXT_LENGTH = 65536;

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			DeviceStore.Load();
			Identity.Load();
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

		public override void Dispose ()
		{
			CompanionCore!.VolumeSetupChanged -= OnVolumeSetupChanged;
			CompanionCore.ChannelVolumeChanged -= OnChannelVolumeChanged;
			CompanionCore.ChannelMuteChanged -= OnChannelMuteChanged;
			CompanionCore.ChannelEqualizerChanged -= OnChannelEqualizerChanged;
			StopCommunicationServer();
		}

		private void StartPairingMode ()
		{
			PairingDeadline = DateTime.UtcNow + PAIRING_WINDOW;
			CompanionCore!.ShowNotification($"Pairing mode on for {PAIRING_WINDOW.TotalSeconds} seconds. Open the remote app to pair it.");
		}

		private void ForgetPairedDevices ()
		{
			DeviceStore.Clear();
			DisconnectClients();
			CompanionCore!.ShowNotification("All paired devices forgotten.");
		}

		private bool IsPairingModeActive ()
		{
			return PairingDeadline != null && DateTime.UtcNow < PairingDeadline;
		}

		private void AttachToEvents ()
		{
			CompanionCore!.VolumeSetupChanged += OnVolumeSetupChanged;
			CompanionCore.ChannelVolumeChanged += OnChannelVolumeChanged;
			CompanionCore.ChannelMuteChanged += OnChannelMuteChanged;
			CompanionCore.ChannelEqualizerChanged += OnChannelEqualizerChanged;
		}

		private void OnChannelVolumeChanged (object? sender, ChannelVolumeChangedEventArgs e)
		{
			SendEvent(new ChannelVolumeEvent(e.Channel, e.Volume), sender as RemoteClientConnection);
		}

		private void OnChannelMuteChanged (object? sender, ChannelMuteChangedEventArgs e)
		{
			SendEvent(new ChannelMuteEvent(e.Channel, e.Mute), sender as RemoteClientConnection);
		}

		private void OnVolumeSetupChanged (object? sender, List<VolumeData> volumeDataCollection)
		{
			SendEvent(new VolumeSetupEvent(volumeDataCollection));
		}

		private void OnChannelEqualizerChanged (object? sender, ChannelEqualizerChangedEventArgs e)
		{
			SendEvent(new EqualizerEvent(e.Equalizer));
		}

		private void SendEvent (BaseExternalCommunicationEvent externalEvent, RemoteClientConnection? excludedConnection = null)
		{
			string json = ExternalCommunicationEventConverter.ConvertToJson(externalEvent);

			lock (ConnectionsLock)
			{
				foreach (RemoteClientConnection connection in Connections)
				{
					if (connection == excludedConnection)
					{
						continue;
					}

					QueueMessage(connection, json, externalEvent.EventName);
				}
			}
		}

		private void SendEventTo (RemoteClientConnection connection, BaseExternalCommunicationEvent externalEvent)
		{
			QueueMessage(connection, ExternalCommunicationEventConverter.ConvertToJson(externalEvent), externalEvent.EventName);
		}

		private void QueueMessage (RemoteClientConnection connection, string json, string eventName)
		{
			if (connection.TryQueueMessage(json) == false)
			{
				Trace.WriteLine($"Remote app is not reading events, closing connection (dropped {eventName})");
				connection.Client.Close();
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
				CacheListenerPort();
				DiscoveryClient = new UdpClient(ExternalCommunicationConstProvider.BROADCAST_PORT);
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to start communication server: {e}");
				CompanionCore!.ShowNotification($"Could not start the remote app server (port {ExternalCommunicationConstProvider.BROADCAST_PORT} may be in use). Remote apps can't connect.");
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

			DisconnectClients();

			Listener?.Dispose();
			Listener = null;

			DiscoveryClient?.Dispose();
			DiscoveryClient = null;
		}

		private void DisconnectClients ()
		{
			lock (ConnectionsLock)
			{
				foreach (RemoteClientConnection connection in Connections)
				{
					connection.Dispose();
				}

				Connections.Clear();
			}
		}

		private async Task RunListeningLoop (TcpListener listener, CancellationToken token)
		{
			while (token.IsCancellationRequested == false)
			{
				try
				{
					TcpClient client = await listener.AcceptTcpClientAsync(token);
					TcpKeepAliveConfigurator.Enable(client.Client);
					_ = HandleClient(client, token);
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
			using RemoteClientConnection connection = new(client);
			using StreamReader reader = new(client.GetStream());

			try
			{
				if (await Authenticate(reader, connection.Writer, token) == false)
				{
					return;
				}

				AddConnection(connection);
				_ = RunSendLoop(connection, token);
				await RunMessageLoop(connection, reader, token);
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Client connection closed: {e.Message}");
			}
			finally
			{
				RemoveConnection(connection);
			}
		}

		private async Task RunSendLoop (RemoteClientConnection connection, CancellationToken token)
		{
			try
			{
				await connection.RunSendLoop(token);
			}
			catch (Exception e) when (e is IOException or ObjectDisposedException or OperationCanceledException)
			{
				Trace.WriteLine($"Stopped sending to remote app: {e.Message}");
				connection.Client.Close();
			}
		}

		private void AddConnection (RemoteClientConnection connection)
		{
			lock (ConnectionsLock)
			{
				Connections.Add(connection);
			}
		}

		private void RemoveConnection (RemoteClientConnection connection)
		{
			lock (ConnectionsLock)
			{
				Connections.Remove(connection);
			}
		}

		private async Task RunMessageLoop (RemoteClientConnection connection, StreamReader reader, CancellationToken token)
		{
			while (token.IsCancellationRequested == false)
			{
				string? message = await reader.ReadLineAsync(token);

				if (message == null)
				{
					break;
				}

				ExecuteMessage(connection, message);
			}
		}

		private async Task<bool> Authenticate (StreamReader reader, StreamWriter writer, CancellationToken token)
		{
			AuthenticateCommand? command = await ReadAuthenticateCommand(reader, token);

			if (command == null)
			{
				return false;
			}

			if (DeviceStore.Contains(command.token) == true)
			{
				await SendAuthenticated(writer, null);
				return true;
			}

			if (IsPairingModeActive() == true)
			{
				await PairDevice(command, writer);
				return true;
			}

			Trace.WriteLine($"Rejected unpaired device '{TrimDeviceName(command.deviceName)}'");
			return false;
		}

		private async Task<AuthenticateCommand?> ReadAuthenticateCommand (StreamReader reader, CancellationToken token)
		{
			using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
			timeout.CancelAfter(AUTHENTICATION_TIMEOUT);

			try
			{
				string? message = await reader.ReadLineAsync(timeout.Token);
				return message == null ? null : ExternalCommunicationCommandConverter.ConvertFromJson(message) as AuthenticateCommand;
			}
			catch (Exception e) when (token.IsCancellationRequested == false)
			{
				Trace.WriteLine($"Authentication failed: {e.Message}");
				return null;
			}
		}

		private async Task PairDevice (AuthenticateCommand command, StreamWriter writer)
		{
			string deviceName = TrimDeviceName(command.deviceName);
			string newToken = DeviceStore.Add(deviceName);
			PairingDeadline = null;
			await SendAuthenticated(writer, newToken);
			CompanionCore!.ShowNotification($"Paired: {deviceName}");
		}

		private async Task SendAuthenticated (StreamWriter writer, string? newToken)
		{
			string json = ExternalCommunicationEventConverter.ConvertToJson(new AuthenticatedEvent(newToken));
			await writer.WriteLineAsync(json);
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

		private void ExecuteMessage (RemoteClientConnection connection, string message)
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
					ExecuteCommand(connection, command);
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

		private void ExecuteCommand (RemoteClientConnection connection, BaseExternalCommunicationCommand command)
		{
			switch (command)
			{
				case SetChannelVolumeCommand volumeCommand:
					CompanionCore!.SetChannelVolume(connection, volumeCommand.channel, volumeCommand.volume);
					break;
				case SetChannelMuteCommand muteCommand:
					CompanionCore!.SetChannelMute(connection, muteCommand.channel, muteCommand.mute);
					break;
				case RequestVolumeSetupCommand:
					CompanionCore!.RequestVolumeSetup(connection);
					break;
				case RefreshAudioDeviceCommand:
					CompanionCore!.RefreshAudioDevice(connection);
					break;
				case RequestEqualizerCommand equalizerCommand:
					_ = SendEqualizer(connection, equalizerCommand.channel);
					break;
				case RequestEqualizerPresetsCommand presetsCommand:
					_ = SendEqualizerPresets(connection, presetsCommand.channel);
					break;
				case SetEqualizerEnabledCommand enabledCommand:
					CompanionCore!.SetEqualizerEnabled(connection, enabledCommand.channel, enabledCommand.enabled);
					break;
				case SelectEqualizerPresetCommand selectCommand when selectCommand.presetId != null:
					CompanionCore!.SelectEqualizerPreset(connection, selectCommand.channel, selectCommand.presetId);
					break;
				case SaveEqualizerPresetCommand saveCommand:
					_ = SaveEqualizerPreset(connection, saveCommand);
					break;
				case ImportEqualizerPresetCommand importCommand:
					_ = ImportEqualizerPreset(connection, importCommand);
					break;
				default:
					Trace.WriteLine($"Unhandled command: {command.CommandName}");
					break;
			}
		}

		private async Task SendEqualizer (RemoteClientConnection connection, SoundChannel channel)
		{
			try
			{
				EqualizerData? equalizer = await CompanionCore!.GetEqualizer(channel);

				if (equalizer != null)
				{
					SendEventTo(connection, new EqualizerEvent(equalizer));
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to send {channel} equalizer: {e.Message}");
			}
		}

		private async Task SendEqualizerPresets (RemoteClientConnection connection, SoundChannel channel)
		{
			try
			{
				List<EqualizerPreset> presets = await CompanionCore!.GetEqualizerPresets(channel);
				SendEventTo(connection, new EqualizerPresetsEvent(channel, presets));
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to send {channel} equalizer presets: {e.Message}");
			}
		}

		private async Task SaveEqualizerPreset (RemoteClientConnection connection, SaveEqualizerPresetCommand command)
		{
			bool isSuccess = false;

			try
			{
				if (command.settings?.Filters != null)
				{
					isSuccess = await CompanionCore!.TrySaveEqualizerPreset(connection, command.channel, command.settings, command.newPresetName);
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to save {command.channel} equalizer preset: {e.Message}");
			}

			SendEventTo(connection, new EqualizerSaveResultEvent(command.channel, isSuccess));
		}

		private async Task ImportEqualizerPreset (RemoteClientConnection connection, ImportEqualizerPresetCommand command)
		{
			bool isSuccess = false;

			try
			{
				if (command.parametricEqText != null && command.parametricEqText.Length <= MAX_IMPORT_TEXT_LENGTH)
				{
					isSuccess = await CompanionCore!.TryImportEqualizerPreset(connection, command.channel, command.parametricEqText, command.newPresetName);
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to import {command.channel} equalizer preset: {e.Message}");
			}

			SendEventTo(connection, new EqualizerSaveResultEvent(command.channel, isSuccess));
		}

		private void CacheListenerPort ()
		{
			if (Listener!.LocalEndpoint is IPEndPoint ipEndPoint)
			{
				ListenerPort = ipEndPoint.Port;
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

					if (message == ExternalCommunicationConstProvider.SERVER_SEARCH_REQUEST && ListenerPort != null)
					{
						DiscoveryResponse discoveryResponse = new()
						{
							ServerId = Identity.ServerId,
							Port = ListenerPort.Value,
							IsPairing = IsPairingModeActive()
						};
						byte[] response = Encoding.UTF8.GetBytes(JsonConverter.ConvertToJSON(discoveryResponse));
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
	}
}
