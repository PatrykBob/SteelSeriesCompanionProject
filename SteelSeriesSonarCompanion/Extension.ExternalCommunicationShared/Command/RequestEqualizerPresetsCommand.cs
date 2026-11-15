using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class RequestEqualizerPresetsCommand (SoundChannel channel) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;

		public const string COMMAND_NAME = "RequestEqualizerPresets";
	}
}
