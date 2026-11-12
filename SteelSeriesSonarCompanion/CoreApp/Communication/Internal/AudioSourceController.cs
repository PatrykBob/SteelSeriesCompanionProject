using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class AudioSourceController
	{
		public void SetVolume (string? deviceId, float volume)
		{
			UseEndpointVolume(deviceId, endpointVolume => endpointVolume.MasterVolumeLevelScalar = volume);
		}

		public float? GetVolume (string? deviceId)
		{
			return UseEndpointVolume(deviceId, endpointVolume => endpointVolume.MasterVolumeLevelScalar);
		}

		public void SetMute (string? deviceId, bool mute)
		{
			UseEndpointVolume(deviceId, endpointVolume => endpointVolume.Mute = mute);
		}

		public bool? GetMute (string? deviceId)
		{
			return UseEndpointVolume(deviceId, endpointVolume => endpointVolume.Mute);
		}

		private T? UseEndpointVolume<T> (string? deviceId, Func<AudioEndpointVolume, T> action) where T : struct
		{
			if (string.IsNullOrEmpty(deviceId) == true)
			{
				return null;
			}

			using MMDeviceEnumerator enumerator = new();

			try
			{
				using MMDevice device = enumerator.GetDevice(deviceId);
				return action(device.AudioEndpointVolume);
			}
			catch (COMException)
			{
				return null;
			}
		}
	}
}
