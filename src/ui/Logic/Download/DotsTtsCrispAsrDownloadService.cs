using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IDotsTtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the dots.tts (CrispASR) core GGUF (Q4_K / Q8_0 / F16, user-picked) plus the two
/// required companions — the BigVGAN 48 kHz vocoder and the CAM++ speaker encoder — into SE's
/// CrispASR/models folder, so the user gets a progress dialog instead of crispasr's silent
/// --auto-download on first synth. Same shape as <see cref="IndexTtsCrispAsrDownloadService"/>.
/// </summary>
/// <remarks>
/// The speaker encoder is only 14 MB and is what makes voice cloning work at all, so it is always
/// fetched rather than being tied to a quant choice.
/// </remarks>
public class DotsTtsCrispAsrDownloadService : IDotsTtsCrispAsrDownloadService
{
    private const string RepoBaseUrl = "https://huggingface.co/cstr/dots-tts-soar-GGUF/resolve/main/";

    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [DotsTtsCrispAsr.CoreQ4KFileName] = RepoBaseUrl + DotsTtsCrispAsr.CoreQ4KFileName,
        [DotsTtsCrispAsr.CoreQ8_0FileName] = RepoBaseUrl + DotsTtsCrispAsr.CoreQ8_0FileName,
        [DotsTtsCrispAsr.CoreF16FileName] = RepoBaseUrl + DotsTtsCrispAsr.CoreF16FileName,
        [DotsTtsCrispAsr.VocoderFileName] = RepoBaseUrl + DotsTtsCrispAsr.VocoderFileName,
        [DotsTtsCrispAsr.SpeakerEncoderFileName] = RepoBaseUrl + DotsTtsCrispAsr.SpeakerEncoderFileName,
    };

    public DotsTtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var coreFileName = DotsTtsCrispAsr.GetCoreFileName(modelKey);
        var corePath = DotsTtsCrispAsr.GetCorePath(modelKey);
        var vocoderPath = DotsTtsCrispAsr.GetVocoderPath();
        var speakerPath = DotsTtsCrispAsr.GetSpeakerEncoderPath();

        // Cache seeding does a synchronous File.Copy of up to ~4.6 GB, and the caller hands the
        // returned Task to a timer without awaiting it, so anything before the first real await
        // would run on the UI thread. Push it onto the threadpool.
        await Task.Run(() =>
        {
            DotsTtsCrispAsr.TrySeedModelFromCrispAsrCache(coreFileName, corePath);
            DotsTtsCrispAsr.TrySeedModelFromCrispAsrCache(DotsTtsCrispAsr.VocoderFileName, vocoderPath);
            DotsTtsCrispAsr.TrySeedModelFromCrispAsrCache(DotsTtsCrispAsr.SpeakerEncoderFileName, speakerPath);
            EnsureRemovedIfInvalid(corePath, coreFileName);
            EnsureRemovedIfInvalid(vocoderPath, DotsTtsCrispAsr.VocoderFileName);
            EnsureRemovedIfInvalid(speakerPath, DotsTtsCrispAsr.SpeakerEncoderFileName);
        }, cancellationToken);

        var pending = new List<(string Path, string FileName)>();
        if (!DotsTtsCrispAsr.IsValidLocalModelFile(corePath, coreFileName))
        {
            pending.Add((corePath, coreFileName));
        }
        if (!DotsTtsCrispAsr.IsValidLocalModelFile(vocoderPath, DotsTtsCrispAsr.VocoderFileName))
        {
            pending.Add((vocoderPath, DotsTtsCrispAsr.VocoderFileName));
        }
        if (!DotsTtsCrispAsr.IsValidLocalModelFile(speakerPath, DotsTtsCrispAsr.SpeakerEncoderFileName))
        {
            pending.Add((speakerPath, DotsTtsCrispAsr.SpeakerEncoderFileName));
        }

        var step = 0;
        foreach (var (path, fileName) in pending)
        {
            step++;
            titleProgress?.Invoke($"Downloading dots.tts (CrispASR) models ({step}/{pending.Count}): {fileName}");
            await DownloadAndVerify(path, fileName, progress, cancellationToken);
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
        if (string.Equals(fileName, DotsTtsCrispAsr.CoreQ4KFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.DotsTtsCrispAsr.CoreQ4K;
        }
        if (string.Equals(fileName, DotsTtsCrispAsr.CoreQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.DotsTtsCrispAsr.CoreQ8_0;
        }
        if (string.Equals(fileName, DotsTtsCrispAsr.CoreF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.DotsTtsCrispAsr.CoreF16;
        }
        if (string.Equals(fileName, DotsTtsCrispAsr.VocoderFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.DotsTtsCrispAsr.Vocoder;
        }
        if (string.Equals(fileName, DotsTtsCrispAsr.SpeakerEncoderFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.DotsTtsCrispAsr.SpeakerEncoder;
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
                $"dots.tts (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown dots.tts (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || DotsTtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
