using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Connection;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class PanelDefinition
	{
		public string TypeId { get; }
		public string DisplayName { get; }
		private Func<CompanionConnection, JObject?, BasePanel> Factory { get; }

		public PanelDefinition (string typeId, string displayName, Func<CompanionConnection, JObject?, BasePanel> factory)
		{
			TypeId = typeId;
			DisplayName = displayName;
			Factory = factory;
		}

		public BasePanel CreatePanel (CompanionConnection connection, JObject? settings)
		{
			return Factory(connection, settings);
		}
	}
}
