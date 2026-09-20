using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IConfucius4TtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the Confucius4-TTS (CrispASR) T2S + S2A GGUF pair (Q8_0 / F16, user-picked) plus the
/// two required companions — the BigVGAN 22.05 kHz vocoder and the w2v-BERT reference encoder —
/// into SE's CrispASR/models folder, so the user gets a progress dialog instead of crispasr's
/// silent --auto-download on first synth. Same shape as <see cref="DotsTtsCrispAsrDownloadService"/>.
/// </summary>
/// <remarks>
/// SE downloading its own quant matters more here than for the other engines: crispasr's registry
/// default for this backend is the Q4_K pair, whose flow-matching degradation is why the engine
/// was first judged too robotic to ship (CrispASR #377).
/// </remarks>
public class Confucius4TtsCrispAsrDownloadService : IConfucius4TtsCrispAsrDownloadService
{
    private const string RepoBaseUrl = "https://huggingface.co/cstr/confucius4-tts-GGUF/resolve/main/";

    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [Confucius4TtsCrispAsr.T2sQ8_0FileName] = RepoBaseUrl + Confucius4TtsCrispAsr.T2sQ8_0FileName,
        [Confucius4TtsCrispAsr.T2sF16FileName] = RepoBaseUrl + Confucius4TtsCrispAsr.T2sF16FileName,
        [Confucius4TtsCrispAsr.S2aQ8_0FileName] = RepoBaseUrl + Confucius4TtsCrispAsr.S2aQ8_0FileName,
        [Confucius4TtsCrispAsr.S2aF16FileName] = RepoBaseUrl + Confucius4TtsCrispAsr.S2aF16FileName,
        [Confucius4TtsCrispAsr.VocoderFileName] = RepoBaseUrl + Confucius4TtsCrispAsr.VocoderFileName,
        [Confucius4TtsCrispAsr.W2vFileName] = RepoBaseUrl + Confucius4TtsCrispAsr.W2vFileName,
    };

    public Confucius4TtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var t2sFileName = Confucius4TtsCrispAsr.GetT2sFileName(modelKey);
        var t2sPath = Confucius4TtsCrispAsr.GetT2sPath(modelKey);
        var s2aFileName = Confucius4TtsCrispAsr.GetS2aFileName(modelKey);
        var s2aPath = Confucius4TtsCrispAsr.GetS2aPath(modelKey);
        var vocoderPath = Confucius4TtsCrispAsr.GetVocoderPath();
        var w2vPath = Confucius4TtsCrispAsr.GetW2vPath();

        // Cache seeding does a synchronous File.Copy of up to ~1.3 GB, and the caller hands the
        // returned Task to a timer without awaiting it, so anything before the first real await
        // would run on the UI thread. Push it onto the threadpool.
        await Task.Run(() =>
        {
            Confucius4TtsCrispAsr.TrySeedModelFromCrispAsrCache(t2sFileName, t2sPath);
            Confucius4TtsCrispAsr.TrySeedModelFromCrispAsrCache(s2aFileName, s2aPath);
            Confucius4TtsCrispAsr.TrySeedModelFromCrispAsrCache(Confucius4TtsCrispAsr.VocoderFileName, vocoderPath);
            Confucius4TtsCrispAsr.TrySeedModelFromCrispAsrCache(Confucius4TtsCrispAsr.W2vFileName, w2vPath);
            EnsureRemovedIfInvalid(t2sPath, t2sFileName);
            EnsureRemovedIfInvalid(s2aPath, s2aFileName);
            EnsureRemovedIfInvalid(vocoderPath, Confucius4TtsCrispAsr.VocoderFileName);
            EnsureRemovedIfInvalid(w2vPath, Confucius4TtsCrispAsr.W2vFileName);
        }, cancellationToken);

        var pending = new List<(string Path, string FileName)>();
        if (!Confucius4TtsCrispAsr.IsValidLocalModelFile(t2sPath, t2sFileName))
        {
            pending.Add((t2sPath, t2sFileName));
        }
        if (!Confucius4TtsCrispAsr.IsValidLocalModelFile(s2aPath, s2aFileName))
        {
            pending.Add((s2aPath, s2aFileName));
        }
        if (!Confucius4TtsCrispAsr.IsValidLocalModelFile(vocoderPath, Confucius4TtsCrispAsr.VocoderFileName))
        {
            pending.Add((vocoderPath, Confucius4TtsCrispAsr.VocoderFileName));
        }
        if (!Confucius4TtsCrispAsr.IsValidLocalModelFile(w2vPath, Confucius4TtsCrispAsr.W2vFileName))
        {
            pending.Add((w2vPath, Confucius4TtsCrispAsr.W2vFileName));
        }

        var step = 0;
        foreach (var (path, fileName) in pending)
        {
            step++;
            titleProgress?.Invoke($"Downloading Confucius4-TTS (CrispASR) models ({step}/{pending.Count}): {fileName}");
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
        if (string.Equals(fileName, Confucius4TtsCrispAsr.T2sQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Confucius4TtsCrispAsr.T2sQ8_0;
        }
        if (string.Equals(fileName, Confucius4TtsCrispAsr.T2sF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Confucius4TtsCrispAsr.T2sF16;
        }
        if (string.Equals(fileName, Confucius4TtsCrispAsr.S2aQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Confucius4TtsCrispAsr.S2aQ8_0;
        }
        if (string.Equals(fileName, Confucius4TtsCrispAsr.S2aF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Confucius4TtsCrispAsr.S2aF16;
        }
        if (string.Equals(fileName, Confucius4TtsCrispAsr.VocoderFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Confucius4TtsCrispAsr.Vocoder;
        }
        if (string.Equals(fileName, Confucius4TtsCrispAsr.W2vFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Confucius4TtsCrispAsr.W2v;
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
                $"Confucius4-TTS (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown Confucius4-TTS (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || Confucius4TtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
