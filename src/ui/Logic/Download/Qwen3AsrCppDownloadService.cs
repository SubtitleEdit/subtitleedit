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

    // Built by https://github.com/niksedk/qwen3-asr.cpp (.github/workflows/release.yml) — these
    // are the binaries with the JSON word-truncation fix (issue #11717).
    private const string ReleaseUrlBase = "https://github.com/niksedk/qwen3-asr.cpp/releases/download/v0.1.1/";
    private const string WindowsUrl = ReleaseUrlBase + "qwen3-asr-cpp-win64.zip";
    private const string MacArmUrl = ReleaseUrlBase + "qwen3-asr-cpp-mac.zip";
    private const string MacX64Url = ReleaseUrlBase + "qwen3-asr-cpp-mac-x64.zip";
    private const string LinuxUrl = ReleaseUrlBase + "qwen3-asr-cpp-linux.zip";
    private const string LinuxArm64Url = ReleaseUrlBase + "qwen3-asr-cpp-linux-arm64.zip";

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
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LinuxArm64Url : LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? MacArmUrl : MacX64Url;
        }

        throw new PlatformNotSupportedException();
    }
}