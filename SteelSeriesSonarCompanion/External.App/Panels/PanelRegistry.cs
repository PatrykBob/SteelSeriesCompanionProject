using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Connection;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class PanelRegistry
	{
		public const string EMPTY_PANEL_TYPE_ID = "Empty";

		public IReadOnlyList<PanelDefinition> DefinitionCollection => Definitions;

		private List<PanelDefinition> Definitions { get; } = new();
		private CompanionConnection Connection { get; }

		private const string EMPTY_PANEL_DISPLAY_NAME = "Empty";

		public PanelRegistry (CompanionConnection connection)
		{
			Connection = connection;
			Register(new PanelDefinition(EMPTY_PANEL_TYPE_ID, EMPTY_PANEL_DISPLAY_NAME, (panelConnection, settings) => new EmptyPanel(panelConnection, settings)));
			Register(new PanelDefinition(VolumeMixerPanel.TYPE_ID, VolumeMixerPanel.DISPLAY_NAME, (panelConnection, settings) => new VolumeMixerPanel(panelConnection, settings)));
			Register(new PanelDefinition(EqualizerPanel.TYPE_ID, EqualizerPanel.DISPLAY_NAME, (panelConnection, settings) => new EqualizerPanel(panelConnection, settings)));
			Register(new PanelDefinition(MediaPanel.TYPE_ID, MediaPanel.DISPLAY_NAME, (panelConnection, settings) => new MediaPanel(panelConnection, settings)));
		}

		public void Register (PanelDefinition definition)
		{
			Definitions.RemoveAll(existingDefinition => existingDefinition.TypeId == definition.TypeId);
			Definitions.Add(definition);
		}

		public bool TryGetDefinition (string typeId, out PanelDefinition? definition)
		{
			for (int i = 0; i < Definitions.Count; i++)
			{
				if (Definitions[i].TypeId == typeId)
				{
					definition = Definitions[i];
					return true;
				}
			}

			definition = null;
			return false;
		}

		public BasePanel CreatePanel (string typeId, JObject? settings)
		{
			if (TryGetDefinition(typeId, out PanelDefinition? definition) == false)
			{
				return new EmptyPanel(Connection, null);
			}

			return definition!.CreatePanel(Connection, settings);
		}
	}
}
