using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension
{
	public interface ISteelSeriesCompanionCore
	{
		public event EventHandler<List<VolumeData>> VolumeSetupChanged;
		public event EventHandler<ChannelVolumeChangedEventArgs> ChannelVolumeChanged;
		public event EventHandler<ChannelMuteChangedEventArgs> ChannelMuteChanged;

		public Task<List<VolumeData>> GetVolumeSettings ();
		public Task SetChannelVolume (object? sender, SoundChannel channel, float volume);
		public Task SetChannelMute (object? sender, SoundChannel channel, bool mute);
		public Task RequestVolumeSetup (object? sender);
		public Task RefreshAudioDevice (object? sender);
		public void ShowNotification (string message);
	}
}
