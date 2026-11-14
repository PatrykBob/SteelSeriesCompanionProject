using SteelSeriesSonarCompanion.External.App.Connection;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class App : Application
	{
		private CompanionConnection Connection { get; }

		public App (CompanionConnection connection)
		{
			InitializeComponent();
			Connection = connection;
		}

		protected override Window CreateWindow (IActivationState? activationState)
		{
			return new Window(new AppShell(Connection));
		}
	}
}
