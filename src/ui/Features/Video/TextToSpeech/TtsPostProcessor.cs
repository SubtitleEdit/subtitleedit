using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Shared post-processing pipeline for TTS audio files (ProAudioChain, silence padding, sample rate conversion).
/// </summary>
public static class TtsPostProcessor
{
    public static async Task<string> ApplyPostProcessing(string currentFile, string waveFolder, CancellationToken cancellationToken)
    {
        var doProChain = Se.Settings.Video.TextToSpeech.ProAudioChainEnabled;
        var silencePaddingMs = Se.Settings.Video.TextToSpeech.SilencePaddingMs;
        var outputSampleRate = Se.Settings.Video.TextToSpeech.OutputSampleRate;

        if (!doProChain && silencePaddingMs <= 0 && outputSampleRate <= 0)
        {
            return currentFile;
        }

        try
        {
            if (doProChain)
            {
                var proChainOutput = Path.Combine(waveFolder, $"pro_{Guid.NewGuid()}.wav");
                var proProcess = FfmpegGenerator.ApplyProAudioChain(currentFile, proChainOutput);
                await proProcess.StartAndWaitAsync(cancellationToken);

                currentFile = AdoptStageOutput(currentFile, proChainOutput, "pro audio chain");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return currentFile;
            }

            if (silencePaddingMs > 0)
            {
                var silenceFile = Path.Combine(waveFolder, $"pad_{Guid.NewGuid()}.wav");
                var silenceProcess = FfmpegGenerator.GenerateSilence(silenceFile, silencePaddingMs);
                await silenceProcess.StartAndWaitAsync(cancellationToken);

                var paddedOutput = Path.Combine(waveFolder, $"padded_{Guid.NewGuid()}.wav");
                var concatProcess = FfmpegGenerator.ConcatAudio(currentFile, silenceFile, paddedOutput);
                await concatProcess.StartAndWaitAsync(cancellationToken);

                SafeDelete(silenceFile);
                currentFile = AdoptStageOutput(currentFile, paddedOutput, "silence padding");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return currentFile;
            }

            if (outputSampleRate > 0)
            {
                var resampledOutput = Path.Combine(waveFolder, $"sr_{Guid.NewGuid()}.wav");
                var srProcess = FfmpegGenerator.ChangeSampleRate(currentFile, resampledOutput, outputSampleRate);
                await srProcess.StartAndWaitAsync(cancellationToken);

                currentFile = AdoptStageOutput(currentFile, resampledOutput, "sample rate conversion");
            }

            return currentFile;
        }
        catch (OperationCanceledException)
        {
            return currentFile;
        }
        catch (Exception ex)
        {
            // Must not escape: AdoptStageOutput deletes the superseded input on each successful
            // stage, so callers that fall back to their *original* file on an exception would
            // point at a file a completed earlier stage already deleted. Returning the last
            // adopted file keeps the audio chain intact.
            SeLogger.Error(ex, "TTS post-processing failed - keeping the last successfully processed file");
            Se.WriteToolsLog($"TTS post-processing failed ({ex.Message}) - keeping \"{currentFile}\"", true);
            return currentFile;
        }
    }

    /// <summary>
    /// Adopts a stage's ffmpeg output when it was actually produced (exists and is non-empty -
    /// a bare Exists check used to adopt zero-byte files from mid-run ffmpeg failures), deleting
    /// the superseded input so a long run doesn't pile up one stale intermediate per stage per
    /// line. A failed stage keeps the input and is force-logged - it used to be silently skipped,
    /// leaving the user's enabled option quietly unapplied with no trace.
    /// </summary>
    private static string AdoptStageOutput(string inputFile, string outputFile, string stageName)
    {
        if (File.Exists(outputFile) && new FileInfo(outputFile).Length > 0)
        {
            SafeDelete(inputFile);
            return outputFile;
        }

        Se.WriteToolsLog($"TTS post-processing: {stageName} produced no output for \"{inputFile}\" - stage skipped (ffmpeg failure?)", true);
        SafeDelete(outputFile);
        return inputFile;
    }

    private static void SafeDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore
        }
    }
}
