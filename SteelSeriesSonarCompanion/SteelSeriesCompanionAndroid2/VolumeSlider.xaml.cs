namespace SteelSeriesCompanionAndroid2;

public partial class VolumeSlider : ContentView
{
	public event Action<float> OnVolumeChange = delegate { };

	public VolumeSlider()
	{
		InitializeComponent();
		VolumeSliderControl.ValueChanged += OnVolumeSliderValueChanged;
	}

	private void OnVolumeSliderValueChanged (object sender, ValueChangedEventArgs e)
	{
		float volume = (float)e.NewValue;
		OnVolumeChange(volume);
		VolumeLabel.Text = $"{volume * 100:F0}%";
	}
}