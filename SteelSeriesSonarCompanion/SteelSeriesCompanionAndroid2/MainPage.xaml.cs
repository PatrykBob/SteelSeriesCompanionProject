using SteelSeriesCompanion.ExternalCommunication.Shared.Command;
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
		private string ServerIPAddress { get; set; }
		private TcpClient Client { get; set; } = new();

		private const string SERVER_SEARCH_REQUEST = "DISCOVER_SERVER";

		public MainPage ()
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			InitializeComponent();
			SpawnVolumeSliders();
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

		private async void ConnectToServer (object sender, EventArgs e)
		{
			ConnectionLabel.Text = "Connecting...";

			await DiscoverServer();

			try
			{
				IPEndPoint endPoint = IPEndPoint.Parse(ServerIPAddress);
				await Client.ConnectAsync(endPoint);
				ConnectionLabel.Text = "Connected";
			}
			catch (Exception ex)
			{
				ConnectionLabel.Text = $"Connection failed: {ex}";
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
