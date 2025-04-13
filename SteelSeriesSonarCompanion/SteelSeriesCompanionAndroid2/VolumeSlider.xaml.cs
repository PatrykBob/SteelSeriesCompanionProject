using SteelSeriesCompanion.SharedCore;
namespace SteelSeriesCompanionAndroid2;

public partial class VolumeSlider : ContentView
{
	public event Action<SoundChannel, float> OnVolumeChange = delegate { };

	private SoundChannel Channel { get; set; }

	public VolumeSlider (SoundChannel channel)
	{
		InitializeComponent();
		Channel = channel;
		ChannelLabel.Text = Channel.ToString();
		VolumeSliderControl.ValueChanged += OnVolumeSliderValueChanged;
	}

	private void OnVolumeSliderValueChanged (object? sender, ValueChangedEventArgs e)
	{
		float volume = (float)e.NewValue;
		OnVolumeChange(Channel, volume);
		VolumeLabel.Text = $"{volume * 100:F0}%";
	}
}