using SteelSeriesCompanion.SharedCore;
namespace SteelSeriesCompanionAndroid2;

public partial class VolumeSlider : ContentView
{
	public event Action<SoundChannel, float> OnVolumeChange = delegate { };
	public event Action<SoundChannel, bool> OnMuteChange = delegate { };

	public SoundChannel Channel { get; private set; }

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
		VolumeSliderControl.Value = volume;
		UpdateVolumeLabel();
	}

	public void SetMute (bool mute)
	{
		MuteCheckBox.IsChecked = mute;
	}

	private void OnVolumeSliderValueChanged (object? sender, ValueChangedEventArgs e)
	{
		float volume = (float)e.NewValue;
		OnVolumeChange(Channel, volume);
		UpdateVolumeLabel();
	}

	private void OnMuteCheckBoxChanged (object? sender, CheckedChangedEventArgs e)
	{
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