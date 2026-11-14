using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class LayoutNode
	{
		public double Weight { get; set; } = 1;

		[JsonConverter(typeof(StringEnumConverter))]
		public SplitAxis Axis { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore, ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public List<LayoutNode>? ChildCollection { get; set; }

		public string PanelTypeId { get; set; } = PanelRegistry.EMPTY_PANEL_TYPE_ID;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public JObject? PanelSettings { get; set; }

		[JsonIgnore]
		public bool IsSplit => ChildCollection != null && ChildCollection.Count > 0;

		public static LayoutNode CreatePanel (string panelTypeId)
		{
			return new LayoutNode
			{
				PanelTypeId = panelTypeId
			};
		}
	}
}
