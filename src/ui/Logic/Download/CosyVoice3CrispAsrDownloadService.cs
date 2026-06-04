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
/// Downloads the CosyVoice3 LLM GGUF (Q4_K ~384 MB or F16 ~1.29 GB) into SE's CrispASR/models
/// folder. Companion files (flow / hift / s3tok / campplus / voices, ~360-665 MB combined) are
/// left to crispasr's --auto-download — see <see cref="CosyVoice3CrispAsr"/> for why we don't
/// stage them ourselves. Same .part + size-check pattern as VibeVoice/IndexTTS download services
/// so an interrupted download can't leave a truncated file at the real path.
/// </summary>
public class CosyVoice3CrispAsrDownloadService : ICosyVoice3CrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [CosyVoice3CrispAsr.LlmQ4KFileName] =
            "https://huggingface.co/cstr/cosyvoice3-0.5b-2512-GGUF/resolve/main/cosyvoice3-llm-q4_k.gguf",
        [CosyVoice3CrispAsr.LlmF16FileName] =
            "https://huggingface.co/cstr/cosyvoice3-0.5b-2512-GGUF/resolve/main/cosyvoice3-llm-f16.gguf",
    };

    public CosyVoice3CrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var llmFileName = CosyVoice3CrispAsr.GetLlmFileName(modelKey);
        var llmPath = CosyVoice3CrispAsr.GetLlmPath(modelKey);

        await Task.Run(() =>
        {
            CosyVoice3CrispAsr.TrySeedModelFromCrispAsrCache(llmFileName, llmPath);
            EnsureRemovedIfInvalid(llmPath, llmFileName);
        }, cancellationToken);

        if (CosyVoice3CrispAsr.IsValidLocalModelFile(llmPath, llmFileName))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading CosyVoice3 (CrispASR) model: {llmFileName}");
        await DownloadAndVerify(llmPath, llmFileName, progress, cancellationToken);
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
        {
            return DownloadHashManager.CosyVoice3CrispAsr.LlmQ4K;
        }
        if (string.Equals(fileName, CosyVoice3CrispAsr.LlmF16FileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.CosyVoice3CrispAsr.LlmF16;
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
