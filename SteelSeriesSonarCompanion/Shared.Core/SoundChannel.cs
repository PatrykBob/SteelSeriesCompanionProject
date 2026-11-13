using Newtonsoft.Json.Converters;

namespace SteelSeriesSonarCompanion.Shared.Core
{
	[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
	public enum SoundChannel
	{
		MASTER,
		GAME,
		CHAT,
		MEDIA,
		AUX,
		MIC
	}
}
