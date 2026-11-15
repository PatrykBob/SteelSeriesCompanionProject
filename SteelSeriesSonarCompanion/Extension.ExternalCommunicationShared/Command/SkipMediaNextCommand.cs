namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SkipMediaNextCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "SkipMediaNext";

		public SkipMediaNextCommand () : base(COMMAND_NAME) { }
	}
}
