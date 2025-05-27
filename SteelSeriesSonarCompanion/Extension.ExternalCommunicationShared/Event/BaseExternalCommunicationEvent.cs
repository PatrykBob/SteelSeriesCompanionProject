namespace SteelSeriesCompanion.ExternalCommunication.Shared.Event
{
	public class BaseExternalCommunicationEvent
	{
		public string EventName { get; }

		public BaseExternalCommunicationEvent (string eventName)
		{
			EventName = eventName;
		}
	}
}
