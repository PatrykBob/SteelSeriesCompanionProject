using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class EqualizerEditResult
	{
		public EqualizerSettings Settings { get; }
		public string? NewPresetName { get; }

		public EqualizerEditResult (EqualizerSettings settings, string? newPresetName)
		{
			Settings = settings;
			NewPresetName = newPresetName;
		}
	}
}
