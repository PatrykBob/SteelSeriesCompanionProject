using SteelSeriesSonarCompanion.Shared.Core;
using System.IO;

namespace SteelSeriesSonarCompanion.CoreApp.Setup
{
	public static class SonarSetupLoader
	{
		private const string SONAR_SETUP_FILE_PATH = "C:\\ProgramData\\SteelSeries\\GG\\coreProps.json";
		private const char ADDRESS_SPLIT_CHARACTER = ':';

		public static bool TryGetSonarSetupPort (out int setupPort)
		{
			setupPort = default;

			if (File.Exists(SONAR_SETUP_FILE_PATH) == false)
			{
				return false;
			}

			string json = File.ReadAllText(SONAR_SETUP_FILE_PATH);
			SonarSetupConfig? config = JsonConverter.ConvertFromJSON<SonarSetupConfig>(json);

			if (config == null || string.IsNullOrEmpty(config.ggEncryptedAddress) == true)
			{
				return false;
			}

			string[] addressPartCollection = config.ggEncryptedAddress.Split(ADDRESS_SPLIT_CHARACTER);

			if (int.TryParse(addressPartCollection[^1], out setupPort) == false)
			{
				return false;
			}

			return true;
		}
	}
}
