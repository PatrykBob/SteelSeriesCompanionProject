using SteelSeriesSonarCompanion.CoreApp;
using SteelSeriesSonarCompanion.CoreApp.Logging;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SteelSeriesSonarCompanion
{
	public partial class App : System.Windows.Application
	{
		private SteelSeriesCompanionCore CompanionCore { get; set; } = new();
		private Mutex? SingleInstanceMutex { get; set; }

		private const string UNEXPECTED_ERROR_MESSAGE = "An unexpected error occurred. The companion keeps running.";
		private const string ALREADY_RUNNING_MESSAGE = "SteelSeries Sonar Companion is already running.";
		private const string ALREADY_RUNNING_CAPTION = "SteelSeries Sonar Companion";
		private const string SINGLE_INSTANCE_MUTEX_NAME = @"Local\SteelSeriesSonarCompanion.SingleInstance";

		protected override void OnStartup (StartupEventArgs e)
		{
			base.OnStartup(e);
			SingleInstanceMutex = new Mutex(true, SINGLE_INSTANCE_MUTEX_NAME, out bool isFirstInstance);

			if (isFirstInstance == false)
			{
				SingleInstanceMutex.Dispose();
				SingleInstanceMutex = null;
				System.Windows.MessageBox.Show(ALREADY_RUNNING_MESSAGE, ALREADY_RUNNING_CAPTION, MessageBoxButton.OK, MessageBoxImage.Information);
				Shutdown();
				return;
			}

			FileTraceListener? fileTraceListener = FileTraceListener.TryCreate();

			if (fileTraceListener != null)
			{
				Trace.Listeners.Add(fileTraceListener);
			}

			Trace.WriteLine("Companion started");
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
			if (SingleInstanceMutex != null)
			{
				CompanionCore.Dispose();
				SingleInstanceMutex.ReleaseMutex();
				SingleInstanceMutex.Dispose();
			}

			base.OnExit(e);
		}
	}

}
