using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IOmniVoiceDownloadService
{
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);

    Task DownloadEngine(Stream stream, string windowsVariant, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class OmniVoiceDownloadService : IOmniVoiceDownloadService
{
    private readonly HttpClient _httpClient;

    public const string ModelBaseFileName = "omnivoice-base-Q8_0.gguf";
    public const string ModelTokenizerFileName = "omnivoice-tokenizer-F32.gguf";

    public const string WindowsVariantCpu = "cpu";
    public const string WindowsVariantVulkan = "vulkan";
    public const string WindowsVariantCuda = "cuda";

    private const string ModelBaseUrl = "https://huggingface.co/Serveurperso/OmniVoice-GGUF/resolve/main/omnivoice-base-Q8_0.gguf";
    private const string ModelTokenizerUrl = "https://huggingface.co/Serveurperso/OmniVoice-GGUF/resolve/main/omnivoice-tokenizer-F32.gguf";

    // omnivoice.cpp release pin. Bump in lockstep with the hashes in DownloadHashManager.OmniVoice
    // (each new release: prepend the new SHA-256 at index 0, keep the previous one for "update available").
    public const string ReleaseTag = "omnivoice-2026-05-22";
    private const string ReleaseUrlBase = "https://github.com/niksedk/omnivoice.cpp/releases/download/" + ReleaseTag + "/";

    private const string WindowsCpuUrl = ReleaseUrlBase + "omnivoice-win64-cpu.zip";
    private const string WindowsVulkanUrl = ReleaseUrlBase + "omnivoice-win64-vulkan.zip";
    private const string WindowsCudaUrl = ReleaseUrlBase + "omnivoice-win64-cuda.zip";
    private const string MacOsUrl = ReleaseUrlBase + "omnivoice-macos-universal-cpu-metal.zip";
    private const string LinuxX64Url = ReleaseUrlBase + "omnivoice-linux-x64-cpu.zip";
    private const string LinuxArm64Url = ReleaseUrlBase + "omnivoice-linux-arm64-cpu.zip";

    private const string VoicesUrl = ReleaseUrlBase + "OmniVoices.zip";

    public OmniVoiceDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(modelsFolder, ModelBaseFileName);
        var tokenizerPath = Path.Combine(modelsFolder, ModelTokenizerFileName);
        var needBase = !File.Exists(basePath);
        var needTokenizer = !File.Exists(tokenizerPath);
        var total = (needBase ? 1 : 0) + (needTokenizer ? 1 : 0);
        var step = 0;

        if (needBase)
        {
            step++;
            titleProgress?.Invoke($"Downloading OmniVoice TTS models ({step}/{total}): {ModelBaseFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, ModelBaseUrl, basePath, progress, cancellationToken);
        }
        if (needTokenizer)
        {
            step++;
            titleProgress?.Invoke($"Downloading OmniVoice TTS models ({step}/{total}): {ModelTokenizerFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, ModelTokenizerUrl, tokenizerPath, progress, cancellationToken);
        }
    }

    public async Task DownloadEngine(Stream stream, string windowsVariant, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(windowsVariant), stream, progress, cancellationToken);
        VerifyArchive(stream, DownloadHashManager.ResolveOmniVoiceKey(windowsVariant), "engine");
    }

    public async Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, VoicesUrl, stream, progress, cancellationToken);
        VerifyArchive(stream, DownloadHashManager.OmniVoice.Voices, "voices");
    }

    // Compares the downloaded bytes against the known SHA-256 for this key and throws on mismatch
    // so the caller's IsFaulted branch surfaces "Download failed" instead of silently unpacking a
    // truncated or tampered file. A null/unknown key (e.g. unrecognised Windows variant) skips the
    // check rather than failing closed - same policy as the rest of DownloadHashManager.
    private static void VerifyArchive(Stream stream, string? key, string label)
    {
        if (string.IsNullOrEmpty(key) || stream.Length == 0)
        {
            return;
        }

        var expected = DownloadHashManager.GetLatestKnownHash(key);
        if (string.IsNullOrEmpty(expected))
        {
            return;
        }

        stream.Position = 0;
        var actual = DownloadHashManager.ComputeSha256(stream);
        stream.Position = 0;

        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(
                $"OmniVoice {label} download failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string windowsVariant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windowsVariant switch
            {
                WindowsVariantCuda => WindowsCudaUrl,
                WindowsVariantVulkan => WindowsVulkanUrl,
                _ => WindowsCpuUrl,
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOsUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? LinuxArm64Url
                : LinuxX64Url;
        }

        throw new PlatformNotSupportedException();
    }
}
