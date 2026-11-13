namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class RequestVolumeSetupCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "RequestVolumeSetup";

		public RequestVolumeSetupCommand () : base(COMMAND_NAME) {	}
	}
}
