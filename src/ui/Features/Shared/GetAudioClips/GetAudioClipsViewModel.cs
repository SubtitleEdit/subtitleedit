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
using System.Diagnostics;
using System.IO;
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

        var count = 0;
        foreach (var line in _lines)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            count++;
            ReportProgress(count);

            var outputFileName = Path.Combine(Path.GetTempPath(), $"se_audioclip_{Guid.NewGuid()}.wav");
            // One process per line - without the using, extracting clips for a long subtitle
            // leaves a handle per line behind.
            using var process = GetExtractProcess(_videoFileName, line, outputFileName);
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
                Dispatcher.UIThread.Post(async () =>
                {
                    await MessageBox.Show(Window!,
                        Se.Language.General.Error,
                        "Could not extract audio clip from video.");
                    Close();
                });
                return;
            }
        }

        OkPressed = true;
        Close();
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

    private Process GetExtractProcess(string videoFileName, SubtitleLineViewModel line, string outputFileName)
    {
        var arguments = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            videoFileName,
            line.StartTime.TotalSeconds,
            line.Duration.TotalSeconds,
            _useCenterChannelOnly,
            outputFileName,
            _audioTrackFfIndex);

        return FfmpegGenerator.GetProcess(arguments, OutputHandler);
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
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