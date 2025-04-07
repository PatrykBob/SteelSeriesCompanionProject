using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;

namespace SteelSeriesSonarCompanion.Communication.Internal
{
	public class InternalCommunicationFacade
	{
		private int SonarCommunicationPort { get; set; }
		private HttpClient CurrentHttpClient { get; set; }

		public async Task Initialize (int sonarSetupPort)
		{
			CacheHttpClient();
			await CacheCommunicationPort();

			void CacheHttpClient ()
			{
				HttpClientHandler handler = new HttpClientHandler();
				handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
				CurrentHttpClient = new HttpClient(handler);
			}

			async Task CacheCommunicationPort ()
			{
				SonarCommunicationPort = await GetSonarCommunicationPort(sonarSetupPort);
				Uri communicationUri = InternalCommunicationAddressProvider.GetCommunicationAddress(SonarCommunicationPort);
				CurrentHttpClient.DefaultRequestHeaders.Host = communicationUri.Host;
			}
		}

		public async Task<object> GetSonarSettings ()
		{
			throw new NotImplementedException();
		}

		public async Task SetChannelVolume (string channel, float volume)
		{
			Uri volumeUri = InternalCommunicationAddressProvider.GetClassicChannelVolumeAddress(SonarCommunicationPort, channel, volume);
			await SendPutRequest(volumeUri);
		}

		public async Task SetChannelMute (string channel, bool mute)
		{
			Uri muteUri = InternalCommunicationAddressProvider.GetClassicChannelMuteAddress(SonarCommunicationPort, channel, mute);
			await SendPutRequest(muteUri);
		}

		public async Task SetChatMix (float value)
		{
			Uri chatMixUri = InternalCommunicationAddressProvider.GetChatMixAddress(SonarCommunicationPort, value);
			await SendPutRequest(chatMixUri);
		}

		private async Task<int> GetSonarCommunicationPort (int setupPort)
		{
			Uri setupUri = InternalCommunicationAddressProvider.GetSubAppsAddress(setupPort);
			SonarSubAppsResponse? response = await SendGetRequest<SonarSubAppsResponse>(setupUri);

			if (response != null)
			{
				return response.subApps.sonar.metadata.webServerAddress.Port;
			}

			return default;
		}


		private async Task<T?> SendGetRequest<T> (Uri uri)
		{
			try
			{
				HttpResponseMessage response = await CurrentHttpClient.GetAsync(uri);

				if (response.IsSuccessStatusCode == true)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					return ConvertFromJSON<T>(responseContent);
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e);
			}

			return default;
		}

		private async Task SendPutRequest (Uri uri, HttpContent content = null)
		{
			await CurrentHttpClient.PutAsync(uri, content);
		}

		private T? ConvertFromJSON<T> (string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
