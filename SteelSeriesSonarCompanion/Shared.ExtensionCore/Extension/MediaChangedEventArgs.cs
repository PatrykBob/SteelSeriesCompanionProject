using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension
{
	public class MediaChangedEventArgs (MediaData media) : EventArgs
	{
		public MediaData Media { get; } = media;
	}
}
