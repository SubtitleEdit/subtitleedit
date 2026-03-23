using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.Http
{
    public class HttpClientDownloader : IDownloader
    {
        private readonly HttpClient _httpClient;

        public HttpClientDownloader(HttpClient httpClient) => _httpClient = httpClient;

        public Uri BaseAddress
        {
            get => _httpClient.BaseAddress;
            set => _httpClient.BaseAddress = value;
        }

        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        public Task<HttpResponseMessage> PostAsync(string uri, StringContent stringContent)
        {
            return _httpClient.PostAsync(uri, stringContent);
        }

        public Task<string> GetStringAsync(string url)
        {
            var response = _httpClient.GetByteArrayAsync(url).Result;
            return Task.FromResult(Encoding.UTF8.GetString(response, 0, response.Length));
        }

        public async Task DownloadAsync(string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _httpClient.Timeout = Timeout.InfiniteTimeSpan;
                using (var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
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
                SeLogger.Error(e, $"DownloadAsync failed - {requestUri}");

                if (Environment.OSVersion.Version.Major < 10)
                {
                    SeLogger.Error("A newer OS might be needed!");
                }

                throw;
            }
        }

        public static async Task CopyToAsync(Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default)
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

        public void Dispose() => _httpClient?.Dispose();
    }
}