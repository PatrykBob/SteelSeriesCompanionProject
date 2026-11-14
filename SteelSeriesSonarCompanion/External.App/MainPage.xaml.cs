using SteelSeriesSonarCompanion.External.App.Connection;
using SteelSeriesSonarCompanion.External.App.Layout;
using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class MainPage : ContentPage
	{
		private CompanionConnection Connection { get; }
		private LayoutStore LayoutStore { get; }
		private LayoutHost LayoutHost { get; }
		private LayoutNode? CurrentRoot { get; set; }
		private LayoutOrientation? CurrentOrientation { get; set; }

		public MainPage (CompanionConnection connection, PanelRegistry panelRegistry, LayoutStore layoutStore)
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			InitializeComponent();
			Connection = connection;
			LayoutStore = layoutStore;
			LayoutHost = new LayoutHost(panelRegistry, this);
			SetupLayoutHost();
			AttachToEvents();
			StartConnection();
		}

		public void EnterEditMode ()
		{
			SetEditMode(true);
		}

		private void SetEditMode (bool isEditMode)
		{
			LayoutHost.SetEditMode(isEditMode);
			DoneEditingButton.IsVisible = isEditMode;
		}

		private void OnDoneEditingClicked (object? sender, EventArgs e)
		{
			SetEditMode(false);
		}

		private void SetupLayoutHost ()
		{
			PanelRoot.Content = LayoutHost;
		}

		private void AttachToEvents ()
		{
			LayoutHost.LayoutEdited += OnLayoutEdited;
			SizeChanged += OnPageSizeChanged;
			Connection.StatusChanged += OnStatusChanged;
		}

		private void StartConnection ()
		{
			ConnectionLabel.Text = Connection.Status;
			Connection.Start();
		}

		private void OnPageSizeChanged (object? sender, EventArgs e)
		{
			if (Width <= 0 || Height <= 0)
			{
				return;
			}

			LayoutOrientation orientation = Width >= Height ? LayoutOrientation.LANDSCAPE : LayoutOrientation.PORTRAIT;

			if (orientation == CurrentOrientation)
			{
				return;
			}

			CurrentOrientation = orientation;
			CurrentRoot = LayoutStore.Load(orientation);
			LayoutHost.Show(CurrentRoot);
		}

		private void OnLayoutEdited ()
		{
			if (CurrentOrientation != null && CurrentRoot != null)
			{
				LayoutStore.Save(CurrentOrientation.Value, CurrentRoot);
			}
		}

		private void OnStatusChanged (string status)
		{
			ConnectionLabel.Text = status;
		}
	}
}
