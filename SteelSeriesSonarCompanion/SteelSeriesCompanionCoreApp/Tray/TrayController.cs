using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace SteelSeriesCompanion.Tray
{
	public class TrayController : IDisposable
	{
		private NotifyIcon TrayIcon { get; set; } = new();

		public void Initialize (List<ToolStripMenuItem> extensionMenuItemCollection)
		{
			ContextMenuStrip contextMenu = new();
			contextMenu.Items.Add(extensionMenuItemCollection[0]);
			TrayIcon.ContextMenuStrip = contextMenu;
			TrayIcon.Icon = new Icon("C:\\Windows\\WinSxS\\wow64_microsoft-windows-onedrive-setup_31bf3856ad364e35_10.0.19041.1_none_e585f901f9ce93e6\\OneDrive.ico");
			TrayIcon.Visible = true;
		}

		public void Dispose ()
		{
			TrayIcon.Visible = false;
			TrayIcon.Dispose();
		}
	}
}
