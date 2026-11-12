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

		private InternalCommunicationFacade InternalFacade { get; set; } = new();
		private SteelSeriesCompanionExtensionController ExtensionController { get; set; } = new();
		private TrayController Tray { get; set; } = new();

		private const string SONAR_CONNECTION_FAILED_MESSAGE =
			"Could not connect to SteelSeries Sonar. Make sure SteelSeries GG is running; the companion will connect automatically.";

		public async Task<List<VolumeData>> GetVolumeSettings ()
		{
			return await InternalFacade.GetVolumeSettings();
		}

		public async Task SetChannelVolume (object? sender, SoundChannel channel, float volume)
		{
			if (float.IsFinite(volume) == false)
			{
				Trace.WriteLine($"Ignored invalid volume {volume} for {channel}");
				return;
			}

			await InternalFacade.SetChannelVolume(channel, Math.Clamp(volume, 0f, 1f));
		}

		public async Task SetChannelMute (object? sender, SoundChannel channel, bool mute)
		{
			await InternalFacade.SetChannelMute(channel, mute);
		}

		public async Task RequestVolumeSetup (object? sender)
		{
			List<VolumeData> volumeDataCollection = await InternalFacade.GetVolumeSettings();
			VolumeSetupChanged.Invoke(sender, volumeDataCollection);
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
			Tray.Initialize(extensionMenuItemCollection);

			if (isConnectedToSonar == false)
			{
				Tray.ShowWarning(SONAR_CONNECTION_FAILED_MESSAGE);
			}
		}

		public void Dispose ()
		{
			Tray.Dispose();
		}
	}
}
