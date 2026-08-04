using System.Net.Http;
using System.Net;

using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Http
{
    public static class HttpClientFactoryWithProxy
    {
        public static HttpClient CreateHttpClientWithProxy()
        {
            var proxySettings = Configuration.Settings.Proxy;
            var proxyAddress = proxySettings.ProxyAddress;
            if (string.IsNullOrEmpty(proxyAddress))
            {
                var systemProxy = HttpClient.DefaultProxy;
                if (systemProxy == null)
                {
                    return new HttpClient();
                }

                var defaultHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new BypassingWebProxy(systemProxy, proxySettings.BypassList),
                };

                return new HttpClient(defaultHandler);
            }

            var handler = new HttpClientHandler();
            var proxy = new WebProxy(proxyAddress);

            var userName = proxySettings.UserName;
            var password = proxySettings.DecodePassword();
            var domain = proxySettings.Domain;

            if (!proxySettings.UseDefaultCredentials && !string.IsNullOrEmpty(userName))
            {
                proxy.Credentials = string.IsNullOrEmpty(domain)
                    ? new NetworkCredential(userName, password)
                    : new NetworkCredential(userName, password, domain);
            }
            else
            {
                proxy.UseDefaultCredentials = true;
            }

            handler.UseProxy = true;
            handler.Proxy = new BypassingWebProxy(proxy, proxySettings.BypassList);

            return new HttpClient(handler);
        }
    }
}
