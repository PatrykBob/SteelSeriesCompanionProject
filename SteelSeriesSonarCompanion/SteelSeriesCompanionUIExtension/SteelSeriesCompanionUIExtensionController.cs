using SteelSeriesCompanion.SharedCore;
using System.Windows.Forms;

namespace SteelSeriesCompanionUIExtension
{
	public class SteelSeriesCompanionUIExtensionController : BaseSteelSeriesCompanionExtension
	{
		private SteelSeriesCompanionWindow? ExtensionWindow { get; set; }

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			PrepareUI(companionCore);
		}

		public override ToolStripMenuItem GetExtensionMenuItem ()
		{
			ToolStripMenuItem toolStripMwnuItem = new();
			toolStripMwnuItem.Text = "Show UI";
			toolStripMwnuItem.Click += ExtensionMenuClicked;

			return toolStripMwnuItem;

			void ExtensionMenuClicked (object? sender, EventArgs e)
			{
				ExtensionWindow?.Show();
			}
		}

		private void PrepareUI (ISteelSeriesCompanionCore companionCore)
		{
			ExtensionWindow = new();
			ExtensionWindow.Initialize(companionCore);
		}
	}
}
