namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class AuthenticateCommand (string deviceName, string? token) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public string deviceName = deviceName;
		public string? token = token;

		public const string COMMAND_NAME = "Authenticate";
	}
}
