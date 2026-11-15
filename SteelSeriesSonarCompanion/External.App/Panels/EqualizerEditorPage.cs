using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class EqualizerEditorPage : ContentPage
	{
		private TaskCompletionSource<EqualizerEditResult?> ResultSource { get; } = new();
		private EqualizerData Equalizer { get; }
		private List<EqualizerFilterEditorRow> RowCollection { get; } = new();
		private Entry PreampEntry { get; }
		private EqualizerResponseDrawable PreviewDrawable { get; } = new();
		private GraphicsView PreviewView { get; } = new()
		{
			HeightRequest = 160
		};
		private Button SaveButton { get; }
		private Button SaveAsNewButton { get; }

		private const string TITLE_FORMAT = "Edit equalizer - {0}";
		private const string PREAMP_TEXT = "Preamp (dB)";
		private const string ON_HEADER_TEXT = "On";
		private const string TYPE_HEADER_TEXT = "Type";
		private const string FREQUENCY_HEADER_TEXT = "Frequency (Hz)";
		private const string GAIN_HEADER_TEXT = "Gain (dB)";
		private const string Q_FACTOR_HEADER_TEXT = "Q";
		private const string FACTORY_PRESET_HINT_TEXT = "SteelSeries presets can only be saved as a new preset.";
		private const string SAVE_TEXT = "Save";
		private const string SAVE_AS_NEW_TEXT = "Save as new...";
		private const string CANCEL_TEXT = "Cancel";
		private const string NEW_PRESET_TITLE = "Save as new preset";
		private const string NEW_PRESET_MESSAGE = "Preset name";
		private const string NEW_PRESET_NAME_FORMAT = "{0} (edited)";
		private const string OK_TEXT = "Save";

		private EqualizerEditorPage (EqualizerData equalizer)
		{
			Equalizer = equalizer;
			PreampEntry = new Entry
			{
				Text = EqualizerFilterEditorRow.FormatNumber(equalizer.Settings.Preamp),
				Keyboard = Keyboard.Numeric,
				WidthRequest = 100,
				HorizontalTextAlignment = TextAlignment.End
			};
			PreampEntry.TextChanged += (sender, e) => OnValuesChanged();
			SaveButton = CreateButton(SAVE_TEXT, () => Close(TryCreateSettings(), null));
			SaveAsNewButton = CreateButton(SAVE_AS_NEW_TEXT, OnSaveAsNewClicked);
			PreviewView.Drawable = PreviewDrawable;

			foreach (EqualizerFilter filter in equalizer.Settings.Filters.Take(EqualizerLimits.MAX_FILTER_COUNT))
			{
				EqualizerFilterEditorRow row = new(filter);
				row.Changed += OnValuesChanged;
				RowCollection.Add(row);
			}

			Content = CreateContent();
			OnValuesChanged();
		}

		public static async Task<EqualizerEditResult?> Show (Page hostPage, EqualizerData equalizer)
		{
			EqualizerEditorPage page = new(equalizer);
			await hostPage.Navigation.PushModalAsync(page);
			return await page.ResultSource.Task;
		}

		protected override bool OnBackButtonPressed ()
		{
			Close(null, null);
			return true;
		}

		protected override void OnDisappearing ()
		{
			base.OnDisappearing();
			ResultSource.TrySetResult(null);
		}

		private View CreateContent ()
		{
			Label titleLabel = new()
			{
				Text = string.Format(TITLE_FORMAT, Equalizer.Preset.Name),
				FontSize = 20
			};

			HorizontalStackLayout preampLayout = new()
			{
				Spacing = 12,
				Children =
				{
					new Label { Text = PREAMP_TEXT, VerticalOptions = LayoutOptions.Center },
					PreampEntry
				}
			};

			VerticalStackLayout rowLayout = new()
			{
				Spacing = 4
			};
			rowLayout.Add(CreateHeaderRow());

			foreach (EqualizerFilterEditorRow row in RowCollection)
			{
				rowLayout.Add(row.Layout);
			}

			HorizontalStackLayout buttonLayout = new()
			{
				Spacing = 12,
				HorizontalOptions = LayoutOptions.End,
				Children =
				{
					CreateButton(CANCEL_TEXT, () => Close(null, null)),
					SaveAsNewButton,
					SaveButton
				}
			};

			Grid contentLayout = new()
			{
				Padding = new Thickness(20),
				RowSpacing = 12,
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Auto)
				}
			};

			contentLayout.Add(titleLabel, 0, 0);
			contentLayout.Add(PreviewView, 0, 1);
			contentLayout.Add(preampLayout, 0, 2);
			contentLayout.Add(new ScrollView { Content = rowLayout }, 0, 3);
			contentLayout.Add(new Label { Text = FACTORY_PRESET_HINT_TEXT, TextColor = Colors.Gray, IsVisible = Equalizer.Preset.IsFactoryPreset }, 0, 4);
			contentLayout.Add(buttonLayout, 0, 5);
			return contentLayout;
		}

		private Grid CreateHeaderRow ()
		{
			Grid headerLayout = new()
			{
				ColumnSpacing = 12,
				ColumnDefinitions = EqualizerFilterEditorRow.CreateColumnDefinitions()
			};
			string[] headerTextCollection = [ON_HEADER_TEXT, TYPE_HEADER_TEXT, FREQUENCY_HEADER_TEXT, GAIN_HEADER_TEXT, Q_FACTOR_HEADER_TEXT];

			for (int i = 0; i < headerTextCollection.Length; i++)
			{
				headerLayout.Add(new Label { Text = headerTextCollection[i], TextColor = Colors.Gray }, i, 0);
			}

			return headerLayout;
		}

		private void OnValuesChanged ()
		{
			EqualizerSettings? settings = TryCreateSettings();
			SaveButton.IsEnabled = settings != null && Equalizer.Preset.IsFactoryPreset == false;
			SaveAsNewButton.IsEnabled = settings != null;

			if (settings != null)
			{
				PreviewDrawable.Equalizer = new EqualizerData(Equalizer.Channel, true, Equalizer.Preset, settings);
				PreviewView.Invalidate();
			}
		}

		private EqualizerSettings? TryCreateSettings ()
		{
			bool isValid = EqualizerFilterEditorRow.TryReadNumber(PreampEntry, EqualizerLimits.MIN_PREAMP, EqualizerLimits.MAX_PREAMP, out double preamp);
			List<EqualizerFilter> filterCollection = new();

			foreach (EqualizerFilterEditorRow row in RowCollection)
			{
				if (row.TryGetFilter(out EqualizerFilter? filter) == true)
				{
					filterCollection.Add(filter!);
				}
				else
				{
					isValid = false;
				}
			}

			return isValid == true && filterCollection.Count > 0 ? new EqualizerSettings(preamp, filterCollection) : null;
		}

		private async void OnSaveAsNewClicked ()
		{
			EqualizerSettings? settings = TryCreateSettings();

			if (settings == null)
			{
				return;
			}

			string? name = await DisplayPromptAsync(NEW_PRESET_TITLE, NEW_PRESET_MESSAGE, OK_TEXT, CANCEL_TEXT, initialValue: string.Format(NEW_PRESET_NAME_FORMAT, Equalizer.Preset.Name));

			if (string.IsNullOrWhiteSpace(name) == true)
			{
				return;
			}

			Close(settings, name.Trim());
		}

		private async void Close (EqualizerSettings? settings, string? newPresetName)
		{
			ResultSource.TrySetResult(settings == null ? null : new EqualizerEditResult(settings, newPresetName));
			await Navigation.PopModalAsync();
		}

		private Button CreateButton (string text, Action onClicked)
		{
			Button button = new()
			{
				Text = text
			};

			button.Clicked += (sender, e) => onClicked();
			return button;
		}
	}
}
