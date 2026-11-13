namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared
{
	public class DiscoveryResponse
	{
		public string ServerId { get; set; } = string.Empty;
		public int Port { get; set; }
		public bool IsPairing { get; set; }
	}
}
