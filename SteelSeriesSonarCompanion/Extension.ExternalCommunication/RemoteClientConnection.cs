using System.Net.Sockets;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunication
{
	internal class RemoteClientConnection : IDisposable
	{
		public TcpClient Client { get; }
		public StreamWriter Writer { get; }

		public RemoteClientConnection (TcpClient client)
		{
			Client = client;
			Writer = new StreamWriter(client.GetStream())
			{
				AutoFlush = true
			};
		}

		public void Dispose ()
		{
			Client.Dispose();
		}
	}
}
