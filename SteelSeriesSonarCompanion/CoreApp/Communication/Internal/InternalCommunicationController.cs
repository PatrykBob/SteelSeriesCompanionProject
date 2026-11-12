using SteelSeriesSonarCompanion.CoreApp.Setup;
using SteelSeriesSonarCompanion.Shared.Core;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
    public class InternalCommunicationController : IDisposable
    {
        private int? SonarCommunicationPort { get; set; }
        private SemaphoreSlim ConnectionLock { get; } = new(1, 1);
        private HttpClient CurrentHttpClient { get; } = CreateHttpClient();

        public async Task<bool> Initialize ()
        {
            return await Reconnect(null) != null;
        }

        public void Dispose ()
        {
            CurrentHttpClient.Dispose();
            ConnectionLock.Dispose();
        }

        private static HttpClient CreateHttpClient ()
        {
            HttpClientHandler handler = new();
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            return new HttpClient(handler);
        }

        public async Task<List<ChannelConfigResponse>?> GetConfig ()
        {
            return await SendRequest<List<ChannelConfigResponse>>(HttpMethod.Get, InternalCommunicationAddressProvider.GetConfigSelectedAddress);
        }

        public async Task<ChannelConfigResponse?> PostConfig (ChannelConfigResponse config)
        {
            string json = JsonConverter.ConvertToJSON(config);
            return await SendRequest<ChannelConfigResponse>(HttpMethod.Put, InternalCommunicationAddressProvider.GetConfigAddress, () => new StringContent(json, Encoding.UTF8, "application/json"));
        }

        public async Task<VolumeSettingsResponse?> GetVolumeSettings ()
        {
            return await SendRequest<VolumeSettingsResponse>(HttpMethod.Get, InternalCommunicationAddressProvider.GetVolumeSettingsUri);
        }

        public async Task<List<ClassicRedirectionResponse>?> GetClassicRedirections ()
        {
            return await SendRequest<List<ClassicRedirectionResponse>>(HttpMethod.Get, InternalCommunicationAddressProvider.GetClassicRedirectionsAddress);
        }

        public async Task SetChannelVolume (string channel, float volume)
        {
            await SendRequest<object>(HttpMethod.Put, port => InternalCommunicationAddressProvider.GetClassicChannelVolumeAddress(port, channel, volume));
        }

        public async Task SetChannelMute (string channel, bool mute)
        {
            await SendRequest<object>(HttpMethod.Put, port => InternalCommunicationAddressProvider.GetClassicChannelMuteAddress(port, channel, mute));
        }

        public async Task SetChatMix (float value)
        {
            await SendRequest<object>(HttpMethod.Put, port => InternalCommunicationAddressProvider.GetChatMixAddress(port, value));
        }

        private async Task<T?> SendRequest<T> (HttpMethod method, Func<int, Uri> createUri, Func<HttpContent>? createContent = null)
        {
            int? port = SonarCommunicationPort ?? await Reconnect(null);

            if (port == null)
            {
                return default;
            }

            try
            {
                return await Send<T>(method, createUri(port.Value), createContent);
            }
            catch (HttpRequestException e)
            {
                Trace.WriteLine(e);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return default;
            }

            int? newPort = await Reconnect(port);

            if (newPort == null || newPort == port)
            {
                return default;
            }

            try
            {
                return await Send<T>(method, createUri(newPort.Value), createContent);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return default;
            }
        }

        private async Task<int?> Reconnect (int? failedPort)
        {
            await ConnectionLock.WaitAsync();

            try
            {
                if (SonarCommunicationPort != failedPort)
                {
                    return SonarCommunicationPort;
                }

                SonarCommunicationPort = await ResolveSonarCommunicationPort();
                return SonarCommunicationPort;
            }
            finally
            {
                ConnectionLock.Release();
            }
        }

        private async Task<int?> ResolveSonarCommunicationPort ()
        {
            try
            {
                if (SonarSetupLoader.TryGetSonarSetupPort(out int setupPort) == false)
                {
                    return null;
                }

                Uri setupUri = InternalCommunicationAddressProvider.GetSubAppsAddress(setupPort);
                SonarSubAppsResponse? response = await Send<SonarSubAppsResponse>(HttpMethod.Get, setupUri, null);
                return response?.subApps?.sonar?.metadata?.webServerAddress?.Port;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed to resolve SteelSeries Sonar port: {e}");
                return null;
            }
        }

        private async Task<T?> Send<T> (HttpMethod method, Uri uri, Func<HttpContent>? createContent)
        {
            using HttpRequestMessage request = new(method, uri);
            request.Content = createContent?.Invoke();
            using HttpResponseMessage response = await CurrentHttpClient.SendAsync(request);

            if (response.IsSuccessStatusCode == false)
            {
                return default;
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonConverter.ConvertFromJSON<T>(responseContent);
        }
    }
}
