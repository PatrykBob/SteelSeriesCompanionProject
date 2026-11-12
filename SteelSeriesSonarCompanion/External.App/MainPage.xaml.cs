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

		private string ServerIPAddress { get; set; }
		private TcpClient Client { get; set; } = new();

		private List<VolumeSlider> VolumeSliderCollection { get; } = new();

		private bool IsAuthenticated { get; set; }

		private const string SERVER_SEARCH_REQUEST = "DISCOVER_SERVER";
		private const string TOKEN_PREFERENCE_KEY = "PairingToken";
		private const string NOT_PAIRED_MESSAGE = "Not paired - click Remote App > Pair New Device on the PC, then tap Pair with PC in the menu";

		public MainPage ()
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			InitializeComponent();
			SpawnVolumeSliders();
			AttachToEvents();
			Task.Run(ConnectToServer);
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

		private async Task DiscoverServer ()
		{
			UdpClient udpClient = new();
			udpClient.EnableBroadcast = true;
			IPEndPoint endPoint = new(IPAddress.Broadcast, 8888);
			byte[] request = Encoding.UTF8.GetBytes(SERVER_SEARCH_REQUEST);
			await udpClient.SendAsync(request, request.Length, endPoint);

			UdpReceiveResult result = await udpClient.ReceiveAsync();
			string response = Encoding.UTF8.GetString(result.Buffer);
			ServerIPAddress = response;
		}

		public void Reconnect ()
		{
			IsAuthenticated = false;
			Client.Dispose();
			Client = new TcpClient();
			ConnectToServer();
		}

		private async void ConnectToServer ()
		{
			ConnectionLabel.Text = "Connecting...";

			await DiscoverServer();

			try
			{
				IPEndPoint endPoint = IPEndPoint.Parse(ServerIPAddress);
				await Client.ConnectAsync(endPoint);

				TcpClient client = Client;
				Task.Run(() => StartListeningLoop(client));

				SendCommand(new AuthenticateCommand(DeviceInfo.Name, Preferences.Get(TOKEN_PREFERENCE_KEY, null)));
			}
			catch (Exception ex)
			{
				ConnectionLabel.Text = $"Connection failed: {ex}";
			}
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

			string disconnectedText = IsAuthenticated == true ? "Disconnected" : NOT_PAIRED_MESSAGE;
			MainThread.BeginInvokeOnMainThread(() => ConnectionLabel.Text = disconnectedText);
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
			MainThread.BeginInvokeOnMainThread(() => ConnectionLabel.Text = "Connected");
			SendCommand(new RequestVolumeSetupCommand());
		}

		private void OnVolumeChanged (SoundChannel channel, float volume)
		{
			SendCommand(new SetChannelVolumeCommand(channel, volume));
		}

		private void OnMuteChanged (SoundChannel channel, bool isMuted)
		{
			SendCommand(new SetChannelMuteCommand(channel, isMuted));
		}

		private async void SendCommand (BaseExternalCommunicationCommand command)
		{
			if (Client.Connected)
			{
				StreamWriter writer = new(Client.GetStream());
				writer.AutoFlush = true;
				string json = ExternalCommunicationCommandConverter.CovertToJson(command);
				await writer.WriteLineAsync(json);
			}
		}
	}
}
