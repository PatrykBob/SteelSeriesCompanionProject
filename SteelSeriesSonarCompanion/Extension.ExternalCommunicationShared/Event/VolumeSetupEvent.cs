using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class VolumeSetupEvent : BaseExternalCommunicationEvent
	{
		public List<VolumeData> VolumeDataCollection { get; } = new();

		public const string EVENT_NAME = "VolumeSetup";

		public VolumeSetupEvent (List<VolumeData> volumeDataCollection) : base(EVENT_NAME)
		{
			VolumeDataCollection = volumeDataCollection;
		}
	}
}
