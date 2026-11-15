using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SetEqualizerEnabledCommand (SoundChannel channel, bool enabled) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public bool enabled = enabled;

		public const string COMMAND_NAME = "SetEqualizerEnabled";
	}
}
