using SteelSeriesCompanion.ExternalCommunication.Shared.Event;
using SteelSeriesCompanion.SharedCore.Converters;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Command
{
	public static class ExternalCommunicationCommandConverter
	{
		public static string CovertToJson (BaseExternalCommunicationCommand command)
		{
			return JsonConverter.ConvertToJSON(command);
		}

		public static string ConvertToJson (BaseExternalCommunicationEvent baseEvent)
		{
			return JsonConverter.ConvertToJSON(baseEvent);
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
					_ => null,
				};
			}

			return null;
		}

		public static BaseExternalCommunicationEvent? ConvertEventFromJson (string json)
		{
			BaseExternalCommunicationEvent? test = JsonConverter.ConvertFromJSON<BaseExternalCommunicationEvent>(json);

			if (test != null)
			{
				return test.EventName switch
				{
					VolumeSetupEvent.EVENT_NAME => JsonConverter.ConvertFromJSON<VolumeSetupEvent>(json),
					_ => null,
				};
			}

			return null;
		}
	}
}
