using SteelSeriesSonarCompanion.Shared.Core;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
    public class InternalCommunicationController
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

        public async Task<List<ChannelConfigResponse>?> GetConfig ()
        {
            Uri configSelectedUri = InternalCommunicationAddressProvider.GetConfigSelectedAddress(SonarCommunicationPort);
            return await SendGetRequest<List<ChannelConfigResponse>?>(configSelectedUri);
        }

        public async Task<ChannelConfigResponse?> PostConfig (ChannelConfigResponse config)
        {
			Uri configUri = InternalCommunicationAddressProvider.GetConfigAddress(SonarCommunicationPort);
            string json = JsonConverter.ConvertToJSON(config);
			StringContent content = new(json, Encoding.UTF8, "application/json");
            return await SendPutRequest<ChannelConfigResponse>(configUri, content);
        }

        public async Task<VolumeSettingsResponse?> GetVolumeSettings ()
        {
            Uri settingsUri = InternalCommunicationAddressProvider.GetVolumeSettingsUri(SonarCommunicationPort);
            return await SendGetRequest<VolumeSettingsResponse>(settingsUri);
        }

        public async Task SetChannelVolume (string channel, float volume)
        {
            Uri volumeUri = InternalCommunicationAddressProvider.GetClassicChannelVolumeAddress(SonarCommunicationPort, channel, volume);
            await SendPutRequest<object>(volumeUri);
        }

        public async Task SetChannelMute (string channel, bool mute)
        {
            Uri muteUri = InternalCommunicationAddressProvider.GetClassicChannelMuteAddress(SonarCommunicationPort, channel, mute);
            await SendPutRequest<object>(muteUri);
        }

        public async Task SetChatMix (float value)
        {
            Uri chatMixUri = InternalCommunicationAddressProvider.GetChatMixAddress(SonarCommunicationPort, value);
            await SendPutRequest<object>(chatMixUri);
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
                    return JsonConverter.ConvertFromJSON<T>(responseContent);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return default;
        }

        private async Task<T?> SendPostRequest<T> (Uri uri, HttpContent content = null)
        {
            try
            {
                HttpResponseMessage response = await CurrentHttpClient.PostAsync(uri, content);

                if (response.IsSuccessStatusCode == true)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConverter.ConvertFromJSON<T>(responseContent);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return default;
        }

        private async Task<T?> SendPutRequest<T> (Uri uri, HttpContent content = null)
        {
            try
            {
                HttpResponseMessage response = await CurrentHttpClient.PutAsync(uri, content);

                if (response.IsSuccessStatusCode == true)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConverter.ConvertFromJSON<T>(responseContent);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return default;
        }
    }
}
