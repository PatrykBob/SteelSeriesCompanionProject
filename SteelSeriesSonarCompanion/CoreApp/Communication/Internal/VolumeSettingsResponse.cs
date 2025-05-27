namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class VolumeSettingsResponse
	{
		public VolumeResponse masters;
		public DevicesResponse devices;

		public VolumeSettingsResponse (VolumeResponse masters, DevicesResponse devices)
		{
			this.masters = masters;
			this.devices = devices;
		}

		public class DevicesResponse
		{
			public VolumeResponse game;
			public VolumeResponse chatRender;
			public VolumeResponse chatCapture;
			public VolumeResponse media;
			public VolumeResponse aux;

			public DevicesResponse (VolumeResponse game, VolumeResponse chatRender, VolumeResponse chatCapture, VolumeResponse media, VolumeResponse aux)
			{
				this.game = game;
				this.chatRender = chatRender;
				this.chatCapture = chatCapture;
				this.media = media;
				this.aux = aux;
			}
		}

		public class VolumeResponse
		{
			public StreamVolumeResponse stream;
			public ClassicVolumeResponse classic;

			public VolumeResponse (StreamVolumeResponse stream, ClassicVolumeResponse classic)
			{
				this.stream = stream;
				this.classic = classic;
			}
		}

		public class StreamVolumeResponse
		{

		}

		public class ClassicVolumeResponse
		{
			public float volume;
			public bool muted;

			public ClassicVolumeResponse (float volume, bool muted)
			{
				this.volume = volume;
				this.muted = muted;
			}
		}
	}
}
