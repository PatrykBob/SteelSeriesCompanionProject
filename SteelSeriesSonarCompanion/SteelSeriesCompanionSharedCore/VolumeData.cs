namespace SteelSeriesCompanion.SharedCore
{
	public class VolumeData
	{
		public SoundChannel Channel { get; }
		public float Volume { get; }
		public bool Mute { get; }

		public VolumeData (SoundChannel channel, float volume, bool mute)
		{
			Channel = channel;
			Volume = volume;
			Mute = mute;
		}
	}
}
