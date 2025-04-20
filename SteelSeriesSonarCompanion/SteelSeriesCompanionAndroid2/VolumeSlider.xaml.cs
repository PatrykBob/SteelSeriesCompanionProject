using SteelSeriesCompanion.SharedCore;
namespace SteelSeriesCompanionAndroid2;

public partial class VolumeSlider : ContentView
{
	public event Action<SoundChannel, float> OnVolumeChange = delegate { };
	public event Action<SoundChannel, bool> OnMuteChange = delegate { };

	private SoundChannel Channel { get; set; }

	public VolumeSlider (SoundChannel channel)
	{
		InitializeComponent();

		Channel = channel;
		ChannelLabel.Text = Channel.ToString();

		VolumeSliderControl.ValueChanged += OnVolumeSliderValueChanged;
		MuteCheckBox.CheckedChanged += OnMuteCheckBoxChanged;
		SliderLayout.SizeChanged += OnSliderSizeChange;
	}

	private void OnVolumeSliderValueChanged (object? sender, ValueChangedEventArgs e)
	{
		float volume = (float)e.NewValue;
		OnVolumeChange(Channel, volume);
		VolumeLabel.Text = $"{volume * 100:F0}%";
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
}