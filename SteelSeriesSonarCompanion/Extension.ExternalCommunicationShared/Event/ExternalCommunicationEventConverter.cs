using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
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
					AuthenticatedEvent.EVENT_NAME => JsonConverter.ConvertFromJSON<AuthenticatedEvent>(json),
					_ => null,
				};
			}

			return null;
		}
	}
}
