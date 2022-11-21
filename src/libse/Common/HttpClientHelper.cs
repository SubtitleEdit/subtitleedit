using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class HttpClientHelper
    {
        public static HttpClient MakeHttpClient()
        {
            return new HttpClient(GetHttpClientHandler(Configuration.Settings.Proxy));
        }

        public static HttpClientHandler GetHttpClientHandler(ProxySettings proxySettings)
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


        public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    var contentLength = response.Content.Headers.ContentLength;

                    using (var downloadStream = await response.Content.ReadAsStreamAsync())
                    {
                        if (progress == null || !contentLength.HasValue)
                        {
                            await downloadStream.CopyToAsync(destination);
                            return;
                        }

                        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                        var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                        // Use extension method to report progress while downloading
                        await CopyToAsync(downloadStream, destination, 81920, relativeProgress, cancellationToken);
                        progress.Report(1);
                    }
                }
            }
            catch (Exception e)
            {
                SeLogger.Error(e, "DownloadAsync failed");
                throw;
            }
        }

        private static async Task CopyToAsync(Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}
