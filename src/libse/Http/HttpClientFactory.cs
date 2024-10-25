﻿using System;
using System.Net;
using System.Net.Http;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.Core.Http
{
    public static class HttpClientFactory
    {
        public static IDownloader CreateHttpClientDownloader()
        {
            var httpClient = CreateProxiedHttpClient();
            if (Configuration.Settings.General.UseLegacyDownloader)
            {
                return new LegacyDownloader(httpClient);
            }

            return new HttpClientDownloader(httpClient);
        }

        public static HttpClient CreateProxiedHttpClient()
        {
            return new HttpClient(CreateHandler(Configuration.Settings.Proxy));
        }

        private static HttpClientHandler CreateHandler(ProxySettings proxySettings)
        {
            var handler = new HttpClientHandler();

            if (!string.IsNullOrEmpty(proxySettings.ProxyAddress))
            {
                handler.Proxy = new WebProxy(proxySettings.ProxyAddress);
                handler.UseProxy = true;
            }

            if (proxySettings.UseDefaultCredentials)
            {
                handler.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                handler.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else if (!string.IsNullOrEmpty(proxySettings.UserName) && !string.IsNullOrEmpty(proxySettings.ProxyAddress))
            {
                var networkCredential = string.IsNullOrWhiteSpace(proxySettings.Domain) ? new NetworkCredential(proxySettings.UserName, proxySettings.Password) : new NetworkCredential(proxySettings.UserName, proxySettings.Password, proxySettings.Domain);
                var credentialCache = new CredentialCache
                {
                    {
                        new Uri(proxySettings.ProxyAddress),
                        proxySettings.AuthType,
                        networkCredential
                    }
                };
                handler.Credentials = credentialCache;
            }

            return handler;
        }
    }
}
