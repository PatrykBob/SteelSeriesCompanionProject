using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension
{
	public class ChannelMuteChangedEventArgs (SoundChannel channel, bool mute) : EventArgs
	{
		public SoundChannel Channel { get; } = channel;
		public bool Mute { get; } = mute;
	}
}
