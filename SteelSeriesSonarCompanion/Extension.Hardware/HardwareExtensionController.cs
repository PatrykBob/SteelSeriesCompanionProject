using SteelSeriesSonarCompanion.Shared.Core;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace SteelSeriesSonarCompanion.Extension.Hardware
{
	public class HardwareExtensionController : BaseSteelSeriesCompanionExtension
	{
		private SerialPort? Port { get; set; }
		private Dictionary<SoundChannel, int> VolumeMap { get; set; } = new();

		private const string PORT_NAME = "COM3";
		private const int BAUD_RATE = 9600;
		private const int READ_TIMEOUT_MS = 500;
		private const int MAX_ANALOG_VALUE = 1023;
		private const int RANGE = 1;

		private static readonly Regex LinePattern = new(@"^(\d+)A(\d+)B(\d+)C$", RegexOptions.Compiled);

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
			ClosePort();

			VolumeMap.Clear();
			VolumeMap.Add(SoundChannel.GAME, 0);
			VolumeMap.Add(SoundChannel.CHAT, 0);
			VolumeMap.Add(SoundChannel.MEDIA, 0);

			try
			{
				Port = new SerialPort(PORT_NAME, BAUD_RATE)
				{
					NewLine = "\n",
					ReadTimeout = READ_TIMEOUT_MS,
					DtrEnable = true
				};
				Port.DataReceived += DataReceivedHandler;
				Port.Open();
			}
			catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
			{
				Trace.WriteLine($"Could not open serial port {PORT_NAME}: {e.Message}");
				ClosePort();
			}
		}

		private void ClosePort ()
		{
			if (Port != null)
			{
				Port.DataReceived -= DataReceivedHandler;
				Port.Close();
				Port.Dispose();
				Port = null;
			}
		}

		private void DataReceivedHandler (object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				SerialPort? port = Port;

				while (port != null && port.IsOpen && port.BytesToRead > 0)
				{
					ProcessLine(port.ReadLine());
				}
			}
			catch (Exception ex) when (ex is IOException or InvalidOperationException or TimeoutException)
			{
				Trace.WriteLine($"Serial read failed: {ex.Message}");
			}
		}

		private void ProcessLine (string line)
		{
			Match match = LinePattern.Match(line.Trim());

			if (match.Success == false)
			{
				return;
			}

			TryUpdateVolume(SoundChannel.GAME, int.Parse(match.Groups[1].Value));
			TryUpdateVolume(SoundChannel.CHAT, int.Parse(match.Groups[2].Value));
			TryUpdateVolume(SoundChannel.MEDIA, int.Parse(match.Groups[3].Value));
		}

		private void TryUpdateVolume (SoundChannel channel, int volume)
		{
			volume = Math.Clamp(volume, 0, MAX_ANALOG_VALUE);
			int previousVolume = VolumeMap[channel];

			if (Math.Abs(volume - previousVolume) > RANGE)
			{
				VolumeMap[channel] = volume;
				Trace.WriteLine($"Channel {channel} volume changed from {previousVolume} to {volume}");
				_ = CompanionCore!.SetChannelVolume(this, channel, (float)volume / MAX_ANALOG_VALUE);
			}
		}
	}

}
