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
#pragma warning disable CA1416 // Validate platform compatibility
                _ = proProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                await proProcess.WaitForExitAsync(cancellationToken);

                if (File.Exists(proChainOutput))
                {
                    currentFile = proChainOutput;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return currentFile;
            }

            if (silencePaddingMs > 0)
            {
                var silenceFile = Path.Combine(waveFolder, $"pad_{Guid.NewGuid()}.wav");
                var silenceProcess = FfmpegGenerator.GenerateSilence(silenceFile, silencePaddingMs);
#pragma warning disable CA1416 // Validate platform compatibility
                _ = silenceProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                await silenceProcess.WaitForExitAsync(cancellationToken);

                var paddedOutput = Path.Combine(waveFolder, $"padded_{Guid.NewGuid()}.wav");
                var concatProcess = FfmpegGenerator.ConcatAudio(currentFile, silenceFile, paddedOutput);
#pragma warning disable CA1416 // Validate platform compatibility
                _ = concatProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                await concatProcess.WaitForExitAsync(cancellationToken);

                SafeDelete(silenceFile);
                if (File.Exists(paddedOutput))
                {
                    currentFile = paddedOutput;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return currentFile;
            }

            if (outputSampleRate > 0)
            {
                var resampledOutput = Path.Combine(waveFolder, $"sr_{Guid.NewGuid()}.wav");
                var srProcess = FfmpegGenerator.ChangeSampleRate(currentFile, resampledOutput, outputSampleRate);
#pragma warning disable CA1416 // Validate platform compatibility
                _ = srProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                await srProcess.WaitForExitAsync(cancellationToken);

                if (File.Exists(resampledOutput))
                {
                    currentFile = resampledOutput;
                }
            }

            return currentFile;
        }
        catch (OperationCanceledException)
        {
            return currentFile;
        }
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
