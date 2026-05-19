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

    // kokoro.cpp release pin. Bump in lockstep with the hashes in
    // DownloadHashManager.KokoroTtsCpp (each new release: prepend the new SHA-256 at index 0).
    public const string ReleaseTag = "v0.1.2";
    private const string ReleaseUrlBase = "https://github.com/niksedk/kokoro.cpp/releases/download/" + ReleaseTag + "/";

    private const string WindowsUrl  = ReleaseUrlBase + "kokoro-tts-server-" + ReleaseTag + "-windows-x64.zip";
    private const string MacUrl      = ReleaseUrlBase + "kokoro-tts-server-" + ReleaseTag + "-macos-arm64.zip";
    private const string LinuxUrl    = ReleaseUrlBase + "kokoro-tts-server-" + ReleaseTag + "-linux-x64.zip";
    private const string LinuxArmUrl = ReleaseUrlBase + "kokoro-tts-server-" + ReleaseTag + "-linux-arm64.zip";

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
        VerifyArchive(stream, DownloadHashManager.ResolveKokoroTtsCppKey(), "engine");
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
                $"Kokoro TTS {label} download failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
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
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LinuxArmUrl : LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacUrl;
        }

        throw new PlatformNotSupportedException();
    }
}
