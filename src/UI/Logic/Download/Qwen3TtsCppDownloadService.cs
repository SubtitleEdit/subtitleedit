using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IQwen3TtsCppDownloadService
{
    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

public class Qwen3TtsCppDownloadService : IQwen3TtsCppDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsUrl = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/v0.4.0/qwen3-tts-server-v0.4.0-windows-vulkan-x64.zip";
    // Linux / macOS server builds are not yet published. The engine (Qwen3TtsCpp.cs)
    // expects the qwen3-tts-server binary; the old CLI zips below will no longer work.
    private const string MacUrl = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/v0.4.0/qwen3-tts-server-v0.4.0-macos-metal-arm64.zip";
    private const string LinuxUrl = "https://github.com/niksedk/qwen3-tts.cpp/releases/download/v0.4.0/qwen3-tts-server-v0.4.0-linux-vulkan-x64.zip";

    private const string TtsModelUrl = "https://huggingface.co/koboldcpp/tts/resolve/main/qwen3-tts-0.6b-q8_0.gguf";
    private const string TokenizerModelUrl = "https://huggingface.co/koboldcpp/tts/resolve/main/qwen3-tts-tokenizer-q8_0.gguf";

    private const string VoicesUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-tts-cpp-2026-4/voices.zip";

    public Qwen3TtsCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, VoicesUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        titleProgress?.Invoke("Downloading Qwen3 TTS models (1/2): qwen3-tts-0.6b-q8_0.gguf");
        await DownloadHelper.DownloadFileAsync(_httpClient, TtsModelUrl, Path.Combine(modelsFolder, "qwen3-tts-0.6b-q8_0.gguf"), progress, cancellationToken);
        titleProgress?.Invoke("Downloading Qwen3 TTS models (2/2): qwen3-tts-tokenizer-q8_0.gguf");
        await DownloadHelper.DownloadFileAsync(_httpClient, TokenizerModelUrl, Path.Combine(modelsFolder, "qwen3-tts-tokenizer-q8_0.gguf"), progress, cancellationToken);
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
            return MacUrl;
        }

        throw new PlatformNotSupportedException();
    }
}