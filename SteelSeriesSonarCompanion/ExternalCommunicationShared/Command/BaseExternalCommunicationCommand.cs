using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Command
{
	public class BaseExternalCommunicationCommand
	{
		public string CommandName { get; }

		public BaseExternalCommunicationCommand (string commandName)
		{
			CommandName = commandName;
		}

		public virtual void ExecuteCommand (ISteelSeriesCompanionCore core)
		{

		}
	}
}
