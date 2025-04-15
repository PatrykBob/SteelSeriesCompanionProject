using SteelSeriesCompanion.SharedCore;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Text;
using SteelSeriesCompanion.ExternalCommunication.Shared;

namespace SteelSeriesCompanionExternalCommunicationExtension
{
	public class SteelSeriesCompanionExternalCommunicationController : BaseSteelSeriesCompanionExtension
	{
		private TcpClient? Client { get; set; }
		private TcpListener? Listener { get; set; }

		public override ToolStripMenuItem GetExtensionMenuItem ()
		{
			ToolStripMenuItem toolMenuItem = new();
			toolMenuItem.Text = "Restart Communication";
			toolMenuItem.Click += RestartCommunicationServer;
			return toolMenuItem;
		}

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			RestartCommunicationServer();
		}

		private void RestartCommunicationServer (object? sender, EventArgs e)
		{
			RestartCommunicationServer();
		}

		private void RestartCommunicationServer ()
		{
			Client?.Dispose();
			Listener?.Dispose();

			Listener = new(IPAddress.Any, 0);
			Task.Run(StartListeningLoop);
			Task.Run(RespondToServerRequest);
		}

		private async void StartListeningLoop ()
		{
			Listener!.Start();

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
						await CompanionCore!.SetChannelVolume(SoundChannel.GAME, volume);
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
			int broadcastPort = ExternalCommunicationConstProvider.BROADCAST_PORT;

			UdpClient udpClient = new(broadcastPort);
			IPEndPoint endPoint = new(IPAddress.Any, broadcastPort);

			while (true)
			{
				UdpReceiveResult result = await udpClient.ReceiveAsync();
				string message = Encoding.UTF8.GetString(result.Buffer);

				if (message == ExternalCommunicationConstProvider.SERVER_SEARCH_REQUEST)
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
