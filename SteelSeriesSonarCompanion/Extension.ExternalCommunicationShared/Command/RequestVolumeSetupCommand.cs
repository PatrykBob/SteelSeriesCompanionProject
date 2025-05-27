using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Command
{
	public class RequestVolumeSetupCommand : BaseExternalCommunicationCommand
	{
		public const string COMMAND_NAME = "RequestVolumeSetup";

		public RequestVolumeSetupCommand () : base(COMMAND_NAME) {	}

		public override void ExecuteCommand (object? sender, ISteelSeriesCompanionCore core)
		{
			base.ExecuteCommand(sender, core);
			core.RequestVolumeSetup(sender);
		}
	}
}
