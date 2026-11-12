namespace SteelSeriesSonarCompanion.External.App
{
	public partial class AppShell : Shell
	{
		public AppShell ()
		{
			InitializeComponent();
		}

		private void OnPairClicked (object? sender, EventArgs e)
		{
			FlyoutIsPresented = false;

			if (CurrentPage is MainPage mainPage)
			{
				mainPage.Reconnect();
			}
		}
	}
}
