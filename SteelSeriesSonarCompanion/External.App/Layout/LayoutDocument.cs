using Newtonsoft.Json;
using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class LayoutDocument
	{
		public int Version { get; set; } = 1;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public LayoutNode Root { get; set; } = LayoutNode.CreatePanel(VolumeMixerPanel.TYPE_ID);
	}
}
