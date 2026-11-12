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
		private HardwareSettingsStore SettingsStore { get; } = new();
		private HardwareSettings Settings { get; set; } = new();
		private bool IsDetecting { get; set; }

		private const int BAUD_RATE = 9600;
		private const int READ_TIMEOUT_MS = 500;
		private const int PROBE_TIMEOUT_MS = 4000;
		private const int MAX_ANALOG_VALUE = 1023;
		private const int RANGE = 4;

		private static readonly Regex LinePattern = new(@"^(\d+)A(\d+)B(\d+)C$", RegexOptions.Compiled);

		static HardwareExtensionController ()
		{
			// HACK: Must run before any method that uses SerialPort is compiled
			EmbeddedAssemblyResolver.Register();
		}

		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			Settings = SettingsStore.Load();
			_ = Connect(false);
		}

		public override SteelSeriesCompanionExtensionMenuItem GetExtensionMenuItem ()
		{
			return new SteelSeriesCompanionExtensionMenuItem("Reset Port", () => _ = Connect(true));
		}

		public override void Dispose ()
		{
			ClosePort();
		}

		private async Task Connect (bool notify)
		{
			if (IsDetecting == true)
			{
				return;
			}

			IsDetecting = true;
			ClosePort();
			string? portName = await Task.Run(FindArduinoPort);
			IsDetecting = false;

			if (portName != null)
			{
				SavePortName(portName);
				OpenPort(portName);
			}

			if (notify == true)
			{
				ShowConnectionResult(portName);
			}
		}

		private string? FindArduinoPort ()
		{
			return SerialPort.GetPortNames().OrderBy(name => name != Settings.PortName).FirstOrDefault(IsArduinoPort);
		}

		private bool IsArduinoPort (string portName)
		{
			try
			{
				using SerialPort port = CreatePort(portName);
				port.ReadTimeout = PROBE_TIMEOUT_MS;
				port.Open();
				port.ReadLine();
				return LinePattern.IsMatch(port.ReadLine().Trim());
			}
			catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or TimeoutException)
			{
				Trace.WriteLine($"Serial port {portName} is not the Arduino: {e.Message}");
				return false;
			}
		}

		private void SavePortName (string portName)
		{
			Settings.PortName = portName;
			SettingsStore.Save(Settings);
		}

		private void ShowConnectionResult (string? portName)
		{
			string message = portName != null ? $"Arduino found on {portName}." : "Arduino not found on any serial port.";
			CompanionCore!.ShowNotification(message);
		}

		private SerialPort CreatePort (string portName)
		{
			return new SerialPort(portName, BAUD_RATE)
			{
				NewLine = "\n",
				ReadTimeout = READ_TIMEOUT_MS,
				DtrEnable = true
			};
		}

		private void OpenPort (string portName)
		{
			VolumeMap.Clear();
			VolumeMap.Add(SoundChannel.GAME, 0);
			VolumeMap.Add(SoundChannel.CHAT, 0);
			VolumeMap.Add(SoundChannel.MEDIA, 0);

			try
			{
				Port = CreatePort(portName);
				Port.DataReceived += DataReceivedHandler;
				Port.Open();
			}
			catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
			{
				Trace.WriteLine($"Could not open serial port {portName}: {e.Message}");
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
			bool reachedLimit = (volume == 0 || volume == MAX_ANALOG_VALUE) && volume != previousVolume;

			if (Math.Abs(volume - previousVolume) > RANGE || reachedLimit == true)
			{
				VolumeMap[channel] = volume;
				Trace.WriteLine($"Channel {channel} volume changed from {previousVolume} to {volume}");
				_ = CompanionCore!.SetChannelVolume(this, channel, (float)volume / MAX_ANALOG_VALUE);
			}
		}
	}

}
