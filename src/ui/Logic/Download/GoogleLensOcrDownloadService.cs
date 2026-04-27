using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IGoogleLensOcrDownloadService
{
    Task DownloadGoogleLensOcrStandalone(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadGoogleLensOcrStandalone(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class GoogleLensOcrDownloadService(HttpClient httpClient) : IGoogleLensOcrDownloadService
{
    //private const string WindowsUrl = "https://github.com/timminator/chrome-lens-py/releases/download/v3.3.0/Chrome-Lens-CLI-v3.3.0.7z";
    private const string WindowsUrl = "https://github.com/timminator/Chrome-Lens-OCR/releases/download/v3.4.0/Chrome-Lens-OCR-v3.4.0.7z";
    private const string LinuxUrl = "https://github.com/timminator/Chrome-Lens-OCR/releases/download/v3.4.0/Chrome-Lens-OCR-v3.4.0-Linux.7z";

    public async Task DownloadGoogleLensOcrStandalone(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadGoogleLensOcrStandalone(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    private string GetUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrl;
        }

        if (OperatingSystem.IsLinux())
        {
            return LinuxUrl;
        }

        throw new PlatformNotSupportedException("Google Lens OCR does not support this platform");
    }
}