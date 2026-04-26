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

    private const string WindowsVulkanUrl = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/v0.4.2/qwen3-tts-server-v0.4.2-windows-vulkan-x64.zip";
    private const string WindowsCpuUrl    = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/v0.4.2/qwen3-tts-server-v0.4.2-windows-cpu-x64.zip";
    private const string MacUrl           = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/v0.4.2/qwen3-tts-server-v0.4.2-macos-metal-arm64.zip";
    private const string LinuxUrl         = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/v0.4.2/qwen3-tts-server-v0.4.2-linux-vulkan-x64.zip";

    private const string TokenizerModelFileName = "qwen3-tts-tokenizer-q8_0.gguf";
    private const string TokenizerModelUrl = "https://huggingface.co/koboldcpp/tts/resolve/main/qwen3-tts-tokenizer-q8_0.gguf";

    private static readonly Dictionary<string, string> TtsModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["qwen3-tts-0.6b-q8_0.gguf"] = "https://huggingface.co/koboldcpp/tts/resolve/main/qwen3-tts-0.6b-q8_0.gguf",
        // khimaros' rebuild includes the projection tensor (named code_pred.mtp_proj.*) that
        // qwen3-tts.cpp needs for the 1.7B variant. The koboldcpp upload predates that change.
        ["Qwen3-TTS-12Hz-1.7B-Base-q8_0.gguf"] = "https://huggingface.co/khimaros/Qwen3-TTS-12Hz-1.7B-Base-GGUF/resolve/main/Qwen3-TTS-12Hz-1.7B-Base-Q8_0.gguf",
    };

    private const string VoicesUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-tts-cpp-2026-4/voices.zip";

    public Qwen3TtsCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, string windowsVariant, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(windowsVariant), stream, progress, cancellationToken);
    }

    public async Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, VoicesUrl, stream, progress, cancellationToken);
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
            return windowsVariant == WindowsVariantCpu ? WindowsCpuUrl : WindowsVulkanUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacUrl;
        }

        throw new PlatformNotSupportedException();
    }
}