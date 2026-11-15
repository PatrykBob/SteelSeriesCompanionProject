namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class ToggleMediaPlayPauseCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "ToggleMediaPlayPause";

		public ToggleMediaPlayPauseCommand () : base(COMMAND_NAME) { }
	}
}
