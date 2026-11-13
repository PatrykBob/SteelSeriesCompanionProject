using SteelSeriesSonarCompanion.Shared.Core;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;
using System.Windows;
using System.Windows.Controls;

namespace SteelSeriesCompanionUIExtension
{
	public partial class SteelSeriesCompanionWindow : Window
	{
		private ISteelSeriesCompanionCore? CompanionCore { get; set; }
		private bool IsApplyingVolumeSettings { get; set; }
		private Dictionary<SoundChannel, float> PendingVolumeMap { get; } = new();
		private HashSet<SoundChannel> ThrottledChannels { get; } = new();

		private static readonly TimeSpan VOLUME_SEND_INTERVAL = TimeSpan.FromMilliseconds(50);

		public SteelSeriesCompanionWindow ()
		{
			InitializeComponent();
		}

		public void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			CompanionCore = companionCore;
			CompanionCore.ChannelVolumeChanged += OnChannelVolumeChanged;
			InitializeVolumeSliders();
		}

		protected override void OnClosed (EventArgs e)
		{
			CompanionCore!.ChannelVolumeChanged -= OnChannelVolumeChanged;
			base.OnClosed(e);
		}

		private void OnChannelVolumeChanged (object? sender, ChannelVolumeChangedEventArgs e)
		{
			if (sender == this)
			{
				return;
			}

			Dispatcher.BeginInvoke(() => SetSliderVolume(e.Channel, e.Volume));
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
				SetSliderVolume(volumeData.Channel, volumeData.Volume);
			}
		}

		private void SetSliderVolume (SoundChannel channel, float volume)
		{
			Slider? slider = GetVolumeSlider(channel);

			if (slider == null)
			{
				return;
			}

			IsApplyingVolumeSettings = true;

			try
			{
				slider.Value = volume;
			}
			finally
			{
				IsApplyingVolumeSettings = false;
			}
		}

		private Slider? GetVolumeSlider (SoundChannel channel)
		{
			return channel switch
			{
				SoundChannel.MASTER => MasterVolumeSlider,
				SoundChannel.GAME => GameVolumeSlider,
				SoundChannel.CHAT => ChatVolumeSlider,
				SoundChannel.MEDIA => MediaVolumeSlider,
				SoundChannel.AUX => AuxVolumeSlider,
				SoundChannel.MIC => MicrophoneVolumeSlider,
				_ => null,
			};
		}

		private void MasterVolumeSliderChange (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SetChannelVolume(SoundChannel.MASTER, e);
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
			if (IsApplyingVolumeSettings == true)
			{
				return;
			}

			SetChannelVolume(channel, (float)volumeArgs.NewValue);
		}

		private async void SetChannelVolume (SoundChannel channel, float volume)
		{
			if (CompanionCore == null)
			{
				return;
			}

			PendingVolumeMap[channel] = volume;

			if (ThrottledChannels.Add(channel) == false)
			{
				return;
			}

			try
			{
				while (PendingVolumeMap.Remove(channel, out float pendingVolume) == true)
				{
					await CompanionCore.SetChannelVolume(this, channel, pendingVolume);
					await Task.Delay(VOLUME_SEND_INTERVAL);
				}
			}
			finally
			{
				ThrottledChannels.Remove(channel);
			}
		}
	}
}
