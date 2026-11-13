namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class RefreshAudioDeviceCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "RefreshAudioDevice";

		public RefreshAudioDeviceCommand () : base(COMMAND_NAME) { }
	}
}
