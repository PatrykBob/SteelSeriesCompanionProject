namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public class SonarSubAppsResponse
	{
		public SubApps subApps;

		public class SubApps
		{
			public Engine engine;
			public Sonar sonar;
			public ThreeDat threeDAT;
		}

		public class Engine
		{
			public string name;
			public bool isEnabled;
			public bool isReady;
			public bool isRunning;
			public long exitCode;
			public bool shouldAutoStart;
			public bool isWindowsSupported;
			public bool isMacSupported;
			public bool toggleViaSettings;
			public bool isBrowserViewSupported;
			public EngineMetadata metadata;
			public SecretMetadata secretMetadata;
		}

		public class EngineMetadata
		{
			public string encryptedWebServerAddress;
			public string webServerAddress;
			public string offlineFrontendAddress;
			public string onlineFrontendAddress;
		}

		public class SecretMetadata
		{
			public string encryptedWebServerAddressCertText;
		}

		public class Sonar
		{
			public string name;
			public bool isEnabled;
			public bool isReady;
			public bool isRunning;
			public long exitCode;
			public bool shouldAutoStart;
			public bool isWindowsSupported;
			public bool isMacSupported;
			public bool toggleViaSettings;
			public bool isBrowserViewSupported;
			public SonarMetadata metadata;
			public SecretMetadata secretMetadata;
		}

		public class SonarMetadata
		{
			public string encryptedWebServerAddress;
			public Uri webServerAddress;
			public string offlineFrontendAddress;
			public string onlineFrontendAddress;
		}

		public class ThreeDat
		{
			public string name;
			public bool isEnabled;
			public bool isReady;
			public bool isRunning;
			public long exitCode;
			public bool shouldAutoStart;
			public bool isWindowsSupported;
			public bool isMacSupported;
			public bool toggleViaSettings;
			public bool isBrowserViewSupported;
			public EngineMetadata metadata;
			public SecretMetadata secretMetadata;
		}
	}
}
