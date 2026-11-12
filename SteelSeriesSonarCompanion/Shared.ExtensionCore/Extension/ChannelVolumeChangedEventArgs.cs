using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension
{
	public class ChannelVolumeChangedEventArgs (SoundChannel channel, float volume) : EventArgs
	{
		public SoundChannel Channel { get; } = channel;
		public float Volume { get; } = volume;
	}
}
