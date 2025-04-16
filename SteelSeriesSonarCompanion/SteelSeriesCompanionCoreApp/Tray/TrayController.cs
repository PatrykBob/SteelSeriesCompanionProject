using SteelSeriesCompanion.SharedCore;
using System.IO;

namespace SteelSeriesCompanion.Tray
{
	public class TrayController : IDisposable
	{
		private NotifyIcon TrayIcon { get; set; } = new();

		private const string ICON_NAME = "icon.ico";

		public void Initialize (List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection)
		{
			ContextMenuStrip contextMenu = new();

			for (int i = 0; i < extensionMenuItemCollection.Count; i++)
			{
				contextMenu.Items.Add(ConvertToToolStripMenuItem(extensionMenuItemCollection[i]));
			}

			TrayIcon.ContextMenuStrip = contextMenu;
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ICON_NAME);
			TrayIcon.Icon = new Icon(path);
			TrayIcon.Visible = true;
		}

		public void Dispose ()
		{
			TrayIcon.Visible = false;
			TrayIcon.Dispose();
		}

		private ToolStripMenuItem ConvertToToolStripMenuItem (SteelSeriesCompanionExtensionMenuItem extensionMenuItem)
		{
			ToolStripMenuItem menuItem = new();
			menuItem.Text = extensionMenuItem.Text;

			if (extensionMenuItem.SubMenuItemCollection.Count > 0)
			{
				for (int i = 0; i < extensionMenuItem.SubMenuItemCollection.Count; i++)
				{
					menuItem.DropDownItems.Add(ConvertToToolStripMenuItem(extensionMenuItem.SubMenuItemCollection[i]));
				}
			}
			else
			{
				if (extensionMenuItem.ClickAction != null)
				{
					menuItem.Click += (_, _) => extensionMenuItem.ClickAction.Invoke();
				}
			}
			return menuItem;
		}
	}
}
