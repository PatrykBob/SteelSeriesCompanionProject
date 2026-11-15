using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class EqualizerPanelSettings
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public SoundChannel Channel { get; set; } = SoundChannel.GAME;
	}
}
