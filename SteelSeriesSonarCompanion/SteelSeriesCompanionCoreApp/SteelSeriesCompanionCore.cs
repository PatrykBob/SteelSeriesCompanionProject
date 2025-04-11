using SteelSeriesCompanion.Extension;
using SteelSeriesCompanion.SharedCore;
using SteelSeriesCompanionCoreApp.Setup;
using SteelSeriesSonarCompanion.Communication.Internal;

namespace SteelSeriesCompanion
{
	public class SteelSeriesCompanionCore : ISteelSeriesCompanionCore
	{
		private InternalCommunicationFacade InternalFacade { get; set; } = new();
		private SteelSeriesCompanionExtensionController ExtensionController { get; set; } = new();

		public async Task SetChannelVolume (SoundChannel channel, float volume)
		{
			await InternalFacade.SetChannelVolume(channel, volume);
		}

		public async void Initialize ()
		{
			if (SonarSetupLoader.TryGetSonarSetupPort(out int setupPort))
			{
				await InternalFacade.Initialize(setupPort);
			}
			else
			{
				// TODO inform user about problem
			}

			ExtensionController.Initialize(this);
		}
	}
}
