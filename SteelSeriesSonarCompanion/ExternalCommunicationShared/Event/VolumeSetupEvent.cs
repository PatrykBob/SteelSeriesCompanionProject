using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Event
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
