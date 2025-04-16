using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Command
{
	public class BaseExternalCommunicationCommand
	{
		public string Command { get; }

		public BaseExternalCommunicationCommand (string command)
		{
			Command = command;
		}

		public virtual void ExecuteCommand (ISteelSeriesCompanionCore core)
		{

		}
	}
}
