using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ICrispAsrDownloadService
{
    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class CrispAsrDownloadService : ICrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/crisp-asr-2026-04/crisp-asr-win64.zip";
    private const string MacUrl = "https://github.com/SubtitleEdit/support-files/releases/download/crisp-asr-2026-04/crisp-asr-mac.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/crisp-asr-2026-04/crisp-asr-linux64.zip";

    public CrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    private static string GetUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacUrl; 
        }

        throw new PlatformNotSupportedException();
    }
}