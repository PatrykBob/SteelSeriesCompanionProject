using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class VolumeMixerSettings
	{
		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace, ItemConverterType = typeof(StringEnumConverter))]
		public List<SoundChannel> ChannelCollection { get; set; } = new()
		{
			SoundChannel.MASTER,
			SoundChannel.GAME,
			SoundChannel.CHAT,
			SoundChannel.MEDIA,
			SoundChannel.AUX,
			SoundChannel.MIC
		};
	}
}
