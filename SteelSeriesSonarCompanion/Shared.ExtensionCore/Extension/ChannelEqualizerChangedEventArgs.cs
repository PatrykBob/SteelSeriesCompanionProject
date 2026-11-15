using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension
{
	public class ChannelEqualizerChangedEventArgs (EqualizerData equalizer) : EventArgs
	{
		public EqualizerData Equalizer { get; } = equalizer;
	}
}
