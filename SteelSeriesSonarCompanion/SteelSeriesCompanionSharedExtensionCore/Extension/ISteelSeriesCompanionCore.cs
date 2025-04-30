namespace SteelSeriesCompanion.SharedCore
{
	public interface ISteelSeriesCompanionCore
	{
		public Task<List<VolumeData>> GetVolumeSettings ();
		public Task SetChannelVolume (SoundChannel channel, float volume);
		public Task SetChannelMute (SoundChannel channel, bool mute);
	}
}
