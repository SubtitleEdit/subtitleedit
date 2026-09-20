using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ISupertonicCrispAsrDownloadService
{
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the Supertonic (CrispASR) model into SE's CrispASR/models folder so the user gets
/// a progress dialog instead of crispasr's silent --auto-download on first synth. One GGUF, no
/// companion: the ten preset voices, the unicode indexer and the text tables are all inside it.
/// Same .part / size-check / hash-verify shape as <see cref="PocketTtsCrispAsrDownloadService"/>.
/// </summary>
public class SupertonicCrispAsrDownloadService : ISupertonicCrispAsrDownloadService
{
    private readonly HttpClient _httpClient;

    private const string ModelUrl =
        "https://huggingface.co/cstr/supertonic-3-GGUF/resolve/main/supertonic3-f16.gguf";

    public SupertonicCrispAsrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var modelPath = SupertonicCrispAsr.GetModelPath();

        // Cache seeding does a synchronous File.Copy of ~200 MB; the caller passes us the
        // resulting Task without awaiting (DownloadTtsViewModel polls it from a timer), so
        // anything before the first real await runs on the caller's thread (the UI thread when
        // invoked from the download dialog's init callback). Push the seeding and size-check
        // work onto the threadpool so the dialog stays responsive.
        await Task.Run(() =>
        {
            SupertonicCrispAsr.TrySeedModelFromCrispAsrCache(modelPath);
            EnsureRemovedIfInvalid(modelPath);
        }, cancellationToken);

        if (SupertonicCrispAsr.IsValidLocalModelFile(modelPath))
        {
            return;
        }

        titleProgress?.Invoke($"Downloading Supertonic (CrispASR) model: {SupertonicCrispAsr.ModelFileName}");
        await DownloadAndVerify(modelPath, progress, cancellationToken);
    }

    private async Task DownloadAndVerify(string finalPath, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var partPath = finalPath + ".part";
        try
        {
            await DownloadHelper.DownloadFileAsync(_httpClient, ModelUrl, partPath, progress, cancellationToken);
            await VerifyFile(partPath, cancellationToken);
            File.Move(partPath, finalPath);
        }
        catch
        {
            TryDelete(partPath);
            throw;
        }
    }

    private static async Task VerifyFile(string filePath, CancellationToken cancellationToken)
    {
        var expected = DownloadHashManager.GetLatestKnownHash(DownloadHashManager.SupertonicCrispAsr.ModelF16);
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
                $"Supertonic (CrispASR) model {SupertonicCrispAsr.ModelFileName} failed integrity check (expected SHA-256 {expected}, got {actual}).");
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }

    private static void EnsureRemovedIfInvalid(string path)
    {
        if (!File.Exists(path) || SupertonicCrispAsr.IsValidLocalModelFile(path))
        {
            return;
        }
        TryDelete(path);
    }
}
