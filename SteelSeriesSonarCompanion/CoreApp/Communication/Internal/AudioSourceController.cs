using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class AudioSourceController : IDisposable
	{
		private MMDeviceEnumerator Enumerator { get; set; } = new();
		private MMDevice? CachedDevice { get; set; }

		public bool TrySetVolume (string? deviceId, float volume)
		{
			return UseEndpointVolume(deviceId, endpointVolume => endpointVolume.MasterVolumeLevelScalar = volume) != null;
		}

		public float? GetVolume (string? deviceId)
		{
			return UseEndpointVolume(deviceId, endpointVolume => endpointVolume.MasterVolumeLevelScalar);
		}

		public bool TrySetMute (string? deviceId, bool mute)
		{
			return UseEndpointVolume(deviceId, endpointVolume => endpointVolume.Mute = mute) != null;
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

			try
			{
				return action(GetDevice(deviceId).AudioEndpointVolume);
			}
			catch (COMException)
			{
				ReleaseCachedDevice();
				return null;
			}
		}

		public void ReleaseCachedDevice ()
		{
			CachedDevice?.Dispose();
			CachedDevice = null;
		}

		private MMDevice GetDevice (string deviceId)
		{
			if (CachedDevice == null || CachedDevice.ID != deviceId)
			{
				ReleaseCachedDevice();
				CachedDevice = Enumerator.GetDevice(deviceId);
			}

			return CachedDevice;
		}

		public void Dispose ()
		{
			ReleaseCachedDevice();
			Enumerator.Dispose();
		}
	}
}
