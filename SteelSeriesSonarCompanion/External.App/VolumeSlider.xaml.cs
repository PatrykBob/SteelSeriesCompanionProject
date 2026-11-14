using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class VolumeSlider : ContentView
	{
		public event Action<VolumeSlider, SoundChannel, float> OnVolumeChange = delegate { };
		public event Action<VolumeSlider, SoundChannel, bool> OnMuteChange = delegate { };

		public SoundChannel Channel { get; private set; }
		public SliderOrientation Orientation { get; private set; } = SliderOrientation.VERTICAL;
		private bool IsApplyingRemoteValue { get; set; }

		private const double VERTICAL_ROW_SPACING = 20;
		private const double HORIZONTAL_COLUMN_SPACING = 12;
		private const double HORIZONTAL_NAME_WIDTH = 80;
		private const double HORIZONTAL_VOLUME_WIDTH = 50;
		private const double VERTICAL_SLIDER_ROTATION = 270;

		public VolumeSlider (SoundChannel channel)
		{
			InitializeComponent();

			Channel = channel;
			ChannelLabel.Text = Channel.ToString();

			VolumeSliderControl.ValueChanged += OnVolumeSliderValueChanged;
			MuteCheckBox.CheckedChanged += OnMuteCheckBoxChanged;
			SliderLayout.SizeChanged += OnSliderSizeChange;
		}

		public void SetOrientation (SliderOrientation orientation)
		{
			if (Orientation == orientation)
			{
				return;
			}

			Orientation = orientation;

			if (orientation == SliderOrientation.VERTICAL)
			{
				ApplyVerticalLayout();
			}
			else
			{
				ApplyHorizontalLayout();
			}
		}

		private void ApplyVerticalLayout ()
		{
			RootLayout.ColumnDefinitions.Clear();
			RootLayout.RowDefinitions.Clear();
			RootLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			RootLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			RootLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
			RootLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			RootLayout.RowSpacing = VERTICAL_ROW_SPACING;
			RootLayout.ColumnSpacing = 0;

			PlaceInLayout(ChannelLabel, 0, 0);
			PlaceInLayout(VolumeLabel, 0, 1);
			PlaceInLayout(SliderLayout, 0, 2);
			PlaceInLayout(MuteCheckBox, 0, 3);

			ChannelLabel.HorizontalOptions = LayoutOptions.Center;
			VolumeSliderControl.Rotation = VERTICAL_SLIDER_ROTATION;
			VolumeSliderControl.HorizontalOptions = LayoutOptions.Center;
			VolumeSliderControl.WidthRequest = SliderLayout.Height;
		}

		private void ApplyHorizontalLayout ()
		{
			RootLayout.RowDefinitions.Clear();
			RootLayout.ColumnDefinitions.Clear();
			RootLayout.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(HORIZONTAL_NAME_WIDTH)));
			RootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			RootLayout.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(HORIZONTAL_VOLUME_WIDTH)));
			RootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
			RootLayout.RowSpacing = 0;
			RootLayout.ColumnSpacing = HORIZONTAL_COLUMN_SPACING;

			PlaceInLayout(ChannelLabel, 0, 0);
			PlaceInLayout(SliderLayout, 1, 0);
			PlaceInLayout(VolumeLabel, 2, 0);
			PlaceInLayout(MuteCheckBox, 3, 0);

			ChannelLabel.HorizontalOptions = LayoutOptions.Start;
			VolumeSliderControl.Rotation = 0;
			VolumeSliderControl.HorizontalOptions = LayoutOptions.Fill;
			VolumeSliderControl.WidthRequest = -1;
		}

		private void PlaceInLayout (View view, int column, int row)
		{
			Grid.SetColumn(view, column);
			Grid.SetRow(view, row);
		}

		public void Setup (VolumeData volumeData)
		{
			SetVolume(volumeData.Volume);
			SetMute(volumeData.Mute);
		}

		public void SetVolume (float volume)
		{
			Dispatcher.Dispatch(() =>
			{
				ApplyRemoteValue(() => VolumeSliderControl.Value = volume);
			});
		}

		public void SetMute (bool mute)
		{
			Dispatcher.Dispatch(() =>
			{
				ApplyRemoteValue(() => MuteCheckBox.IsChecked = mute);
			});
		}

		private void ApplyRemoteValue (Action apply)
		{
			IsApplyingRemoteValue = true;

			try
			{
				apply();
			}
			finally
			{
				IsApplyingRemoteValue = false;
			}
		}

		private void OnVolumeSliderValueChanged (object? sender, ValueChangedEventArgs e)
		{
			UpdateVolumeLabel();

			if (IsApplyingRemoteValue == true)
			{
				return;
			}

			OnVolumeChange(this, Channel, (float)e.NewValue);
		}

		private void OnMuteCheckBoxChanged (object? sender, CheckedChangedEventArgs e)
		{
			if (IsApplyingRemoteValue == true)
			{
				return;
			}

			bool isMuted = e.Value;
			OnMuteChange(this, Channel, isMuted);
		}

		private void OnSliderSizeChange (object? sender, EventArgs e)
		{
			if (Orientation != SliderOrientation.VERTICAL)
			{
				return;
			}

			VolumeSliderControl.WidthRequest = SliderLayout.Height;
		}

		private void UpdateVolumeLabel ()
		{
			float volume = (float)VolumeSliderControl.Value;
			VolumeLabel.Text = $"{volume * 100:F0}%";
		}
	}
}