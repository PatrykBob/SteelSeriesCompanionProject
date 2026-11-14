using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class LayoutHost : ContentView
	{
		public event Action LayoutEdited = delegate { };

		private PanelRegistry PanelRegistry { get; }
		private List<BasePanel> PanelCollection { get; } = new();

		private const double PANEL_SPACING = 8;

		public LayoutHost (PanelRegistry panelRegistry)
		{
			PanelRegistry = panelRegistry;
		}

		public void Show (LayoutNode root)
		{
			DetachPanels();
			Content = CreateView(root);
		}

		private View CreateView (LayoutNode node)
		{
			return node.IsSplit == true ? CreateSplitView(node) : CreatePanelView(node);
		}

		private View CreateSplitView (LayoutNode splitNode)
		{
			Grid grid = new();
			bool isLeftToRight = splitNode.Axis == SplitAxis.LEFT_TO_RIGHT;

			for (int i = 0; i < splitNode.ChildCollection!.Count; i++)
			{
				LayoutNode child = splitNode.ChildCollection[i];
				GridLength length = new(child.Weight, GridUnitType.Star);
				View childView = CreateView(child);

				if (isLeftToRight == true)
				{
					grid.ColumnDefinitions.Add(new ColumnDefinition(length));
					grid.Add(childView, i, 0);
				}
				else
				{
					grid.RowDefinitions.Add(new RowDefinition(length));
					grid.Add(childView, 0, i);
				}
			}

			grid.ColumnSpacing = PANEL_SPACING;
			grid.RowSpacing = PANEL_SPACING;
			return grid;
		}

		private View CreatePanelView (LayoutNode panelNode)
		{
			BasePanel panel = PanelRegistry.CreatePanel(panelNode.PanelTypeId, panelNode.PanelSettings);
			panel.SettingsChanged += changedPanel => OnPanelSettingsChanged(panelNode, changedPanel);
			PanelCollection.Add(panel);
			return panel;
		}

		private void OnPanelSettingsChanged (LayoutNode panelNode, BasePanel panel)
		{
			panelNode.PanelSettings = panel.Settings;
			LayoutEdited();
		}

		private void DetachPanels ()
		{
			for (int i = 0; i < PanelCollection.Count; i++)
			{
				PanelCollection[i].DetachFromEvents();
			}

			PanelCollection.Clear();
		}
	}
}
