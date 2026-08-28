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

    private const string WindowsCudaUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-windows-x86_64-cuda.zip";
    private const string WindowsVulkanUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-windows-x86_64-vulkan.zip";
    private const string WindowsCpuUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-windows-x86_64-cpu.zip";
    private const string WindowsCpuLegacyUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-windows-x86_64-cpu-legacy.zip";
    private const string MacUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-macos.tar.gz";

    /// <summary>
    /// Intel Macs. Upstream's crispasr-macos.tar.gz is arm64-only - its build job runs on
    /// macos-latest, which is Apple Silicon since macos-13 was retired, so an Intel Mac gets
    /// "Bad CPU type in executable" after a 15 MB download, and Rosetta cannot bridge it
    /// (x86_64 -> arm64 only, never the reverse). Issue #13559. Until upstream ships an
    /// x86_64 or universal build, Subtitle Edit builds the x86_64 slice itself from the same
    /// pinned tag: SubtitleEdit/support-files, workflow build-crispasr-macos-x64-release.yml.
    /// It is a CPU + Accelerate build (ggml's Metal kernels crash on the AMD GPUs in Intel
    /// Macs) and targets macOS 12, and the archive's inner folder matches upstream's so the
    /// unpack path is shared.
    /// </summary>
    private const string MacIntelUrl = "https://github.com/SubtitleEdit/support-files/releases/download/crispasr-0830-macos-x64/crispasr-macos-x86_64.tar.gz";
    private const string LinuxUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-linux-x86_64.tar.gz";
    private const string LinuxCudaUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-linux-x86_64-cuda.tar.gz";
    private const string LinuxCuda13Url = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-linux-x86_64-cuda13.tar.gz";
    private const string LinuxVulkanUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-linux-x86_64-vulkan.tar.gz";
    private const string LinuxHipUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-linux-x86_64-hip.tar.gz";
    private const string LinuxArmUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.30/crispasr-linux-arm64.tar.gz";

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
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? MacUrl : MacIntelUrl;
        }

        throw new PlatformNotSupportedException();
    }
}