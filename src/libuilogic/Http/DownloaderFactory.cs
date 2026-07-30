using System;
using System.Net;
using System.Net.Http;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.Http
{
    public static class DownloaderFactory
    {
        public static IDownloader MakeHttpClient()
        {
            var httpClient = new HttpClient(CreateHandler(Configuration.Settings.Proxy))
            {
                Timeout = TimeSpan.FromMinutes(30) // 30 minutes for large downloads
            };

            if (Configuration.Settings.General.UseLegacyDownloader)
            {
                return new LegacyDownloader(httpClient);
            }

            return new HttpClientDownloader(httpClient);
        }

        private static HttpClientHandler CreateHandler(ProxySettings proxySettings)
        {
            var handler = new HttpClientHandler();

            if (string.IsNullOrEmpty(proxySettings.ProxyAddress))
            {
                // No proxy configured in SE - downloads still go through the system/environment
                // proxy, so the loopback + bypass-list behavior must apply there too, same as
                // HttpClientFactoryWithProxy.
                var systemProxy = HttpClient.DefaultProxy;
                if (systemProxy != null)
                {
                    handler.UseProxy = true;
                    handler.Proxy = new BypassingWebProxy(systemProxy, proxySettings.BypassList);
                }

                if (proxySettings.UseDefaultCredentials)
                {
                    handler.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                return handler;
            }

            var proxy = new WebProxy(proxySettings.ProxyAddress);

            if (!proxySettings.UseDefaultCredentials && !string.IsNullOrEmpty(proxySettings.UserName))
            {
                // Credentials go on the proxy itself, like in HttpClientFactoryWithProxy - a
                // CredentialCache would need ProxySettings.AuthType, which nothing ever sets,
                // and CredentialCache.Add throws on a null auth type.
                proxy.Credentials = string.IsNullOrWhiteSpace(proxySettings.Domain)
                    ? new NetworkCredential(proxySettings.UserName, proxySettings.DecodePassword())
                    : new NetworkCredential(proxySettings.UserName, proxySettings.DecodePassword(), proxySettings.Domain);
            }
            else
            {
                proxy.UseDefaultCredentials = true;
            }

            handler.UseProxy = true;
            handler.Proxy = new BypassingWebProxy(proxy, proxySettings.BypassList);

            return handler;
        }
    }
}
