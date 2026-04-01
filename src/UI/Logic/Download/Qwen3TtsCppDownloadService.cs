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
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class Qwen3TtsCppDownloadService : IQwen3TtsCppDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-tts-cpp-2026-4/qwen3-tts-cli-win64.zip";
    private const string MacUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-tts-cpp-2026-4/qwen3-tts-cli-mac.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-tts-cpp-2026-4/qwen3-tts-cli-linux64.zip";

    private const string TtsModelUrl = "https://huggingface.co/koboldcpp/tts/resolve/main/Qwen3-TTS-12Hz-1.7B-Base-q8_0.gguf";
    private const string TokenizerModelUrl = "https://huggingface.co/koboldcpp/tts/resolve/main/qwen3-tts-tokenizer-q8_0.gguf";
    private const string WavTokenizerModelUrl = "https://huggingface.co/koboldcpp/tts/resolve/main/WavTokenizer-Large-75-Q4_0.gguf";

    public Qwen3TtsCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, TtsModelUrl, Path.Combine(modelsFolder, "Qwen3-TTS-12Hz-1.7B-Base-q8_0.gguf"), progress, cancellationToken);
        await DownloadHelper.DownloadFileAsync(_httpClient, TokenizerModelUrl, Path.Combine(modelsFolder, "qwen3-tts-tokenizer-q8_0.gguf"), progress, cancellationToken);
        await DownloadHelper.DownloadFileAsync(_httpClient, WavTokenizerModelUrl, Path.Combine(modelsFolder, "WavTokenizer-Large-75-Q4_0.gguf"), progress, cancellationToken);
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