using SteelSeriesSonarCompanion.CoreApp.Communication.Internal;
using SteelSeriesSonarCompanion.CoreApp.Extension;
using SteelSeriesSonarCompanion.CoreApp.Setup;
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

		public async Task<List<VolumeData>> GetVolumeSettings ()
		{
			return await InternalFacade.GetVolumeSettings();
		}

		public async Task SetChannelVolume (object? sender, SoundChannel channel, float volume)
		{
			await InternalFacade.SetChannelVolume(channel, volume);
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

		public async void Initialize ()
		{
			if (SonarSetupLoader.TryGetSonarSetupPort(out int setupPort) == true)
			{
				await InternalFacade.Initialize(setupPort);
			}
			else
			{
				// TODO inform user about problem
			}

			ExtensionController.Initialize(this);
			List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection = ExtensionController.GetExtensionToolMenuItemCollection();
			Tray.Initialize(extensionMenuItemCollection);
		}

		public void Dispose ()
		{
			Tray.Dispose();
		}
	}
}
