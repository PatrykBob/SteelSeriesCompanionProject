namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class BaseExternalCommunicationCommand
	{
		public string CommandName { get; }

		public BaseExternalCommunicationCommand (string commandName)
		{
			CommandName = commandName;
		}
	}
}
