using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.CloneReferenceCleaning;

/// <summary>
/// The optional "Clean voice-clone references" step: runs a cloning reference through
/// <see cref="SidonAudioCpp"/> before an engine sees it, so the clone does not pick up the music
/// or room noise under the speech it was cut from. Covers every reference SE makes or takes in -
/// waveform "Clone voice to", per-line clone clips, auto-cast speaker references and voices
/// imported in the voice manager.
/// </summary>
/// <remarks>
/// Cleaning is best effort by design. A failure (or a declined download) leaves the reference as
/// it was and cloning goes ahead with it: a reference with music under it clones a bit worse, but
/// a clone that does not happen at all is not what the user asked for.
/// </remarks>
public static class CloneReferenceCleaner
{
    public static bool IsEnabled => Se.Settings.Video.TextToSpeech.CleanCloneReferences;

    /// <summary>
    /// The audio.cpp runtime with the <c>sidon</c> family, its CLI, and the model - asking before
    /// each download. False when the user declines or something is missing; the caller then
    /// clones from the uncleaned reference.
    /// </summary>
    public static async Task<bool> EnsureInstalledAsync(Window window, IWindowService windowService)
    {
        if (!await TtsVoiceInstaller.EnsureAudioCppRuntime(window, windowService, forceRedownload: false, SidonAudioCpp.DisplayName, SidonAudioCpp.FamilyName))
        {
            return false;
        }

        if (!File.Exists(SidonAudioCpp.GetCliExecutable()))
        {
            Se.WriteToolsLog("Sidon (audio.cpp): audiocpp_cli not found: " + SidonAudioCpp.GetCliExecutable());
            return false;
        }

        if (SidonAudioCpp.IsModelInstalled())
        {
            return true;
        }

        var l = Se.Language.Video.TextToSpeech;
        var answer = await MessageBox.Show(
            window,
            l.DownloadSidonTitle,
            string.Format(l.DownloadSidonQuestionX, SidonAudioCpp.ModelSizeText),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return false;
        }

        var result = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(
            window, vm => vm.StartDownloadSidonAudioCppModel());
        return result.OkPressed && SidonAudioCpp.IsModelInstalled();
    }

    /// <summary>
    /// Cleans <paramref name="fileName"/> in place. With <paramref name="sampleRate"/> the result
    /// is resampled to that rate (the per-line clips are handed to engines as they are); without
    /// it the 48 kHz Sidon output is kept, for references an engine resamples on import.
    /// </summary>
    /// <returns>False when cleaning failed and the file was left as it was.</returns>
    public static async Task<bool> CleanFileAsync(string fileName, int? sampleRate, CancellationToken cancellationToken)
    {
        var cleanedFileName = fileName + ".sidon.wav";
        try
        {
            await SidonAudioCpp.RestoreFileAsync(fileName, cleanedFileName, cancellationToken);
            return await ReplaceWithCleanedAsync(fileName, cleanedFileName, sampleRate, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Clean voice-clone reference: Sidon failed for " + fileName);
            return false;
        }
        finally
        {
            TryDelete(cleanedFileName);
        }
    }

    /// <summary>
    /// A cleaned copy of a recording the user picked, leaving their file alone: converted to a
    /// mono WAV with the same base name (the engines name an imported voice after the file) in
    /// <paramref name="tempFolder"/>, then cleaned. Null when either step failed.
    /// </summary>
    public static async Task<string?> CleanCopyAsync(string sourceFileName, string tempFolder, CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(tempFolder);
            var copyFileName = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(sourceFileName) + ".wav");
            var arguments = $"-y -i \"{sourceFileName}\" -vn -ac 1 -c:a pcm_s16le \"{copyFileName}\"";
            using (var process = FfmpegGenerator.GetProcess(arguments, (_, _) => { }))
            {
                await process.StartAndWaitAsync(cancellationToken);
                if (process.ExitCode != 0 || !File.Exists(copyFileName) || new FileInfo(copyFileName).Length <= 44)
                {
                    Se.WriteToolsLog($"Clean voice-clone reference: converting the recording failed ({arguments})");
                    return null;
                }
            }

            return await CleanFileAsync(copyFileName, null, cancellationToken) ? copyFileName : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Clean voice-clone reference: could not clean a copy of " + sourceFileName);
            return null;
        }
    }

    /// <summary>
    /// Cleans every .wav in <paramref name="folder"/> in place, in one model load. The .txt
    /// transcript sidecars next to them are untouched. Clips Sidon did not produce output for
    /// stay as they were.
    /// </summary>
    /// <returns>The number of clips that were cleaned.</returns>
    public static async Task<int> CleanFolderAsync(string folder, int? sampleRate, Action<int, int>? progress, CancellationToken cancellationToken)
    {
        var outputFolder = Path.Combine(folder, "sidon");
        var cleaned = 0;
        try
        {
            await SidonAudioCpp.RestoreFolderAsync(folder, outputFolder, progress, cancellationToken);
            foreach (var fileName in Directory.GetFiles(folder, "*.wav"))
            {
                var cleanedFileName = Path.Combine(outputFolder, Path.GetFileName(fileName));
                if (File.Exists(cleanedFileName) && await ReplaceWithCleanedAsync(fileName, cleanedFileName, sampleRate, cancellationToken))
                {
                    cleaned++;
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Clean voice-clone references: Sidon failed for " + folder);
        }
        finally
        {
            try
            {
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
            }
            catch
            {
                // a leftover folder of cleaned copies is harmless
            }
        }

        Se.WriteToolsLog($"Clean voice-clone references: {cleaned} clip(s) cleaned in {folder}");
        return cleaned;
    }

    private static async Task<bool> ReplaceWithCleanedAsync(string fileName, string cleanedFileName, int? sampleRate, CancellationToken cancellationToken)
    {
        if (sampleRate == null)
        {
            File.Move(cleanedFileName, fileName, overwrite: true);
            return true;
        }

        var resampledFileName = fileName + ".resampled.wav";
        try
        {
            var arguments = ResampleParameters(cleanedFileName, resampledFileName, sampleRate.Value);
            using var process = FfmpegGenerator.GetProcess(arguments, (_, _) => { });
            await process.StartAndWaitAsync(cancellationToken);

            // 44 bytes is a WAV header with no samples - keep the uncleaned clip rather than that.
            if (process.ExitCode != 0 || !File.Exists(resampledFileName) || new FileInfo(resampledFileName).Length <= 44)
            {
                Se.WriteToolsLog($"Clean voice-clone reference: resampling failed ({arguments})");
                return false;
            }

            File.Move(resampledFileName, fileName, overwrite: true);
            return true;
        }
        finally
        {
            TryDelete(resampledFileName);
        }
    }

    internal static string ResampleParameters(string inputFileName, string outputFileName, int sampleRate) =>
        $"-y -i \"{inputFileName}\" -ar {sampleRate} -ac 1 -c:a pcm_s16le \"{outputFileName}\"";

    private static void TryDelete(string fileName)
    {
        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch
        {
            // best effort
        }
    }
}
