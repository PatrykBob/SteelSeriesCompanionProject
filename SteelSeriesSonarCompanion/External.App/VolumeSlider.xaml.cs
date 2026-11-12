using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class VolumeSlider : ContentView
	{
		public event Action<SoundChannel, float> OnVolumeChange = delegate { };
		public event Action<SoundChannel, bool> OnMuteChange = delegate { };

		public SoundChannel Channel { get; private set; }
		private bool IsApplyingRemoteValue { get; set; }

		public VolumeSlider (SoundChannel channel)
		{
			InitializeComponent();

			Channel = channel;
			ChannelLabel.Text = Channel.ToString();

			VolumeSliderControl.ValueChanged += OnVolumeSliderValueChanged;
			MuteCheckBox.CheckedChanged += OnMuteCheckBoxChanged;
			SliderLayout.SizeChanged += OnSliderSizeChange;
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

			OnVolumeChange(Channel, (float)e.NewValue);
		}

		private void OnMuteCheckBoxChanged (object? sender, CheckedChangedEventArgs e)
		{
			if (IsApplyingRemoteValue == true)
			{
				return;
			}

			bool isMuted = e.Value;
			OnMuteChange(Channel, isMuted);
		}

		private void OnSliderSizeChange (object? sender, EventArgs e)
		{
			VolumeSliderControl.WidthRequest = SliderLayout.Height;
		}

		private void UpdateVolumeLabel ()
		{
			float volume = (float)VolumeSliderControl.Value;
			VolumeLabel.Text = $"{volume * 100:F0}%";
		}
	}
}