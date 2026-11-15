using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class MediaEvent : BaseExternalCommunicationEvent
	{
		public MediaData Media { get; }

		public const string EVENT_NAME = "Media";

		public MediaEvent (MediaData media) : base(EVENT_NAME)
		{
			Media = media;
		}
	}
}
