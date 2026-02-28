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
}

public class WhisperDownloadService : IWhisperDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/whisper-blas-bin-x64.zip";
    private const string MacArmUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/WhisperCppMac.zip";
    private const string MacX64Url = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/WhisperCppMac.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/WhisperCppLinux64.zip";

    private const string WindowsUrlCuBlass = "https://github.com/ggml-org/whisper.cpp/releases/download/v1.8.3/whisper-cublas-12.4.0-bin-x64.zip";
    private const string WindowsUrlCppVulkan = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/WhisperCppWindowsVulcan.zip";
    private const string LinuxUrlCuBlass = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/WhisperCppCudaLinux64.zip";
    
    private const string DownloadUrlConstMe = "https://github.com/Const-me/Whisper/releases/download/1.12.0/cli.zip";

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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            url = DownloadUrlPurfviewFasterWhisperXxlLinux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

    private static string GetUrlTranslate2()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowCTranslate2;
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return MacArmCTranslate2;
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return LinuxCTranslate2;
        }

        throw new PlatformNotSupportedException();
    }

    private static string GetUrlCppVulkan()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsUrlCppVulkan;
        }

        throw new PlatformNotSupportedException();
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsUrlCuBlass;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxUrlCuBlass;
        }

        throw new PlatformNotSupportedException();
    }
}