using SteelSeriesCompanion;
using System.Windows;

namespace SteelSeriesSonarCompanion
{
	public partial class App : System.Windows.Application
	{
		private SteelSeriesCompanionCore CompanionCore { get; set; } = new();

		protected override void OnStartup (StartupEventArgs e)
		{
			base.OnStartup(e);
			CompanionCore.Initialize();
		}

		protected override void OnExit (ExitEventArgs e)
		{
			CompanionCore.Dispose();
			base.OnExit(e);
		}
	}

}
