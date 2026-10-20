using System.Net.Http.Headers;

namespace Nikse.SubtitleEdit.UiLogic.Http
{
    public interface IDownloader : IDisposable
    {
        Task DownloadAsync(string requestUri, Stream destination, IProgress<float>? progress = null, CancellationToken cancellationToken = default);
        Uri? BaseAddress { get; set; }
        HttpRequestHeaders DefaultRequestHeaders { get; }
        Task<HttpResponseMessage> PostAsync(string uri, StringContent stringContent, CancellationToken cancellationToken = default);
        Task<string> GetStringAsync(string url);
    }
}