namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SkipMediaPreviousCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "SkipMediaPrevious";

		public SkipMediaPreviousCommand () : base(COMMAND_NAME) { }
	}
}
