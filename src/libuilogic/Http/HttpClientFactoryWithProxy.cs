using System.Net.Http;
using System.Net;

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.UiLogic.Http
{
    public static class HttpClientFactoryWithProxy
    {
        public static HttpClient CreateHttpClientWithProxy()
        {
            return new HttpClient(CreateHandler());
        }

        public static HttpClientHandler CreateHandler()
        {
            return CreateHandler(Configuration.Settings.Proxy);
        }

        public static HttpClientHandler CreateHandler(ProxySettings proxySettings)
        {
            var handler = new HttpClientHandler();

            if (string.IsNullOrEmpty(proxySettings.ProxyAddress))
            {
                // No proxy configured in SE - requests still go through the system/environment
                // proxy, so the loopback + bypass-list behavior must apply there too.
                var systemProxy = HttpClient.DefaultProxy;
                if (systemProxy != null)
                {
                    handler.UseProxy = true;
                    handler.Proxy = new BypassingWebProxy(systemProxy, proxySettings.BypassList);
                }

                if (proxySettings.UseDefaultCredentials)
                {
                    // These answer the proxy's 407 challenge. Handler.Credentials would instead
                    // answer 401s from the target servers themselves - offering the machine's
                    // credentials to any external host - while the authenticating proxy the user
                    // configured this for would still be refused.
                    handler.DefaultProxyCredentials = CredentialCache.DefaultNetworkCredentials;
                }

                return handler;
            }

            var proxy = new WebProxy(proxySettings.ProxyAddress);

            if (!proxySettings.UseDefaultCredentials && !string.IsNullOrEmpty(proxySettings.UserName))
            {
                // Credentials go on the proxy itself - a CredentialCache would need
                // ProxySettings.AuthType, which nothing ever sets, and
                // CredentialCache.Add throws on a null auth type.
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
