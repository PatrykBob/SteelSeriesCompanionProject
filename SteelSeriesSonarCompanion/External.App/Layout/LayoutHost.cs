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
		private Dictionary<LayoutNode, PanelCell> PanelCellCollection { get; } = new();
		private List<LayoutDivider> DividerCollection { get; } = new();

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
			DividerCollection.Clear();
			HashSet<LayoutNode> shownNodeCollection = new();
			View rootView = CreateView(root, shownNodeCollection);
			DiscardHiddenCells(shownNodeCollection);
			Content = rootView;
		}

		public void SetEditMode (bool isEditMode)
		{
			IsEditMode = isEditMode;

			foreach (PanelCell cell in PanelCellCollection.Values)
			{
				SetCellEditMode(cell);
			}

			for (int i = 0; i < DividerCollection.Count; i++)
			{
				DividerCollection[i].SetEditMode(isEditMode);
			}
		}

		private View CreateView (LayoutNode node, HashSet<LayoutNode> shownNodeCollection)
		{
			return node.IsSplit == true ? CreateSplitView(node, shownNodeCollection) : CreatePanelView(node, shownNodeCollection);
		}

		private View CreateSplitView (LayoutNode splitNode, HashSet<LayoutNode> shownNodeCollection)
		{
			Grid grid = new();
			bool isLeftToRight = splitNode.Axis == SplitAxis.LEFT_TO_RIGHT;

			GridLength dividerLength = new(IsEditMode == true ? LayoutDivider.EDIT_SIZE : LayoutDivider.NORMAL_SIZE);
			List<LayoutDivider> dividerCollection = new();

			for (int i = 0; i < splitNode.ChildCollection!.Count; i++)
			{
				if (i > 0)
				{
					LayoutDivider divider = new(grid, splitNode, i - 1);
					divider.DragCompleted += OnDividerDragCompleted;
					AddToSplitGrid(grid, divider, dividerLength, i * 2 - 1, isLeftToRight);
					dividerCollection.Add(divider);
				}

				LayoutNode child = splitNode.ChildCollection[i];
				AddToSplitGrid(grid, CreateView(child, shownNodeCollection), new GridLength(child.Weight, GridUnitType.Star), i * 2, isLeftToRight);
			}

			AddDividerHandles(grid, dividerCollection, isLeftToRight);
			return grid;
		}

		private void AddDividerHandles (Grid grid, List<LayoutDivider> dividerCollection, bool isLeftToRight)
		{
			for (int i = 0; i < dividerCollection.Count; i++)
			{
				LayoutDivider divider = dividerCollection[i];
				BoxView handle = divider.Handle;
				int index = i * 2 + 1;

				if (isLeftToRight == true)
				{
					grid.Add(handle, index, 0);
				}
				else
				{
					grid.Add(handle, 0, index);
				}

				divider.SetEditMode(IsEditMode);
				DividerCollection.Add(divider);
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

		private View CreatePanelView (LayoutNode panelNode, HashSet<LayoutNode> shownNodeCollection)
		{
			shownNodeCollection.Add(panelNode);

			if (PanelCellCollection.TryGetValue(panelNode, out PanelCell? cell) == false || cell!.PanelTypeId != panelNode.PanelTypeId)
			{
				DiscardCell(panelNode);
				cell = CreateCell(panelNode);
			}
			else
			{
				DetachFromParent(cell);
			}

			SetCellEditMode(cell);
			return cell;
		}

		private PanelCell CreateCell (LayoutNode panelNode)
		{
			BasePanel panel = PanelRegistry.CreatePanel(panelNode.PanelTypeId, panelNode.PanelSettings);
			PanelCell cell = new(panelNode, panel);
			panel.SettingsChanged += changedPanel => OnPanelSettingsChanged(cell, changedPanel);
			PanelCellCollection[panelNode] = cell;
			return cell;
		}

		private void SetCellEditMode (PanelCell cell)
		{
			if (IsEditMode == true && cell.HasOverlay == false)
			{
				cell.SetOverlay(CreateOverlay(cell));
			}

			cell.SetEditMode(IsEditMode);
		}

		private PanelEditOverlay CreateOverlay (PanelCell cell)
		{
			PanelEditOverlay overlay = new(GetPanelDisplayName(cell.PanelTypeId), cell.Panel.HasSettings);
			overlay.SplitRequested += axis => OnSplitRequested(cell, axis);
			overlay.ChangeTypeRequested += () => OnChangeTypeRequested(cell);
			overlay.SettingsRequested += () => OnSettingsRequested(cell);
			overlay.RemoveRequested += () => OnRemoveRequested(cell);
			return overlay;
		}

		private string GetPanelDisplayName (string panelTypeId)
		{
			return PanelRegistry.TryGetDefinition(panelTypeId, out PanelDefinition? definition) == true ? definition!.DisplayName : panelTypeId;
		}

		private void OnSplitRequested (PanelCell cell, SplitAxis axis)
		{
			LayoutNode panelNode = cell.Node;
			LayoutEditor.SplitPanel(Root!, panelNode, axis);

			if (panelNode.IsSplit == true)
			{
				MoveCell(cell, panelNode.ChildCollection![0]);
			}

			ApplyEdit();
		}

		private async void OnChangeTypeRequested (PanelCell cell)
		{
			string[] displayNameCollection = PanelRegistry.DefinitionCollection.Select(definition => definition.DisplayName).ToArray();
			string? chosenDisplayName = await HostPage.DisplayActionSheetAsync(CHANGE_TYPE_TITLE, CANCEL_TEXT, null, displayNameCollection);
			PanelDefinition? chosenDefinition = PanelRegistry.DefinitionCollection.FirstOrDefault(definition => definition.DisplayName == chosenDisplayName);

			if (chosenDefinition == null)
			{
				return;
			}

			LayoutEditor.ChangePanelType(cell.Node, chosenDefinition.TypeId);
			DiscardCell(cell.Node);
			ApplyEdit();
		}

		private async void OnSettingsRequested (PanelCell cell)
		{
			await cell.Panel.TryEditSettings(HostPage);
		}

		private async void OnRemoveRequested (PanelCell cell)
		{
			bool isConfirmed = await HostPage.DisplayAlertAsync(REMOVE_TITLE, REMOVE_MESSAGE, REMOVE_ACCEPT_TEXT, CANCEL_TEXT);

			if (isConfirmed == false)
			{
				return;
			}

			LayoutNode panelNode = cell.Node;
			bool hasParent = LayoutEditor.TryFindParent(Root!, panelNode, out LayoutNode? parent);
			LayoutNode? lastSiblingNode = hasParent == true && parent!.ChildCollection!.Count == 2 ? parent.ChildCollection.Find(child => child != panelNode) : null;
			LayoutEditor.RemovePanel(Root!, panelNode);

			if (lastSiblingNode != null && PanelCellCollection.TryGetValue(lastSiblingNode, out PanelCell? siblingCell) == true)
			{
				MoveCell(siblingCell!, parent!);
			}

			ApplyEdit();
		}

		private void ApplyEdit ()
		{
			Show(Root!);
			LayoutEdited();
		}

		private void OnPanelSettingsChanged (PanelCell cell, BasePanel panel)
		{
			cell.Node.PanelSettings = panel.Settings;
			LayoutEdited();
		}

		private void MoveCell (PanelCell cell, LayoutNode newNode)
		{
			PanelCellCollection.Remove(cell.Node);
			cell.Node = newNode;
			PanelCellCollection[newNode] = cell;
		}

		private void DiscardCell (LayoutNode panelNode)
		{
			if (PanelCellCollection.Remove(panelNode, out PanelCell? cell) == false)
			{
				return;
			}

			cell!.Panel.DetachFromEvents();
			DetachFromParent(cell);
		}

		private void DiscardHiddenCells (HashSet<LayoutNode> shownNodeCollection)
		{
			List<LayoutNode> hiddenNodeCollection = PanelCellCollection.Keys.Where(node => shownNodeCollection.Contains(node) == false).ToList();

			for (int i = 0; i < hiddenNodeCollection.Count; i++)
			{
				DiscardCell(hiddenNodeCollection[i]);
			}
		}

		private void DetachFromParent (View view)
		{
			if (view.Parent is Microsoft.Maui.Controls.Layout parentLayout)
			{
				parentLayout.Remove(view);
			}
			else if (view.Parent is ContentView parentContentView)
			{
				parentContentView.Content = null;
			}
		}
	}
}
