using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ICrispAsrDownloadService
{
    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsCuda13(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
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

    private const string WindowsCudaUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-windows-x86_64-cuda.zip";
    /// <summary>
    /// The CUDA 13 build, added upstream in v0.8.31 next to the CUDA 12 one rather than
    /// replacing it. Offered as its own option because CUDA 13 needs a newer NVIDIA driver than
    /// CUDA 12 - repointing <see cref="WindowsCudaUrl"/> at it would have broken everyone still
    /// on an older driver. Mirrors the Linux pair, which has had both since v0.8.30.
    /// </summary>
    private const string WindowsCuda13Url = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-windows-x86_64-cuda13.zip";
    private const string WindowsVulkanUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-windows-x86_64-vulkan.zip";
    private const string WindowsCpuUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-windows-x86_64-cpu.zip";
    private const string WindowsCpuLegacyUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-windows-x86_64-cpu-legacy.zip";
    private const string MacUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-macos.tar.gz";

    /// <summary>
    /// Intel Macs. Upstream's crispasr-macos.tar.gz is arm64-only (issue #13559: "Bad CPU type
    /// in executable"), so up to v0.8.38 Subtitle Edit built the x86_64 slice itself in
    /// SubtitleEdit/support-files. From v0.8.39 upstream ships an official CPU + Accelerate
    /// Intel build (Metal disabled). v0.8.39's was SSE2-only and 2.3x slower than the old slice
    /// (issue #15514, CrispASR #484); from v0.8.40 it is built with AVX2 + FMA + F16C, and
    /// CPUs without those get <see cref="MacIntelLegacyUrl"/> instead - see
    /// <see cref="MacIntelUseLegacy"/>. Its inner folder is crispasr-macos-x86_64, not
    /// crispasr-macos - see <see cref="MacUnpackFolder"/>.
    /// </summary>
    private const string MacIntelUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-macos-x86_64.tar.gz";

    /// <summary>
    /// Intel Macs without AVX2/FMA (pre-Haswell, i.e. 2012 and older models) and the x64 build
    /// under Rosetta, where
    /// <see cref="MacIntelUrl"/> would die with an illegal instruction.
    /// </summary>
    private const string MacIntelLegacyUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-macos-x86_64-cpu-legacy.tar.gz";

    /// <summary>
    /// True when an x64 process on macOS should get the portable legacy Intel build. Upstream's
    /// standard Intel build needs AVX2, FMA and F16C; .NET has no F16C probe, but every CPU with
    /// AVX2 + FMA (Haswell / Excavator and newer) also has F16C. Rosetta exposes no AVX at all
    /// (checked on macOS 26), so the x64 build on Apple Silicon lands here too - the standard
    /// build refuses to start there ("this CPU lacks: AVX2, FMA, F16C, AVX").
    /// </summary>
    public static bool MacIntelUseLegacy =>
        OperatingSystem.IsMacOS()
        && RuntimeInformation.ProcessArchitecture == Architecture.X64
        && !(Avx2.IsSupported && Fma.IsSupported);

    /// <summary>
    /// The folder inside the macOS archive that <see cref="DownloadEngine"/> fetches for this
    /// process architecture. crispasr-macos.tar.gz (arm64) keeps its original crispasr-macos
    /// folder as a compatibility alias; the Intel archives use their own names.
    /// </summary>
    public static string MacUnpackFolder =>
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? "crispasr-macos"
            : MacIntelUseLegacy ? "crispasr-macos-x86_64-cpu-legacy" : "crispasr-macos-x86_64";
    private const string LinuxUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-linux-x86_64.tar.gz";
    private const string LinuxCudaUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-linux-x86_64-cuda.tar.gz";
    private const string LinuxCuda13Url = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-linux-x86_64-cuda13.tar.gz";
    private const string LinuxVulkanUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-linux-x86_64-vulkan.tar.gz";
    private const string LinuxHipUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-linux-x86_64-hip.tar.gz";
    private const string LinuxArmUrl = "https://github.com/CrispStrobe/CrispASR/releases/download/v0.8.41/crispasr-linux-arm64.tar.gz";

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

    public async Task DownloadEngineWindowsCuda13(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsCuda13Url, stream, progress, cancellationToken);
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
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return MacUrl;
            }

            return MacIntelUseLegacy ? MacIntelLegacyUrl : MacIntelUrl;
        }

        throw new PlatformNotSupportedException();
    }
}