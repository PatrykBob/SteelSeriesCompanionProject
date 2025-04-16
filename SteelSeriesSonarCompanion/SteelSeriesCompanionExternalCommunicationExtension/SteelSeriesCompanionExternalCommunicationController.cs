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
		private IPEndPoint? LocalEndPoint { get; set; }

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			RestartCommunicationServer();
		}

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			return new SteelSeriesCompanionExtensionMenuItem("Restart Communication Server", RestartCommunicationServer);
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
			CacheLocalEndpoint();

			while (true)
			{
				StreamReader networkStream = await GetNetworkStreamAsync(Listener);

				while (networkStream != null)
				{
					string? message = await networkStream.ReadLineAsync();

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

		private void CacheLocalEndpoint ()
		{
			if (Listener!.LocalEndpoint is IPEndPoint ipEndPoint)
			{
				LocalEndPoint = GetLocalIPAddress(ipEndPoint.Port);
			}
		}

		private async Task<StreamReader> GetNetworkStreamAsync (TcpListener listener)
		{
			TcpClient client = await listener.AcceptTcpClientAsync();
			NetworkStream stream = client.GetStream();
			return new StreamReader(stream);
		}

		private async Task RespondToServerRequest ()
		{
			UdpClient udpClient = new(ExternalCommunicationConstProvider.BROADCAST_PORT);

			while (true)
			{
				UdpReceiveResult result = await udpClient.ReceiveAsync();
				string message = Encoding.UTF8.GetString(result.Buffer);

				if (message == ExternalCommunicationConstProvider.SERVER_SEARCH_REQUEST)
				{
					byte[] response = Encoding.UTF8.GetBytes(LocalEndPoint!.ToString());
					await udpClient.SendAsync(response, response.Length, result.RemoteEndPoint);
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
