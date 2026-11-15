using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SaveEqualizerPresetCommand (SoundChannel channel, EqualizerSettings settings, string? newPresetName) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public EqualizerSettings settings = settings;
		public string? newPresetName = newPresetName;

		public const string COMMAND_NAME = "SaveEqualizerPreset";
	}
}
