using SteelSeriesSonarCompanion.Shared.Core;
using System.Diagnostics;
using System.IO;

namespace SteelSeriesSonarCompanion.Extension.Hardware
{
	public class HardwareSettings
	{
		public string? PortName { get; set; }
	}

	public class HardwareSettingsStore
	{
		private const string FOLDER_NAME = "SteelSeriesSonarCompanion";
		private const string FILE_NAME = "hardware.json";

		private static string FilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FOLDER_NAME, FILE_NAME);

		public HardwareSettings Load ()
		{
			try
			{
				if (File.Exists(FilePath) == true)
				{
					return JsonConverter.ConvertFromJSON<HardwareSettings>(File.ReadAllText(FilePath)) ?? new();
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to load hardware settings: {e.Message}");
			}

			return new();
		}

		public void Save (HardwareSettings settings)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
				File.WriteAllText(FilePath, JsonConverter.ConvertToJSON(settings));
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to save hardware settings: {e.Message}");
			}
		}
	}
}
