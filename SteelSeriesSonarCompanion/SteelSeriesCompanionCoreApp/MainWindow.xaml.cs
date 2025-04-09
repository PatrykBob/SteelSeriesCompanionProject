using SteelSeriesCompanionCoreApp.Setup;
using SteelSeriesSonarCompanion.Communication.Internal;
using System.Windows;

namespace SteelSeriesSonarCompanion
{
	public partial class MainWindow : Window
	{
		private InternalCommunicationFacade InternalFacade { get; set; } = new();

		public MainWindow ()
		{
			Initialize();
			InitializeComponent();
		}

		private async void Initialize ()
		{
			if (SonarSetupLoader.TryGetSonarSetupPort(out int setupPort))
			{
				await InternalFacade.Initialize(setupPort);
			}
			else
			{
				// TODO inform user about problem
			}
		}
	}
}