using System;
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
                // Wrap the system/environment proxy so loopback urls (localhost, 127.x, [::1])
                // always connect directly - .NET Framework did this implicitly, modern .NET does not
#if NETSTANDARD
                var systemProxy = WebRequest.DefaultWebProxy;
#else
                var systemProxy = HttpClient.DefaultProxy;
#endif
                if (systemProxy == null)
                {
                    return new HttpClient();
                }

                var defaultHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new LoopbackBypassingProxy(systemProxy),
                };

                return new HttpClient(defaultHandler);
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
            handler.Proxy = new LoopbackBypassingProxy(proxy);

            return new HttpClient(handler);
        }

        private sealed class LoopbackBypassingProxy : IWebProxy
        {
            private readonly IWebProxy _inner;

            public LoopbackBypassingProxy(IWebProxy inner)
            {
                _inner = inner;
            }

            public ICredentials Credentials
            {
                get => _inner.Credentials;
                set => _inner.Credentials = value;
            }

            public Uri GetProxy(Uri destination) => IsBypassed(destination) ? null : _inner.GetProxy(destination);

            public bool IsBypassed(Uri host) => host.IsLoopback || _inner.IsBypassed(host);
        }
    }
}
