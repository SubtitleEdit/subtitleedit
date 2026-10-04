using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IFfmpegDownloadService
{
    Task DownloadFfmpeg(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadFfmpeg(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class FfmpegDownloadService : IFfmpegDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v9-2/ffmpeg902.zip";

    // Intel is still 8.0: osxexperts.net, where both macOS builds come from, has not published an
    // Intel build past 8.0.
    private const string MacUrl = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v8/ffmpeg80intel.zip";
    private const string MacUrlArm = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v9-1/ffmpeg90arm.zip";
    
    public FfmpegDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static string GetFfmpegUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrl;
        }

        if (OperatingSystem.IsMacOS())
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    return MacUrlArm; // e.g., for M1, M2, M3, M4 chips
                case Architecture.X64:
                    return MacUrl;
                default:
                    throw new PlatformNotSupportedException("Unsupported macOS architecture.");
            }
        }

        throw new PlatformNotSupportedException();
    }

    /// <summary>The <see cref="DownloadHashManager.Ffmpeg"/> key matching <see cref="GetFfmpegUrl"/>.</summary>
    internal static string? GetHashKey(string url)
    {
        return url switch
        {
            WindowsUrl => DownloadHashManager.Ffmpeg.Windows,
            MacUrl => DownloadHashManager.Ffmpeg.MacOsX64,
            MacUrlArm => DownloadHashManager.Ffmpeg.MacOsArm64,
            _ => null,
        };
    }

    internal static string[] AllUrls => [WindowsUrl, MacUrl, MacUrlArm];

    public async Task DownloadFfmpeg(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = GetFfmpegUrl();
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
        await DownloadHashManager.VerifyDownloadAsync(destinationFileName, GetHashKey(url), "ffmpeg", cancellationToken);
    }

    public async Task DownloadFfmpeg(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = GetFfmpegUrl();
        await DownloadHelper.DownloadFileAsync(_httpClient, url, stream, progress, cancellationToken);
        await DownloadHashManager.VerifyDownloadAsync(stream, GetHashKey(url), "ffmpeg", cancellationToken);
    }
}