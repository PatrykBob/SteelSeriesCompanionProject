using Microsoft.Maui.Controls.Shapes;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class PanelEditOverlay : ContentView
	{
		public event Action<SplitAxis> SplitRequested = delegate { };
		public event Action ChangeTypeRequested = delegate { };
		public event Action SettingsRequested = delegate { };
		public event Action RemoveRequested = delegate { };

		private const string OVERLAY_COLOR = "#CC141414";
		private const string OUTLINE_COLOR = "#ac99ea";
		private const string ADD_VERTICAL_DIVIDER_TEXT = "Add vertical divider";
		private const string ADD_HORIZONTAL_DIVIDER_TEXT = "Add horizontal divider";
		private const string CHANGE_TYPE_TEXT = "Change type";
		private const string SETTINGS_TEXT = "Settings";
		private const string REMOVE_TEXT = "Remove";

		public PanelEditOverlay (string title, bool hasSettings)
		{
			Content = new Border
			{
				BackgroundColor = Color.FromArgb(OVERLAY_COLOR),
				Stroke = Color.FromArgb(OUTLINE_COLOR),
				StrokeThickness = 2,
				StrokeDashArray = new DoubleCollection { 4, 2 },
				Content = new ScrollView
				{
					Content = CreateContentLayout(title, hasSettings)
				}
			};
		}

		private View CreateContentLayout (string title, bool hasSettings)
		{
			Label titleLabel = new()
			{
				Text = title,
				FontSize = 18,
				HorizontalOptions = LayoutOptions.Center
			};

			return new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Padding = new Thickness(12),
				Spacing = 12,
				Children =
				{
					titleLabel,
					CreateButtonLayout(hasSettings)
				}
			};
		}

		private VerticalStackLayout CreateButtonLayout (bool hasSettings)
		{
			VerticalStackLayout buttonLayout = new()
			{
				Spacing = 8,
				HorizontalOptions = LayoutOptions.Center
			};

			buttonLayout.Add(CreateButton(ADD_VERTICAL_DIVIDER_TEXT, () => SplitRequested(SplitAxis.LEFT_TO_RIGHT)));
			buttonLayout.Add(CreateButton(ADD_HORIZONTAL_DIVIDER_TEXT, () => SplitRequested(SplitAxis.TOP_TO_BOTTOM)));
			buttonLayout.Add(CreateButton(CHANGE_TYPE_TEXT, () => ChangeTypeRequested()));

			if (hasSettings == true)
			{
				buttonLayout.Add(CreateButton(SETTINGS_TEXT, () => SettingsRequested()));
			}

			buttonLayout.Add(CreateButton(REMOVE_TEXT, () => RemoveRequested()));
			return buttonLayout;
		}

		private Button CreateButton (string text, Action onClicked)
		{
			Button button = new()
			{
				Text = text,
				MinimumWidthRequest = 160
			};

			button.Clicked += (sender, e) => onClicked();
			return button;
		}
	}
}
