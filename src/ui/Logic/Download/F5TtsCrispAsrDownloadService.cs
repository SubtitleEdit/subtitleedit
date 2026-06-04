using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IF5TtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the F5-TTS v1 base GGUF (~953 MB F16) into SE's CrispASR/models folder. Same
/// .part + size-check + optional SHA-256 verify pattern as VibeVoice/IndexTTS so an interrupted
/// download can't leave a truncated file at the real path. There is no separate codec/vocoder
/// file — F5-TTS bundles the Vocos vocoder into the talker GGUF.
/// </summary>
public class F5TtsCrispAsrDownloadService : IF5TtsCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [F5TtsCrispAsr.TalkerF16FileName] =
            "https://huggingface.co/cstr/f5-tts-GGUF/resolve/main/f5-tts-v1-base-f16.gguf",
    };

    public F5TtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        // modelsFolder is intentionally ignored — the destination path is derived from
        // F5TtsCrispAsr.GetTalkerPath so SE-managed callers and crispasr's own --auto-download
        // cache adoption end up at the same canonical location. The parameter stays on the
        // interface for symmetry with the other CrispASR TTS download services.
        _ = modelsFolder;

        var talkerFileName = F5TtsCrispAsr.GetTalkerFileName(modelKey);
        var talkerPath = F5TtsCrispAsr.GetTalkerPath(modelKey);

        await Task.Run(() =>
        {
            F5TtsCrispAsr.TrySeedModelFromCrispAsrCache(talkerFileName, talkerPath);
            EnsureRemovedIfInvalid(talkerPath, talkerFileName);
        }, cancellationToken);

        if (F5TtsCrispAsr.IsValidLocalModelFile(talkerPath, talkerFileName))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading F5-TTS (CrispASR) model: {talkerFileName}");
        await DownloadAndVerify(talkerPath, talkerFileName, progress, cancellationToken);
    }

    private async Task DownloadAndVerify(string finalPath, string fileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(fileName), partPath, progress, cancellationToken);
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
        if (string.Equals(fileName, F5TtsCrispAsr.TalkerF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.F5TtsCrispAsr.TalkerF16;
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
            actual = await DownloadHashManager.ComputeSha256Async(stream, cancellationToken);
        }

        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(
                $"F5-TTS (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown F5-TTS (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || F5TtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
