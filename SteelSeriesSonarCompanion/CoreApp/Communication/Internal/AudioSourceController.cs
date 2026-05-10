using NAudio.CoreAudioApi;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class AudioSourceController
	{
		private const string TEMPORARY_FRIENDLY_NAME = "Głośniki (Razer Audio Controller - Game)";

		public void SetMasterVolume (float volume)
		{
			MMDevice? device = GetMasterDevice();

			if (device != null)
			{
				device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
			}
		}

		public float? GetMasterVolume ()
		{
			return GetMasterDevice()?.AudioEndpointVolume.MasterVolumeLevelScalar;
		}

		public void SetMasterMute (bool mute)
		{
			MMDevice? device = GetMasterDevice();

			if (device != null)
			{
				device.AudioEndpointVolume.Mute = mute;
			}
		}

		public bool? GetMasterMute ()
		{
			return GetMasterDevice()?.AudioEndpointVolume.Mute;
		}

		private MMDevice? GetMasterDevice ()
		{
			var enumerator = new MMDeviceEnumerator();
			var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

			foreach (var device in devices)
			{
				if (device.FriendlyName == TEMPORARY_FRIENDLY_NAME)
				{
					return device;
				}
			}

			return null;
		}
	}
}
