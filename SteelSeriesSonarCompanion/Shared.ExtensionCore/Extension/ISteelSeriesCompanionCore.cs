using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension
{
	public interface ISteelSeriesCompanionCore
	{
		public event EventHandler<List<VolumeData>> VolumeSetupChanged;
		public event EventHandler<ChannelVolumeChangedEventArgs> ChannelVolumeChanged;
		public event EventHandler<ChannelMuteChangedEventArgs> ChannelMuteChanged;
		public event EventHandler<ChannelEqualizerChangedEventArgs> ChannelEqualizerChanged;
		public event EventHandler<MediaChangedEventArgs> MediaChanged;

		public Task<List<VolumeData>> GetVolumeSettings ();
		public Task SetChannelVolume (object? sender, SoundChannel channel, float volume);
		public Task SetChannelMute (object? sender, SoundChannel channel, bool mute);
		public Task RequestVolumeSetup (object? sender);
		public Task RefreshAudioDevice (object? sender);
		public void ShowNotification (string message);
		public Task<EqualizerData?> GetEqualizer (SoundChannel channel);
		public Task<List<EqualizerPreset>> GetEqualizerPresets (SoundChannel channel);
		public Task SetEqualizerEnabled (object? sender, SoundChannel channel, bool enabled);
		public Task SelectEqualizerPreset (object? sender, SoundChannel channel, string presetId);
		public Task<bool> TrySaveEqualizerPreset (object? sender, SoundChannel channel, EqualizerSettings settings, string? newPresetName);
		public Task<bool> TryImportEqualizerPreset (object? sender, SoundChannel channel, string parametricEqText, string? newPresetName);
		public Task<MediaData> GetMedia ();
		public Task<bool> TryToggleMediaPlayPause ();
		public Task<bool> TrySkipMediaNext ();
		public Task<bool> TrySkipMediaPrevious ();
	}
}
