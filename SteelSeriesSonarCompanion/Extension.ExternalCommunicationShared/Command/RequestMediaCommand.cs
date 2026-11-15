namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class RequestMediaCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "RequestMedia";

		public RequestMediaCommand () : base(COMMAND_NAME) { }
	}
}
