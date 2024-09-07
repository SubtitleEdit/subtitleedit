using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.Http
{
    public interface IDownloader : IDisposable
    {
        Task DownloadAsync(string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default);
        Uri BaseAddress { get; set; }
        HttpRequestHeaders DefaultRequestHeaders { get; }
        Task<HttpResponseMessage> PostAsync(string uri, StringContent stringContent);
        Task<string> GetStringAsync(string url);
    }
}