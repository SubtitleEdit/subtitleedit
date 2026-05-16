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
    Task DownloadEngineWindowsCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsVulkan(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsCpu(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsCpuLegacy(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class CrispAsrDownloadService : ICrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsCudaUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.6.6/crispasr-windows-x86_64-cuda.zip";
    private const string WindowsVulkanUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.6.6/crispasr-windows-x86_64-vulkan.zip";
    private const string WindowsCpuUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.6.6/crispasr-windows-x86_64-cpu.zip";
    private const string WindowsCpuLegacyUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.6.6/crispasr-windows-x86_64-cpu-legacy.zip";
    private const string MacUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.6.6/crispasr-macos.tar.gz";
    private const string LinuxUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.6.6/crispasr-linux-x86_64.tar.gz";
    private const string LinuxArmUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.6.6/crispasr-linux-arm64.tar.gz";

    public CrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadEngineWindowsCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsCudaUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineWindowsVulkan(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsVulkanUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineWindowsCpu(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsCpuUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineWindowsCpuLegacy(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsCpuLegacyUrl, stream, progress, cancellationToken);
    }

    private static string GetUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsVulkanUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LinuxArmUrl : LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacUrl; 
        }

        throw new PlatformNotSupportedException();
    }
}