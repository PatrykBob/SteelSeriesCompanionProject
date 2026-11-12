namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Event
{
	public class AuthenticatedEvent : BaseExternalCommunicationEvent
	{
		public string? NewToken { get; }

		public const string EVENT_NAME = "Authenticated";

		public AuthenticatedEvent (string? newToken) : base(EVENT_NAME)
		{
			NewToken = newToken;
		}
	}
}
