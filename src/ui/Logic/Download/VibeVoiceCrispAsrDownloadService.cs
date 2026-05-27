using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IVibeVoiceCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the VibeVoice (CrispASR) talker GGUF (Q4_K / Q8_0 / F16, user-picked) into SE's
/// CrispASR/models folder so the user gets a progress dialog instead of crispasr's silent
/// --auto-download on first synth. Unlike Qwen3 TTS (CrispASR) there's no separate codec
/// GGUF; VibeVoice's single-LM architecture bundles everything. Same .part + size-check
/// pattern as <see cref="Qwen3TtsCrispAsrDownloadService"/> so a cancelled / interrupted
/// download can't leave a truncated file at the real path that loads to a segfault later.
/// </summary>
public class VibeVoiceCrispAsrDownloadService : IVibeVoiceCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [VibeVoiceCrispAsr.TalkerQ4KFileName] =
            "https://huggingface.co/cstr/vibevoice-1.5b-GGUF/resolve/main/vibevoice-1.5b-tts-q4_k.gguf",
        [VibeVoiceCrispAsr.TalkerQ8_0FileName] =
            "https://huggingface.co/cstr/vibevoice-1.5b-GGUF/resolve/main/vibevoice-1.5b-tts-q8_0.gguf",
        [VibeVoiceCrispAsr.TalkerF16FileName] =
            "https://huggingface.co/cstr/vibevoice-1.5b-GGUF/resolve/main/vibevoice-1.5b-tts-f16.gguf",
    };

    public VibeVoiceCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var talkerFileName = VibeVoiceCrispAsr.GetTalkerFileName(modelKey);
        var talkerPath = VibeVoiceCrispAsr.GetTalkerPath(modelKey);

        // Cache seeding does synchronous File.Copy of up to ~5 GB (F16); the caller passes
        // us the resulting Task without awaiting (DownloadTtsViewModel polls it from a
        // timer), so anything before the first real await runs on the caller's thread (the
        // UI thread when invoked from the download dialog's init callback). Push the
        // seeding + size-check work onto the threadpool so the dialog stays responsive.
        await Task.Run(() =>
        {
            VibeVoiceCrispAsr.TrySeedModelFromCrispAsrCache(talkerFileName, talkerPath);
            EnsureRemovedIfInvalid(talkerPath, talkerFileName);
        }, cancellationToken);

        if (VibeVoiceCrispAsr.IsValidLocalModelFile(talkerPath, talkerFileName))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading VibeVoice (CrispASR) model: {talkerFileName}");
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
        if (string.Equals(fileName, VibeVoiceCrispAsr.TalkerQ4KFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VibeVoiceCrispAsr.TalkerQ4K;
        }
        if (string.Equals(fileName, VibeVoiceCrispAsr.TalkerQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VibeVoiceCrispAsr.TalkerQ8_0;
        }
        if (string.Equals(fileName, VibeVoiceCrispAsr.TalkerF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VibeVoiceCrispAsr.TalkerF16;
        }
        return null;
    }

    // Mirrors Qwen3TtsCrispAsrDownloadService.VerifyFile — async + cancellable since
    // these GGUFs are multi-GB and a synchronous SHA-256 would freeze the dialog for
    // 10-20 s on cancel.
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
                $"VibeVoice (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown VibeVoice (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || VibeVoiceCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
