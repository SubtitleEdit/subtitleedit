using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IQwen3AsrCppDownloadService
{
    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken, bool useVulkan = false);
}

public class Qwen3AsrCppDownloadService : IQwen3AsrCppDownloadService
{
    private readonly HttpClient _httpClient;

    // Built by https://github.com/niksedk/qwen3-asr.cpp (.github/workflows/release.yml) — these
    // are the binaries with the JSON word-truncation fix (issue #11717) plus the valid-JSON fix
    // for non-finite/oversized timestamps (issue #11375), on the ggml 0.15.3 backend. v0.1.6
    // merges upstream (mixed-CJK tokenization fix, multilingual alignment encoding, tokenizer-
    // config GGUF vocab, thread-count propagation). CPU for every platform, plus Vulkan (GPU)
    // for win64 and linux-x64. When bumping: also prepend the new archive + executable hashes
    // in DownloadHashManager (Qwen3AsrCpp keys) so the update prompt fires for old installs.
    private const string ReleaseUrlBase = "https://github.com/niksedk/qwen3-asr.cpp/releases/download/v0.1.6/";
    private const string WindowsUrl = ReleaseUrlBase + "qwen3-asr-cpp-win64.zip";
    private const string WindowsVulkanUrl = ReleaseUrlBase + "qwen3-asr-cpp-win64-vulkan.zip";
    private const string MacArmUrl = ReleaseUrlBase + "qwen3-asr-cpp-mac.zip";
    private const string MacX64Url = ReleaseUrlBase + "qwen3-asr-cpp-mac-x64.zip";
    private const string LinuxUrl = ReleaseUrlBase + "qwen3-asr-cpp-linux.zip";
    private const string LinuxVulkanUrl = ReleaseUrlBase + "qwen3-asr-cpp-linux-vulkan.zip";
    private const string LinuxArm64Url = ReleaseUrlBase + "qwen3-asr-cpp-linux-arm64.zip";

    public Qwen3AsrCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// True when a Vulkan (GPU) build exists for the current platform — win64 and linux-x64.
    /// Used to decide whether to offer the user a CPU/GPU choice before downloading.
    /// </summary>
    public static bool IsVulkanBuildAvailable()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return false;
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken, bool useVulkan = false)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(useVulkan), stream, progress, cancellationToken);
    }

    private static string GetUrl(bool useVulkan)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return useVulkan ? WindowsVulkanUrl : WindowsUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return LinuxArm64Url; // no Vulkan build for ARM64
            }

            return useVulkan ? LinuxVulkanUrl : LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? MacArmUrl : MacX64Url;
        }

        throw new PlatformNotSupportedException();
    }
}
