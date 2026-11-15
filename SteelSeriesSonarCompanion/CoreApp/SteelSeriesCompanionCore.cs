using System.Diagnostics;
using SteelSeriesSonarCompanion.CoreApp.Communication.Internal;
using SteelSeriesSonarCompanion.CoreApp.Extension;
using SteelSeriesSonarCompanion.CoreApp.Tray;
using SteelSeriesSonarCompanion.Shared.Core;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.CoreApp
{
	public class SteelSeriesCompanionCore : ISteelSeriesCompanionCore, IDisposable
	{
		public event EventHandler<List<VolumeData>> VolumeSetupChanged = delegate { };
		public event EventHandler<ChannelVolumeChangedEventArgs> ChannelVolumeChanged = delegate { };
		public event EventHandler<ChannelMuteChangedEventArgs> ChannelMuteChanged = delegate { };
		public event EventHandler<ChannelEqualizerChangedEventArgs> ChannelEqualizerChanged = delegate { };

		private InternalCommunicationFacade InternalFacade { get; set; } = new();
		private SteelSeriesCompanionExtensionController ExtensionController { get; set; } = new();
		private TrayController Tray { get; set; } = new();
		private Dictionary<SoundChannel, float> ConfirmedVolumeMap { get; } = new();
		private Dictionary<SoundChannel, bool> ConfirmedMuteMap { get; } = new();
		private SynchronizationContext? UiContext { get; set; }

		private const float VOLUME_CHANGE_TOLERANCE = 0.001f;

		private const string SONAR_CONNECTION_FAILED_MESSAGE =
			"Could not connect to SteelSeries Sonar. Make sure SteelSeries GG is running; the companion will connect automatically.";
		private const string AUDIO_DEVICE_REFRESHED_MESSAGE = "Audio device refreshed.";
		private const string REFRESH_AUDIO_DEVICE_MENU_TEXT = "Refresh Audio Device";
		private const string INVALID_EQUALIZER_SETTINGS_MESSAGE = "Equalizer preset was not saved: it needs 1 to 10 filters with valid values.";
		private const string INVALID_PRESET_NAME_MESSAGE = "Equalizer preset was not saved: the preset name is empty.";
		private const string FACTORY_PRESET_OVERWRITE_MESSAGE = "SteelSeries presets cannot be overwritten. Save the equalizer as a new preset instead.";
		private const string EQUALIZER_SAVE_FAILED_MESSAGE_FORMAT = "Could not save the {0} equalizer preset.";

		public async Task<List<VolumeData>> GetVolumeSettings ()
		{
			List<VolumeData> volumeSettings = await InternalFacade.GetVolumeSettings();
			StoreConfirmedVolumeSettings(volumeSettings);
			return volumeSettings;
		}

		private void StoreConfirmedVolumeSettings (List<VolumeData> volumeSettings)
		{
			foreach (VolumeData volumeData in volumeSettings)
			{
				ConfirmedVolumeMap[volumeData.Channel] = volumeData.Volume;
				ConfirmedMuteMap[volumeData.Channel] = volumeData.Mute;
			}
		}

		public async Task SetChannelVolume (object? sender, SoundChannel channel, float volume)
		{
			if (IsKnownChannel(channel) == false)
			{
				return;
			}

			if (float.IsFinite(volume) == false)
			{
				Trace.WriteLine($"Ignored invalid volume {volume} for {channel}");
				return;
			}

			float clampedVolume = Math.Clamp(volume, 0f, 1f);

			if (await InternalFacade.TrySetChannelVolume(channel, clampedVolume) == false)
			{
				RestoreConfirmedVolume(channel);
				return;
			}

			ConfirmedVolumeMap[channel] = clampedVolume;
			ChannelVolumeChanged.Invoke(sender, new ChannelVolumeChangedEventArgs(channel, clampedVolume));
		}

		public async Task SetChannelMute (object? sender, SoundChannel channel, bool mute)
		{
			if (IsKnownChannel(channel) == false)
			{
				return;
			}

			if (await InternalFacade.TrySetChannelMute(channel, mute) == false)
			{
				RestoreConfirmedMute(channel);
				return;
			}

			ConfirmedMuteMap[channel] = mute;
			ChannelMuteChanged.Invoke(sender, new ChannelMuteChangedEventArgs(channel, mute));
		}

		private void RestoreConfirmedVolume (SoundChannel channel)
		{
			if (ConfirmedVolumeMap.TryGetValue(channel, out float volume) == false)
			{
				Trace.WriteLine($"Failed to set {channel} volume, no confirmed volume to restore");
				return;
			}

			Trace.WriteLine($"Failed to set {channel} volume, restoring {volume}");
			ChannelVolumeChanged.Invoke(this, new ChannelVolumeChangedEventArgs(channel, volume));
		}

		private void RestoreConfirmedMute (SoundChannel channel)
		{
			if (ConfirmedMuteMap.TryGetValue(channel, out bool mute) == false)
			{
				Trace.WriteLine($"Failed to set {channel} mute, no confirmed mute to restore");
				return;
			}

			Trace.WriteLine($"Failed to set {channel} mute, restoring {mute}");
			ChannelMuteChanged.Invoke(this, new ChannelMuteChangedEventArgs(channel, mute));
		}

		private bool IsKnownChannel (SoundChannel channel)
		{
			if (Enum.IsDefined(channel) == true)
			{
				return true;
			}

			Trace.WriteLine($"Ignored unknown channel {(int)channel}");
			return false;
		}

		private bool IsEqualizerChannel (SoundChannel channel)
		{
			if (IsKnownChannel(channel) == false)
			{
				return false;
			}

			if (channel == SoundChannel.MASTER)
			{
				Trace.WriteLine("Ignored equalizer request for MASTER, it has no equalizer");
				return false;
			}

			return true;
		}

		public async Task<EqualizerData?> GetEqualizer (SoundChannel channel)
		{
			if (IsEqualizerChannel(channel) == false)
			{
				return null;
			}

			return await InternalFacade.GetEqualizerData(channel);
		}

		public async Task<List<EqualizerPreset>> GetEqualizerPresets (SoundChannel channel)
		{
			if (IsEqualizerChannel(channel) == false)
			{
				return [];
			}

			return await InternalFacade.GetEqualizerPresets(channel);
		}

		public async Task SetEqualizerEnabled (object? sender, SoundChannel channel, bool enabled)
		{
			if (IsEqualizerChannel(channel) == false)
			{
				return;
			}

			if (await InternalFacade.TrySetEqualizerEnabled(channel, enabled) == false)
			{
				Trace.WriteLine($"Failed to set {channel} equalizer enabled to {enabled}, restoring current state");
				await PublishEqualizer(this, channel);
				return;
			}

			await PublishEqualizer(sender, channel);
		}

		public async Task SelectEqualizerPreset (object? sender, SoundChannel channel, string presetId)
		{
			if (IsEqualizerChannel(channel) == false)
			{
				return;
			}

			if (await InternalFacade.TrySelectEqualizerPreset(channel, presetId) == false)
			{
				Trace.WriteLine($"Failed to select {channel} equalizer preset {presetId}, restoring current state");
				await PublishEqualizer(this, channel);
				return;
			}

			await PublishEqualizer(sender, channel);
		}

		public async Task<bool> TrySaveEqualizerPreset (object? sender, SoundChannel channel, EqualizerSettings settings, string? newPresetName)
		{
			if (IsEqualizerChannel(channel) == false)
			{
				return false;
			}

			return await TrySaveEqualizer(sender, channel, settings, newPresetName);
		}

		private async Task<bool> TrySaveEqualizer (object? sender, SoundChannel channel, EqualizerSettings settings, string? newPresetName)
		{
			if (IsValidEqualizerSettings(settings) == false)
			{
				ShowNotification(INVALID_EQUALIZER_SETTINGS_MESSAGE);
				return false;
			}

			if (newPresetName != null && string.IsNullOrWhiteSpace(newPresetName) == true)
			{
				ShowNotification(INVALID_PRESET_NAME_MESSAGE);
				return false;
			}

			switch (await InternalFacade.SaveEqualizerSettings(channel, settings, newPresetName?.Trim()))
			{
				case EqualizerSaveResult.SAVED:
					await PublishEqualizer(sender, channel);
					return true;
				case EqualizerSaveResult.FACTORY_PRESET:
					ShowNotification(FACTORY_PRESET_OVERWRITE_MESSAGE);
					return false;
				default:
					ShowNotification(string.Format(EQUALIZER_SAVE_FAILED_MESSAGE_FORMAT, channel));
					return false;
			}
		}

		private bool IsValidEqualizerSettings (EqualizerSettings settings)
		{
			if (settings.Filters.Count == 0 || settings.Filters.Count > EqualizerLimits.MAX_FILTER_COUNT || double.IsFinite(settings.Preamp) == false)
			{
				return false;
			}

			return settings.Filters.All(filter => Enum.IsDefined(filter.Type) == true && double.IsFinite(filter.Frequency) == true && double.IsFinite(filter.Gain) == true && double.IsFinite(filter.QFactor) == true);
		}

		private async Task PublishEqualizer (object? sender, SoundChannel channel)
		{
			EqualizerData? equalizer = await InternalFacade.GetEqualizerData(channel);

			if (equalizer == null)
			{
				return;
			}

			ChannelEqualizerChanged.Invoke(sender, new ChannelEqualizerChangedEventArgs(equalizer));
		}

		public async Task RequestVolumeSetup (object? sender)
		{
			List<VolumeData> volumeDataCollection = await GetVolumeSettings();
			VolumeSetupChanged.Invoke(sender, volumeDataCollection);
		}

		public async Task RefreshAudioDevice (object? sender)
		{
			InternalFacade.ResetMasterDevice();
			await RequestVolumeSetup(sender);
			ShowNotification(AUDIO_DEVICE_REFRESHED_MESSAGE);
		}

		public void ShowNotification (string message)
		{
			Tray.ShowInfo(message);
		}

		public async void Initialize ()
		{
			UiContext = SynchronizationContext.Current;
			InternalFacade.MasterVolumeChangedExternally += OnMasterVolumeChangedExternally;
			bool isConnectedToSonar = await InternalFacade.Initialize();

			ExtensionController.Initialize(this);
			List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection = ExtensionController.GetExtensionToolMenuItemCollection();
			extensionMenuItemCollection.Add(new SteelSeriesCompanionExtensionMenuItem(REFRESH_AUDIO_DEVICE_MENU_TEXT, () => _ = RefreshAudioDevice(this)));
			Tray.Initialize(extensionMenuItemCollection);

			if (isConnectedToSonar == false)
			{
				Tray.ShowWarning(SONAR_CONNECTION_FAILED_MESSAGE);
			}

			await GetVolumeSettings();
		}

		private void OnMasterVolumeChangedExternally (float volume, bool mute)
		{
			UiContext?.Post(_ => ApplyExternalChange(SoundChannel.MASTER, volume, mute), null);
		}

		private void ApplyExternalChange (SoundChannel channel, float volume, bool mute)
		{
			if (ConfirmedVolumeMap.TryGetValue(channel, out float confirmedVolume) == false || Math.Abs(confirmedVolume - volume) > VOLUME_CHANGE_TOLERANCE)
			{
				ConfirmedVolumeMap[channel] = volume;
				ChannelVolumeChanged.Invoke(this, new ChannelVolumeChangedEventArgs(channel, volume));
			}

			if (ConfirmedMuteMap.TryGetValue(channel, out bool confirmedMute) == false || confirmedMute != mute)
			{
				ConfirmedMuteMap[channel] = mute;
				ChannelMuteChanged.Invoke(this, new ChannelMuteChangedEventArgs(channel, mute));
			}
		}

		public void Dispose ()
		{
			InternalFacade.MasterVolumeChangedExternally -= OnMasterVolumeChangedExternally;
			ExtensionController.Dispose();
			InternalFacade.Dispose();
			Tray.Dispose();
		}
	}
}
