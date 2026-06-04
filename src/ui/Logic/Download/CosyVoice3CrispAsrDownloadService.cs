using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ICosyVoice3CrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads every GGUF the cosyvoice3-tts backend needs (LLM + flow + hift + s3tok + campplus
/// + voices) into SE's CrispASR/models folder. crispasr looks for the companions as siblings of
/// the LLM at server startup; staging them ourselves means the user gets a single SE progress
/// dialog instead of crispasr's silent --auto-download. Same .part + size-check + optional
/// SHA-256 verify pattern as VibeVoice/IndexTTS.
/// </summary>
public class CosyVoice3CrispAsrDownloadService : ICosyVoice3CrispAsrDownloadService
{
    private const string RepoBase = "https://huggingface.co/cstr/cosyvoice3-0.5b-2512-GGUF/resolve/main/";

    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [CosyVoice3CrispAsr.LlmQ4KFileName] = RepoBase + CosyVoice3CrispAsr.LlmQ4KFileName,
        [CosyVoice3CrispAsr.LlmF16FileName] = RepoBase + CosyVoice3CrispAsr.LlmF16FileName,
        [CosyVoice3CrispAsr.FlowF16FileName] = RepoBase + CosyVoice3CrispAsr.FlowF16FileName,
        [CosyVoice3CrispAsr.HiftF16FileName] = RepoBase + CosyVoice3CrispAsr.HiftF16FileName,
        [CosyVoice3CrispAsr.S3TokF16FileName] = RepoBase + CosyVoice3CrispAsr.S3TokF16FileName,
        [CosyVoice3CrispAsr.CampPlusF16FileName] = RepoBase + CosyVoice3CrispAsr.CampPlusF16FileName,
        [CosyVoice3CrispAsr.VoicesGgufFileName] = RepoBase + CosyVoice3CrispAsr.VoicesGgufFileName,
    };

    public CosyVoice3CrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        // modelsFolder is intentionally ignored — destinations are derived from
        // CosyVoice3CrispAsr.GetSetModelsFolder() so cache-seed adoption and SE-managed downloads
        // converge on the same canonical location. The parameter stays on the interface for
        // symmetry with the other CrispASR TTS download services.
        _ = modelsFolder;

        var required = CosyVoice3CrispAsr.GetRequiredFileNames(modelKey);

        // Seed any matching files already in crispasr's --auto-download cache before deciding
        // what to fetch — saves a re-download for users who tried CosyVoice3 via raw crispasr
        // before the SE integration.
        await Task.Run(() =>
        {
            foreach (var fileName in required)
            {
                var dest = Path.Combine(modelsFolder, fileName);
                CosyVoice3CrispAsr.TrySeedModelFromCrispAsrCache(fileName, dest);
                EnsureRemovedIfInvalid(dest, fileName);
            }
        }, cancellationToken);

        // Count missing files for the "(n/total)" title prefix. Doing this after the seed pass
        // so the user doesn't see "1/6" right before the seeder turns 5 of them into hits.
        var missing = new List<string>();
        foreach (var fileName in required)
        {
            var dest = Path.Combine(modelsFolder, fileName);
            if (!CosyVoice3CrispAsr.IsValidLocalModelFile(dest, fileName))
            {
                missing.Add(fileName);
            }
        }

        var step = 0;
        foreach (var fileName in missing)
        {
            step++;
            titleProgress?.Invoke($"Downloading CosyVoice3 (CrispASR) models ({step}/{missing.Count}): {fileName}");
            var dest = Path.Combine(modelsFolder, fileName);
            await DownloadAndVerify(dest, fileName, progress, cancellationToken);
        }
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
        if (string.Equals(fileName, CosyVoice3CrispAsr.LlmQ4KFileName, StringComparison.OrdinalIgnoreCase))
            return DownloadHashManager.CosyVoice3CrispAsr.LlmQ4K;
        if (string.Equals(fileName, CosyVoice3CrispAsr.LlmF16FileName, StringComparison.OrdinalIgnoreCase))
            return DownloadHashManager.CosyVoice3CrispAsr.LlmF16;
        if (string.Equals(fileName, CosyVoice3CrispAsr.FlowF16FileName, StringComparison.OrdinalIgnoreCase))
            return DownloadHashManager.CosyVoice3CrispAsr.FlowF16;
        if (string.Equals(fileName, CosyVoice3CrispAsr.HiftF16FileName, StringComparison.OrdinalIgnoreCase))
            return DownloadHashManager.CosyVoice3CrispAsr.HiftF16;
        if (string.Equals(fileName, CosyVoice3CrispAsr.S3TokF16FileName, StringComparison.OrdinalIgnoreCase))
            return DownloadHashManager.CosyVoice3CrispAsr.S3TokF16;
        if (string.Equals(fileName, CosyVoice3CrispAsr.CampPlusF16FileName, StringComparison.OrdinalIgnoreCase))
            return DownloadHashManager.CosyVoice3CrispAsr.CampPlusF16;
        if (string.Equals(fileName, CosyVoice3CrispAsr.VoicesGgufFileName, StringComparison.OrdinalIgnoreCase))
            return DownloadHashManager.CosyVoice3CrispAsr.VoicesGguf;
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
                $"CosyVoice3 (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown CosyVoice3 (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || CosyVoice3CrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
