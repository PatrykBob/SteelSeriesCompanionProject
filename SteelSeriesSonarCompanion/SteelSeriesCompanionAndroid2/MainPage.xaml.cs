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
			VolumeSlider volumeSlider = new VolumeSlider(channel);
			SliderRoot.Children.Add(volumeSlider);
			volumeSlider.OnVolumeChange += OnVolumeChanged;
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
				await Client.ConnectAsync(IPAddress.Parse(ServerIPAddress), 12345);
				ConnectionLabel.Text = "Connected";
			}
			catch (Exception ex)
			{
				ConnectionLabel.Text = $"Connection failed: {ex}";
			}
		}

		private async void OnVolumeChanged (SoundChannel channel, float volume)
		{
			if (Client.Connected)
			{
				StreamWriter writer = new(Client.GetStream());
				writer.AutoFlush = true;
				await writer.WriteLineAsync(volume.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}
