using SteelSeriesSonarCompanion.External.App.Connection;
using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class MainPage : ContentPage
	{
		private CompanionConnection Connection { get; }
		private PanelRegistry PanelRegistry { get; }

		public MainPage (CompanionConnection connection, PanelRegistry panelRegistry)
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			InitializeComponent();
			Connection = connection;
			PanelRegistry = panelRegistry;
			PanelRoot.Content = PanelRegistry.CreatePanel(VolumeMixerPanel.TYPE_ID, null);
			Connection.StatusChanged += OnStatusChanged;
			ConnectionLabel.Text = Connection.Status;
			Connection.Start();
		}

		private void OnStatusChanged (string status)
		{
			ConnectionLabel.Text = status;
		}
	}
}
