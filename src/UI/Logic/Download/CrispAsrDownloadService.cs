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

    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-184/parakeet-main-win64.zip";
    private const string MacArmUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-184/parakeet-main-mac-arm.zip";
    //private const string MacX64Url = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/parakeet-main-mac64.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/chat-llm-2026-2/parakeet-main-linux64.zip";

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
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    return MacArmUrl; // e.g., for M1, M2, M3, M4, M5 chips
                // case Architecture.X64:
                //     return MacX64Url;
                default:
                    throw new PlatformNotSupportedException("Unsupported macOS architecture.");
            }
        }
        throw new PlatformNotSupportedException();
    }
}