using SteelSeriesCompanion.SharedCore.Converters;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Event
{
	public static class ExternalCommunicationEventConverter
	{
		public static string ConvertToJson (BaseExternalCommunicationEvent baseEvent)
		{
			return JsonConverter.ConvertToJSON(baseEvent);
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
