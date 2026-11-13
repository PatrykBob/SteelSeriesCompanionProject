using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class RefreshAudioDeviceCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "RefreshAudioDevice";

		public RefreshAudioDeviceCommand () : base(COMMAND_NAME) { }

		public override void ExecuteCommand (object? sender, ISteelSeriesCompanionCore core)
		{
			base.ExecuteCommand(sender, core);
			core.RefreshAudioDevice(sender);
		}
	}
}
