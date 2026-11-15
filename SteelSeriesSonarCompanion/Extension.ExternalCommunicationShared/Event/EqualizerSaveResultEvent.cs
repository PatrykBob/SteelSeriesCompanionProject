using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class EqualizerSaveResultEvent : BaseExternalCommunicationEvent
	{
		public SoundChannel Channel { get; }
		public bool IsSuccess { get; }

		public const string EVENT_NAME = "EqualizerSaveResult";

		public EqualizerSaveResultEvent (SoundChannel channel, bool isSuccess) : base(EVENT_NAME)
		{
			Channel = channel;
			IsSuccess = isSuccess;
		}
	}
}
