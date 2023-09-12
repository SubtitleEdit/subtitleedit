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
    public class HttpClientAdapter : IHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly Encoding _encoding;

        public HttpClientAdapter(HttpClient httpClient) : this(httpClient, Encoding.UTF8)
        {
        }

        public HttpClientAdapter(HttpClient httpClient, Encoding encoding)
        {
            _httpClient = httpClient;
            // fall back wil always be UTF-8
            _encoding = encoding ?? Encoding.UTF8;
        }

        public Uri BaseAddress
        {
            get => _httpClient.BaseAddress;
            set => _httpClient.BaseAddress = value;
        }

        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        public Task<HttpResponseMessage> PostAsync(string uri, StringContent stringContent) => _httpClient.PostAsync(uri, stringContent);

        public async Task<string> GetStringAsync(string url)
        {
            var buffer = await _httpClient.GetByteArrayAsync(url);
            return _encoding.GetString(buffer, 0, buffer.Length);
            // return Task.FromResult(_encoding.GetString(buffer, 0, buffer.Length - 1));
        }

        public async Task DownloadAsync(string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _httpClient.Timeout = Timeout.InfiniteTimeSpan;
                using (var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                using (var downloadStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var contentLength = response.Content.Headers.ContentLength;
                    if (progress == null || !contentLength.HasValue)
                    {
                        await downloadStream.CopyToAsync(destination).ConfigureAwait(false);
                    }
                    else
                    {
                        await downloadStream.CopyToAsync(destination, new StreamCopyContext(81920, contentLength.Value), progress, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                SeLogger.Error(e, $"DownloadAsync failed - {requestUri}");

                if (Environment.OSVersion.Version.Major < 10)
                {
                    Configuration.Settings.General.UseLegacyDownloader = true;
                    SeLogger.Error("Switching to legacy downloader due to old OS!");
                }

                throw;
            }
        }
        
        public void Dispose() => _httpClient?.Dispose();
    }
}