using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class ChannelVolumeEvent : BaseExternalCommunicationEvent
	{
		public SoundChannel Channel { get; }
		public float Volume { get; }

		public const string EVENT_NAME = "ChannelVolume";

		public ChannelVolumeEvent (SoundChannel channel, float volume) : base(EVENT_NAME)
		{
			Channel = channel;
			Volume = volume;
		}
	}
}
