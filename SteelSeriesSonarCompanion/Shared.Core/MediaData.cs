namespace SteelSeriesSonarCompanion.Shared.Core
{
	public class MediaData
	{
		public bool HasSession { get; }
		public bool IsPlaying { get; }
		public string Title { get; }
		public string Artist { get; }

		public static MediaData NoSession { get; } = new(false, false, string.Empty, string.Empty);

		public MediaData (bool hasSession, bool isPlaying, string title, string artist)
		{
			HasSession = hasSession;
			IsPlaying = isPlaying;
			Title = title;
			Artist = artist;
		}

		public bool IsSameAs (MediaData other)
		{
			return HasSession == other.HasSession && IsPlaying == other.IsPlaying && Title == other.Title && Artist == other.Artist;
		}
	}
}
