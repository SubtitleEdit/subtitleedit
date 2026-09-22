using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

// "Show speech only": the waveform drawn from the audio with music and sound effects removed
// (CrispASR source separation). Deliberately kept apart from the normal waveform extraction:
// that one always runs first and shows its result, and this swaps the peaks once - minutes
// later - the speech-only ones exist. Both peak files stay cached side by side.
public partial class MainViewModel
{
    private int _speechOnlyWaveformSequence;
    private readonly Lock _speechOnlyWaveformLock = new();
    private string? _speechOnlyWaveformRunningFor;
    private int _speechOnlyWaveformRunningSequence;
    private volatile bool _speechOnlyWaveformIndicatorShown;
    private Process? _speechOnlyWaveformProcess;

    /// <summary>
    /// On shutdown nothing polls the generation any more, so its process has to be killed here or
    /// the separation keeps the GPU busy for minutes after Subtitle Edit is gone.
    /// </summary>
    private void StopSpeechOnlyWaveform()
    {
        Interlocked.Increment(ref _speechOnlyWaveformSequence);
        try
        {
            var process = _speechOnlyWaveformProcess;
            if (process != null && !process.HasExited)
            {
#pragma warning disable CA1416
                process.Kill(true);
#pragma warning restore CA1416
            }
        }
        catch
        {
            // already gone or disposed
        }
    }

    /// <summary>Context menu toggle. Turning it on may first ask for the runtime and the model.</summary>
    internal async Task SetWaveformSpeechOnlyAsync(bool showSpeechOnly)
    {
        if (showSpeechOnly)
        {
            var featureName = Se.Language.Waveform.ShowSpeechOnly;
            if (Window == null ||
                !await TtsVoiceInstaller.EnsureCrispAsrForSpeechRemoval(Window, _windowService, featureName) ||
                !await SpeechIsolationModelDownload.EnsureDownloadedAsync(Window, _windowService, new CrispAsrCohere(), featureName))
            {
                return;
            }
        }

        Se.Settings.Waveform.ShowSpeechOnly = showSpeechOnly;

        var videoFileName = _videoFileName;
        if (string.IsNullOrEmpty(videoFileName))
        {
            return;
        }

        var trackNumber = _audioTrack?.FfIndex ?? -1;
        var peakWaveFileName = await Task.Run(() => WavePeakGenerator2.GetPeakWaveFileName(videoFileName, trackNumber));
        if (showSpeechOnly)
        {
            ApplySpeechOnlyWaveformIfEnabled(videoFileName, trackNumber, peakWaveFileName);
            return;
        }

        // Back to the normal waveform: stop a generation that is still running and reload the
        // normal peaks, which are always on disk by the time the speech-only ones were wanted.
        Interlocked.Increment(ref _speechOnlyWaveformSequence);
        var normalPeaks = File.Exists(peakWaveFileName) ? await Task.Run(() => TryLoadCachedPeaks(peakWaveFileName)) : null;
        if (AudioVisualizer == null || _videoFileName != videoFileName)
        {
            return;
        }

        if (normalPeaks != null)
        {
            // only the peaks: the shot changes and the spectrogram are on the SMPTE time line already
            AudioVisualizer.WavePeaks = IsSmpteTimingEnabled ? Controls.AudioVisualizerControl.AudioVisualizer.ToSmpteDropFrameTime(normalPeaks) : normalPeaks;
            _updateAudioVisualizer = true;
        }
        else if (!IsWaveformGenerating)
        {
            // The normal waveform was never made (auto-generate off) or its cache was cleared.
            // Leaving the speech-only one up would be the option ignoring being switched off.
            AudioVisualizer.WavePeaks = null;
            ShowClickToGenerateWaveformHint();
        }
    }

    /// <summary>
    /// Called whenever the normal peaks of a video have just been shown. With the option on, the
    /// speech-only peaks replace them - from the cache, or once they have been generated.
    /// </summary>
    private void ApplySpeechOnlyWaveformIfEnabled(string videoFileName, int trackNumber, string peakWaveFileName)
    {
        if (!Se.Settings.Waveform.ShowSpeechOnly ||
            string.IsNullOrEmpty(videoFileName) ||
            videoFileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            videoFileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // The normal peaks get shown more than once for the same video - the extraction finishing
        // after the option was switched on, a regenerated spectrogram - and a generation that is
        // minutes in must not be killed and started over for that.
        var speechPeakFileName = SpeechOnlyWaveform.GetPeakFileName(peakWaveFileName);
        lock (_speechOnlyWaveformLock)
        {
            if (_speechOnlyWaveformRunningFor == speechPeakFileName &&
                _speechOnlyWaveformRunningSequence == Volatile.Read(ref _speechOnlyWaveformSequence) &&
                _videoFileName == videoFileName)
            {
                return;
            }
        }

        var sequence = Interlocked.Increment(ref _speechOnlyWaveformSequence);
        bool IsStale() => sequence != Volatile.Read(ref _speechOnlyWaveformSequence) || _videoFileName != videoFileName;

        _ = Task.Run(async () =>
        {
            try
            {
                var speechPeaks = File.Exists(speechPeakFileName) ? TryLoadCachedPeaks(speechPeakFileName) : null;
                var generated = false;
                if (speechPeaks == null)
                {
                    lock (_speechOnlyWaveformLock)
                    {
                        _speechOnlyWaveformRunningFor = speechPeakFileName;
                        _speechOnlyWaveformRunningSequence = sequence;
                    }

                    try
                    {
                        speechPeaks = await GenerateSpeechOnlyPeaksAsync(videoFileName, trackNumber, speechPeakFileName, IsStale);
                    }
                    finally
                    {
                        lock (_speechOnlyWaveformLock)
                        {
                            if (_speechOnlyWaveformRunningSequence == sequence)
                            {
                                _speechOnlyWaveformRunningFor = null;
                            }
                        }

                        ClearSpeechOnlyWaveformIndicator();
                    }

                    generated = speechPeaks != null;
                }

                if (speechPeaks == null || IsStale())
                {
                    return;
                }

                Dispatcher.UIThread.Post(() =>
                {
                    if (IsStale() || AudioVisualizer == null || !Se.Settings.Waveform.ShowSpeechOnly)
                    {
                        return;
                    }

                    // With SMPTE timing on, the peaks they replace were compressed - raw ones
                    // drift some 3.6 seconds per hour against the subtitles and shot changes.
                    AudioVisualizer.WavePeaks = IsSmpteTimingEnabled ? Controls.AudioVisualizerControl.AudioVisualizer.ToSmpteDropFrameTime(speechPeaks) : speechPeaks;
                    _updateAudioVisualizer = true;
                    if (generated)
                    {
                        ShowStatus(Se.Language.Waveform.SpeechOnlyWaveformReady);
                    }
                });
            }
            catch (Exception exception)
            {
                Se.LogError(exception, $"Speech-only waveform failed for \"{videoFileName}\"");
            }
        });
    }

    private async Task<WavePeakData2?> GenerateSpeechOnlyPeaksAsync(string videoFileName, int trackNumber, string speechPeakFileName, Func<bool> isStale)
    {
        var crispAsr = new CrispAsrCohere();
        var executable = crispAsr.GetExecutable();
        var modelFileName = crispAsr.GetModelForCmdLine(SpeechIsolationModel.FileName);
        if (!File.Exists(executable) || !File.Exists(modelFileName) || !FfmpegHelper.IsFfmpegInstalled())
        {
            // Opening a video must never pop a download prompt - the context menu toggle is
            // where the runtime and the model are asked for.
            return null;
        }

        // The stem is 44.1 kHz stereo - over a gigabyte for a feature film - so everything goes
        // into a folder of its own that is gone as soon as the peaks are written.
        var workFolder = Path.Combine(Path.GetTempPath(), "se-waveform-speech-" + Guid.NewGuid());
        Directory.CreateDirectory(workFolder);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var audioFileName = Path.Combine(workFolder, "audio.wav");
            var ffmpeg = File.Exists(Se.Settings.General.FfmpegPath) ? Se.Settings.General.FfmpegPath : "ffmpeg";
            var extractExitCode = await RunSpeechOnlyWaveformProcessAsync(
                ffmpeg, SpeechOnlyWaveform.BuildExtractArguments(videoFileName, trackNumber, audioFileName), stopwatch, isStale);
            if (extractExitCode != 0 || !File.Exists(audioFileName))
            {
                return FailSpeechOnlyWaveform(isStale, $"ffmpeg could not extract the audio (exit code {extractExitCode})");
            }

            var separateArguments = SpeechIsolationModel.BuildSeparateArguments(modelFileName, audioFileName, workFolder);
            Se.WriteToolsLog($"{executable} {separateArguments}");
            var progress = new SpeechIsolationProgress(SpeechIsolationProgress.GetChunkCountFromWaveFile(audioFileName));
            var separateExitCode = await RunSpeechOnlyWaveformProcessAsync(executable, separateArguments, stopwatch, isStale, progress);
            var stemFileName = SpeechIsolationModel.GetSpeechStemFileName(audioFileName, workFolder);
            if (separateExitCode != 0 || !File.Exists(stemFileName))
            {
                return FailSpeechOnlyWaveform(isStale, $"the separation failed (exit code {separateExitCode})");
            }

            using var waveFile = new WavePeakGenerator2(stemFileName);
            return waveFile.GeneratePeaks(0, speechPeakFileName);
        }
        finally
        {
            try
            {
                Directory.Delete(workFolder, true);
            }
            catch
            {
                // ignore
            }
        }
    }

    private void ClearSpeechOnlyWaveformIndicator()
    {
        if (_speechOnlyWaveformIndicatorShown && _currentWaveExtractionProcess == null)
        {
            IsWaveformGenerating = false;
            WaveformGeneratingText = string.Empty;
        }

        _speechOnlyWaveformIndicatorShown = false;
    }

    private WavePeakData2? FailSpeechOnlyWaveform(Func<bool> isStale, string reason)
    {
        // A stale run was killed on purpose (video closed, option switched off) - not a failure.
        if (!isStale())
        {
            Se.WriteToolsLog("Speech-only waveform: " + reason, true);
            ShowStatus(Se.Language.Waveform.IsolatingSpeechForWaveformFailed);
        }

        return null;
    }

    /// <returns>The exit code, or -1 when the run went stale and the process was killed.</returns>
    /// <param name="progress">
    /// Fed the process output when the process is the separator, whose per-chunk lines are its
    /// only progress (#15176). Null for ffmpeg, whose output is drained and dropped.
    /// </param>
    private async Task<int> RunSpeechOnlyWaveformProcessAsync(string executable, string arguments, Stopwatch stopwatch, Func<bool> isStale, SpeechIsolationProgress? progress = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(executable, arguments)
            {
                WorkingDirectory = Path.GetDirectoryName(executable) ?? string.Empty,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        DataReceivedEventHandler onLine = (_, args) => progress?.TryUpdate(args.Data);
        process.OutputDataReceived += onLine;
        process.ErrorDataReceived += onLine;

#pragma warning disable CA1416
        process.Start();
#pragma warning restore CA1416
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _speechOnlyWaveformProcess = process;

        var lastStatusSecond = -1L;
        while (!process.HasExited)
        {
            if (isStale())
            {
                try
                {
#pragma warning disable CA1416
                    process.Kill(true);
#pragma warning restore CA1416
                }
                catch
                {
                    // already gone
                }

                return -1;
            }

            // Elapsed time, plus the separator's percentage once it has one (the GPU path never
            // prints any, and is quick). It goes in the footer's waveform indicator, not the status
            // bar: that has one slot, and a message every second for the length of a film would
            // bury every other one. While a normal extraction runs the indicator is that
            // extraction's.
            var second = stopwatch.ElapsedMilliseconds / 1000;
            if (second != lastStatusSecond && _currentWaveExtractionProcess == null)
            {
                lastStatusSecond = second;
                _speechOnlyWaveformIndicatorShown = true;
                var elapsed = new TimeCode(stopwatch.ElapsedMilliseconds).ToShortDisplayString();
                WaveformGeneratingText = string.Format(
                    Se.Language.Waveform.IsolatingSpeechForWaveformX,
                    progress?.Percent is { } percent ? $"{elapsed} - {percent}%" : elapsed);
                IsWaveformGenerating = true;
            }

            await Task.Delay(200);
        }

        return process.ExitCode;
    }
}
