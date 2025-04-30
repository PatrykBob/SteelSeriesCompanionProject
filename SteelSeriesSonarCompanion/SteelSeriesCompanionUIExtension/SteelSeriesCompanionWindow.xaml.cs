using SteelSeriesCompanion.SharedCore;
using System.Windows;

namespace SteelSeriesCompanionUIExtension
{
	public partial class SteelSeriesCompanionWindow : Window
	{
		private ISteelSeriesCompanionCore? CompanionCore { get; set; }

		public SteelSeriesCompanionWindow ()
		{
			InitializeComponent();
		}

		public void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			CompanionCore = companionCore;
			InitializeVolumeSliders();
		}

		private async void InitializeVolumeSliders ()
		{
			List<VolumeData> volumeSettings = await CompanionCore!.GetVolumeSettings();
			InitializeVolumeSliders(volumeSettings);
		}

		private void InitializeVolumeSliders (List<VolumeData> volumeSettings)
		{
			foreach (VolumeData volumeData in volumeSettings)
			{
				switch (volumeData.Channel)
				{
					case SoundChannel.GAME:
						GameVolumeSlider.Value = volumeData.Volume;
						break;
					case SoundChannel.CHAT:
						ChatVolumeSlider.Value = volumeData.Volume;
						break;
					case SoundChannel.MEDIA:
						MediaVolumeSlider.Value = volumeData.Volume;
						break;
					case SoundChannel.AUX:
						AuxVolumeSlider.Value = volumeData.Volume;
						break;
					case SoundChannel.MIC:
						MicrophoneVolumeSlider.Value = volumeData.Volume;
						break;
				}
			}
		}

		private void GameVolumeSliderChange (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SetChannelVolume(SoundChannel.GAME, e);
		}

		private void ChatVolumeSliderChange (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SetChannelVolume(SoundChannel.CHAT, e);
		}

		private void MediaVolumeSliderChange (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SetChannelVolume(SoundChannel.MEDIA, e);
		}

		private void AuxVolumeSliderChange (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SetChannelVolume(SoundChannel.AUX, e);
		}

		private void MicrophoneVolumeSliderChange (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SetChannelVolume(SoundChannel.MIC, e);
		}

		private void SetChannelVolume (SoundChannel channel, RoutedPropertyChangedEventArgs<double> volumeArgs)
		{
			SetChannelVolume(channel, (float)volumeArgs.NewValue);
		}

		private void SetChannelVolume (SoundChannel channel, float volume)
		{
			if (CompanionCore != null)
			{
				CompanionCore.SetChannelVolume(channel, volume);
			}
		}
	}
}
