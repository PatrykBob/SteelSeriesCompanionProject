using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;
using System.IO;

namespace SteelSeriesSonarCompanion.CoreApp.Tray
{
	public class TrayController : IDisposable
	{
		private NotifyIcon TrayIcon { get; set; } = new();
		private List<(string Message, ToolTipIcon Icon)> PendingBalloonCollection { get; set; } = new();

		private const string ICON_NAME = "icon.ico";
		private const string TRAY_TITLE = "SteelSeries Sonar Companion";
		private const int BALLOON_TIMEOUT_MS = 5000;

		public void Initialize (List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection)
		{
			TrayIcon.ContextMenuStrip = GenerateContextMenu(extensionMenuItemCollection);
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ICON_NAME);
			TrayIcon.Icon = File.Exists(path) ? new Icon(path) : SystemIcons.Application;
			TrayIcon.Text = TRAY_TITLE;
			TrayIcon.Visible = true;

			for (int i = 0; i < PendingBalloonCollection.Count; i++)
			{
				TrayIcon.ShowBalloonTip(BALLOON_TIMEOUT_MS, TRAY_TITLE, PendingBalloonCollection[i].Message, PendingBalloonCollection[i].Icon);
			}

			PendingBalloonCollection.Clear();
		}

		public void ShowWarning (string message)
		{
			ShowBalloon(message, ToolTipIcon.Warning);
		}

		public void ShowInfo (string message)
		{
			ShowBalloon(message, ToolTipIcon.Info);
		}

		private void ShowBalloon (string message, ToolTipIcon icon)
		{
			if (TrayIcon.Visible == false)
			{
				PendingBalloonCollection.Add((message, icon));
				return;
			}

			TrayIcon.ShowBalloonTip(BALLOON_TIMEOUT_MS, TRAY_TITLE, message, icon);
		}

		public void Dispose ()
		{
			TrayIcon.Visible = false;
			TrayIcon.Dispose();
		}

		private ContextMenuStrip GenerateContextMenu (List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection)
		{
			ContextMenuStrip contextMenu = new();

			for (int i = 0; i < extensionMenuItemCollection.Count; i++)
			{
				contextMenu.Items.Add(ConvertToToolStripMenuItem(extensionMenuItemCollection[i]));
			}

			AddExitItem();

			return contextMenu;

			void AddExitItem ()
			{
				contextMenu.Items.Add(new ToolStripSeparator());
				contextMenu.Items.Add("Exit", null, Exit);
			}

			void Exit (object? sender, EventArgs e)
			{
				System.Windows.Application.Current.Shutdown();
			}
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
