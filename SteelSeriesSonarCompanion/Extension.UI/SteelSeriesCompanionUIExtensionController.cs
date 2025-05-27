using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesCompanionUIExtension
{
	public class SteelSeriesCompanionUIExtensionController : BaseSteelSeriesCompanionExtension
	{
		private SteelSeriesCompanionWindow? ExtensionWindow { get; set; }

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			SteelSeriesCompanionExtensionMenuItem menuItem = new("Show UI", ShowUI);
			return menuItem;
		}

		private void ShowUI ()
		{
			if (CompanionCore != null)
			{
				ExtensionWindow = new();
				ExtensionWindow.Initialize(CompanionCore);
				ExtensionWindow.Show();
			}
		}
	}
}
