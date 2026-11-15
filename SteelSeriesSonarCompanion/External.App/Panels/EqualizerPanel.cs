using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Connection;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class EqualizerPanel : BasePanel
	{
		public const string TYPE_ID = "Equalizer";
		public const string DISPLAY_NAME = "Equalizer";

		private SoundChannel Channel { get; set; }
		private EqualizerData? CurrentEqualizer { get; set; }
		private List<EqualizerPreset> PresetCollection { get; set; } = new();
		private bool IsApplyingRemoteState { get; set; }

		private Label ChannelLabel { get; } = new()
		{
			FontSize = 18,
			VerticalOptions = LayoutOptions.Center
		};
		private Switch EnabledSwitch { get; } = new()
		{
			HorizontalOptions = LayoutOptions.End,
			VerticalOptions = LayoutOptions.Center
		};
		private Picker PresetPicker { get; } = new()
		{
			Title = PRESET_PICKER_TITLE
		};
		private EqualizerResponseDrawable ResponseDrawable { get; } = new();
		private GraphicsView ResponseView { get; } = new();
		private Grid RootLayout { get; } = new()
		{
			Padding = new Thickness(16, 8),
			RowSpacing = 8,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};

		private const string PRESET_PICKER_TITLE = "Preset";
		private const string CHANNEL_TITLE_FORMAT = "{0} equalizer";
		private const string CHANNEL_PICKER_TITLE = "Equalizer channel";
		private const string CANCEL_TEXT = "Cancel";
		private const string EDIT_TEXT = "Edit";
		private const string SAVE_FAILED_TITLE = "Equalizer";
		private const string SAVE_FAILED_MESSAGE = "The preset was not saved. The notification on the PC says why.";
		private const string OK_TEXT = "OK";
		private const string IMPORT_TEXT = "Import";
		private const string IMPORT_PICKER_TITLE = "Choose an AutoEQ ParametricEQ.txt file";
		private const string IMPORT_ACTION_TITLE = "Import equalizer preset";
		private const string IMPORT_AS_NEW_TEXT = "Save as new preset";
		private const string IMPORT_OVERWRITE_FORMAT = "Overwrite \"{0}\"";
		private const string IMPORT_NAME_TITLE = "New preset";
		private const string IMPORT_NAME_MESSAGE = "Preset name";
		private const string IMPORT_SAVE_TEXT = "Import";
		private const string IMPORT_FAILED_MESSAGE = "The file could not be read.";
		private const string IMPORT_TOO_LARGE_MESSAGE = "The file is too large for an equalizer preset.";
		private const string AUTOEQ_FILE_SUFFIX = " ParametricEQ";
		private const string STORAGE_PERMISSION_MESSAGE = "Importing a file needs permission to read storage.";
		private const int MAX_IMPORT_FILE_LENGTH = 65536;
		private const int ANDROID_WITHOUT_STORAGE_PERMISSION_VERSION = 13;

		private static readonly FilePickerFileType IMPORT_FILE_TYPES = new(new Dictionary<DevicePlatform, IEnumerable<string>>
		{
			{ DevicePlatform.Android, ["text/plain"] },
			{ DevicePlatform.WinUI, [".txt"] }
		});

		private bool IsSavePending { get; set; }
		private HorizontalStackLayout ButtonLayout { get; } = new()
		{
			Spacing = 12,
			HorizontalOptions = LayoutOptions.End
		};

		public EqualizerPanel (CompanionConnection connection, JObject? settings) : base(connection, settings)
		{
			Content = CreateContent();
			EnabledSwitch.Toggled += OnEnabledSwitchToggled;
			PresetPicker.SelectedIndexChanged += OnPresetPickerSelectedIndexChanged;
			AttachToEvents();
			ShowChannel(GetSettings<EqualizerPanelSettings>().Channel);
		}

		public override bool HasSettings => true;

		public override async Task<bool> TryEditSettings (Page hostPage)
		{
			SoundChannel[] channelCollection = Enum.GetValues<SoundChannel>().Where(channel => channel != SoundChannel.MASTER).ToArray();
			string? chosenChannelName = await hostPage.DisplayActionSheetAsync(CHANNEL_PICKER_TITLE, CANCEL_TEXT, null, channelCollection.Select(channel => channel.ToString()).ToArray());

			if (Enum.TryParse(chosenChannelName, out SoundChannel chosenChannel) == false || chosenChannel == Channel)
			{
				return false;
			}

			SaveSettings(new EqualizerPanelSettings { Channel = chosenChannel });
			ShowChannel(chosenChannel);
			return true;
		}

		public override void DetachFromEvents ()
		{
			Connection.EqualizerChanged -= OnEqualizerChanged;
			Connection.EqualizerPresetsReceived -= OnEqualizerPresetsReceived;
			Connection.EqualizerSaveCompleted -= OnEqualizerSaveCompleted;
		}

		private View CreateContent ()
		{
			Grid headerLayout = new()
			{
				ColumnDefinitions =
				{
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(GridLength.Auto)
				}
			};
			headerLayout.Add(ChannelLabel, 0, 0);
			headerLayout.Add(EnabledSwitch, 1, 0);

			RootLayout.Add(headerLayout, 0, 0);
			RootLayout.Add(PresetPicker, 0, 1);
			ResponseView.Drawable = ResponseDrawable;
			RootLayout.Add(ResponseView, 0, 2);
			ButtonLayout.Add(CreateButton(IMPORT_TEXT, OnImportClicked));
			ButtonLayout.Add(CreateButton(EDIT_TEXT, OnEditClicked));
			RootLayout.Add(ButtonLayout, 0, 3);
			return RootLayout;
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

		private void AttachToEvents ()
		{
			Connection.EqualizerChanged += OnEqualizerChanged;
			Connection.EqualizerPresetsReceived += OnEqualizerPresetsReceived;
			Connection.EqualizerSaveCompleted += OnEqualizerSaveCompleted;
		}

		private async void OnEditClicked ()
		{
			Page? hostPage = Window?.Page;

			if (CurrentEqualizer == null || hostPage == null)
			{
				return;
			}

			EqualizerEditResult? result = await EqualizerEditorPage.Show(hostPage, CurrentEqualizer);

			if (result == null)
			{
				return;
			}

			IsSavePending = true;
			Connection.SaveEqualizerPreset(Channel, result.Settings, result.NewPresetName);
		}

		private async void OnImportClicked ()
		{
			Page? hostPage = Window?.Page;
			EqualizerData? equalizer = CurrentEqualizer;

			if (equalizer == null || hostPage == null)
			{
				return;
			}

			FileResult? file = await PickImportFile(hostPage);

			if (file == null)
			{
				return;
			}

			string? text = await ReadImportFile(hostPage, file);

			if (text == null)
			{
				return;
			}

			string overwriteText = string.Format(IMPORT_OVERWRITE_FORMAT, equalizer.Preset.Name);
			string[] actionCollection = equalizer.Preset.IsFactoryPreset == true ? [IMPORT_AS_NEW_TEXT] : [IMPORT_AS_NEW_TEXT, overwriteText];
			string? action = await hostPage.DisplayActionSheetAsync(IMPORT_ACTION_TITLE, CANCEL_TEXT, null, actionCollection);
			string? newPresetName = null;

			if (action == IMPORT_AS_NEW_TEXT)
			{
				newPresetName = await hostPage.DisplayPromptAsync(IMPORT_NAME_TITLE, IMPORT_NAME_MESSAGE, IMPORT_SAVE_TEXT, CANCEL_TEXT, initialValue: CreatePresetName(file.FileName));

				if (string.IsNullOrWhiteSpace(newPresetName) == true)
				{
					return;
				}
			}
			else if (action != overwriteText)
			{
				return;
			}

			IsSavePending = true;
			Connection.ImportEqualizerPreset(equalizer.Channel, text, newPresetName?.Trim());
		}

		private async Task<FileResult?> PickImportFile (Page hostPage)
		{
			if (await TryGetStorageReadPermission() == false)
			{
				await hostPage.DisplayAlertAsync(SAVE_FAILED_TITLE, STORAGE_PERMISSION_MESSAGE, OK_TEXT);
				return null;
			}

			try
			{
				return await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = IMPORT_PICKER_TITLE, FileTypes = IMPORT_FILE_TYPES });
			}
			catch (Exception)
			{
				await hostPage.DisplayAlertAsync(SAVE_FAILED_TITLE, IMPORT_FAILED_MESSAGE, OK_TEXT);
				return null;
			}
		}

		private async Task<bool> TryGetStorageReadPermission ()
		{
			if (DeviceInfo.Platform != DevicePlatform.Android || DeviceInfo.Version.Major >= ANDROID_WITHOUT_STORAGE_PERMISSION_VERSION)
			{
				return true;
			}

			return await Permissions.RequestAsync<Permissions.StorageRead>() == PermissionStatus.Granted;
		}

		private async Task<string?> ReadImportFile (Page hostPage, FileResult file)
		{
			try
			{
				using Stream stream = await file.OpenReadAsync();
				using StreamReader reader = new(stream);
				char[] buffer = new char[MAX_IMPORT_FILE_LENGTH + 1];
				int length = await reader.ReadBlockAsync(buffer, 0, buffer.Length);

				if (length <= MAX_IMPORT_FILE_LENGTH)
				{
					return new string(buffer, 0, length);
				}

				await hostPage.DisplayAlertAsync(SAVE_FAILED_TITLE, IMPORT_TOO_LARGE_MESSAGE, OK_TEXT);
				return null;
			}
			catch (Exception)
			{
				await hostPage.DisplayAlertAsync(SAVE_FAILED_TITLE, IMPORT_FAILED_MESSAGE, OK_TEXT);
				return null;
			}
		}

		private string CreatePresetName (string fileName)
		{
			string name = Path.GetFileNameWithoutExtension(fileName);
			return name.EndsWith(AUTOEQ_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase) == true ? name[..^AUTOEQ_FILE_SUFFIX.Length] : name;
		}

		private async void OnEqualizerSaveCompleted (SoundChannel channel, bool isSuccess)
		{
			if (channel != Channel || IsSavePending == false)
			{
				return;
			}

			IsSavePending = false;
			Page? hostPage = Window?.Page;

			if (isSuccess == false && hostPage != null)
			{
				await hostPage.DisplayAlertAsync(SAVE_FAILED_TITLE, SAVE_FAILED_MESSAGE, OK_TEXT);
			}
		}

		private void ShowChannel (SoundChannel channel)
		{
			Channel = channel;
			CurrentEqualizer = null;
			PresetCollection = new List<EqualizerPreset>();
			ChannelLabel.Text = string.Format(CHANNEL_TITLE_FORMAT, channel);
			RootLayout.IsEnabled = false;
			RefreshPresetPicker();
			RefreshResponseView();

			if (Connection.TryGetEqualizerState(channel, out EqualizerData? equalizer) == true)
			{
				ApplyEqualizer(equalizer!);
			}

			Connection.RequestEqualizer(channel);
			Connection.RequestEqualizerPresets(channel);
		}

		private void OnEqualizerChanged (EqualizerData equalizer)
		{
			if (equalizer.Channel != Channel)
			{
				return;
			}

			bool isUnknownPreset = PresetCollection.Any(preset => preset.Id == equalizer.Preset.Id) == false;
			ApplyEqualizer(equalizer);

			if (isUnknownPreset == true)
			{
				Connection.RequestEqualizerPresets(Channel);
			}
		}

		private void OnEqualizerPresetsReceived (SoundChannel channel, List<EqualizerPreset> presetCollection)
		{
			if (channel != Channel)
			{
				return;
			}

			PresetCollection = presetCollection
				.OrderBy(preset => preset.IsFactoryPreset)
				.ThenBy(preset => preset.Name, StringComparer.CurrentCultureIgnoreCase)
				.ToList();
			RefreshPresetPicker();
		}

		private void ApplyEqualizer (EqualizerData equalizer)
		{
			CurrentEqualizer = equalizer;
			RootLayout.IsEnabled = true;
			IsApplyingRemoteState = true;
			EnabledSwitch.IsToggled = equalizer.Enabled;
			IsApplyingRemoteState = false;
			RefreshPresetPicker();
			RefreshResponseView();
		}

		private void RefreshResponseView ()
		{
			ResponseDrawable.Equalizer = CurrentEqualizer;
			ResponseView.Invalidate();
		}

		private void RefreshPresetPicker ()
		{
			List<EqualizerPreset> shownPresetCollection = GetShownPresetCollection();
			EqualizerPreset? currentPreset = CurrentEqualizer?.Preset;

			IsApplyingRemoteState = true;
			PresetPicker.ItemsSource = shownPresetCollection.Select(preset => preset.Name).ToList();
			PresetPicker.SelectedIndex = currentPreset == null ? -1 : shownPresetCollection.FindIndex(preset => preset.Id == currentPreset.Id);
			IsApplyingRemoteState = false;
		}

		private void OnEnabledSwitchToggled (object? sender, ToggledEventArgs e)
		{
			if (IsApplyingRemoteState == true || CurrentEqualizer == null)
			{
				return;
			}

			Connection.SetEqualizerEnabled(Channel, e.Value);
		}

		private void OnPresetPickerSelectedIndexChanged (object? sender, EventArgs e)
		{
			if (IsApplyingRemoteState == true || CurrentEqualizer == null)
			{
				return;
			}

			List<EqualizerPreset> shownPresetCollection = GetShownPresetCollection();
			int selectedIndex = PresetPicker.SelectedIndex;

			if (selectedIndex < 0 || selectedIndex >= shownPresetCollection.Count || shownPresetCollection[selectedIndex].Id == CurrentEqualizer.Preset.Id)
			{
				return;
			}

			Connection.SelectEqualizerPreset(Channel, shownPresetCollection[selectedIndex].Id);
		}

		private List<EqualizerPreset> GetShownPresetCollection ()
		{
			List<EqualizerPreset> shownPresetCollection = new(PresetCollection);
			EqualizerPreset? currentPreset = CurrentEqualizer?.Preset;

			if (currentPreset != null && shownPresetCollection.Any(preset => preset.Id == currentPreset.Id) == false)
			{
				shownPresetCollection.Insert(0, currentPreset);
			}

			return shownPresetCollection;
		}
	}
}
