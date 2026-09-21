using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.GetAudioClips;

public partial class GetAudioClipsViewModel : ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public List<AudioClip> AudioClips { get; set; }
    public bool OkPressed { get; private set; }

    private string _videoFileName;
    private int _audioTrackFfIndex;
    private bool _useCenterChannelOnly;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private List<SubtitleLineViewModel> _lines;
    private int _started;

    private const int MaxLoggedFailures = 5;
    private const int MaxShownLineNumbers = 25;

    public GetAudioClipsViewModel()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        StatusText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;
        AudioClips = new List<AudioClip>();
        _videoFileName = string.Empty;
        _audioTrackFfIndex = -1;
        _lines = new List<SubtitleLineViewModel>();
    }

    public void Initialize(string videoFileName, List<SubtitleLineViewModel> lines, int audioTrackFfIndex = -1)
    {
        _videoFileName = videoFileName;
        _lines = lines;
        _audioTrackFfIndex = audioTrackFfIndex;
    }

    private void ExtractLines()
    {
        // Probed once per run - FfmpegMediaInfo.Parse spawns ffmpeg, so it must not run per line.
        _useCenterChannelOnly = Se.Settings.General.FfmpegUseCenterChannelOnly &&
                                FfmpegMediaInfo.Parse(_videoFileName).HasFrontCenterAudio(_audioTrackFfIndex);

        // A line without a clip is skipped, not fatal: one line with its end before its start used
        // to throw away the clips of every other selected line.
        var skippedLineNumbers = new List<int>();
        var count = 0;
        foreach (var line in _lines)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            count++;
            ReportProgress(count);

            if (!FfmpegGenerator.HasClipDuration(line.Duration.TotalSeconds))
            {
                Se.LogError($"Audio clips: line {line.Number} skipped, it has no duration ({line.StartTime} --> {line.EndTime})");
                skippedLineNumbers.Add(line.Number);
                continue;
            }

            var outputFileName = Path.Combine(Path.GetTempPath(), $"se_audioclip_{Guid.NewGuid()}.wav");
            var arguments = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
                _videoFileName,
                line.StartTime.TotalSeconds,
                line.Duration.TotalSeconds,
                _useCenterChannelOnly,
                outputFileName,
                _audioTrackFfIndex);
            var output = new FfmpegOutputTail();

            // One process per line - without the using, extracting clips for a long subtitle
            // leaves a handle per line behind.
            using var process = FfmpegGenerator.GetProcess(arguments, output.Handler);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Process may have already exited
                }
                return;
            }

            if (process.ExitCode == 0 && File.Exists(outputFileName))
            {
                AudioClips.Add(new AudioClip(outputFileName, line));
            }
            else
            {
                // An unreadable video fails on every line - the first few say all there is to say.
                if (skippedLineNumbers.Count < MaxLoggedFailures)
                {
                    FfmpegGenerator.LogClipFailure($"Audio clips: line {line.Number} skipped", arguments, process.ExitCode, output);
                }

                TryDelete(outputFileName);
                skippedLineNumbers.Add(line.Number);
            }
        }

        if (AudioClips.Count == 0 && skippedLineNumbers.Count > 0)
        {
            Se.LogError($"Audio clips: no clip could be extracted from {_lines.Count} line(s) of \"{_videoFileName}\"");
            Dispatcher.UIThread.Post(async () =>
            {
                await MessageBox.Show(Window!,
                    Se.Language.General.Error,
                    "Could not extract audio clip from video.");
                Close();
            });
            return;
        }

        OkPressed = true;
        if (skippedLineNumbers.Count == 0)
        {
            Close();
            return;
        }

        Se.LogError($"Audio clips: {skippedLineNumbers.Count} of {_lines.Count} line(s) skipped: {string.Join(", ", skippedLineNumbers)}");
        // The full list is in the error log; the message box only has room for so many numbers.
        var shown = string.Join(", ", skippedLineNumbers.Take(MaxShownLineNumbers)) +
                    (skippedLineNumbers.Count > MaxShownLineNumbers ? ", ..." : string.Empty);
        var message = string.Format(Se.Language.Waveform.AudioClipsSkippedLinesX, shown);
        Dispatcher.UIThread.Post(async () =>
        {
            await MessageBox.Show(Window!, Se.Language.General.Information, message);
            Close();
        });
    }

    private static void TryDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // A leftover (usually empty) clip in the temp folder is not worth failing over.
        }
    }

    /// <summary>
    /// Pushes the progress of clip <paramref name="count"/> to the UI.
    /// </summary>
    /// <remarks>
    /// ExtractLines runs on a worker thread, and these two properties are bound to the progress
    /// bar and the status line - so the assignments have to be marshalled, the same way this class
    /// already marshals its message box and its Close().
    /// </remarks>
    private void ReportProgress(int count)
    {
        var percentage = (double)count / _lines.Count * 100.0;
        var status = string.Format(Se.Language.General.FileXOfY, count, _lines.Count);
        Dispatcher.UIThread.Post(() =>
        {
            Progress = percentage;
            StatusText = status;
        });
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    [RelayCommand]
    private void CommandCancel()
    {
        _cancellationTokenSource?.Cancel();
        Close();
    }

    /// <summary>
    /// Starts the extraction, once. Returns false when a run is already going.
    /// </summary>
    /// <remarks>
    /// The window used to start this from its <c>Activated</c> event, which fires again every time
    /// the user tabs away and back - so a second extraction loop began from line one while the
    /// first was still running, and the two fought over the progress counter (#13777). The window
    /// starts it from <c>Loaded</c> now; this guard keeps the invariant with the state it belongs
    /// to, since a second loop also appends to <see cref="AudioClips"/> from another thread.
    /// </remarks>
    public bool StartAudioExtract()
    {
        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            return false;
        }

        _ = Task.Run(ExtractLines, _cancellationTokenSource.Token);
        return true;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}