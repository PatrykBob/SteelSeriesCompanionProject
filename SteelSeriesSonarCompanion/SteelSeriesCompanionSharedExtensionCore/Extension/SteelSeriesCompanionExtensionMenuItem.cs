namespace SteelSeriesCompanion.SharedCore
{
	public class SteelSeriesCompanionExtensionMenuItem
	{
		public string? Text { get; private set; }
		public Action? ClickAction { get; private set; }
		public List<SteelSeriesCompanionExtensionMenuItem> SubMenuItemCollection { get; private set; } = new();

		public SteelSeriesCompanionExtensionMenuItem (string text, Action? clickAction = null)
		{
			Text = text;
			ClickAction = clickAction;
		}

		public void AddSubMenuItem (SteelSeriesCompanionExtensionMenuItem subMenuItem)
		{
			SubMenuItemCollection.Add(subMenuItem);
		}
	}
}
