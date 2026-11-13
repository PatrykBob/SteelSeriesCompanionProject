using System.Net.Sockets;
using System.Threading.Channels;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunication
{
	internal class RemoteClientConnection : IDisposable
	{
		public TcpClient Client { get; }
		public StreamWriter Writer { get; }
		private Channel<string> OutgoingMessageChannel { get; } = Channel.CreateBounded<string>(OUTGOING_QUEUE_CAPACITY);

		private const int OUTGOING_QUEUE_CAPACITY = 256;

		public RemoteClientConnection (TcpClient client)
		{
			Client = client;
			Writer = new StreamWriter(client.GetStream())
			{
				AutoFlush = true
			};
		}

		public bool TryQueueMessage (string message)
		{
			return OutgoingMessageChannel.Writer.TryWrite(message);
		}

		public async Task RunSendLoop (CancellationToken token)
		{
			await foreach (string message in OutgoingMessageChannel.Reader.ReadAllAsync(token))
			{
				await Writer.WriteLineAsync(message);
			}
		}

		public void Dispose ()
		{
			OutgoingMessageChannel.Writer.TryComplete();
			Client.Dispose();
		}
	}
}
