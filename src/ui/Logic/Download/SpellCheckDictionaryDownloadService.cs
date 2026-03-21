using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ISpellCheckDictionaryDownloadService
{
    Task DownloadDictionary(Stream stream, string url, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class SpellCheckDictionaryDownloadService : ISpellCheckDictionaryDownloadService
{
    private readonly HttpClient _httpClient;

    public SpellCheckDictionaryDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadDictionary(Stream stream, string url, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
    }
}