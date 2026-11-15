using System.Diagnostics;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class InternalCommunicationFacade : IDisposable
	{
		private const string MASTER_REDIRECTION_ID = "game";

		private InternalCommunicationController CommunicationController { get; set; } = new();
		private AudioSourceController AudioController { get; set; } = new();
		private string? CachedMasterDeviceId { get; set; }

		public event Action<float, bool> MasterVolumeChangedExternally
		{
			add => AudioController.ExternalVolumeChanged += value;
			remove => AudioController.ExternalVolumeChanged -= value;
		}

		public async Task<bool> Initialize ()
		{
			return await CommunicationController.Initialize();
		}

		public void Dispose ()
		{
			CommunicationController.Dispose();
			AudioController.Dispose();
		}

		public async Task<List<VolumeData>> GetVolumeSettings ()
		{
			List<VolumeData> volumeDataCollection = [await GetMasterVolumeData()];
			VolumeSettingsResponse? volumeSettings = await CommunicationController.GetVolumeSettings();

			if (volumeSettings != null)
			{
				volumeDataCollection.AddRange(ConvertToVolumeDataCollection(volumeSettings));
			}

			return volumeDataCollection;
		}

		private async Task<VolumeData> GetMasterVolumeData ()
		{
			string? deviceId = await GetMasterDeviceId();
			float? volume = AudioController.GetVolume(deviceId);
			bool? mute = AudioController.GetMute(deviceId);

			return new VolumeData(SoundChannel.MASTER, volume ??= 0.0f, mute ??= false);
		}

		private async Task<string?> GetMasterDeviceId ()
		{
			if (CachedMasterDeviceId != null)
			{
				return CachedMasterDeviceId;
			}

			List<ClassicRedirectionResponse>? redirections = await CommunicationController.GetClassicRedirections();
			CachedMasterDeviceId = redirections?.FirstOrDefault(redirection => redirection.id == MASTER_REDIRECTION_ID)?.deviceId;
			return CachedMasterDeviceId;
		}

		public void ResetMasterDevice ()
		{
			CachedMasterDeviceId = null;
			AudioController.ReleaseCachedDevice();
		}

		public async Task<bool> TrySetChannelVolume (SoundChannel channel, float volume)
		{
			if (channel == SoundChannel.MASTER)
			{
				return AudioController.TrySetVolume(await GetMasterDeviceId(), volume);
			}

			string channelName = ConvertSoundChannel(channel);
			float convertedVolume = ConvertVolumeToLinearGain(volume);
			return await CommunicationController.TrySetChannelVolume(channelName, convertedVolume);
		}

		public async Task<bool> TrySetChannelMute (SoundChannel channel, bool mute)
		{
			if (channel == SoundChannel.MASTER)
			{
				return AudioController.TrySetMute(await GetMasterDeviceId(), mute);
			}

			string channelName = ConvertSoundChannel(channel);
			return await CommunicationController.TrySetChannelMute(channelName, mute);
		}

		public async Task<EqualizerData?> GetEqualizerData (SoundChannel channel)
		{
			ChannelConfigResponse? config = await GetSelectedConfig(channel);
			return config == null ? null : EqualizerConfigConverter.ConvertToEqualizerData(channel, config);
		}

		public async Task<List<EqualizerPreset>> GetEqualizerPresets (SoundChannel channel)
		{
			List<ChannelConfigResponse>? configs = await CommunicationController.GetConfigs(ConvertSoundChannel(channel));
			return configs?.Select(EqualizerConfigConverter.ConvertToEqualizerPreset).ToList() ?? [];
		}

		public async Task<bool> TrySetEqualizerEnabled (SoundChannel channel, bool enabled)
		{
			ChannelConfigResponse? config = await GetSelectedConfig(channel);
			ChannelConfigResponse.EqualizerResponse? equalizer = config?.data?.parametricEQ;

			if (config == null || equalizer == null)
			{
				return false;
			}

			equalizer.enabled = enabled;
			return await CommunicationController.TryUpdateConfig(config);
		}

		private async Task<ChannelConfigResponse?> GetSelectedConfig (SoundChannel channel)
		{
			string virtualAudioDevice = ConvertSoundChannel(channel);
			List<ChannelConfigResponse>? configs = await CommunicationController.GetSelectedConfigs();
			ChannelConfigResponse? config = configs?.FirstOrDefault(config => config.virtualAudioDevice == virtualAudioDevice);

			if (config == null)
			{
				Trace.WriteLine($"Sonar returned no selected config for {channel}");
			}

			return config;
		}

		private string ConvertSoundChannel (SoundChannel channel)
		{
			return channel switch
			{
				SoundChannel.GAME => "game",
				SoundChannel.CHAT => "chatRender",
				SoundChannel.MEDIA => "media",
				SoundChannel.AUX => "aux",
				SoundChannel.MIC => "chatCapture",
				_ => UnimplementedSoundChannel(),
			};

			string UnimplementedSoundChannel ()
			{
				return string.Empty;
			}
		}

		private List<VolumeData> ConvertToVolumeDataCollection (VolumeSettingsResponse volumeSettings)
		{
			List<VolumeData> volumeDataCollection = [];
			VolumeSettingsResponse.DevicesResponse? devices = volumeSettings.devices;

			if (devices == null)
			{
				Trace.WriteLine("Sonar returned no device volume data");
				return volumeDataCollection;
			}

			AddVolumeData(volumeDataCollection, SoundChannel.GAME, devices.game);
			AddVolumeData(volumeDataCollection, SoundChannel.CHAT, devices.chatRender);
			AddVolumeData(volumeDataCollection, SoundChannel.MEDIA, devices.media);
			AddVolumeData(volumeDataCollection, SoundChannel.AUX, devices.aux);
			AddVolumeData(volumeDataCollection, SoundChannel.MIC, devices.chatCapture);

			return volumeDataCollection;
		}

		private void AddVolumeData (List<VolumeData> volumeDataCollection, SoundChannel channel, VolumeSettingsResponse.VolumeResponse? response)
		{
			VolumeSettingsResponse.ClassicVolumeResponse? classic = response?.classic;

			if (classic == null)
			{
				Trace.WriteLine($"Sonar returned no volume data for {channel}");
				return;
			}

			float volume = ConvertLinearGainToVolume(classic.volume);
			volumeDataCollection.Add(new VolumeData(channel, volume, classic.muted));
		}

		private float ConvertLinearGainToVolume (float gain)
		{
			return (float)Math.Sqrt(gain);
		}

		private float ConvertVolumeToLinearGain (float volume)
		{
			return (float)Math.Pow(volume, 2);
		}
	}
}
