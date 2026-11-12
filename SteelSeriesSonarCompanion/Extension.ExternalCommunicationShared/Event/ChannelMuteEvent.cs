using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class ChannelMuteEvent : BaseExternalCommunicationEvent
	{
		public SoundChannel Channel { get; }
		public bool Mute { get; }

		public const string EVENT_NAME = "ChannelMute";

		public ChannelMuteEvent (SoundChannel channel, bool mute) : base(EVENT_NAME)
		{
			Channel = channel;
			Mute = mute;
		}
	}
}
