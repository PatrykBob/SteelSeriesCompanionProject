using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class ImportEqualizerPresetCommand (SoundChannel channel, string parametricEqText, string? newPresetName) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public string parametricEqText = parametricEqText;
		public string? newPresetName = newPresetName;

		public const string COMMAND_NAME = "ImportEqualizerPreset";
	}
}
