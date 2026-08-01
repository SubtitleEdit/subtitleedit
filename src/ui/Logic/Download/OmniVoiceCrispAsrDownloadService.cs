using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IOmniVoiceCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the GGUFs the omnivoice backend needs — the picked main quant plus the shared F16
/// tokenizer — into SE's CrispASR/models folder, so the user gets one SE progress dialog instead
/// of crispasr's silent --auto-download. Same .part + size-check + SHA-256 verify pattern as
/// VoxCPM2/CosyVoice3.
///
/// Only the F16 tokenizer is ever fetched: the q8_0 build in the same repo cannot be read back
/// as f32 when encoding a reference voice, so it silently turns voice cloning into noise. See
/// <see cref="OmniVoiceCrispAsr"/> for the measurements.
/// </summary>
public class OmniVoiceCrispAsrDownloadService : IOmniVoiceCrispAsrDownloadService
{
    private const string RepoBase = "https://huggingface.co/cstr/omnivoice-GGUF/resolve/main/";

    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [OmniVoiceCrispAsr.ModelQ4KFileName] = RepoBase + OmniVoiceCrispAsr.ModelQ4KFileName,
        [OmniVoiceCrispAsr.ModelQ8_0FileName] = RepoBase + OmniVoiceCrispAsr.ModelQ8_0FileName,
        [OmniVoiceCrispAsr.ModelF16FileName] = RepoBase + OmniVoiceCrispAsr.ModelF16FileName,
        [OmniVoiceCrispAsr.TokenizerFileName] = RepoBase + OmniVoiceCrispAsr.TokenizerFileName,
    };

    public OmniVoiceCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        // Destinations come from the engine so cache-seed adoption and SE-managed downloads
        // converge on the same canonical folder.
        modelsFolder = OmniVoiceCrispAsr.GetSetModelsFolder();

        var required = OmniVoiceCrispAsr.GetRequiredFileNames(modelKey);

        await Task.Run(() =>
        {
            foreach (var fileName in required)
            {
                var dest = Path.Combine(modelsFolder, fileName);
                OmniVoiceCrispAsr.TrySeedModelFromCrispAsrCache(fileName, dest);
                EnsureRemovedIfInvalid(dest, fileName);
            }
        }, cancellationToken);

        var missing = new List<string>();
        foreach (var fileName in required)
        {
            var dest = Path.Combine(modelsFolder, fileName);
            if (!OmniVoiceCrispAsr.IsValidLocalModelFile(dest, fileName))
            {
                missing.Add(fileName);
            }
        }

        var step = 0;
        foreach (var fileName in missing)
        {
            step++;
            titleProgress?.Invoke($"Downloading OmniVoice (CrispASR) models ({step}/{missing.Count}): {fileName}");
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
        if (string.Equals(fileName, OmniVoiceCrispAsr.ModelQ4KFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.OmniVoiceCrispAsr.ModelQ4K;
        }
        if (string.Equals(fileName, OmniVoiceCrispAsr.ModelQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.OmniVoiceCrispAsr.ModelQ8_0;
        }
        if (string.Equals(fileName, OmniVoiceCrispAsr.ModelF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.OmniVoiceCrispAsr.ModelF16;
        }
        if (string.Equals(fileName, OmniVoiceCrispAsr.TokenizerFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.OmniVoiceCrispAsr.TokenizerF16;
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
                $"OmniVoice (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown OmniVoice (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || OmniVoiceCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
