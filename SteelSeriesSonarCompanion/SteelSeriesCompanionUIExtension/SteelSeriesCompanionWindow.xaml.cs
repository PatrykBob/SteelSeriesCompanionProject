using SteelSeriesCompanion.SharedCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteelSeriesCompanionUIExtension
{
	/// <summary>
	/// Logika interakcji dla klasy Window1.xaml
	/// </summary>
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
