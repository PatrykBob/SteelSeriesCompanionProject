using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class EqualizerEvent : BaseExternalCommunicationEvent
	{
		public EqualizerData Equalizer { get; }

		public const string EVENT_NAME = "Equalizer";

		public EqualizerEvent (EqualizerData equalizer) : base(EVENT_NAME)
		{
			Equalizer = equalizer;
		}
	}
}
