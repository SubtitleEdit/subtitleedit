using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IZonosTtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the Zonos TTS (CrispASR) transformer (Q8_0) plus the DAC 44 kHz codec into SE's
/// CrispASR/models folder so the user gets a progress dialog instead of crispasr's silent
/// --auto-download on first synth. Same shape as <see cref="IndexTtsCrispAsrDownloadService"/>,
/// minus the model-key plumbing (Zonos is a single fixed quant here).
/// </summary>
public class ZonosTtsCrispAsrDownloadService : IZonosTtsCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [ZonosTtsCrispAsr.TalkerFileName] =
            "https://huggingface.co/cstr/zonos-v0.1-transformer-GGUF/resolve/main/zonos-v0.1-transformer-q8_0.gguf",
        [ZonosTtsCrispAsr.CodecFileName] =
            "https://huggingface.co/cstr/dac-44khz-GGUF/resolve/main/dac-44khz-f16.gguf",
    };

    public ZonosTtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var talkerFileName = ZonosTtsCrispAsr.TalkerFileName;
        var codecFileName = ZonosTtsCrispAsr.CodecFileName;

        var talkerPath = ZonosTtsCrispAsr.GetTalkerPath();
        var codecPath = ZonosTtsCrispAsr.GetCodecPath();

        // Cache seeding does synchronous File.Copy of up to ~1.8 GB; the caller passes us the
        // resulting Task without awaiting (DownloadTtsViewModel polls it from a timer), so
        // anything before the first real await runs on the caller's thread (the UI thread when
        // invoked from the download dialog's init callback). Push the seeding and size-check
        // work onto the threadpool so the dialog stays responsive.
        await Task.Run(() =>
        {
            ZonosTtsCrispAsr.TrySeedModelFromCrispAsrCache(talkerFileName, talkerPath);
            ZonosTtsCrispAsr.TrySeedModelFromCrispAsrCache(codecFileName, codecPath);
            EnsureRemovedIfInvalid(talkerPath, talkerFileName);
            EnsureRemovedIfInvalid(codecPath, codecFileName);
        }, cancellationToken);

        var needTalker = !ZonosTtsCrispAsr.IsValidLocalModelFile(talkerPath, talkerFileName);
        var needCodec = !ZonosTtsCrispAsr.IsValidLocalModelFile(codecPath, codecFileName);
        var total = (needTalker ? 1 : 0) + (needCodec ? 1 : 0);
        var step = 0;

        if (needTalker)
        {
            step++;
            titleProgress?.Invoke($"Downloading Zonos TTS (CrispASR) models ({step}/{total}): {talkerFileName}");
            await DownloadAndVerify(talkerPath, talkerFileName, progress, cancellationToken);
        }
        if (needCodec)
        {
            step++;
            titleProgress?.Invoke($"Downloading Zonos TTS (CrispASR) models ({step}/{total}): {codecFileName}");
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
        if (string.Equals(fileName, ZonosTtsCrispAsr.TalkerFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.ZonosTtsCrispAsr.TalkerQ8_0;
        }
        if (string.Equals(fileName, ZonosTtsCrispAsr.CodecFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.ZonosTtsCrispAsr.Codec;
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
                $"Zonos TTS (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown Zonos TTS (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || ZonosTtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
