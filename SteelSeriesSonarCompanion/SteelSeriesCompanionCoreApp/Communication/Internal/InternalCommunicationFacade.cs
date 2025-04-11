using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesSonarCompanion.Communication.Internal
{
	public class InternalCommunicationFacade
	{
		private InternalCommunicationController CommunicationController { get; set; } = new();

		public async Task Initialize (int sonarSetupPort)
		{
			await CommunicationController.Initialize(sonarSetupPort);
		}

		public async Task SetChannelVolume (SoundChannel channel, float volume)
		{
			string channelName = ConvertSoundChannel(channel);
			await CommunicationController.SetChannelVolume(channelName, volume);
		}

		public async Task SetChannelMute (SoundChannel channel, bool mute)
		{
			string channelName = ConvertSoundChannel (channel);
			await CommunicationController.SetChannelMute(channelName, mute);
		}

		private string ConvertSoundChannel (SoundChannel channel)
		{
			switch (channel)
			{
				case SoundChannel.GAME:
					return "game";
				case SoundChannel.CHAT:
					return "chatRender";
				case SoundChannel.MEDIA:
					return "media";
				case SoundChannel.AUX:
					return "aux";
				case SoundChannel.MIC:
					return "chatCapture";
				default:
					return UnimplementedSoundChannel();
			}

			string UnimplementedSoundChannel ()
			{
				return string.Empty;
			}
		}
	}
}
