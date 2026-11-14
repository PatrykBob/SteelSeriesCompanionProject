using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class VolumeMixerSettingsPage : ContentPage
	{
		private TaskCompletionSource<VolumeMixerSettings?> ResultSource { get; } = new();
		private List<SoundChannel> ChannelOrder { get; }
		private HashSet<SoundChannel> SelectedChannels { get; }
		private VerticalStackLayout RowLayout { get; } = new()
		{
			Spacing = 8
		};
		private Button SaveButton { get; }

		private const string TITLE_TEXT = "Volume Mixer channels";
		private const string SAVE_TEXT = "Save";
		private const string CANCEL_TEXT = "Cancel";
		private const string MOVE_UP_TEXT = "▲";
		private const string MOVE_DOWN_TEXT = "▼";

		private VolumeMixerSettingsPage (VolumeMixerSettings currentSettings)
		{
			ChannelOrder = CreateChannelOrder(currentSettings.ChannelCollection);
			SelectedChannels = new HashSet<SoundChannel>(currentSettings.ChannelCollection);
			SaveButton = CreateButton(SAVE_TEXT, OnSaveClicked);
			Content = CreateContent();
			RebuildRows();
		}

		public static async Task<VolumeMixerSettings?> Show (Page hostPage, VolumeMixerSettings currentSettings)
		{
			VolumeMixerSettingsPage page = new(currentSettings);
			await hostPage.Navigation.PushModalAsync(page);
			return await page.ResultSource.Task;
		}

		protected override bool OnBackButtonPressed ()
		{
			Close(null);
			return true;
		}

		protected override void OnDisappearing ()
		{
			base.OnDisappearing();
			ResultSource.TrySetResult(null);
		}

		private List<SoundChannel> CreateChannelOrder (List<SoundChannel> savedChannelCollection)
		{
			List<SoundChannel> channelOrder = savedChannelCollection.Distinct().ToList();
			SoundChannel[] allChannels = Enum.GetValues<SoundChannel>();

			for (int i = 0; i < allChannels.Length; i++)
			{
				if (channelOrder.Contains(allChannels[i]) == false)
				{
					channelOrder.Add(allChannels[i]);
				}
			}

			return channelOrder;
		}

		private View CreateContent ()
		{
			Label titleLabel = new()
			{
				Text = TITLE_TEXT,
				FontSize = 20
			};

			HorizontalStackLayout buttonLayout = new()
			{
				Spacing = 12,
				HorizontalOptions = LayoutOptions.End,
				Children =
				{
					CreateButton(CANCEL_TEXT, () => Close(null)),
					SaveButton
				}
			};

			Grid contentLayout = new()
			{
				Padding = new Thickness(20),
				RowSpacing = 20,
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Auto)
				}
			};

			contentLayout.Add(titleLabel, 0, 0);
			contentLayout.Add(new ScrollView { Content = RowLayout }, 0, 1);
			contentLayout.Add(buttonLayout, 0, 2);
			return contentLayout;
		}

		private void RebuildRows ()
		{
			RowLayout.Clear();

			for (int i = 0; i < ChannelOrder.Count; i++)
			{
				RowLayout.Add(CreateRow(ChannelOrder[i], i));
			}

			UpdateSaveButton();
		}

		private View CreateRow (SoundChannel channel, int index)
		{
			CheckBox selectionCheckBox = new()
			{
				IsChecked = SelectedChannels.Contains(channel),
				VerticalOptions = LayoutOptions.Center
			};
			selectionCheckBox.CheckedChanged += (sender, e) => OnChannelSelectionChanged(channel, e.Value);

			Label channelLabel = new()
			{
				Text = channel.ToString(),
				VerticalOptions = LayoutOptions.Center
			};

			Button moveUpButton = CreateButton(MOVE_UP_TEXT, () => MoveChannel(channel, -1));
			moveUpButton.IsEnabled = index > 0;

			Button moveDownButton = CreateButton(MOVE_DOWN_TEXT, () => MoveChannel(channel, 1));
			moveDownButton.IsEnabled = index < ChannelOrder.Count - 1;

			Grid row = new()
			{
				ColumnSpacing = 8,
				ColumnDefinitions =
				{
					new ColumnDefinition(GridLength.Auto),
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(GridLength.Auto),
					new ColumnDefinition(GridLength.Auto)
				}
			};

			row.Add(selectionCheckBox, 0, 0);
			row.Add(channelLabel, 1, 0);
			row.Add(moveUpButton, 2, 0);
			row.Add(moveDownButton, 3, 0);
			return row;
		}

		private void OnChannelSelectionChanged (SoundChannel channel, bool isSelected)
		{
			if (isSelected == true)
			{
				SelectedChannels.Add(channel);
			}
			else
			{
				SelectedChannels.Remove(channel);
			}

			UpdateSaveButton();
		}

		private void MoveChannel (SoundChannel channel, int offset)
		{
			int index = ChannelOrder.IndexOf(channel);
			int targetIndex = index + offset;

			if (targetIndex < 0 || targetIndex >= ChannelOrder.Count)
			{
				return;
			}

			ChannelOrder[index] = ChannelOrder[targetIndex];
			ChannelOrder[targetIndex] = channel;
			RebuildRows();
		}

		private void UpdateSaveButton ()
		{
			SaveButton.IsEnabled = SelectedChannels.Count > 0;
		}

		private void OnSaveClicked ()
		{
			VolumeMixerSettings settings = new()
			{
				ChannelCollection = ChannelOrder.Where(SelectedChannels.Contains).ToList()
			};

			Close(settings);
		}

		private async void Close (VolumeMixerSettings? result)
		{
			ResultSource.TrySetResult(result);
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
