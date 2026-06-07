using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IVoxCPM2CrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the GGUFs the voxcpm2-tts backend needs — the picked main quant plus the small
/// shared voxcpm2-ref.gguf companion — into SE's CrispASR/models folder. crispasr discovers the
/// ref companion as a sibling of the main model at server startup, so staging both ourselves
/// gives the user a single SE progress dialog instead of crispasr's silent --auto-download. Same
/// .part + size-check + optional SHA-256 verify pattern as CosyVoice3/F5-TTS.
/// </summary>
public class VoxCPM2CrispAsrDownloadService : IVoxCPM2CrispAsrDownloadService
{
    private const string RepoBase = "https://huggingface.co/cstr/voxcpm2-GGUF/resolve/main/";

    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [VoxCPM2CrispAsr.ModelQ4KFileName] = RepoBase + VoxCPM2CrispAsr.ModelQ4KFileName,
        [VoxCPM2CrispAsr.ModelF16FileName] = RepoBase + VoxCPM2CrispAsr.ModelF16FileName,
        [VoxCPM2CrispAsr.RefFileName] = RepoBase + VoxCPM2CrispAsr.RefFileName,
    };

    public VoxCPM2CrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        // Destinations are derived from VoxCPM2CrispAsr.GetSetModelsFolder() so cache-seed adoption
        // and SE-managed downloads converge on the same canonical location.
        modelsFolder = VoxCPM2CrispAsr.GetSetModelsFolder();

        var required = VoxCPM2CrispAsr.GetRequiredFileNames(modelKey);

        // Seed any matching files already in crispasr's --auto-download cache before deciding
        // what to fetch.
        await Task.Run(() =>
        {
            foreach (var fileName in required)
            {
                var dest = Path.Combine(modelsFolder, fileName);
                VoxCPM2CrispAsr.TrySeedModelFromCrispAsrCache(fileName, dest);
                EnsureRemovedIfInvalid(dest, fileName);
            }
        }, cancellationToken);

        var missing = new List<string>();
        foreach (var fileName in required)
        {
            var dest = Path.Combine(modelsFolder, fileName);
            if (!VoxCPM2CrispAsr.IsValidLocalModelFile(dest, fileName))
            {
                missing.Add(fileName);
            }
        }

        var step = 0;
        foreach (var fileName in missing)
        {
            step++;
            titleProgress?.Invoke($"Downloading VoxCPM2 (CrispASR) models ({step}/{missing.Count}): {fileName}");
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
        if (string.Equals(fileName, VoxCPM2CrispAsr.ModelQ4KFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VoxCPM2CrispAsr.ModelQ4K;
        }
        if (string.Equals(fileName, VoxCPM2CrispAsr.ModelF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VoxCPM2CrispAsr.ModelF16;
        }
        if (string.Equals(fileName, VoxCPM2CrispAsr.RefFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.VoxCPM2CrispAsr.Ref;
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
                $"VoxCPM2 (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown VoxCPM2 (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || VoxCPM2CrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
