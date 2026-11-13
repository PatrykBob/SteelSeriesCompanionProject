using System.Net.Sockets;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared
{
	public static class TcpKeepAliveConfigurator
	{
		private const int KEEP_ALIVE_TIME_SECONDS = 5;
		private const int KEEP_ALIVE_INTERVAL_SECONDS = 2;
		private const int KEEP_ALIVE_RETRY_COUNT = 3;

		public static void Enable (Socket socket)
		{
			try
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, KEEP_ALIVE_TIME_SECONDS);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, KEEP_ALIVE_INTERVAL_SECONDS);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, KEEP_ALIVE_RETRY_COUNT);
			}
			catch (SocketException)
			{
			}
		}
	}
}
