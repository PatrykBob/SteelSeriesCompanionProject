using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class InternalCommunicationFacade
	{
		private InternalCommunicationController CommunicationController { get; set; } = new();

		public async Task Initialize (int sonarSetupPort)
		{
			await CommunicationController.Initialize(sonarSetupPort);
		}

		public async Task<List<VolumeData>> GetVolumeSettings ()
		{
			VolumeSettingsResponse? volumeSettings = await CommunicationController.GetVolumeSettings();

			if (volumeSettings != null)
			{
				return ConvertToVolumeDataCollection(volumeSettings);
			}

			return [];
		}

		public async Task SetChannelVolume (SoundChannel channel, float volume)
		{
			string channelName = ConvertSoundChannel(channel);
			await CommunicationController.SetChannelVolume(channelName, volume);
		}

		public async Task SetChannelMute (SoundChannel channel, bool mute)
		{
			string channelName = ConvertSoundChannel(channel);
			await CommunicationController.SetChannelMute(channelName, mute);
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
			return new VolumeData(channel, response.classic.volume, response.classic.muted);
		}
	}
}
