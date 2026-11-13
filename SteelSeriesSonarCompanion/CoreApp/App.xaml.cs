using SteelSeriesSonarCompanion.CoreApp;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace SteelSeriesSonarCompanion
{
	public partial class App : System.Windows.Application
	{
		private SteelSeriesCompanionCore CompanionCore { get; set; } = new();

		private const string UNEXPECTED_ERROR_MESSAGE = "An unexpected error occurred. The companion keeps running.";

		protected override void OnStartup (StartupEventArgs e)
		{
			base.OnStartup(e);
			DispatcherUnhandledException += OnDispatcherUnhandledException;
			CompanionCore.Initialize();
		}

		private void OnDispatcherUnhandledException (object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Trace.WriteLine($"Unhandled exception: {e.Exception}");
			CompanionCore.ShowNotification(UNEXPECTED_ERROR_MESSAGE);
			e.Handled = true;
		}

		protected override void OnExit (ExitEventArgs e)
		{
			CompanionCore.Dispose();
			base.OnExit(e);
		}
	}

}
