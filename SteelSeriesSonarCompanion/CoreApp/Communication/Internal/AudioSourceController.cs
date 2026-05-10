using NAudio.CoreAudioApi;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class AudioSourceController
	{
		public void SetMasterVolume (float volume)
		{
			var enumerator = new MMDeviceEnumerator();
			var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

			foreach (var device in devices)
			{
				if (device.FriendlyName == "Głośniki (Razer Audio Controller - Game)")
				{
					device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
				}
			}
		}
	}
}
