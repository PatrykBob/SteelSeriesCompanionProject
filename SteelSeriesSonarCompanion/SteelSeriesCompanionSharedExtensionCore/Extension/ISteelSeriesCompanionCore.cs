namespace SteelSeriesCompanion.SharedCore
{
	public interface ISteelSeriesCompanionCore
	{
		public event EventHandler<List<VolumeData>> VolumeSetupChanged;

		public Task<List<VolumeData>> GetVolumeSettings ();
		public Task SetChannelVolume (object? sender, SoundChannel channel, float volume);
		public Task SetChannelMute (object? sender, SoundChannel channel, bool mute);
		public Task RequestVolumeSetup (object? sender);
	}
}
