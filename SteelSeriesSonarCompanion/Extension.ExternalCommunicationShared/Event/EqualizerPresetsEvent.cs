using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class EqualizerPresetsEvent : BaseExternalCommunicationEvent
	{
		public SoundChannel Channel { get; }
		public List<EqualizerPreset> Presets { get; }

		public const string EVENT_NAME = "EqualizerPresets";

		public EqualizerPresetsEvent (SoundChannel channel, List<EqualizerPreset> presets) : base(EVENT_NAME)
		{
			Channel = channel;
			Presets = presets;
		}
	}
}
