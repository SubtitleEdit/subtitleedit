using Nikse.SubtitleEdit.Features.Video.TextToSpeech.CloneReferenceCleaning;
using Nikse.SubtitleEdit.UiLogic;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ISidonAudioCppDownloadService
{
    /// <summary>Downloads the Sidon v0.1 GGUF into audio.cpp's models folder.</summary>
    Task DownloadModel(IProgress<float>? progress, CancellationToken cancellationToken);
}

/// <summary>
/// Model weights only: the audio.cpp binaries are the shared runtime downloaded through
/// <see cref="IndexTts25AudioCppDownloadService"/>. The GGUF is audio.cpp's own conversion of the
/// MIT-licensed Sidon v0.1 checkpoint, so there is no licence gate in front of it.
/// </summary>
public class SidonAudioCppDownloadService : ISidonAudioCppDownloadService
{
    private readonly HttpClient _httpClient;

    public SidonAudioCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModel(IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var finalPath = SidonAudioCpp.GetModelPath();

        // A truncated GGUF fails inside audio.cpp with an out-of-bounds tensor error that reads
        // like a corrupt model, so anything with the wrong size is removed and fetched again.
        await Task.Run(() =>
        {
            if (File.Exists(finalPath) && !SidonAudioCpp.IsValidLocalModelFile(finalPath))
            {
                TryDelete(finalPath);
            }
        }, cancellationToken);

        if (SidonAudioCpp.IsValidLocalModelFile(finalPath))
        {
            return;
        }

        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, SidonAudioCpp.ModelUrl, partPath, progress, cancellationToken);
            await VerifyFile(partPath, cancellationToken);
            File.Move(partPath, finalPath, overwrite: true);
        }
        catch
        {
            TryDelete(partPath);
            throw;
        }
    }

    private static async Task VerifyFile(string filePath, CancellationToken cancellationToken)
    {
        var expected = DownloadHashManager.GetLatestKnownHash(DownloadHashManager.SidonAudioCpp.ModelF32);
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
                $"Sidon model {SidonAudioCpp.ModelFileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }
}
