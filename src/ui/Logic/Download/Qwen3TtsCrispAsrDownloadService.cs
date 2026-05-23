using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IQwen3TtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the Qwen3 TTS (CrispASR) talker + codec GGUFs into SE's TextToSpeech\Qwen3TtsCrispAsr\models
/// folder, replacing crispasr's built-in --auto-download fallback that drops files into
/// ~/.cache/crispasr/. Keeping models inside SE's data folder means uninstalling SE removes them,
/// portable installs stay self-contained, and the engine settings dialog can report accurate sizes.
///
/// Three files are tracked:
///  - VoiceDesign talker  : qwen3-tts-12hz-1.7b-voicedesign-q8_0.gguf (cstr's HF upload)
///  - CustomVoice talker  : qwen3-tts-12hz-1.7b-customvoice-q8_0.gguf (cstr's HF upload)
///  - 12 Hz codec/tokenizer: qwen3-tts-tokenizer-12hz.gguf (distinct from qwen3-tts.cpp's q8_0 tokenizer)
///
/// If a file already lives in ~/.cache/crispasr/ from an earlier crispasr auto-download the engine's
/// TrySeedModelFromCrispAsrCache copies it into SE's folder instead of re-pulling 2 GB - the cache
/// copy stays put so other crispasr users on the same machine are unaffected.
/// </summary>
public class Qwen3TtsCrispAsrDownloadService : IQwen3TtsCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private static readonly Dictionary<string, string> ModelUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        [Qwen3TtsCrispAsr.VoiceDesignTalkerFileName] =
            "https://huggingface.co/cstr/qwen3-tts-1.7b-voicedesign-GGUF/resolve/main/qwen3-tts-12hz-1.7b-voicedesign-q8_0.gguf",
        [Qwen3TtsCrispAsr.CustomVoiceTalkerFileName] =
            "https://huggingface.co/cstr/qwen3-tts-1.7b-customvoice-GGUF/resolve/main/qwen3-tts-12hz-1.7b-customvoice-q8_0.gguf",
        [Qwen3TtsCrispAsr.CodecFileName] =
            "https://huggingface.co/cstr/qwen3-tts-tokenizer-12hz-GGUF/resolve/main/qwen3-tts-tokenizer-12hz.gguf",
    };

    public Qwen3TtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var talkerFileName = Qwen3TtsCrispAsr.GetTalkerFileName(modelKey);
        var codecFileName = Qwen3TtsCrispAsr.CodecFileName;

        var talkerPath = Qwen3TtsCrispAsr.GetTalkerPath(modelKey);
        var codecPath = Qwen3TtsCrispAsr.GetCodecPath();

        // Try the local crispasr cache before the network — same bytes, no 2 GB re-download.
        Qwen3TtsCrispAsr.TrySeedModelFromCrispAsrCache(talkerFileName, talkerPath);
        Qwen3TtsCrispAsr.TrySeedModelFromCrispAsrCache(codecFileName, codecPath);

        var needTalker = !File.Exists(talkerPath);
        var needCodec = !File.Exists(codecPath);
        var total = (needTalker ? 1 : 0) + (needCodec ? 1 : 0);
        var step = 0;

        if (needTalker)
        {
            step++;
            titleProgress?.Invoke($"Downloading Qwen3 TTS (CrispASR) models ({step}/{total}): {talkerFileName}");
            await DownloadAndVerify(talkerPath, talkerFileName, GetHashKey(talkerFileName), progress, cancellationToken);
        }
        if (needCodec)
        {
            step++;
            titleProgress?.Invoke($"Downloading Qwen3 TTS (CrispASR) models ({step}/{total}): {codecFileName}");
            await DownloadAndVerify(codecPath, codecFileName, GetHashKey(codecFileName), progress, cancellationToken);
        }
    }

    // Download to <path>.part, verify, then atomically rename. Without the .part dance a
    // cancel mid-download leaves a truncated file at the real path; the next attempt's
    // File.Exists check would skip the re-download. The hashed files would still fail verify
    // on next start, but CustomVoiceTalker has no hash yet — there a truncated file would be
    // silently accepted. Rename-on-success closes that hole for all three files.
    private async Task DownloadAndVerify(string finalPath, string fileName, string? hashKey, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(fileName), partPath, progress, cancellationToken);
            await VerifyFile(partPath, hashKey, fileName, cancellationToken);
            File.Move(partPath, finalPath);
        }
        catch
        {
            TryDelete(partPath);
            throw;
        }
    }

    private static string GetUrl(string fileName)
    {
        if (!ModelUrls.TryGetValue(fileName, out var url))
        {
            throw new ArgumentException($"Unknown Qwen3 TTS (CrispASR) model: {fileName}", nameof(fileName));
        }
        return url;
    }

    private static string? GetHashKey(string fileName)
    {
        if (string.Equals(fileName, Qwen3TtsCrispAsr.VoiceDesignTalkerFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Qwen3TtsCrispAsr.VoiceDesignTalker;
        }
        if (string.Equals(fileName, Qwen3TtsCrispAsr.CustomVoiceTalkerFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Qwen3TtsCrispAsr.CustomVoiceTalker;
        }
        if (string.Equals(fileName, Qwen3TtsCrispAsr.CodecFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DownloadHashManager.Qwen3TtsCrispAsr.Codec;
        }
        return null;
    }

    // Mirrors Qwen3TtsCppDownloadService.VerifyArchive but async/cancellable since these GGUFs
    // are 2 GB and synchronous SHA-256 would freeze the dialog for ~10-20 s on cancel.
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
                $"Qwen3 TTS (CrispASR) model {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }
}
