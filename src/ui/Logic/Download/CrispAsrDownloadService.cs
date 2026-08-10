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
    Task DownloadEngineLinuxCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineLinuxCuda13(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineLinuxVulkan(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineLinuxHip(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class CrispAsrDownloadService : ICrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsCudaUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-windows-x86_64-cuda.zip";
    private const string WindowsVulkanUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-windows-x86_64-vulkan.zip";
    private const string WindowsCpuUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-windows-x86_64-cpu.zip";
    private const string WindowsCpuLegacyUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-windows-x86_64-cpu-legacy.zip";
    private const string MacUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-macos.tar.gz";
    private const string LinuxUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-linux-x86_64.tar.gz";
    private const string LinuxCudaUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-linux-x86_64-cuda.tar.gz";
    private const string LinuxCuda13Url = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-linux-x86_64-cuda13.tar.gz";
    private const string LinuxVulkanUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-linux-x86_64-vulkan.tar.gz";

    // ROCm is deliberately a release behind. v0.8.26 published only one of its seven Linux
    // tarballs, and v0.8.27 - the repair release - brought back six of them but not this one:
    // ROCm's clang links OpenMP against LLVM's libomp.so, reachable only through a RUNPATH that
    // the bundling script erased before it asked ldd what to copy, so the dependency check
    // rejects the staged directory (CrispStrobe/CrispASR#339). v0.8.25 is therefore the newest
    // release that has a crispasr-linux-x86_64-hip.tar.gz at all; pointing this at v0.8.27 would
    // 404 for every AMD user. Move it up with the rest once upstream ships a HIP build again.
    private const string LinuxHipUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.25/crispasr-linux-x86_64-hip.tar.gz";

    private const string LinuxArmUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.27/crispasr-linux-arm64.tar.gz";

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

    public async Task DownloadEngineLinuxCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, LinuxCudaUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineLinuxCuda13(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, LinuxCuda13Url, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineLinuxVulkan(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, LinuxVulkanUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineLinuxHip(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, LinuxHipUrl, stream, progress, cancellationToken);
    }

    private static string GetUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsVulkanUrl;
        }

        if (OperatingSystem.IsLinux())
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LinuxArmUrl : LinuxUrl;
        }

        if (OperatingSystem.IsMacOS())
        {
            return MacUrl; 
        }

        throw new PlatformNotSupportedException();
    }
}