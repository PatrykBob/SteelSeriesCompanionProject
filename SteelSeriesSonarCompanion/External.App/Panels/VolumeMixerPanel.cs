using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Connection;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class VolumeMixerPanel : BasePanel
	{
		public const string TYPE_ID = "VolumeMixer";
		public const string DISPLAY_NAME = "Volume Mixer";

		private Grid SliderRoot { get; } = new()
		{
			Padding = new Thickness(30, 0),
			ColumnSpacing = 30
		};
		private List<VolumeSlider> VolumeSliderCollection { get; } = new();

		private static readonly Thickness VERTICAL_PADDING = new(30, 0);
		private static readonly Thickness HORIZONTAL_PADDING = new(16, 8);
		private const double VERTICAL_COLUMN_SPACING = 30;
		private const double HORIZONTAL_ROW_SPACING = 8;

		public VolumeMixerPanel (CompanionConnection connection, JObject? settings) : base(connection, settings)
		{
			Content = SliderRoot;
			SpawnVolumeSliders();
			AttachToEvents();
		}

		public override bool HasSettings => true;

		public override async Task<bool> TryEditSettings (Page hostPage)
		{
			VolumeMixerSettings? newSettings = await VolumeMixerSettingsPage.Show(hostPage, GetSettings<VolumeMixerSettings>());

			if (newSettings == null)
			{
				return false;
			}

			SaveSettings(newSettings);
			RespawnVolumeSliders();
			return true;
		}

		public override void DetachFromEvents ()
		{
			Connection.VolumeSetupChanged -= OnVolumeSetupChanged;
			Connection.ChannelVolumeChanged -= OnChannelVolumeChanged;
			Connection.ChannelMuteChanged -= OnChannelMuteChanged;
		}

		private void SpawnVolumeSliders ()
		{
			VolumeMixerSettings mixerSettings = GetSettings<VolumeMixerSettings>();

			for (int i = 0; i < mixerSettings.ChannelCollection.Count; i++)
			{
				SpawnVolumeSlider(mixerSettings.ChannelCollection[i], mixerSettings.IsMuteVisible);
			}

			ArrangeVolumeSliders(mixerSettings.Orientation);
		}

		private void ArrangeVolumeSliders (SliderOrientation orientation)
		{
			bool isVertical = orientation == SliderOrientation.VERTICAL;
			SliderRoot.ColumnDefinitions.Clear();
			SliderRoot.RowDefinitions.Clear();
			SliderRoot.Padding = isVertical == true ? VERTICAL_PADDING : HORIZONTAL_PADDING;
			SliderRoot.ColumnSpacing = isVertical == true ? VERTICAL_COLUMN_SPACING : 0;
			SliderRoot.RowSpacing = isVertical == true ? 0 : HORIZONTAL_ROW_SPACING;

			for (int i = 0; i < VolumeSliderCollection.Count; i++)
			{
				VolumeSlider volumeSlider = VolumeSliderCollection[i];
				volumeSlider.SetOrientation(orientation);

				if (isVertical == true)
				{
					SliderRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
					Grid.SetColumn(volumeSlider, i);
					Grid.SetRow(volumeSlider, 0);
				}
				else
				{
					SliderRoot.RowDefinitions.Add(new RowDefinition(GridLength.Star));
					Grid.SetColumn(volumeSlider, 0);
					Grid.SetRow(volumeSlider, i);
				}
			}
		}

		private void RespawnVolumeSliders ()
		{
			for (int i = 0; i < VolumeSliderCollection.Count; i++)
			{
				VolumeSliderCollection[i].OnVolumeChange -= OnVolumeChanged;
				VolumeSliderCollection[i].OnMuteChange -= OnMuteChanged;
			}

			VolumeSliderCollection.Clear();
			SliderRoot.Clear();
			SpawnVolumeSliders();
		}

		private void SpawnVolumeSlider (SoundChannel channel, bool isMuteVisible)
		{
			VolumeSlider volumeSlider = new(channel);
			volumeSlider.SetMuteVisible(isMuteVisible);
			SliderRoot.Add(volumeSlider);
			volumeSlider.OnVolumeChange += OnVolumeChanged;
			volumeSlider.OnMuteChange += OnMuteChanged;

			if (Connection.TryGetChannelState(channel, out VolumeData? volumeData) == true)
			{
				volumeSlider.Setup(volumeData!);
			}

			VolumeSliderCollection.Add(volumeSlider);
		}

		private void AttachToEvents ()
		{
			Connection.VolumeSetupChanged += OnVolumeSetupChanged;
			Connection.ChannelVolumeChanged += OnChannelVolumeChanged;
			Connection.ChannelMuteChanged += OnChannelMuteChanged;
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
