using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IZonosTtsCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads ZONOS3-Mini for Zonos TTS (CrispASR) from Zyphra's own repo at a pinned revision:
/// the backbone and the fast codec (~1.1 GB together), plus the 27 preset speaker embeddings
/// (896 bytes each). crispasr has no --auto-download for this backend, so this is the only way
/// the files arrive. Every file goes to a .part file first and is SHA-256 checked before it is
/// moved into place.
/// </summary>
public class ZonosTtsCrispAsrDownloadService : IZonosTtsCrispAsrDownloadService
{
    private const string BaseUrl = "https://huggingface.co/Zyphra/ZONOS3-Mini/resolve/" + ZonosTtsCrispAsr.RepoRevision + "/gguf/";

    private readonly HttpClient _httpClient;

    public ZonosTtsCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    internal static string GetModelUrl(string fileName) => BaseUrl + fileName;

    internal static string GetVoiceUrl(ZonosVoicePresets.Preset preset) =>
        BaseUrl + "voices/" + Uri.EscapeDataString(preset.RepoFolder) + "/" + preset.FileName;

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var talkerPath = ZonosTtsCrispAsr.GetTalkerPath();
        var codecPath = ZonosTtsCrispAsr.GetCodecPath();

        // Off the caller's thread: DownloadTtsViewModel polls the returned task from a timer,
        // so anything before the first await would run on the UI thread.
        await Task.Run(() =>
        {
            EnsureRemovedIfInvalid(talkerPath, ZonosTtsCrispAsr.IsValidLocalModelFile(talkerPath, ZonosTtsCrispAsr.TalkerFileName));
            EnsureRemovedIfInvalid(codecPath, ZonosTtsCrispAsr.IsValidLocalModelFile(codecPath, ZonosTtsCrispAsr.CodecFileName));
        }, cancellationToken);

        var needTalker = !File.Exists(talkerPath);
        var needCodec = !File.Exists(codecPath);
        var missingVoices = ZonosVoicePresets.All
            .Where(p => !ZonosTtsCrispAsr.IsValidVoiceFile(ZonosTtsCrispAsr.GetVoicePath(p)))
            .ToList();
        var total = (needTalker ? 1 : 0) + (needCodec ? 1 : 0) + (missingVoices.Count > 0 ? 1 : 0);
        var step = 0;

        if (needTalker)
        {
            step++;
            titleProgress?.Invoke($"Downloading Zonos TTS (CrispASR) models ({step}/{total}): {ZonosTtsCrispAsr.TalkerFileName}");
            await DownloadAndVerify(GetModelUrl(ZonosTtsCrispAsr.TalkerFileName), talkerPath,
                DownloadHashManager.GetLatestKnownHash(DownloadHashManager.ZonosTtsCrispAsr.Backbone), progress, cancellationToken);
        }

        if (needCodec)
        {
            step++;
            titleProgress?.Invoke($"Downloading Zonos TTS (CrispASR) models ({step}/{total}): {ZonosTtsCrispAsr.CodecFileName}");
            await DownloadAndVerify(GetModelUrl(ZonosTtsCrispAsr.CodecFileName), codecPath,
                DownloadHashManager.GetLatestKnownHash(DownloadHashManager.ZonosTtsCrispAsr.Codec), progress, cancellationToken);
        }

        if (missingVoices.Count > 0)
        {
            step++;
            titleProgress?.Invoke($"Downloading Zonos TTS (CrispASR) models ({step}/{total}): {missingVoices.Count} voices");
            for (var i = 0; i < missingVoices.Count; i++)
            {
                var preset = missingVoices[i];
                var path = ZonosTtsCrispAsr.GetVoicePath(preset);
                TryDelete(path);
                await DownloadAndVerify(GetVoiceUrl(preset), path, preset.Sha256, null, cancellationToken);
                progress?.Report((i + 1) / (float)missingVoices.Count);
            }
        }
    }

    private async Task DownloadAndVerify(string url, string finalPath, string? expectedSha256, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, url, partPath, progress, cancellationToken);
            await VerifyFile(partPath, expectedSha256, Path.GetFileName(finalPath), cancellationToken);
            File.Move(partPath, finalPath, overwrite: true);
        }
        catch
        {
            TryDelete(partPath);
            throw;
        }
    }

    private static async Task VerifyFile(string filePath, string? expected, string fileName, CancellationToken cancellationToken)
    {
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
                $"Zonos TTS (CrispASR) file {fileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path, bool isValid)
    {
        if (File.Exists(path) && !isValid)
        {
            TryDelete(path);
        }
    }
}
