namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class ClassicRedirectionResponse
	{
		public string id;
		public string deviceId;
		public bool isRunning;

		public ClassicRedirectionResponse (string id, string deviceId, bool isRunning)
		{
			this.id = id;
			this.deviceId = deviceId;
			this.isRunning = isRunning;
		}
	}
}
