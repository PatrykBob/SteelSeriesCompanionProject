using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SetChannelVolumeCommand (SoundChannel channel, float volume) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public float volume = volume;

		public const string COMMAND_NAME = "SetVolume";
	}
}
