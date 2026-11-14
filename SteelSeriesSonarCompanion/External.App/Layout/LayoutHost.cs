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

			GridLength dividerLength = new(IsEditMode == true ? LayoutDivider.EDIT_SIZE : LayoutDivider.NORMAL_SIZE);
			List<LayoutDivider> dividerCollection = new();

			for (int i = 0; i < splitNode.ChildCollection!.Count; i++)
			{
				if (i > 0)
				{
					LayoutDivider divider = new(grid, splitNode, i - 1, IsEditMode);
					divider.DragCompleted += OnDividerDragCompleted;
					AddToSplitGrid(grid, divider, dividerLength, i * 2 - 1, isLeftToRight);
					dividerCollection.Add(divider);
				}

				LayoutNode child = splitNode.ChildCollection[i];
				AddToSplitGrid(grid, CreateView(child), new GridLength(child.Weight, GridUnitType.Star), i * 2, isLeftToRight);
			}

			AddDividerHandles(grid, dividerCollection, isLeftToRight);
			return grid;
		}

		private void AddDividerHandles (Grid grid, List<LayoutDivider> dividerCollection, bool isLeftToRight)
		{
			for (int i = 0; i < dividerCollection.Count; i++)
			{
				BoxView? handle = dividerCollection[i].Handle;

				if (handle == null)
				{
					continue;
				}

				int index = i * 2 + 1;

				if (isLeftToRight == true)
				{
					grid.Add(handle, index, 0);
				}
				else
				{
					grid.Add(handle, 0, index);
				}
			}
		}

		private void AddToSplitGrid (Grid grid, View view, GridLength length, int index, bool isLeftToRight)
		{
			if (isLeftToRight == true)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition(length));
				grid.Add(view, index, 0);
			}
			else
			{
				grid.RowDefinitions.Add(new RowDefinition(length));
				grid.Add(view, 0, index);
			}
		}

		private void OnDividerDragCompleted ()
		{
			LayoutEdited();
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
