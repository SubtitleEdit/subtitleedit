using System.Net.Http;
using System.Net;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class HttpClientFactoryWithProxy
    {
        public static HttpClient CreateHttpClientWithProxy()
        {

            var proxyAddress = Configuration.Settings.Proxy.ProxyAddress;
            if (string.IsNullOrEmpty(proxyAddress))
            {
                return new HttpClient();
            }

            var handler = new HttpClientHandler();
            var proxy = new WebProxy(proxyAddress);

            var userName = Configuration.Settings.Proxy.UserName;
            var password = Configuration.Settings.Proxy.DecodePassword();
            var domain = Configuration.Settings.Proxy.Domain;

            if (!string.IsNullOrEmpty(userName))
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
            handler.Proxy = proxy;

            return new HttpClient(handler);
        }
    }
}