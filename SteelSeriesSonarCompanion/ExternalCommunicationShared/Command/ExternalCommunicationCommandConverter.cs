using SteelSeriesCompanion.SharedCore.Converters;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Command
{
	public static class ExternalCommunicationCommandConverter
	{
		public static string CovertToJson (BaseExternalCommunicationCommand command)
		{
			return JsonConverter.ConvertToJSON(command);
		}

		public static BaseExternalCommunicationCommand? ConvertFromJson (string json)
		{
			BaseExternalCommunicationCommand? test = JsonConverter.ConvertFromJSON<BaseExternalCommunicationCommand>(json);

			if (test != null)
			{
				return test.Command switch
				{
					SetChannelMuteCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SetChannelMuteCommand>(json),
					SetChannelVolumeCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SetChannelVolumeCommand>(json),
					_ => null,
				};
			}

			return null;
		}
	}
}
