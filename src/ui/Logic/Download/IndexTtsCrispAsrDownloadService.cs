using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IIndexTtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the IndexTTS (CrispASR) GPT talker + BigVGAN codec GGUFs into SE's CrispASR/models
/// folder so the user gets a progress dialog instead of crispasr's silent --auto-download on
/// first synth. Two files (~870 MB total Q8_0): GPT talker + BigVGAN codec — same shape as
/// <see cref="Qwen3TtsCrispAsrDownloadService"/> but without model-key variants since there's
/// only one quant exposed in SE today.
/// </summary>
public class IndexTtsCrispAsrDownloadService : IIndexTtsCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [IndexTtsCrispAsr.TalkerQ4KFileName] =
            "https://huggingface.co/cstr/indextts-1.5-GGUF/resolve/main/indextts-gpt-q4_k.gguf",
        [IndexTtsCrispAsr.TalkerQ8_0FileName] =
            "https://huggingface.co/cstr/indextts-1.5-GGUF/resolve/main/indextts-gpt-q8_0.gguf",
        [IndexTtsCrispAsr.TalkerF16FileName] =
            "https://huggingface.co/cstr/indextts-1.5-GGUF/resolve/main/indextts-gpt.gguf",
        [IndexTtsCrispAsr.CodecFileName] =
            "https://huggingface.co/cstr/indextts-1.5-GGUF/resolve/main/indextts-bigvgan.gguf",
    };

    public IndexTtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var talkerFileName = IndexTtsCrispAsr.GetTalkerFileName(modelKey);
        var codecFileName = IndexTtsCrispAsr.CodecFileName;

        var talkerPath = IndexTtsCrispAsr.GetTalkerPath(modelKey);
        var codecPath = IndexTtsCrispAsr.GetCodecPath();

        // Cache seeding does synchronous File.Copy of up to ~870 MB; the caller passes us
        // the resulting Task without awaiting (DownloadTtsViewModel polls it from a timer),
        // so anything before the first real await runs on the caller's thread (the UI
        // thread when invoked from the download dialog's init callback). Push the seeding
        // and size-check work onto the threadpool so the dialog stays responsive.
        await Task.Run(() =>
        {
            IndexTtsCrispAsr.TrySeedModelFromCrispAsrCache(talkerFileName, talkerPath);
            IndexTtsCrispAsr.TrySeedModelFromCrispAsrCache(codecFileName, codecPath);
            EnsureRemovedIfInvalid(talkerPath, talkerFileName);
            EnsureRemovedIfInvalid(codecPath, codecFileName);
        }, cancellationToken);

        var needTalker = !IndexTtsCrispAsr.IsValidLocalModelFile(talkerPath, talkerFileName);
        var needCodec = !IndexTtsCrispAsr.IsValidLocalModelFile(codecPath, codecFileName);
        var total = (needTalker ? 1 : 0) + (needCodec ? 1 : 0);
        var step = 0;

        if (needTalker)
        {
            step++;
            titleProgress?.Invoke($"Downloading IndexTTS (CrispASR) models ({step}/{total}): {talkerFileName}");
            await DownloadAndVerify(talkerPath, talkerFileName, progress, cancellationToken);
        }
        if (needCodec)
        {
            step++;
            titleProgress?.Invoke($"Downloading IndexTTS (CrispASR) models ({step}/{total}): {codecFileName}");
            await DownloadAndVerify(codecPath, codecFileName, progress, cancellationToken);
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
        if (string.Equals(fileName, IndexTtsCrispAsr.TalkerQ4KFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.IndexTtsCrispAsr.TalkerQ4K;
        }
        if (string.Equals(fileName, IndexTtsCrispAsr.TalkerQ8_0FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.IndexTtsCrispAsr.TalkerQ8_0;
        }
        if (string.Equals(fileName, IndexTtsCrispAsr.TalkerF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.IndexTtsCrispAsr.TalkerF16;
        }
        if (string.Equals(fileName, IndexTtsCrispAsr.CodecFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.IndexTtsCrispAsr.Codec;
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
                $"IndexTTS (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown IndexTTS (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || IndexTtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
