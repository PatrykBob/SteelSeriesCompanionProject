using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class InternalCommunicationFacade
	{
		private InternalCommunicationController CommunicationController { get; set; } = new();
		private AudioSourceController AudioController { get; set; } = new();

		public async Task Initialize (int sonarSetupPort)
		{
			await CommunicationController.Initialize(sonarSetupPort);
		}

		public async Task<List<ChannelConfigResponse>> GetConfig ()
		{
			return await CommunicationController.GetConfig();
		}

		public async Task<ChannelConfigResponse?> PostConfig (ChannelConfigResponse config)
		{
			return await CommunicationController.PostConfig(config);
		}

		public async Task<List<VolumeData>> GetVolumeSettings ()
		{
			List<VolumeData> volumeDataCollection = [GetMasterVolumeData()];
			VolumeSettingsResponse? volumeSettings = await CommunicationController.GetVolumeSettings();

			if (volumeSettings != null)
			{
				volumeDataCollection.AddRange(ConvertToVolumeDataCollection(volumeSettings));
			}

			return volumeDataCollection;
		}

		public VolumeData GetMasterVolumeData ()
		{
			float? volume = AudioController.GetMasterVolume();
			bool? mute = AudioController.GetMasterMute();

			return new VolumeData(SoundChannel.MASTER, volume ??= 0.0f, mute ??= false);
		}

		public async Task SetChannelVolume (SoundChannel channel, float volume)
		{
			if (channel == SoundChannel.MASTER)
			{
				AudioController.SetMasterVolume(volume);
			}
			else
			{
				string channelName = ConvertSoundChannel(channel);
				float convertedVolume = ConvertVolumeToLinearGain(volume);
				await CommunicationController.SetChannelVolume(channelName, convertedVolume);
			}
		}

		public async Task SetChannelMute (SoundChannel channel, bool mute)
		{
			if (channel == SoundChannel.MASTER)
			{
				AudioController.SetMasterMute(mute);
			}
			else
			{
				string channelName = ConvertSoundChannel(channel);
				await CommunicationController.SetChannelMute(channelName, mute);
			}
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
			List<VolumeData> volumeDataCollection =
			[
				ConvertToVolumeData(SoundChannel.GAME, volumeSettings.devices.game),
				ConvertToVolumeData(SoundChannel.CHAT, volumeSettings.devices.chatRender),
				ConvertToVolumeData(SoundChannel.MEDIA, volumeSettings.devices.media),
				ConvertToVolumeData(SoundChannel.AUX, volumeSettings.devices.aux),
				ConvertToVolumeData(SoundChannel.MIC, volumeSettings.devices.chatCapture),
			];

			return volumeDataCollection;
		}

		private VolumeData ConvertToVolumeData (SoundChannel channel, VolumeSettingsResponse.VolumeResponse response)
		{
			float volume = ConvertLinearGainToVolume(response.classic.volume);
			return new VolumeData(channel, volume, response.classic.muted);
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
