using System.Diagnostics;
using SteelSeriesSonarCompanion.CoreApp.Communication.Internal;
using SteelSeriesSonarCompanion.CoreApp.Extension;
using SteelSeriesSonarCompanion.CoreApp.Tray;
using SteelSeriesSonarCompanion.Shared.Core;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.CoreApp
{
	public class SteelSeriesCompanionCore : ISteelSeriesCompanionCore, IDisposable
	{
		public event EventHandler<List<VolumeData>> VolumeSetupChanged = delegate { };
		public event EventHandler<ChannelVolumeChangedEventArgs> ChannelVolumeChanged = delegate { };
		public event EventHandler<ChannelMuteChangedEventArgs> ChannelMuteChanged = delegate { };

		private InternalCommunicationFacade InternalFacade { get; set; } = new();
		private SteelSeriesCompanionExtensionController ExtensionController { get; set; } = new();
		private TrayController Tray { get; set; } = new();

		private const string SONAR_CONNECTION_FAILED_MESSAGE =
			"Could not connect to SteelSeries Sonar. Make sure SteelSeries GG is running; the companion will connect automatically.";
		private const string AUDIO_DEVICE_REFRESHED_MESSAGE = "Audio device refreshed.";
		private const string REFRESH_AUDIO_DEVICE_MENU_TEXT = "Refresh Audio Device";

		public async Task<List<VolumeData>> GetVolumeSettings ()
		{
			return await InternalFacade.GetVolumeSettings();
		}

		public async Task SetChannelVolume (object? sender, SoundChannel channel, float volume)
		{
			if (IsKnownChannel(channel) == false)
			{
				return;
			}

			if (float.IsFinite(volume) == false)
			{
				Trace.WriteLine($"Ignored invalid volume {volume} for {channel}");
				return;
			}

			float clampedVolume = Math.Clamp(volume, 0f, 1f);
			await InternalFacade.SetChannelVolume(channel, clampedVolume);
			ChannelVolumeChanged.Invoke(sender, new ChannelVolumeChangedEventArgs(channel, clampedVolume));
		}

		public async Task SetChannelMute (object? sender, SoundChannel channel, bool mute)
		{
			if (IsKnownChannel(channel) == false)
			{
				return;
			}

			await InternalFacade.SetChannelMute(channel, mute);
			ChannelMuteChanged.Invoke(sender, new ChannelMuteChangedEventArgs(channel, mute));
		}

		private bool IsKnownChannel (SoundChannel channel)
		{
			if (Enum.IsDefined(channel) == true)
			{
				return true;
			}

			Trace.WriteLine($"Ignored unknown channel {(int)channel}");
			return false;
		}

		public async Task RequestVolumeSetup (object? sender)
		{
			List<VolumeData> volumeDataCollection = await InternalFacade.GetVolumeSettings();
			VolumeSetupChanged.Invoke(sender, volumeDataCollection);
		}

		public async Task RefreshAudioDevice (object? sender)
		{
			InternalFacade.ResetMasterDevice();
			await RequestVolumeSetup(sender);
			ShowNotification(AUDIO_DEVICE_REFRESHED_MESSAGE);
		}

		public void ShowNotification (string message)
		{
			Tray.ShowInfo(message);
		}

		public async void Initialize ()
		{
			bool isConnectedToSonar = await InternalFacade.Initialize();

			ExtensionController.Initialize(this);
			List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection = ExtensionController.GetExtensionToolMenuItemCollection();
			extensionMenuItemCollection.Add(new SteelSeriesCompanionExtensionMenuItem(REFRESH_AUDIO_DEVICE_MENU_TEXT, () => _ = RefreshAudioDevice(this)));
			Tray.Initialize(extensionMenuItemCollection);

			if (isConnectedToSonar == false)
			{
				Tray.ShowWarning(SONAR_CONNECTION_FAILED_MESSAGE);
			}
		}

		public void Dispose ()
		{
			ExtensionController.Dispose();
			InternalFacade.Dispose();
			Tray.Dispose();
		}
	}
}
