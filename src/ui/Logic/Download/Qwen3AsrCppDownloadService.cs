using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IQwen3AsrCppDownloadService
{
    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class Qwen3AsrCppDownloadService : IQwen3AsrCppDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-asr-cpp-2026-3/qwen3-asr-cpp-win64.zip";
    private const string MacArmUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-asr-cpp-2026-3/qwen3-asr-cpp-mac.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-asr-cpp-2026-3/qwen3-asr-cpp-linux.zip";

    public Qwen3AsrCppDownloadService(HttpClient httpClient)
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
            return MacArmUrl;
        }

        throw new PlatformNotSupportedException();
    }
}