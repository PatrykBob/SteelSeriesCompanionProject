using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SelectEqualizerPresetCommand (SoundChannel channel, string presetId) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public string presetId = presetId;

		public const string COMMAND_NAME = "SelectEqualizerPreset";
	}
}
