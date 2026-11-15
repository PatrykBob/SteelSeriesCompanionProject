namespace SteelSeriesSonarCompanion.Shared.Core
{
	public class EqualizerData
	{
		public SoundChannel Channel { get; }
		public bool Enabled { get; }
		public EqualizerPreset Preset { get; }
		public EqualizerSettings Settings { get; }

		public EqualizerData (SoundChannel channel, bool enabled, EqualizerPreset preset, EqualizerSettings settings)
		{
			Channel = channel;
			Enabled = enabled;
			Preset = preset;
			Settings = settings;
		}
	}
}
