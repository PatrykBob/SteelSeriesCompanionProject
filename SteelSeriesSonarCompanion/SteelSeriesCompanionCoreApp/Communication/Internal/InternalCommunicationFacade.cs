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
	}
}
