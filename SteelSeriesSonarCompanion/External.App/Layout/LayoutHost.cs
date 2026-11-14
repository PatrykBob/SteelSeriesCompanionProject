using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class LayoutHost : ContentView
	{
		public event Action LayoutEdited = delegate { };

		public bool IsEditMode { get; private set; }

		private PanelRegistry PanelRegistry { get; }
		private Page HostPage { get; }
		private LayoutNode? Root { get; set; }
		private List<BasePanel> PanelCollection { get; } = new();

		private const double PANEL_SPACING = 8;
		private const string CHANGE_TYPE_TITLE = "Panel type";
		private const string CANCEL_TEXT = "Cancel";
		private const string REMOVE_TITLE = "Remove panel";
		private const string REMOVE_MESSAGE = "Remove this panel?";
		private const string REMOVE_ACCEPT_TEXT = "Remove";

		public LayoutHost (PanelRegistry panelRegistry, Page hostPage)
		{
			PanelRegistry = panelRegistry;
			HostPage = hostPage;
		}

		public void Show (LayoutNode root)
		{
			Root = root;
			DetachPanels();
			Content = CreateView(root);
		}

		public void SetEditMode (bool isEditMode)
		{
			IsEditMode = isEditMode;

			if (Root != null)
			{
				Show(Root);
			}
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

			if (IsEditMode == false)
			{
				return panel;
			}

			panel.InputTransparent = true;
			PanelEditOverlay overlay = new(GetPanelDisplayName(panelNode.PanelTypeId), panel.HasSettings);
			overlay.SplitRequested += axis => OnSplitRequested(panelNode, axis);
			overlay.ChangeTypeRequested += () => OnChangeTypeRequested(panelNode);
			overlay.SettingsRequested += () => OnSettingsRequested(panel);
			overlay.RemoveRequested += () => OnRemoveRequested(panelNode);

			return new Grid
			{
				panel,
				overlay
			};
		}

		private string GetPanelDisplayName (string panelTypeId)
		{
			return PanelRegistry.TryGetDefinition(panelTypeId, out PanelDefinition? definition) == true ? definition!.DisplayName : panelTypeId;
		}

		private void OnSplitRequested (LayoutNode panelNode, SplitAxis axis)
		{
			LayoutEditor.SplitPanel(Root!, panelNode, axis);
			ApplyEdit();
		}

		private async void OnChangeTypeRequested (LayoutNode panelNode)
		{
			string[] displayNameCollection = PanelRegistry.DefinitionCollection.Select(definition => definition.DisplayName).ToArray();
			string? chosenDisplayName = await HostPage.DisplayActionSheetAsync(CHANGE_TYPE_TITLE, CANCEL_TEXT, null, displayNameCollection);
			PanelDefinition? chosenDefinition = PanelRegistry.DefinitionCollection.FirstOrDefault(definition => definition.DisplayName == chosenDisplayName);

			if (chosenDefinition == null)
			{
				return;
			}

			LayoutEditor.ChangePanelType(panelNode, chosenDefinition.TypeId);
			ApplyEdit();
		}

		private async void OnSettingsRequested (BasePanel panel)
		{
			await panel.TryEditSettings(HostPage);
		}

		private async void OnRemoveRequested (LayoutNode panelNode)
		{
			bool isConfirmed = await HostPage.DisplayAlertAsync(REMOVE_TITLE, REMOVE_MESSAGE, REMOVE_ACCEPT_TEXT, CANCEL_TEXT);

			if (isConfirmed == false)
			{
				return;
			}

			LayoutEditor.RemovePanel(Root!, panelNode);
			ApplyEdit();
		}

		private void ApplyEdit ()
		{
			Show(Root!);
			LayoutEdited();
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
