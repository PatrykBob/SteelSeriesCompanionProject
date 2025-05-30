﻿using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class BaseExternalCommunicationCommand
	{
		public string CommandName { get; }

		public BaseExternalCommunicationCommand (string commandName)
		{
			CommandName = commandName;
		}

		public virtual void ExecuteCommand (object? sender, ISteelSeriesCompanionCore core)
		{

		}
	}
}
