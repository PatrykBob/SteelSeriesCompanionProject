using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class PanelCell : Grid
	{
		public LayoutNode Node { get; set; }
		public BasePanel Panel { get; }
		public string PanelTypeId { get; }
		public bool HasOverlay => Overlay != null;

		private PanelEditOverlay? Overlay { get; set; }

		public PanelCell (LayoutNode node, BasePanel panel)
		{
			Node = node;
			Panel = panel;
			PanelTypeId = node.PanelTypeId;
			Add(panel);
		}

		public void SetOverlay (PanelEditOverlay overlay)
		{
			Overlay = overlay;
			Add(overlay);
		}

		public void SetEditMode (bool isEditMode)
		{
			Panel.InputTransparent = isEditMode;

			if (Overlay != null)
			{
				Overlay.IsVisible = isEditMode;
			}
		}
	}
}
