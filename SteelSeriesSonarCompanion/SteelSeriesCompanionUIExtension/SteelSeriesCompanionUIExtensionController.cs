using SteelSeriesCompanion.SharedCore;
using System.Windows.Forms;

namespace SteelSeriesCompanionUIExtension
{
	public class SteelSeriesCompanionUIExtensionController : BaseSteelSeriesCompanionExtension
	{
		private SteelSeriesCompanionWindow? ExtensionWindow { get; set; }

		public override ToolStripMenuItem GetExtensionMenuItem ()
		{
			ToolStripMenuItem toolStripMwnuItem = new();
			toolStripMwnuItem.Text = "Show UI";
			toolStripMwnuItem.Click += ExtensionMenuClicked;

			return toolStripMwnuItem;

			void ExtensionMenuClicked (object? sender, EventArgs e)
			{
				ShowUI();
			}
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
