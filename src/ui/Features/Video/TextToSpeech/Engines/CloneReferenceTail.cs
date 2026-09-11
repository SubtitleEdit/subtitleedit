using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Prepares the reference WAV an in-context voice-cloning model is conditioned on, so the clone
/// does not end the way the reference ends.
/// </summary>
/// <remarks>
/// Higgs Audio v3 clips kept ending in a rising broadband hiss over the last 200-300 ms after the
/// codec-side fix (audio.cpp #454) had landed. Decoding identical codes with 8 or 64 frames of
/// tail context gave the same samples, and holding the final frame's codes decoded to a steady
/// hiss at the same level - so the noise is in the codes the language model emits, not in the
/// codec. What decides it is the reference: the model continues the reference audio in context
/// and ends its own clip the way the reference ends. A reference cut mid-noise or ending in room
/// tone (film dialogue) yields a hissy ending; one that ends in clean silence yields a clean one.
/// <para>
/// So the reference is conditioned on a copy whose tail is trimmed to the last sound (at the same
/// peak-relative threshold the pipeline trims with), faded out over <see cref="FadeOutSeconds"/>
/// and followed by <see cref="SilencePadSeconds"/> of digital silence. On 48 seeded clips per
/// treatment (four voices, same seeds) this moved the median level of the last four frames from
/// -46 dBFS to -64 dBFS and loud endings (above -40 dBFS) from 18 to 3, with no runaway
/// generations in 192 requests. The pad alone was not enough: an abrupt cut followed by silence
/// made the model emit a single loud burst before its silence, which is why the fade is there.
/// </para>
/// <para>
/// The prepared copy lives in a <c>prepared</c> folder next to the reference and is keyed on the
/// reference's size and modification time plus <see cref="RecipeVersion"/>, so an edited or
/// re-imported voice is prepared again and a recipe change invalidates every cached copy.
/// Preparation is best-effort: when ffmpeg is missing or fails, synthesis uses the reference as
/// is, exactly as before.
/// </para>
/// </remarks>
public static class CloneReferenceTail
{
    /// <summary>Bump when the recipe changes so cached copies made with the old one are redone.</summary>
    public const int RecipeVersion = 1;

    public const string PreparedFolderName = "prepared";

    /// <summary>Long enough to hide the trim edge, short enough not to soften a final consonant.</summary>
    public const double FadeOutSeconds = 0.05;

    /// <summary>Enough silence for the model to read "the utterance is over" from the reference.</summary>
    public const double SilencePadSeconds = 0.4;

    public const int SampleRate = 24000;

    /// <summary>
    /// A prepared copy with less than this much audio before the pad means the trim ate the whole
    /// reference (a clip that is all noise floor, or a threshold gone wrong) - use the original.
    /// </summary>
    public const double MinimumAudioSeconds = 1.0;

    private static readonly TimeSpan FfmpegTimeout = TimeSpan.FromSeconds(60);
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly HashSet<string> FailedThisSession = new(StringComparer.Ordinal);

    /// <summary>Where the prepared copy of <paramref name="referenceFileName"/> goes.</summary>
    public static string GetPreparedFileName(string referenceFileName)
    {
        var folder = Path.GetDirectoryName(referenceFileName) ?? string.Empty;
        return Path.Combine(folder, PreparedFolderName, Path.GetFileNameWithoutExtension(referenceFileName) + ".wav");
    }

    /// <summary>
    /// Identity of the reference the prepared copy was made from, or null when it cannot be read.
    /// </summary>
    public static string? BuildStamp(string referenceFileName)
    {
        try
        {
            var info = new FileInfo(referenceFileName);
            if (!info.Exists)
            {
                return null;
            }

            return string.Create(CultureInfo.InvariantCulture, $"v{RecipeVersion}|{info.Length}|{info.LastWriteTimeUtc.Ticks}");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Minimum byte size of a usable prepared copy: header plus audio plus pad.</summary>
    public static long MinimumPreparedBytes =>
        44 + (long)((MinimumAudioSeconds + SilencePadSeconds) * SampleRate) * 2;

    /// <summary>
    /// The prepared copy of <paramref name="referenceFileName"/>, made now if it is missing or
    /// stale; the reference itself when it cannot be prepared. Never throws except for
    /// cancellation.
    /// </summary>
    /// <param name="enginePrefix">Engine name for log lines, e.g. "Higgs Audio v3 (audio.cpp)".</param>
    public static async Task<string> PrepareAsync(string referenceFileName, string enginePrefix, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(referenceFileName) || !File.Exists(referenceFileName))
        {
            return referenceFileName;
        }

        var stamp = BuildStamp(referenceFileName);
        if (stamp == null)
        {
            return referenceFileName;
        }

        var prepared = GetPreparedFileName(referenceFileName);
        var stampFileName = prepared + ".stamp";
        if (IsCurrent(prepared, stampFileName, stamp))
        {
            return prepared;
        }

        await Gate.WaitAsync(cancellationToken);
        try
        {
            if (IsCurrent(prepared, stampFileName, stamp))
            {
                return prepared;
            }

            if (FailedThisSession.Contains(referenceFileName))
            {
                return referenceFileName;
            }

            var partFileName = prepared + ".part.wav";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(prepared)!);
                TryDelete(partFileName);

                var peakDbfs = await TtsSilenceThreshold.MeasurePeakDbfsAsync(referenceFileName, cancellationToken, FfmpegTimeout);
                using (var ffmpeg = FfmpegGenerator.PrepareCloneReferenceTail(
                           referenceFileName,
                           partFileName,
                           TtsSilenceThreshold.Amplitude(peakDbfs),
                           FadeOutSeconds,
                           SilencePadSeconds,
                           SampleRate))
                {
                    await ffmpeg.StartAndWaitAsync(cancellationToken, FfmpegTimeout);
                    if (ffmpeg.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"ffmpeg exited with code {ffmpeg.ExitCode}");
                    }
                }

                var length = new FileInfo(partFileName).Length;
                if (length < MinimumPreparedBytes)
                {
                    throw new InvalidOperationException(
                        $"prepared copy is {length} bytes; less than {MinimumAudioSeconds:0.#} s of audio survived the trim");
                }

                File.Move(partFileName, prepared, overwrite: true);
                File.WriteAllText(stampFileName, stamp);
                Se.WriteToolsLog(
                    $"{enginePrefix}: prepared cloning reference '{Path.GetFileName(referenceFileName)}' "
                    + $"(peak {FormatDb(peakDbfs)}, trim threshold {TtsSilenceThreshold.DbLiteral(peakDbfs)}, "
                    + $"fade {FadeOutSeconds * 1000:0} ms, pad {SilencePadSeconds * 1000:0} ms) -> {prepared}");
                return prepared;
            }
            catch (OperationCanceledException)
            {
                TryDelete(partFileName);
                throw;
            }
            catch (Exception ex)
            {
                TryDelete(partFileName);
                FailedThisSession.Add(referenceFileName);
                Se.LogError(ex, $"{enginePrefix}: could not prepare cloning reference '{referenceFileName}'; using it as is");
                Se.WriteToolsLog($"{enginePrefix}: could not prepare cloning reference '{referenceFileName}' ({ex.Message}); using it as is");
                return referenceFileName;
            }
        }
        finally
        {
            Gate.Release();
        }
    }

    private static bool IsCurrent(string prepared, string stampFileName, string stamp)
    {
        try
        {
            return File.Exists(prepared)
                   && File.Exists(stampFileName)
                   && string.Equals(File.ReadAllText(stampFileName), stamp, StringComparison.Ordinal)
                   && new FileInfo(prepared).Length >= MinimumPreparedBytes;
        }
        catch
        {
            return false;
        }
    }

    private static string FormatDb(double? dbfs) =>
        dbfs.HasValue ? dbfs.Value.ToString("0.0", CultureInfo.InvariantCulture) + " dBFS" : "unknown";

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
            // Best effort - a leftover .part file is overwritten by the next attempt.
        }
    }
}
