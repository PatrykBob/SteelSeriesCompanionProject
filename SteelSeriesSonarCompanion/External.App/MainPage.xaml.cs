using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared;
using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command;
using SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event;
using SteelSeriesSonarCompanion.Shared.Core;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class MainPage : ContentPage
	{
		public event Action<List<VolumeData>> ExternalVolumeSetupChanged = delegate { };

		private TcpClient Client { get; set; } = new();
		private StreamWriter? Writer { get; set; }
		private SemaphoreSlim WriterLock { get; } = new(1, 1);

		private List<VolumeSlider> VolumeSliderCollection { get; } = new();

		private bool IsAuthenticated { get; set; }
		private Dictionary<SoundChannel, float> PendingVolumeMap { get; } = new();
		private HashSet<SoundChannel> ThrottledChannels { get; } = new();

		private static readonly TimeSpan VOLUME_SEND_INTERVAL = TimeSpan.FromMilliseconds(50);
		private static readonly TimeSpan DISCOVERY_TIMEOUT = TimeSpan.FromSeconds(3);
		private static readonly TimeSpan RECONNECT_DELAY = TimeSpan.FromSeconds(5);

		private const string TOKEN_PREFERENCE_KEY = "PairingToken";
		private const string CONNECTING_MESSAGE = "Connecting...";
		private const string CONNECTED_MESSAGE = "Connected";
		private const string CONNECTION_FAILED_MESSAGE_FORMAT = "Connection failed: {0}";
		private const string NOT_PAIRED_MESSAGE = "Not paired - click Remote App > Pair New Device on the PC, then tap Pair with PC in the menu";
		private const string SERVER_NOT_FOUND_MESSAGE = "PC not found - make sure the companion is running on the same network";
		private const string RECONNECTING_MESSAGE = "Disconnected - reconnecting...";

		public MainPage ()
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			InitializeComponent();
			SpawnVolumeSliders();
			AttachToEvents();
			ConnectToServer();
		}

		private void SpawnVolumeSliders ()
		{
			SpawnVolumeSlider(SoundChannel.MASTER);
			SpawnVolumeSlider(SoundChannel.GAME);
			SpawnVolumeSlider(SoundChannel.CHAT);
			SpawnVolumeSlider(SoundChannel.MEDIA);
			SpawnVolumeSlider(SoundChannel.AUX);
			SpawnVolumeSlider(SoundChannel.MIC);
		}

		private void SpawnVolumeSlider (SoundChannel channel)
		{
			VolumeSlider volumeSlider = new(channel);
			SliderRoot.Add(volumeSlider, (int)channel);
			volumeSlider.OnVolumeChange += OnVolumeChanged;
			volumeSlider.OnMuteChange += OnMuteChanged;

			VolumeSliderCollection.Add(volumeSlider);
		}

		private void AttachToEvents ()
		{
			ExternalVolumeSetupChanged += OnExternalVolumeSetupChanged;
		}

		private void OnExternalVolumeSetupChanged (List<VolumeData> volumeDataCollection)
		{
			for (int i = 0; i < volumeDataCollection.Count; i++)
			{
				FindVolumeSlider(volumeDataCollection[i].Channel)?.Setup(volumeDataCollection[i]);
			}
		}

		private VolumeSlider? FindVolumeSlider (SoundChannel channel)
		{
			for (int i = 0; i < VolumeSliderCollection.Count; i++)
			{
				if (VolumeSliderCollection[i].Channel == channel)
				{
					return VolumeSliderCollection[i];
				}
			}

			return null;
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
			UdpReceiveResult result = await udpClient.ReceiveAsync(timeout.Token);
			return IPEndPoint.Parse(Encoding.UTF8.GetString(result.Buffer));
		}

		public void Reconnect ()
		{
			IsAuthenticated = false;
			Writer = null;
			Client.Dispose();
			Client = new TcpClient();
			ConnectToServer();
		}

		private async void ConnectToServer ()
		{
			TcpClient client = Client;
			string failureMessage;
			ConnectionLabel.Text = CONNECTING_MESSAGE;

			try
			{
				IPEndPoint endPoint = await DiscoverServer();
				await client.ConnectAsync(endPoint);

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
				failureMessage = SERVER_NOT_FOUND_MESSAGE;
			}
			catch (Exception ex)
			{
				failureMessage = string.Format(CONNECTION_FAILED_MESSAGE_FORMAT, ex.Message);
			}

			if (client != Client)
			{
				return;
			}

			ConnectionLabel.Text = failureMessage;
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
				ConnectionLabel.Text = NOT_PAIRED_MESSAGE;
				return;
			}

			ConnectionLabel.Text = RECONNECTING_MESSAGE;
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
				ExternalVolumeSetupChanged(volumeSetupEvent.VolumeDataCollection);
			}
			else if (externalEvent is ChannelVolumeEvent channelVolumeEvent)
			{
				FindVolumeSlider(channelVolumeEvent.Channel)?.SetVolume(channelVolumeEvent.Volume);
			}
			else if (externalEvent is ChannelMuteEvent channelMuteEvent)
			{
				FindVolumeSlider(channelMuteEvent.Channel)?.SetMute(channelMuteEvent.Mute);
			}
		}

		private void OnAuthenticated (AuthenticatedEvent authenticatedEvent)
		{
			if (authenticatedEvent.NewToken != null)
			{
				Preferences.Set(TOKEN_PREFERENCE_KEY, authenticatedEvent.NewToken);
			}

			IsAuthenticated = true;
			ConnectionLabel.Text = CONNECTED_MESSAGE;
			SendCommand(new RequestVolumeSetupCommand());
		}

		private async void OnVolumeChanged (SoundChannel channel, float volume)
		{
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

		private void OnMuteChanged (SoundChannel channel, bool isMuted)
		{
			SendCommand(new SetChannelMuteCommand(channel, isMuted));
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
