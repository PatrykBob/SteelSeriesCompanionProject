using SteelSeriesSonarCompanion.External.App.Connection;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class AppShell : Shell
	{
		private CompanionConnection Connection { get; }

		public AppShell (CompanionConnection connection)
		{
			InitializeComponent();
			Connection = connection;
		}

		private void OnPairClicked (object? sender, EventArgs e)
		{
			FlyoutIsPresented = false;
			Connection.StartPairing();
		}

		private void OnEditLayoutClicked (object? sender, EventArgs e)
		{
			FlyoutIsPresented = false;

			if (CurrentPage is MainPage mainPage)
			{
				mainPage.EnterEditMode();
			}
		}

		private void OnRefreshAudioDeviceClicked (object? sender, EventArgs e)
		{
			FlyoutIsPresented = false;
			Connection.RefreshAudioDevice();
		}
	}
}
