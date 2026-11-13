using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SetChannelMuteCommand (SoundChannel channel, bool mute) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public bool mute = mute;

		public const string COMMAND_NAME = "SetMute";
	}
}
