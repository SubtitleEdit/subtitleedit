using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IKokoroTtsCppDownloadService
{
    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

public class KokoroTtsCppDownloadService : IKokoroTtsCppDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsUrl = "https://github.com/niksedk/kokoro.cpp/releases/download/v0.1.1/kokoro-tts-server-v0.1.1-windows-x64.zip";
    private const string MacUrl     = "https://github.com/niksedk/kokoro.cpp/releases/download/v0.1.1/kokoro-tts-server-v0.1.1-macos-arm64.zip";
    private const string LinuxUrl   = "https://github.com/niksedk/kokoro.cpp/releases/download/v0.1.1/kokoro-tts-server-v0.1.1-linux-x64.zip";

    private const string TtsModelFileName    = "kokoro-v1.1-zh.onnx";
    private const string VoicesModelFileName = "voices-v1.1-zh.bin";
    private const string TtsModelUrl     = "https://github.com/koth/kokoro.cpp/releases/download/voices_model_files/kokoro-v1.1-zh.onnx";
    private const string VoicesModelUrl  = "https://github.com/koth/kokoro.cpp/releases/download/voices_model_files/voices-v1.1-zh.bin";

    public KokoroTtsCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var ttsPath    = Path.Combine(modelsFolder, TtsModelFileName);
        var voicesPath = Path.Combine(modelsFolder, VoicesModelFileName);
        var needTts    = !File.Exists(ttsPath);
        var needVoices = !File.Exists(voicesPath);
        var total      = (needTts ? 1 : 0) + (needVoices ? 1 : 0);
        var step       = 0;

        if (needTts)
        {
            step++;
            titleProgress?.Invoke($"Downloading Kokoro TTS models ({step}/{total}): {TtsModelFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, TtsModelUrl, ttsPath, progress, cancellationToken);
        }
        if (needVoices)
        {
            step++;
            titleProgress?.Invoke($"Downloading Kokoro TTS models ({step}/{total}): {VoicesModelFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, VoicesModelUrl, voicesPath, progress, cancellationToken);
        }
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
