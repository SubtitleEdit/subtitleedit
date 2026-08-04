using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IWhisperDownloadService
{
    Task DownloadFile(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperCpp(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperCppCuBlas(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperConstMe(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperPurfviewFasterWhisperXxl(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperCppVulkan(Stream stream, Progress<float> progress, CancellationToken cancellationToken);
    Task DownloadWhisperCTranslate2(Stream stream, Progress<float> progress, CancellationToken cancellationToken);
    Task DownloadSileroVad(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class WhisperDownloadService : IWhisperDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsUrl = "https://github.com/ggml-org/whisper.cpp/releases/download/v1.9.1/whisper-blas-bin-x64.zip";
    private const string MacArmUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-191/whisper-mac.zip";
    private const string MacX64Url = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-191/whisper-mac.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-191/whisper-vulkan-linux64.zip";

    private const string WindowsUrlCuBlass = "https://github.com/ggml-org/whisper.cpp/releases/download/v1.9.1/whisper-cublas-12.4.0-bin-x64.zip";
    private const string WindowsUrlCppVulkan = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-191/whisper-vulkan-x64.zip";
    private const string LinuxUrlCuBlass = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-191/whisper-cuda-linux64.zip";
    
    private const string DownloadUrlConstMe = "https://github.com/Const-me/Whisper/releases/download/1.12.0/cli.zip";
    private const string SileroVadUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-184/ggml-silero-v6.2.0.zip";

    // smaller file for testing download: private const string DownloadUrlPurfviewFasterWhisperXxl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-182/whisper-cublas-11.8.0-bin-x64.7z";
    private const string DownloadUrlPurfviewFasterWhisperXxl = "https://github.com/Purfview/whisper-standalone-win/releases/download/Faster-Whisper-XXL/Faster-Whisper-XXL_r245.4_windows.7z";
    private const string DownloadUrlPurfviewFasterWhisperXxlLinux = "https://github.com/Purfview/whisper-standalone-win/releases/download/Faster-Whisper-XXL/Faster-Whisper-XXL_r245.4_linux.7z";

    private const string MacArmCTranslate2 = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/whisper-ctranslate2-mac.zip";
    private const string LinuxCTranslate2 = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/whisper-ctranslate2-Linux64.zip";
    private const string WindowCTranslate2 = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/whisper-ctranslate2-win64.zip";
    
    public WhisperDownloadService(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
    "(KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
        _httpClient = httpClient;
    }

    public async Task DownloadFile(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadWhisperCpp(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadWhisperCppCuBlas(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrlCuBlas(), stream, progress, cancellationToken);
    }

    public async Task DownloadWhisperConstMe(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, DownloadUrlConstMe, stream, progress, cancellationToken);
    }

    public async Task DownloadWhisperPurfviewFasterWhisperXxl(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = DownloadUrlPurfviewFasterWhisperXxl;

        if (OperatingSystem.IsLinux())
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                throw new PlatformNotSupportedException("Faster-Whisper-XXL is not available for Linux ARM64.");
            }

            url = DownloadUrlPurfviewFasterWhisperXxlLinux;
        }

        if (OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("MacOS not supported.");
        }

        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadWhisperCppVulkan(Stream stream,  Progress<float> progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrlCppVulkan(), stream, progress, cancellationToken);
    }
    
    public async Task DownloadWhisperCTranslate2(Stream stream,  Progress<float> progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrlTranslate2(), stream, progress, cancellationToken);
    }

    public async Task DownloadSileroVad(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, SileroVadUrl, stream, progress, cancellationToken);
    }

    private static string GetUrlTranslate2()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowCTranslate2;
        }
        
        if (OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return MacArmCTranslate2;
        }
        
        if (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return LinuxCTranslate2;
        }

        throw new PlatformNotSupportedException();
    }

    private static string GetUrlCppVulkan()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrlCppVulkan;
        }

        throw new PlatformNotSupportedException();
    }
    
    private static string GetUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrl;
        }

        if (OperatingSystem.IsLinux())
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                throw new PlatformNotSupportedException("whisper.cpp Vulkan build is not available for Linux ARM64.");
            }

            return LinuxUrl;
        }

        if (OperatingSystem.IsMacOS())
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    return MacArmUrl; // e.g., for M1, M2, M3, M4 chips
                case Architecture.X64:
                    return MacX64Url;
                default:
                    throw new PlatformNotSupportedException("Unsupported macOS architecture.");
            }
        }

        throw new PlatformNotSupportedException();
    }

    private static string GetUrlCuBlas()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrlCuBlass;
        }

        if (OperatingSystem.IsLinux())
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                throw new PlatformNotSupportedException("whisper.cpp CUDA build is not available for Linux ARM64.");
            }

            return LinuxUrlCuBlass;
        }

        throw new PlatformNotSupportedException();
    }
}