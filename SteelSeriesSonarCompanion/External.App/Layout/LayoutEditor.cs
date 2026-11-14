using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public static class LayoutEditor
	{
		public static LayoutNode SplitPanel (LayoutNode root, LayoutNode panelNode, SplitAxis axis)
		{
			LayoutNode newPanelNode = LayoutNode.CreatePanel(PanelRegistry.EMPTY_PANEL_TYPE_ID);

			if (TryFindParent(root, panelNode, out LayoutNode? parent) == true && parent!.Axis == axis)
			{
				double sharedWeight = panelNode.Weight / 2;
				panelNode.Weight = sharedWeight;
				newPanelNode.Weight = sharedWeight;
				parent.ChildCollection!.Insert(parent.ChildCollection.IndexOf(panelNode) + 1, newPanelNode);
				return newPanelNode;
			}

			LayoutNode movedPanelNode = new()
			{
				PanelTypeId = panelNode.PanelTypeId,
				PanelSettings = panelNode.PanelSettings
			};

			panelNode.Axis = axis;
			panelNode.ChildCollection = new() { movedPanelNode, newPanelNode };
			panelNode.PanelTypeId = PanelRegistry.EMPTY_PANEL_TYPE_ID;
			panelNode.PanelSettings = null;
			return newPanelNode;
		}

		public static void RemovePanel (LayoutNode root, LayoutNode panelNode)
		{
			if (panelNode == root)
			{
				ChangePanelType(panelNode, PanelRegistry.EMPTY_PANEL_TYPE_ID);
				return;
			}

			if (TryFindParent(root, panelNode, out LayoutNode? parent) == false)
			{
				return;
			}

			parent!.ChildCollection!.Remove(panelNode);

			if (parent.ChildCollection.Count == 1)
			{
				LayoutNode remainingNode = parent.ChildCollection[0];
				parent.Axis = remainingNode.Axis;
				parent.ChildCollection = remainingNode.ChildCollection;
				parent.PanelTypeId = remainingNode.PanelTypeId;
				parent.PanelSettings = remainingNode.PanelSettings;
			}
		}

		public static void ChangePanelType (LayoutNode panelNode, string panelTypeId)
		{
			panelNode.PanelTypeId = panelTypeId;
			panelNode.PanelSettings = null;
		}

		public static bool TryFindParent (LayoutNode root, LayoutNode node, out LayoutNode? parent)
		{
			if (root.IsSplit == true)
			{
				for (int i = 0; i < root.ChildCollection!.Count; i++)
				{
					LayoutNode child = root.ChildCollection[i];

					if (child == node)
					{
						parent = root;
						return true;
					}

					if (TryFindParent(child, node, out parent) == true)
					{
						return true;
					}
				}
			}

			parent = null;
			return false;
		}
	}
}
