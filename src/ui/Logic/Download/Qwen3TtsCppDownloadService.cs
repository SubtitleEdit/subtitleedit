using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IQwen3TtsCppDownloadService
{
    Task DownloadEngine(Stream stream, string windowsVariant, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadModels(string modelsFolder, string ttsModelFileName, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

public class Qwen3TtsCppDownloadService : IQwen3TtsCppDownloadService
{
    private readonly HttpClient _httpClient;

    public const string WindowsVariantVulkan = "vulkan";
    public const string WindowsVariantCpu = "cpu";
    public const string WindowsVariantCuda = "cuda";

    // qwen3-tts.cpp release pin. Bump in lockstep with the hashes in
    // DownloadHashManager.Qwen3TtsCpp (each new release: prepend the new SHA-256 at index 0).
    public const string ReleaseTag = "v0.4.4";
    private const string ReleaseUrlBase = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/" + ReleaseTag + "/";

    private const string WindowsVulkanUrl = ReleaseUrlBase + "qwen3-tts-server-" + ReleaseTag + "-windows-vulkan-x64.zip";
    private const string WindowsCpuUrl    = ReleaseUrlBase + "qwen3-tts-server-" + ReleaseTag + "-windows-cpu-x64.zip";
    private const string WindowsCudaUrl   = ReleaseUrlBase + "qwen3-tts-server-" + ReleaseTag + "-windows-cuda-x64.zip";
    private const string MacUrl           = ReleaseUrlBase + "qwen3-tts-server-" + ReleaseTag + "-macos-metal-arm64.zip";
    private const string LinuxUrl         = ReleaseUrlBase + "qwen3-tts-server-" + ReleaseTag + "-linux-vulkan-x64.zip";
    private const string LinuxArmUrl      = ReleaseUrlBase + "qwen3-tts-server-" + ReleaseTag + "-linux-arm64.zip";

    private const string TokenizerModelFileName = "qwen3-tts-tokenizer-q8_0.gguf";
    private const string TokenizerModelUrl = "https://huggingface.co/koboldcpp/tts/resolve/main/qwen3-tts-tokenizer-q8_0.gguf";

    private static readonly Dictionary<string, string> TtsModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["qwen3-tts-0.6b-q8_0.gguf"] = "https://huggingface.co/koboldcpp/tts/resolve/main/qwen3-tts-0.6b-q8_0.gguf",
        // khimaros' rebuild includes the projection tensor (named code_pred.mtp_proj.*) that
        // qwen3-tts.cpp needs for the 1.7B variant. The koboldcpp upload predates that change.
        ["Qwen3-TTS-12Hz-1.7B-Base-q8_0.gguf"] = "https://huggingface.co/khimaros/Qwen3-TTS-12Hz-1.7B-Base-GGUF/resolve/main/Qwen3-TTS-12Hz-1.7B-Base-Q8_0.gguf",
        // Instruction-tuned VoiceDesign variant - the voice is described by free-text rather
        // than cloned or picked from a speaker table. Shares the 12Hz tokenizer above.
        ["qwen3-tts-12hz-1.7b-voicedesign-q8_0.gguf"] = "https://huggingface.co/cstr/qwen3-tts-1.7b-voicedesign-GGUF/resolve/main/qwen3-tts-12hz-1.7b-voicedesign-q8_0.gguf",
    };

    private const string VoicesUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-tts-cpp-2026-4/voices.zip";

    public Qwen3TtsCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, string windowsVariant, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(windowsVariant), stream, progress, cancellationToken);
        VerifyArchive(stream, DownloadHashManager.ResolveQwen3TtsCppKey(windowsVariant), "engine");
    }

    public async Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, VoicesUrl, stream, progress, cancellationToken);
        VerifyArchive(stream, DownloadHashManager.Qwen3TtsCpp.Voices, "voices");
    }

    // Compares the downloaded bytes against the known SHA-256 for this key and throws on mismatch
    // so the caller's IsFaulted branch surfaces "Download failed" instead of silently unpacking a
    // truncated or tampered file. Mirrors OmniVoiceDownloadService.VerifyArchive.
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
                $"Qwen3 TTS {label} download failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    public async Task DownloadModels(string modelsFolder, string ttsModelFileName, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        if (!TtsModelUrls.TryGetValue(ttsModelFileName, out var ttsModelUrl))
        {
            throw new ArgumentException($"Unknown Qwen3 TTS model: {ttsModelFileName}", nameof(ttsModelFileName));
        }

        var ttsModelPath = Path.Combine(modelsFolder, ttsModelFileName);
        var tokenizerPath = Path.Combine(modelsFolder, TokenizerModelFileName);
        var needTts = !File.Exists(ttsModelPath);
        var needTokenizer = !File.Exists(tokenizerPath);
        var total = (needTts ? 1 : 0) + (needTokenizer ? 1 : 0);
        var step = 0;

        if (needTts)
        {
            step++;
            titleProgress?.Invoke($"Downloading Qwen3 TTS models ({step}/{total}): {ttsModelFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, ttsModelUrl, ttsModelPath, progress, cancellationToken);
        }
        if (needTokenizer)
        {
            step++;
            titleProgress?.Invoke($"Downloading Qwen3 TTS models ({step}/{total}): {TokenizerModelFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, TokenizerModelUrl, tokenizerPath, progress, cancellationToken);
        }
    }

    private static string GetUrl(string windowsVariant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windowsVariant switch
            {
                WindowsVariantCpu => WindowsCpuUrl,
                WindowsVariantCuda => WindowsCudaUrl,
                _ => WindowsVulkanUrl,
            };
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