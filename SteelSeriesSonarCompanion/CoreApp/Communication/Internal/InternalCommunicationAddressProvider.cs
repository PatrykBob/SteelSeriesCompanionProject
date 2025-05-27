using System.Globalization;

namespace SteelSeriesSonarCompanion.Communication.Internal
{
	public static class InternalCommunicationAddressProvider
	{
		private const string LOCALHOST_ADDRESS = "http://127.0.0.1";
		private const string SECURE_LOCALHOST_ADDRESS = "https://127.0.0.1";

		private const string SUB_APPS_ENDPOINT_FORMAT = "{0}:{1}/subApps";
		private const string GET_CLASSIC_VOLUME_ENDPOINT_FORMAT = "{0}:{1}/volumeSettings/classic";
		private const string PUT_CLASSIC_VOLUME_ENDPOINT_FORMAT = "{0}:{1}/volumeSettings/classic/{2}/Volume/{3}";
		private const string PUT_CLASSIC_MUTE_ENDPOINT_FORMAT = "{0}:{1}/volumeSettings/classic/{2}/Mute/{3}";
		private const string CHAT_MIX_ENDPOINT_FORMAT = "{0}:{1}/ChatMix?balance={2}";

		public static Uri GetCommunicationAddress (int port)
		{
			return new Uri($"{LOCALHOST_ADDRESS}:{port}");
		}

		public static Uri GetSubAppsAddress (int port)
		{
			return new Uri(string.Format(SUB_APPS_ENDPOINT_FORMAT, SECURE_LOCALHOST_ADDRESS, port));
		}

		public static Uri GetVolumeSettingsUri (int port)
		{
			return new Uri(string.Format(GET_CLASSIC_VOLUME_ENDPOINT_FORMAT, LOCALHOST_ADDRESS, port));
		}

		public static Uri GetClassicChannelVolumeAddress (int port, string channel, float volume)
		{
			return new Uri(string.Format(PUT_CLASSIC_VOLUME_ENDPOINT_FORMAT, LOCALHOST_ADDRESS, port, channel, volume.ToString(CultureInfo.InvariantCulture)));
		}

		public static Uri GetClassicChannelMuteAddress (int port, string channel, bool mute)
		{
			return new Uri(string.Format(PUT_CLASSIC_MUTE_ENDPOINT_FORMAT, LOCALHOST_ADDRESS, port, channel, mute));
		}

		public static Uri GetChatMixAddress (int port, float value)
		{
			return new Uri(string.Format(CHAT_MIX_ENDPOINT_FORMAT, LOCALHOST_ADDRESS, port, value.ToString(CultureInfo.InvariantCulture)));
		}
	}
}
