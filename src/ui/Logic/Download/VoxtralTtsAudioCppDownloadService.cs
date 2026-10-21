using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.UiLogic;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IVoxtralTtsAudioCppDownloadService
{
    /// <summary>Downloads the selected Voxtral TTS GGUF into audio.cpp's models folder.</summary>
    Task DownloadModels(string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Model weights only: the audio.cpp engine binaries are shared with the other audio.cpp
/// engines and downloaded through <see cref="IndexTts25AudioCppDownloadService"/>. The GGUF
/// comes straight from HuggingFace (audio.cpp's own conversion of Mistral's Voxtral-4B-TTS-2603).
/// The weights are CC BY-NC 4.0, so callers show the licence gate first (see
/// <see cref="VoxtralTtsAudioCpp.LicenseDefinition"/>).
/// </summary>
public class VoxtralTtsAudioCppDownloadService : IVoxtralTtsAudioCppDownloadService
{
    private readonly HttpClient _httpClient;

    private const string ModelUrlBase =
        AudioCppRuntime.ModelRepoBaseUrl + "Voxtral-4B-TTS-2603-GGUF/";

    public VoxtralTtsAudioCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var fileName = VoxtralTtsAudioCpp.GetModelFileName(modelKey);
        var finalPath = VoxtralTtsAudioCpp.GetModelPath(modelKey);

        // A partial GGUF is worse than none: audio.cpp rejects it with "GGUF tensor data range
        // is out of bounds", which reads like a corrupt model rather than a short download.
        // Size-check on the threadpool — this runs from the download dialog's init callback.
        await Task.Run(() => EnsureRemovedIfInvalid(finalPath, fileName), cancellationToken);

        if (VoxtralTtsAudioCpp.IsValidLocalModelFile(finalPath, fileName))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading Voxtral TTS model: {fileName}");

        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, ModelUrlBase + fileName, partPath, progress, cancellationToken);
            await VerifyFile(partPath, GetHashKey(fileName), fileName, cancellationToken);
            File.Move(partPath, finalPath);
        }
        catch
        {
            TryDelete(partPath);
            throw;
        }
    }

    private static string? GetHashKey(string fileName)
    {
        if (string.Equals(fileName, VoxtralTtsAudioCpp.ModelBf16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VoxtralTtsAudioCpp.ModelBf16;
        }

        if (string.Equals(fileName, VoxtralTtsAudioCpp.ModelQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VoxtralTtsAudioCpp.ModelQ8_0;
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
                $"Voxtral TTS model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || VoxtralTtsAudioCpp.IsValidLocalModelFile(path, fileName))
        {
            return;
        }

        TryDelete(path);
    }
}
