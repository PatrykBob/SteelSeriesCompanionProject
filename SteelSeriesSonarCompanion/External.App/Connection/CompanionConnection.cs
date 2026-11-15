using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared;
using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command;
using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event;
using SteelSeriesSonarCompanion.Shared.Core;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SteelSeriesSonarCompanion.External.App.Connection
{
	public class CompanionConnection
	{
		public event Action<string> StatusChanged = delegate { };
		public event Action<List<VolumeData>> VolumeSetupChanged = delegate { };
		public event Action<object?, SoundChannel, float> ChannelVolumeChanged = delegate { };
		public event Action<object?, SoundChannel, bool> ChannelMuteChanged = delegate { };
		public event Action<EqualizerData> EqualizerChanged = delegate { };
		public event Action<SoundChannel, List<EqualizerPreset>> EqualizerPresetsReceived = delegate { };
		public event Action<SoundChannel, bool> EqualizerSaveCompleted = delegate { };

		public string Status { get; private set; } = NOT_CONNECTED_MESSAGE;

		private TcpClient Client { get; set; } = new();
		private StreamWriter? Writer { get; set; }
		private SemaphoreSlim WriterLock { get; } = new(1, 1);

		private bool IsStarted { get; set; }
		private bool IsAuthenticated { get; set; }
		private bool IsPairingRequested { get; set; }
		private string? ConnectingServerId { get; set; }
		private Dictionary<SoundChannel, VolumeData> ChannelStateMap { get; } = new();
		private Dictionary<SoundChannel, float> PendingVolumeMap { get; } = new();
		private HashSet<SoundChannel> ThrottledChannels { get; } = new();
		private Dictionary<SoundChannel, EqualizerData> EqualizerStateMap { get; } = new();
		private HashSet<SoundChannel> WatchedEqualizerChannels { get; } = new();

		private static readonly TimeSpan VOLUME_SEND_INTERVAL = TimeSpan.FromMilliseconds(50);
		private static readonly TimeSpan DISCOVERY_TIMEOUT = TimeSpan.FromSeconds(3);
		private static readonly TimeSpan RECONNECT_DELAY = TimeSpan.FromSeconds(5);

		private const string TOKEN_PREFERENCE_KEY = "PairingToken";
		private const string SERVER_ID_PREFERENCE_KEY = "PairedServerId";
		private const string NOT_CONNECTED_MESSAGE = "Not connected";
		private const string CONNECTING_MESSAGE = "Connecting...";
		private const string CONNECTED_MESSAGE = "Connected";
		private const string CONNECTION_FAILED_MESSAGE_FORMAT = "Connection failed: {0}";
		private const string NOT_PAIRED_MESSAGE = "Not paired - click Remote App > Pair New Device on the PC, then tap Pair with PC in the menu";
		private const string SERVER_NOT_FOUND_MESSAGE = "PC not found - make sure the companion is running on the same network";
		private const string RECONNECTING_MESSAGE = "Disconnected - reconnecting...";
		private const string WAITING_FOR_PAIRING_MESSAGE = "Waiting for pairing - click Remote App > Pair New Device on the PC";

		public void Start ()
		{
			if (IsStarted == true)
			{
				return;
			}

			IsStarted = true;
			ConnectToServer();
		}

		public bool TryGetChannelState (SoundChannel channel, out VolumeData? volumeData)
		{
			return ChannelStateMap.TryGetValue(channel, out volumeData);
		}

		public void StartPairing ()
		{
			IsPairingRequested = true;
			Reconnect();
		}

		public void RefreshAudioDevice ()
		{
			SendCommand(new RefreshAudioDeviceCommand());
		}

		public async void SetChannelVolume (object? source, SoundChannel channel, float volume)
		{
			UpdateChannelState(channel, volume, GetChannelMute(channel));
			ChannelVolumeChanged(source, channel, volume);
			PendingVolumeMap[channel] = volume;

			if (ThrottledChannels.Add(channel) == false)
			{
				return;
			}

			while (PendingVolumeMap.Remove(channel, out float pendingVolume) == true)
			{
				SendCommand(new SetChannelVolumeCommand(channel, pendingVolume));
				await Task.Delay(VOLUME_SEND_INTERVAL);
			}

			ThrottledChannels.Remove(channel);
		}

		public void SetChannelMute (object? source, SoundChannel channel, bool isMuted)
		{
			UpdateChannelState(channel, GetChannelVolume(channel), isMuted);
			ChannelMuteChanged(source, channel, isMuted);
			SendCommand(new SetChannelMuteCommand(channel, isMuted));
		}

		public bool TryGetEqualizerState (SoundChannel channel, out EqualizerData? equalizer)
		{
			return EqualizerStateMap.TryGetValue(channel, out equalizer);
		}

		public void RequestEqualizer (SoundChannel channel)
		{
			WatchedEqualizerChannels.Add(channel);
			SendCommand(new RequestEqualizerCommand(channel));
		}

		public void RequestEqualizerPresets (SoundChannel channel)
		{
			SendCommand(new RequestEqualizerPresetsCommand(channel));
		}

		public void SetEqualizerEnabled (SoundChannel channel, bool enabled)
		{
			SendCommand(new SetEqualizerEnabledCommand(channel, enabled));
		}

		public void SelectEqualizerPreset (SoundChannel channel, string presetId)
		{
			SendCommand(new SelectEqualizerPresetCommand(channel, presetId));
		}

		public void SaveEqualizerPreset (SoundChannel channel, EqualizerSettings settings, string? newPresetName)
		{
			SendCommand(new SaveEqualizerPresetCommand(channel, settings, newPresetName));
		}

		public void ImportEqualizerPreset (SoundChannel channel, string parametricEqText, string? newPresetName)
		{
			SendCommand(new ImportEqualizerPresetCommand(channel, parametricEqText, newPresetName));
		}

		private void Reconnect ()
		{
			IsAuthenticated = false;
			Writer = null;
			Client.Dispose();
			Client = new TcpClient();
			ConnectToServer();
		}

		private void SetStatus (string status)
		{
			Status = status;
			StatusChanged(status);
		}

		private async Task<IPEndPoint> DiscoverServer ()
		{
			using UdpClient udpClient = new()
			{
				EnableBroadcast = true
			};
			IPEndPoint broadcastEndPoint = new(IPAddress.Broadcast, ExternalCommunicationConstProvider.BROADCAST_PORT);
			byte[] request = Encoding.UTF8.GetBytes(ExternalCommunicationConstProvider.SERVER_SEARCH_REQUEST);
			await udpClient.SendAsync(request, request.Length, broadcastEndPoint);

			using CancellationTokenSource timeout = new(DISCOVERY_TIMEOUT);

			while (true)
			{
				UdpReceiveResult result = await udpClient.ReceiveAsync(timeout.Token);
				DiscoveryResponse? response = ParseDiscoveryResponse(result.Buffer);

				if (response != null && IsWantedServer(response) == true)
				{
					ConnectingServerId = response.ServerId;
					return new IPEndPoint(result.RemoteEndPoint.Address, response.Port);
				}
			}
		}

		private DiscoveryResponse? ParseDiscoveryResponse (byte[] buffer)
		{
			try
			{
				return JsonConverter.ConvertFromJSON<DiscoveryResponse>(Encoding.UTF8.GetString(buffer));
			}
			catch (Exception)
			{
				return null;
			}
		}

		private bool IsWantedServer (DiscoveryResponse response)
		{
			if (IsWaitingForPairing() == true)
			{
				return response.IsPairing == true;
			}

			string? pairedServerId = Preferences.Get(SERVER_ID_PREFERENCE_KEY, null);
			return pairedServerId == null || response.ServerId == pairedServerId;
		}

		private bool IsWaitingForPairing ()
		{
			return IsPairingRequested == true || Preferences.Get(TOKEN_PREFERENCE_KEY, null) == null;
		}

		private async void ConnectToServer ()
		{
			TcpClient client = Client;
			string failureMessage;
			SetStatus(CONNECTING_MESSAGE);

			try
			{
				IPEndPoint endPoint = await DiscoverServer();
				await client.ConnectAsync(endPoint);
				TcpKeepAliveConfigurator.Enable(client.Client);

				Writer = new StreamWriter(client.GetStream())
				{
					AutoFlush = true
				};
				StartListeningLoop(client);

				SendCommand(new AuthenticateCommand(DeviceInfo.Name, Preferences.Get(TOKEN_PREFERENCE_KEY, null)));
				return;
			}
			catch (OperationCanceledException)
			{
				failureMessage = IsWaitingForPairing() == true ? WAITING_FOR_PAIRING_MESSAGE : SERVER_NOT_FOUND_MESSAGE;
			}
			catch (Exception ex)
			{
				failureMessage = string.Format(CONNECTION_FAILED_MESSAGE_FORMAT, ex.Message);
			}

			if (client != Client)
			{
				return;
			}

			SetStatus(failureMessage);
			ScheduleReconnect(client);
		}

		private async void StartListeningLoop (TcpClient client)
		{
			try
			{
				using StreamReader reader = new(client.GetStream());

				while (true)
				{
					string? message = await reader.ReadLineAsync();

					if (message == null)
					{
						break;
					}

					BaseExternalCommunicationEvent? externalEvent = ExternalCommunicationEventConverter.ConvertEventFromJson(message);
					NotifyOnExternalEvent(externalEvent);
				}
			}
			catch (Exception e) when (e is IOException or ObjectDisposedException)
			{
			}

			if (client != Client)
			{
				return;
			}

			if (IsAuthenticated == false)
			{
				SetStatus(NOT_PAIRED_MESSAGE);
				return;
			}

			SetStatus(RECONNECTING_MESSAGE);
			ScheduleReconnect(client);
		}

		private async void ScheduleReconnect (TcpClient client)
		{
			await Task.Delay(RECONNECT_DELAY);

			if (client == Client)
			{
				Reconnect();
			}
		}

		private void NotifyOnExternalEvent (BaseExternalCommunicationEvent? externalEvent)
		{
			if (externalEvent is AuthenticatedEvent authenticatedEvent)
			{
				OnAuthenticated(authenticatedEvent);
			}
			else if (externalEvent is VolumeSetupEvent volumeSetupEvent)
			{
				OnVolumeSetupReceived(volumeSetupEvent.VolumeDataCollection);
			}
			else if (externalEvent is ChannelVolumeEvent channelVolumeEvent)
			{
				UpdateChannelState(channelVolumeEvent.Channel, channelVolumeEvent.Volume, GetChannelMute(channelVolumeEvent.Channel));
				ChannelVolumeChanged(null, channelVolumeEvent.Channel, channelVolumeEvent.Volume);
			}
			else if (externalEvent is ChannelMuteEvent channelMuteEvent)
			{
				UpdateChannelState(channelMuteEvent.Channel, GetChannelVolume(channelMuteEvent.Channel), channelMuteEvent.Mute);
				ChannelMuteChanged(null, channelMuteEvent.Channel, channelMuteEvent.Mute);
			}
			else if (externalEvent is EqualizerEvent equalizerEvent)
			{
				EqualizerStateMap[equalizerEvent.Equalizer.Channel] = equalizerEvent.Equalizer;
				EqualizerChanged(equalizerEvent.Equalizer);
			}
			else if (externalEvent is EqualizerPresetsEvent presetsEvent)
			{
				EqualizerPresetsReceived(presetsEvent.Channel, presetsEvent.Presets);
			}
			else if (externalEvent is EqualizerSaveResultEvent saveResultEvent)
			{
				EqualizerSaveCompleted(saveResultEvent.Channel, saveResultEvent.IsSuccess);
			}
		}

		private void OnAuthenticated (AuthenticatedEvent authenticatedEvent)
		{
			if (authenticatedEvent.NewToken != null)
			{
				Preferences.Set(TOKEN_PREFERENCE_KEY, authenticatedEvent.NewToken);
			}

			if (ConnectingServerId != null)
			{
				Preferences.Set(SERVER_ID_PREFERENCE_KEY, ConnectingServerId);
			}

			IsPairingRequested = false;

			IsAuthenticated = true;
			SetStatus(CONNECTED_MESSAGE);
			SendCommand(new RequestVolumeSetupCommand());

			foreach (SoundChannel channel in WatchedEqualizerChannels)
			{
				SendCommand(new RequestEqualizerCommand(channel));
			}
		}

		private void OnVolumeSetupReceived (List<VolumeData> volumeDataCollection)
		{
			for (int i = 0; i < volumeDataCollection.Count; i++)
			{
				ChannelStateMap[volumeDataCollection[i].Channel] = volumeDataCollection[i];
			}

			VolumeSetupChanged(volumeDataCollection);
		}

		private void UpdateChannelState (SoundChannel channel, float volume, bool isMuted)
		{
			ChannelStateMap[channel] = new VolumeData(channel, volume, isMuted);
		}

		private float GetChannelVolume (SoundChannel channel)
		{
			return ChannelStateMap.TryGetValue(channel, out VolumeData? volumeData) == true ? volumeData.Volume : 0;
		}

		private bool GetChannelMute (SoundChannel channel)
		{
			return ChannelStateMap.TryGetValue(channel, out VolumeData? volumeData) == true && volumeData.Mute;
		}

		private async void SendCommand (BaseExternalCommunicationCommand command)
		{
			StreamWriter? writer = Writer;

			if (writer == null)
			{
				return;
			}

			string json = ExternalCommunicationCommandConverter.CovertToJson(command);
			await WriterLock.WaitAsync();

			try
			{
				await writer.WriteLineAsync(json);
			}
			catch (Exception e) when (e is IOException or ObjectDisposedException)
			{
			}
			finally
			{
				WriterLock.Release();
			}
		}
	}
}
