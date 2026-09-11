using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.UiLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IIndexTts25AudioCppDownloadService
{
    /// <summary>Downloads the audio.cpp engine archive for this platform and backend.</summary>
    Task DownloadEngine(Stream stream, string backend, IProgress<float>? progress, CancellationToken cancellationToken);

    /// <summary>Downloads the selected IndexTTS-2.5 GGUF into audio.cpp's models folder.</summary>
    Task DownloadModels(string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);

    /// <summary>Backends with an archive for the current platform, best first.</summary>
    string[] GetAvailableBackends();
}

/// <summary>
/// Engine binaries come from SubtitleEdit's own support-files release (built from upstream
/// audio.cpp by .github/workflows/build-audiocpp-indextts-release.yml, since upstream ships
/// Windows prebuilts only). Model weights come straight from HuggingFace and are never
/// redistributed by us — they carry the bilibili Model Use License, which the user accepts in
/// <see cref="Features.Video.TextToSpeech.IndexTts25License.IndexTts25LicenseWindow"/> first.
/// </summary>
public class IndexTts25AudioCppDownloadService : IIndexTts25AudioCppDownloadService
{
    private readonly HttpClient _httpClient;

    // 2026-09-06: upstream main @ b0757573, compiled with the index_tts2 + higgs_audio_tts +
    // fish_audio + fireredtts3 families — the same archives back all four audio.cpp engines.
    // The new family is the reason for the bump: FireRedTTS3 is only compiled into this build.
    private const string ReleaseTag = "audiocpp-indextts25-2026-09-06";
    private const string ReleaseBase =
        "https://github.com/SubtitleEdit/support-files/releases/download/" + ReleaseTag + "/";

    public const string BackendMetal = "metal";
    public const string BackendCuda = "cuda";
    public const string BackendVulkan = "vulkan";
    public const string BackendCpu = "cpu";

    private static readonly Dictionary<string, string> WindowsArchives = new(StringComparer.OrdinalIgnoreCase)
    {
        [BackendCuda] = "audiocpp-indextts25-windows-x86_64-cuda.zip",
        [BackendVulkan] = "audiocpp-indextts25-windows-x86_64-vulkan.zip",
        [BackendCpu] = "audiocpp-indextts25-windows-x86_64-cpu.zip",
    };

    private static readonly Dictionary<string, string> LinuxArchives = new(StringComparer.OrdinalIgnoreCase)
    {
        [BackendCuda] = "audiocpp-indextts25-linux-x86_64-cuda.tar.gz",
        [BackendVulkan] = "audiocpp-indextts25-linux-x86_64-vulkan.tar.gz",
        [BackendCpu] = "audiocpp-indextts25-linux-x86_64.tar.gz",
    };

    private const string MacArchive = "audiocpp-indextts25-macos-arm64.tar.gz";

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [IndexTts25AudioCpp.ModelQ8_0FileName] =
            "https://huggingface.co/audio-cpp/audio.cpp-gguf/resolve/main/IndexTTS2.5-GGUF/index-tts2_5-q8_0.gguf",
        [IndexTts25AudioCpp.ModelF16FileName] =
            "https://huggingface.co/audio-cpp/audio.cpp-gguf/resolve/main/IndexTTS2.5-GGUF/index-tts2_5-f16.gguf",
    };

    public IndexTts25AudioCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Backends that have an archive for this platform, best first. macOS is Metal-only (the
    /// archive is arm64; Intel Macs are not covered). Vulkan and CUDA archives need a driver
    /// present — the binaries import their GPU runtime at load time and die with 0xC0000135
    /// when it is missing — so CPU is always offered as the fallback.
    /// </summary>
    public string[] GetAvailableBackends()
    {
        if (OperatingSystem.IsMacOS())
        {
            return new[] { BackendMetal };
        }

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            return new[] { BackendCuda, BackendVulkan, BackendCpu };
        }

        return Array.Empty<string>();
    }

    public async Task DownloadEngine(Stream stream, string backend, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetEngineUrl(backend), stream, progress, cancellationToken);
    }

    private static string GetEngineUrl(string backend)
    {
        if (OperatingSystem.IsMacOS())
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            {
                throw new PlatformNotSupportedException(
                    "IndexTTS 2.5 (audio.cpp) requires an Apple Silicon Mac.");
            }

            return ReleaseBase + MacArchive;
        }

        var archives = OperatingSystem.IsWindows() ? WindowsArchives
            : OperatingSystem.IsLinux() ? LinuxArchives
            : throw new PlatformNotSupportedException();

        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException(
                "IndexTTS 2.5 (audio.cpp) is only built for x86-64 on Windows and Linux.");
        }

        if (!archives.TryGetValue(backend, out var archive))
        {
            archive = archives[BackendCpu];
        }

        return ReleaseBase + archive;
    }

    public async Task DownloadModels(string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var fileName = IndexTts25AudioCpp.GetModelFileName(modelKey);
        var finalPath = IndexTts25AudioCpp.GetModelPath(modelKey);

        // A partial GGUF is worse than none: audio.cpp rejects it with "GGUF tensor data range
        // is out of bounds", which reads like a corrupt model rather than a short download.
        // Size-check on the threadpool — this runs from the download dialog's init callback.
        await Task.Run(() => EnsureRemovedIfInvalid(finalPath, fileName), cancellationToken);

        if (IndexTts25AudioCpp.IsValidLocalModelFile(finalPath, fileName))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading IndexTTS 2.5 model: {fileName}");

        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, GetModelUrl(fileName), partPath, progress, cancellationToken);
            await VerifyFile(partPath, GetHashKey(fileName), fileName, cancellationToken);
            File.Move(partPath, finalPath);
        }
        catch
        {
            TryDelete(partPath);
            throw;
        }
    }

    private static string GetModelUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown IndexTTS 2.5 model: {fileName}", nameof(fileName));
        }

        return url;
    }

    private static string? GetHashKey(string fileName)
    {
        if (string.Equals(fileName, IndexTts25AudioCpp.ModelQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.IndexTts25AudioCpp.ModelQ8_0;
        }

        if (string.Equals(fileName, IndexTts25AudioCpp.ModelF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.IndexTts25AudioCpp.ModelF16;
        }

        return null;
    }

    private static async Task VerifyFile(string filePath, string? key, string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        var expected = DownloadHashManager.GetLatestKnownHash(key);
        if (string.IsNullOrEmpty(expected))
        {
            return;
        }

        string actual;
        await using (var stream = File.OpenRead(filePath))
        {
            actual = await Sha256Util.ComputeSha256Async(stream, cancellationToken);
        }

        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(
                $"IndexTTS 2.5 model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || IndexTts25AudioCpp.IsValidLocalModelFile(path, fileName))
        {
            return;
        }

        TryDelete(path);
    }
}
