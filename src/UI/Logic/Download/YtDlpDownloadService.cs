using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IYtDlpDownloadService
{
    Task DownloadYtDlp(IProgress<float>? progress, CancellationToken cancellationToken);
}

public class YtDlpDownloadService : IYtDlpDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/2025.11.12/yt-dlp.exe";
    private const string LinuxUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/2025.11.12/yt-dlp_linux";
    private const string LinuxAarch64Url = "https://github.com/yt-dlp/yt-dlp/releases/download/2025.11.12/yt-dlp_linux_aarch64.zip";
    private const string MacUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/2025.11.12/yt-dlp_macos";
    private const string MacUrlArm = "https://github.com/yt-dlp/yt-dlp/releases/download/2025.11.12/yt-dlp_macos";

    public YtDlpDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static string GetFullFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Se.DataFolder, "yt-dlp.exe");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(Se.DataFolder, "yt-dlp_linux");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(Se.DataFolder, "yt-dlp_macos");
        }

        throw new PlatformNotSupportedException("Unsupported OS platform");
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
            return MacUrlArm;
        }

        throw new PlatformNotSupportedException("Unsupported OS platform");
    }

    public async Task DownloadYtDlp(IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var path = Path.Combine(GetFullFileName());
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), path, progress, cancellationToken);
    }
}