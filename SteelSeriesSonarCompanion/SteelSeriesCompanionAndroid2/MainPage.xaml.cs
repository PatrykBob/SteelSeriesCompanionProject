using SteelSeriesCompanion.ExternalCommunication.Shared.Command;
using SteelSeriesCompanion.ExternalCommunication.Shared.Event;
using SteelSeriesCompanion.SharedCore;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SteelSeriesCompanionAndroid2
{
	public partial class MainPage : ContentPage
	{
		public event Action<List<VolumeData>> ExternalVolumeSetupChanged = delegate { };

		private string ServerIPAddress { get; set; }
		private TcpClient Client { get; set; } = new();

		private List<VolumeSlider> VolumeSliderCollection { get; } = new();

		private const string SERVER_SEARCH_REQUEST = "DISCOVER_SERVER";

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
				VolumeData volumeData = volumeDataCollection[i];

				for (int j = 0; j < VolumeSliderCollection.Count; j++)
				{
					if (VolumeSliderCollection[i].Channel == volumeData.Channel)
					{
						VolumeSliderCollection[i].Setup(volumeData);
						break;
					}
				}
			}
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

		private async void ConnectToServer ()
		{
			ConnectionLabel.Text = "Connecting...";

			await DiscoverServer();

			try
			{
				IPEndPoint endPoint = IPEndPoint.Parse(ServerIPAddress);
				await Client.ConnectAsync(endPoint);

				Task.Run(StartListeningLoop);

				ConnectionLabel.Text = "Connected";
				SendCommand(new RequestVolumeSetupCommand());
			}
			catch (Exception ex)
			{
				ConnectionLabel.Text = $"Connection failed: {ex}";
			}
		}

		private async void StartListeningLoop ()
		{
			StreamReader reader = new(Client.GetStream());

			while (reader != null)
			{
				string? message = await reader.ReadLineAsync();

				if (message != null)
				{
					BaseExternalCommunicationEvent? externalEvent = ExternalCommunicationEventConverter.ConvertEventFromJson(message);
					NotifyOnExternalEvent(externalEvent);
				}
			}
		}

		private void NotifyOnExternalEvent (BaseExternalCommunicationEvent? externalEvent)
		{
			if (externalEvent is VolumeSetupEvent volumeSetupEvent)
			{
				ExternalVolumeSetupChanged(volumeSetupEvent.VolumeDataCollection);
			}
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
