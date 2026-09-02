using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IPocketTtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the Pocket TTS (CrispASR) checkpoint for the user-picked language into SE's
/// CrispASR/models folder so the user gets a progress dialog instead of crispasr's silent
/// --auto-download on first synth. One GGUF per language, no companion. Same shape as
/// <see cref="IndexTtsCrispAsrDownloadService"/>: ModelUrls maps every filename to its HF
/// URL so the .part / size-check / hash-verify path doesn't care which language was picked.
/// Note the GGUF mirror (cstr/pocket-tts-GGUF) is public and ungated — the gated repo is
/// Kyutai's own upstream, which SE never fetches from.
/// </summary>
public class PocketTtsCrispAsrDownloadService : IPocketTtsCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [PocketTtsCrispAsr.EnglishF16FileName] =
            "https://huggingface.co/cstr/pocket-tts-GGUF/resolve/main/pocket-tts-english-f16.gguf",
        [PocketTtsCrispAsr.EnglishQ8_0FileName] =
            "https://huggingface.co/cstr/pocket-tts-GGUF/resolve/main/pocket-tts-english-q8_0.gguf",
        [PocketTtsCrispAsr.GermanQ8_0FileName] =
            "https://huggingface.co/cstr/pocket-tts-GGUF/resolve/main/pocket-tts-german-q8_0.gguf",
        [PocketTtsCrispAsr.SpanishQ8_0FileName] =
            "https://huggingface.co/cstr/pocket-tts-GGUF/resolve/main/pocket-tts-spanish-q8_0.gguf",
        [PocketTtsCrispAsr.ItalianQ8_0FileName] =
            "https://huggingface.co/cstr/pocket-tts-GGUF/resolve/main/pocket-tts-italian-q8_0.gguf",
        [PocketTtsCrispAsr.PortugueseQ8_0FileName] =
            "https://huggingface.co/cstr/pocket-tts-GGUF/resolve/main/pocket-tts-portuguese-q8_0.gguf",
        [PocketTtsCrispAsr.FrenchQ8_0FileName] =
            "https://huggingface.co/cstr/pocket-tts-GGUF/resolve/main/pocket-tts-french_24l-q8_0.gguf",
    };

    public PocketTtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var modelFileName = PocketTtsCrispAsr.GetModelFileName(modelKey);
        var modelPath = PocketTtsCrispAsr.GetModelPath(modelKey);

        // Cache seeding does a synchronous File.Copy of up to ~365 MB; the caller passes us
        // the resulting Task without awaiting (DownloadTtsViewModel polls it from a timer),
        // so anything before the first real await runs on the caller's thread (the UI
        // thread when invoked from the download dialog's init callback). Push the seeding
        // and size-check work onto the threadpool so the dialog stays responsive.
        await Task.Run(() =>
        {
            PocketTtsCrispAsr.TrySeedModelFromCrispAsrCache(modelFileName, modelPath);
            EnsureRemovedIfInvalid(modelPath, modelFileName);
        }, cancellationToken);

        if (PocketTtsCrispAsr.IsValidLocalModelFile(modelPath, modelFileName))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading Pocket TTS (CrispASR) model: {modelFileName}");
        await DownloadAndVerify(modelPath, modelFileName, progress, cancellationToken);
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
        if (string.Equals(fileName, PocketTtsCrispAsr.EnglishF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.PocketTtsCrispAsr.EnglishF16;
        }
        if (string.Equals(fileName, PocketTtsCrispAsr.EnglishQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.PocketTtsCrispAsr.EnglishQ8_0;
        }
        if (string.Equals(fileName, PocketTtsCrispAsr.GermanQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.PocketTtsCrispAsr.GermanQ8_0;
        }
        if (string.Equals(fileName, PocketTtsCrispAsr.SpanishQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.PocketTtsCrispAsr.SpanishQ8_0;
        }
        if (string.Equals(fileName, PocketTtsCrispAsr.ItalianQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.PocketTtsCrispAsr.ItalianQ8_0;
        }
        if (string.Equals(fileName, PocketTtsCrispAsr.PortugueseQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.PocketTtsCrispAsr.PortugueseQ8_0;
        }
        if (string.Equals(fileName, PocketTtsCrispAsr.FrenchQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.PocketTtsCrispAsr.FrenchQ8_0;
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
                $"Pocket TTS (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown Pocket TTS (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || PocketTtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
