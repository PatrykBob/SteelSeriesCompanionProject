using SteelSeriesSonarCompanion.CoreApp.Setup;
using SteelSeriesSonarCompanion.Shared.Core;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
    public class InternalCommunicationController : IDisposable
    {
        private int? SonarCommunicationPort { get; set; }
        private SemaphoreSlim ConnectionLock { get; } = new(1, 1);
        private HttpClient CurrentHttpClient { get; } = CreateHttpClient();

        private const int REQUEST_TIMEOUT_SECONDS = 5;

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
            handler.ServerCertificateCustomValidationCallback = IsTrustedCertificate;
            return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(REQUEST_TIMEOUT_SECONDS) };
        }

        private static bool IsTrustedCertificate (HttpRequestMessage request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
            {
                return true;
            }

            return request.RequestUri?.IsLoopback == true;
        }

        public async Task<List<ChannelConfigResponse>?> GetSelectedConfigs ()
        {
            return (await SendRequest<List<ChannelConfigResponse>>(HttpMethod.Get, InternalCommunicationAddressProvider.GetConfigSelectedAddress)).Value;
        }

        public async Task<List<ChannelConfigResponse>?> GetConfigs (string virtualAudioDevice)
        {
            return (await SendRequest<List<ChannelConfigResponse>>(HttpMethod.Get, port => InternalCommunicationAddressProvider.GetConfigsByDeviceAddress(port, virtualAudioDevice))).Value;
        }

        public async Task<ChannelConfigResponse?> AddConfig (ChannelConfigResponse config)
        {
            return (await SendRequest<ChannelConfigResponse>(HttpMethod.Post, InternalCommunicationAddressProvider.GetConfigAddress, () => CreateJsonContent(config))).Value;
        }

        public async Task<bool> TryUpdateConfig (ChannelConfigResponse config)
        {
            return (await SendRequest<object>(HttpMethod.Put, InternalCommunicationAddressProvider.GetConfigAddress, () => CreateJsonContent(config))).IsSuccess;
        }

        public async Task<bool> TrySelectConfig (string configId)
        {
            return (await SendRequest<object>(HttpMethod.Put, port => InternalCommunicationAddressProvider.GetSelectConfigAddress(port, configId))).IsSuccess;
        }

        private static StringContent CreateJsonContent<T> (T value)
        {
            return new StringContent(JsonConverter.ConvertToJSON(value), Encoding.UTF8, "application/json");
        }

        public async Task<VolumeSettingsResponse?> GetVolumeSettings ()
        {
            return (await SendRequest<VolumeSettingsResponse>(HttpMethod.Get, InternalCommunicationAddressProvider.GetVolumeSettingsUri)).Value;
        }

        public async Task<List<ClassicRedirectionResponse>?> GetClassicRedirections ()
        {
            return (await SendRequest<List<ClassicRedirectionResponse>>(HttpMethod.Get, InternalCommunicationAddressProvider.GetClassicRedirectionsAddress)).Value;
        }

        public async Task<bool> TrySetChannelVolume (string channel, float volume)
        {
            return (await SendRequest<object>(HttpMethod.Put, port => InternalCommunicationAddressProvider.GetClassicChannelVolumeAddress(port, channel, volume))).IsSuccess;
        }

        public async Task<bool> TrySetChannelMute (string channel, bool mute)
        {
            return (await SendRequest<object>(HttpMethod.Put, port => InternalCommunicationAddressProvider.GetClassicChannelMuteAddress(port, channel, mute))).IsSuccess;
        }

        public async Task<bool> TrySetChatMix (float value)
        {
            return (await SendRequest<object>(HttpMethod.Put, port => InternalCommunicationAddressProvider.GetChatMixAddress(port, value))).IsSuccess;
        }

        private async Task<(bool IsSuccess, T? Value)> SendRequest<T> (HttpMethod method, Func<int, Uri> createUri, Func<HttpContent>? createContent = null)
        {
            int? port = SonarCommunicationPort ?? await Reconnect(null);

            if (port == null)
            {
                return (false, default);
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
                return (false, default);
            }

            int? newPort = await Reconnect(port);

            if (newPort == null || newPort == port)
            {
                return (false, default);
            }

            try
            {
                return await Send<T>(method, createUri(newPort.Value), createContent);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return (false, default);
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
                SonarSubAppsResponse? response = (await Send<SonarSubAppsResponse>(HttpMethod.Get, setupUri, null)).Value;
                return response?.subApps?.sonar?.metadata?.webServerAddress?.Port;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed to resolve SteelSeries Sonar port: {e}");
                return null;
            }
        }

        private async Task<(bool IsSuccess, T? Value)> Send<T> (HttpMethod method, Uri uri, Func<HttpContent>? createContent)
        {
            using HttpRequestMessage request = new(method, uri);
            request.Content = createContent?.Invoke();
            using HttpResponseMessage response = await CurrentHttpClient.SendAsync(request);

            if (response.IsSuccessStatusCode == false)
            {
                return (false, default);
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            return (true, JsonConverter.ConvertFromJSON<T>(responseContent));
        }
    }
}
