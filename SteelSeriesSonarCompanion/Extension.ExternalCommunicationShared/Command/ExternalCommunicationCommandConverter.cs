using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public static class ExternalCommunicationCommandConverter
	{
		public static string CovertToJson (BaseExternalCommunicationCommand command)
		{
			return JsonConverter.ConvertToJSON(command);
		}

		public static BaseExternalCommunicationCommand? ConvertFromJson (string json)
		{
			BaseExternalCommunicationCommand? command = JsonConverter.ConvertFromJSON<BaseExternalCommunicationCommand>(json);

			if (command != null)
			{
				return command.CommandName switch
				{
					SetChannelMuteCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SetChannelMuteCommand>(json),
					SetChannelVolumeCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SetChannelVolumeCommand>(json),
					RequestVolumeSetupCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<RequestVolumeSetupCommand>(json),
					_ => null,
				};
			}

			return null;
		}
	}
}
