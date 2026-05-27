using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IVibeVoiceCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the VibeVoice (CrispASR) talker GGUF into SE's CrispASR/models folder so the
/// user gets a progress dialog instead of crispasr's silent --auto-download on first synth.
/// Single file (~2.8 GB Q8_0) — unlike Qwen3 TTS (CrispASR) there's no separate codec GGUF;
/// VibeVoice's single-LM architecture bundles everything. Same .part + size-check pattern
/// as <see cref="Qwen3TtsCrispAsrDownloadService"/> so a cancelled / interrupted download
/// can't leave a truncated file at the real path that loads to a segfault later.
/// </summary>
public class VibeVoiceCrispAsrDownloadService : IVibeVoiceCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private const string TalkerUrl =
        "https://huggingface.co/cstr/vibevoice-1.5b-GGUF/resolve/main/vibevoice-1.5b-tts-q8_0.gguf";

    public VibeVoiceCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var talkerFileName = VibeVoiceCrispAsr.TalkerFileName;
        var talkerPath = VibeVoiceCrispAsr.GetTalkerPath();

        // Try the local crispasr cache before the network — same bytes, no 2.8 GB re-download.
        VibeVoiceCrispAsr.TrySeedModelFromCrispAsrCache(talkerFileName, talkerPath);

        // Size-checked validity gate so a previously truncated file at the final path triggers
        // re-download instead of being mistaken for an installed model.
        EnsureRemovedIfInvalid(talkerPath, talkerFileName);

        if (VibeVoiceCrispAsr.IsValidLocalModelFile(talkerPath, talkerFileName))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading VibeVoice (CrispASR) model: {talkerFileName}");
        await DownloadAndVerify(talkerPath, talkerFileName, progress, cancellationToken);
    }

    // Download to <path>.part, then atomically rename. A cancel mid-download leaves only the
    // .part file, never a partial at the real path that the engine's IsValidLocalModelFile
    // gate would still mistake for installed.
    private async Task DownloadAndVerify(string finalPath, string fileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, TalkerUrl, partPath, progress, cancellationToken);
            // Hash registry doesn't have a VibeVoice entry yet (would require local SHA-256
            // computation); the engine's size check on next load catches truncation, which is
            // the failure mode we actually see in the wild.
            File.Move(partPath, finalPath);
        }
        catch
        {
            TryDelete(partPath);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, string fileName)
    {
        if (!File.Exists(path) || VibeVoiceCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            return;
        }
        TryDelete(path);
    }
}
