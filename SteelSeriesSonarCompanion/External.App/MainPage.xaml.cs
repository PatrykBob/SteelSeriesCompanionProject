using SteelSeriesSonarCompanion.External.App.Connection;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App
{
	public partial class MainPage : ContentPage
	{
		private CompanionConnection Connection { get; }
		private List<VolumeSlider> VolumeSliderCollection { get; } = new();

		public MainPage (CompanionConnection connection)
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			InitializeComponent();
			Connection = connection;
			SpawnVolumeSliders();
			AttachToEvents();
			ConnectionLabel.Text = Connection.Status;
			Connection.Start();
		}

		private void SpawnVolumeSliders ()
		{
			SpawnVolumeSlider(SoundChannel.MASTER);
			SpawnVolumeSlider(SoundChannel.GAME);
			SpawnVolumeSlider(SoundChannel.CHAT);
			SpawnVolumeSlider(SoundChannel.MEDIA);
			SpawnVolumeSlider(SoundChannel.AUX);
			SpawnVolumeSlider(SoundChannel.MIC);
		}

		private void SpawnVolumeSlider (SoundChannel channel)
		{
			VolumeSlider volumeSlider = new(channel);
			SliderRoot.Add(volumeSlider, (int)channel);
			volumeSlider.OnVolumeChange += OnVolumeChanged;
			volumeSlider.OnMuteChange += OnMuteChanged;

			VolumeSliderCollection.Add(volumeSlider);
		}

		private void AttachToEvents ()
		{
			Connection.StatusChanged += OnStatusChanged;
			Connection.VolumeSetupChanged += OnVolumeSetupChanged;
			Connection.ChannelVolumeChanged += OnChannelVolumeChanged;
			Connection.ChannelMuteChanged += OnChannelMuteChanged;
		}

		private void OnStatusChanged (string status)
		{
			ConnectionLabel.Text = status;
		}

		private void OnVolumeSetupChanged (List<VolumeData> volumeDataCollection)
		{
			for (int i = 0; i < volumeDataCollection.Count; i++)
			{
				FindVolumeSlider(volumeDataCollection[i].Channel)?.Setup(volumeDataCollection[i]);
			}
		}

		private void OnChannelVolumeChanged (object? source, SoundChannel channel, float volume)
		{
			VolumeSlider? volumeSlider = FindVolumeSlider(channel);

			if (volumeSlider != null && volumeSlider != source)
			{
				volumeSlider.SetVolume(volume);
			}
		}

		private void OnChannelMuteChanged (object? source, SoundChannel channel, bool isMuted)
		{
			VolumeSlider? volumeSlider = FindVolumeSlider(channel);

			if (volumeSlider != null && volumeSlider != source)
			{
				volumeSlider.SetMute(isMuted);
			}
		}

		private void OnVolumeChanged (VolumeSlider volumeSlider, SoundChannel channel, float volume)
		{
			Connection.SetChannelVolume(volumeSlider, channel, volume);
		}

		private void OnMuteChanged (VolumeSlider volumeSlider, SoundChannel channel, bool isMuted)
		{
			Connection.SetChannelMute(volumeSlider, channel, isMuted);
		}

		private VolumeSlider? FindVolumeSlider (SoundChannel channel)
		{
			for (int i = 0; i < VolumeSliderCollection.Count; i++)
			{
				if (VolumeSliderCollection[i].Channel == channel)
				{
					return VolumeSliderCollection[i];
				}
			}

			return null;
		}
	}
}
