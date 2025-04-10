using SteelSeriesCompanion;
using System.Windows;

namespace SteelSeriesSonarCompanion
{
	public partial class MainWindow : Window
	{
		private SteelSeriesCompanionCore Core { get; set; } = new();

		public MainWindow ()
		{
			Core.Initialize();
			InitializeComponent();
		}
	}
}