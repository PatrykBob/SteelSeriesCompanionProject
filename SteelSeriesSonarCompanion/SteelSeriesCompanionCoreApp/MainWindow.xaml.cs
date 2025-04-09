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
			await InternalFacade.Initialize(6327);
		}
	}
}