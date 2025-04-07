using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

namespace SteelSeriesSonarCompanion.Communication.External
{
	public class ExternalCommunicationFacade
	{
		public event Action<float> TestVolumeChanged = delegate { };

		private TcpClient Client { get; set; }
		private TcpListener Listener { get; set; }

		private const string SERVER_SEARCH_REQUEST = "DISCOVER_SERVER";

		public void Initialize ()
		{
			Listener = new(IPAddress.Any, 12345);
			Task.Run(StartListeningLoop);
			Task.Run(RespondToServerRequest);
		}

		private async void StartListeningLoop ()
		{
			Listener.Start();

			while (true)
			{
				Trace.WriteLine("Waiting for connection");
				Client = await Listener.AcceptTcpClientAsync();

				Trace.WriteLine("Connected");
				NetworkStream stream = Client.GetStream();
				StreamReader reader = new(stream);

				while (reader != null)
				{
					string? message = await reader.ReadLineAsync();
					Trace.WriteLine($"Received: {message}");

					if (float.TryParse(message, NumberStyles.Any, CultureInfo.InvariantCulture, out float volume))
					{
						TestVolumeChanged(volume);
					}
					else
					{
						Trace.WriteLine($"Invalid volume value: {message}");
					}
				}
			}
		}

		private async Task RespondToServerRequest ()
		{
			UdpClient udpClient = new(8888);
			IPEndPoint endPoint = new(IPAddress.Any, 8888);

			while (true)
			{
				UdpReceiveResult result = await udpClient.ReceiveAsync();
				string message = Encoding.UTF8.GetString(result.Buffer);

				if (message == SERVER_SEARCH_REQUEST)
				{
					byte[] response = Encoding.UTF8.GetBytes(GetLocalIPAddress());
					await udpClient.SendAsync(response, response.Length, result.RemoteEndPoint);
				}
			}
		}

		private string GetLocalIPAddress ()
		{
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}

			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}
