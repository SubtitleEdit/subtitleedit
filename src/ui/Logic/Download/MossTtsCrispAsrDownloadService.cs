using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IMossTtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the GGUFs the moss-tts backend needs — the picked backbone quant plus the shared
/// moss-tts-v1.5-codec.gguf companion — into SE's CrispASR/models folder. Staging both ourselves
/// gives the user a single SE progress dialog instead of crispasr's silent --auto-download. Same
/// .part + size-check + optional SHA-256 verify pattern as VoxCPM2/CosyVoice3/F5-TTS.
/// </summary>
public class MossTtsCrispAsrDownloadService : IMossTtsCrispAsrDownloadService
{
    private const string RepoBase = "https://huggingface.co/cstr/moss-tts-v1.5-GGUF/resolve/main/";

    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [MossTtsCrispAsr.ModelQ4KFileName] = RepoBase + MossTtsCrispAsr.ModelQ4KFileName,
        [MossTtsCrispAsr.ModelF16FileName] = RepoBase + MossTtsCrispAsr.ModelF16FileName,
        [MossTtsCrispAsr.CodecFileName] = RepoBase + MossTtsCrispAsr.CodecFileName,
    };

    public MossTtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        // Destinations are derived from MossTtsCrispAsr.GetSetModelsFolder() so cache-seed
        // adoption and SE-managed downloads converge on the same canonical location.
        modelsFolder = MossTtsCrispAsr.GetSetModelsFolder();

        var required = MossTtsCrispAsr.GetRequiredFileNames(modelKey);

        // Seed any matching files already in crispasr's --auto-download cache before deciding
        // what to fetch.
        await Task.Run(() =>
        {
            foreach (var fileName in required)
            {
                var dest = Path.Combine(modelsFolder, fileName);
                MossTtsCrispAsr.TrySeedModelFromCrispAsrCache(fileName, dest);
                EnsureRemovedIfInvalid(dest, fileName);
            }
        }, cancellationToken);

        var missing = new List<string>();
        foreach (var fileName in required)
        {
            var dest = Path.Combine(modelsFolder, fileName);
            if (!MossTtsCrispAsr.IsValidLocalModelFile(dest, fileName))
            {
                missing.Add(fileName);
            }
        }

        var step = 0;
        foreach (var fileName in missing)
        {
            step++;
            titleProgress?.Invoke($"Downloading MOSS-TTS (CrispASR) models ({step}/{missing.Count}): {fileName}");
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
        if (string.Equals(fileName, MossTtsCrispAsr.ModelQ4KFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.MossTtsCrispAsr.ModelQ4K;
        }
        if (string.Equals(fileName, MossTtsCrispAsr.ModelF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.MossTtsCrispAsr.ModelF16;
        }
        if (string.Equals(fileName, MossTtsCrispAsr.CodecFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.MossTtsCrispAsr.Codec;
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
                $"MOSS-TTS (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown MOSS-TTS (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || MossTtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
