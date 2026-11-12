using System.Windows;
using SteelSeriesCompanionUIExtension;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.Extension.UI
{
	public class SteelSeriesCompanionUIExtensionController : BaseSteelSeriesCompanionExtension
	{
		private SteelSeriesCompanionWindow? ExtensionWindow { get; set; }

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			SteelSeriesCompanionExtensionMenuItem menuItem = new("Show UI", ShowUI);
			return menuItem;
		}

		public override void Dispose ()
		{
			ExtensionWindow?.Close();
		}

		private void ShowUI ()
		{
			if (CompanionCore == null)
			{
				return;
			}

			if (ExtensionWindow != null)
			{
				FocusWindow(ExtensionWindow);
				return;
			}

			ExtensionWindow = new();
			ExtensionWindow.Closed += OnExtensionWindowClosed;
			ExtensionWindow.Initialize(CompanionCore);
			ExtensionWindow.Show();
		}

		private void FocusWindow (Window window)
		{
			if (window.WindowState == WindowState.Minimized)
			{
				window.WindowState = WindowState.Normal;
			}

			window.Activate();
		}

		private void OnExtensionWindowClosed (object? sender, EventArgs e)
		{
			ExtensionWindow!.Closed -= OnExtensionWindowClosed;
			ExtensionWindow = null;
		}
	}
}
