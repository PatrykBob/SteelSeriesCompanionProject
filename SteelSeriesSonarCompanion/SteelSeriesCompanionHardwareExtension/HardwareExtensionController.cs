
using SteelSeriesCompanion.SharedCore;
using System.Diagnostics;
using System.IO.Ports;

namespace SteelSeriesCompanionHardwareExtension
{
	public class HardwareExtensionController : BaseSteelSeriesCompanionExtension
	{
		private SerialPort? Port { get; set; }
		private Dictionary<SoundChannel, int> VolumeMap { get; set; } = new();

		private const int RANGE = 1;

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			Initialize();
		}

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			return new SteelSeriesCompanionExtensionMenuItem("Reset Port", Initialize);
		}

		private void Initialize ()
		{
			Port?.Close();
			Port?.Dispose();

			VolumeMap.Clear();
			VolumeMap.Add(SoundChannel.GAME, 0);
			VolumeMap.Add(SoundChannel.CHAT, 0);
			VolumeMap.Add(SoundChannel.MEDIA, 0);

			Port = new SerialPort("COM3", 9600);
			Port.Open();
			Port.DtrEnable = true;
			Port.DataReceived += DataReceivedHandler;
		}

		private void DataReceivedHandler (object sender, SerialDataReceivedEventArgs e)
		{			
			string test1 = Port!.ReadTo("A");
			string test2 = Port!.ReadTo("B");
			string test3 = Port!.ReadTo("C");

			int int1 = int.Parse(test1);
			int int2 = int.Parse(test2);
			int int3 = int.Parse(test3);

			TryUpdateVolume(SoundChannel.GAME, int1);
			TryUpdateVolume(SoundChannel.CHAT, int2);
			TryUpdateVolume(SoundChannel.MEDIA, int3);
		}

		private void TryUpdateVolume (SoundChannel channel, int volume)
		{
			int previousVolume = VolumeMap[channel];

			if (Math.Abs(volume - previousVolume) > RANGE)
			{
				VolumeMap[channel] = volume;
				Trace.WriteLine($"Channel {channel} volume changed from {previousVolume} to {volume}");
				CompanionCore!.SetChannelVolume(channel, (float)volume / 1023);
			}
		}
	}

}
