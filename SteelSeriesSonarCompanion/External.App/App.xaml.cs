namespace SteelSeriesSonarCompanion.External.App
{
	public partial class App : Application
	{
		private AppShell Shell { get; }

		public App (AppShell shell)
		{
			InitializeComponent();
			Shell = shell;
		}

		protected override Window CreateWindow (IActivationState? activationState)
		{
			return new Window(Shell);
		}
	}
}