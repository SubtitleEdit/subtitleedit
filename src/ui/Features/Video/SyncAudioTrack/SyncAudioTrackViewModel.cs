using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SyncAudioTrack;

public sealed class SyncAudioTrackItem
{
    /// <summary>The ffmpeg stream index ("Stream #0:N").</summary>
    public int StreamIndex { get; init; }

    /// <summary>1-based position among the audio tracks - mpv's "aid" when it reports no ffmpeg index.</summary>
    public int AudioNumber { get; init; }

    public string Display { get; init; } = string.Empty;

    public override string ToString() => Display;
}

/// <summary>
/// Video > Sync audio track: play the video with one audio track moved by an offset (live, via the
/// player's audio delay), then save a copy of the video with that track shifted - stream copy, no
/// re-encode (<see cref="FfmpegGenerator.ShiftAudioTrack"/>).
/// </summary>
public partial class SyncAudioTrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SyncAudioTrackItem> _audioTracks = new();
    [ObservableProperty] private SyncAudioTrackItem? _selectedAudioTrack;
    [ObservableProperty] private bool _isAudioTrackPickerVisible;
    [ObservableProperty] private int _offsetMs;
    [ObservableProperty] private bool _isPreviewNotSupported;
    [ObservableProperty] private bool _isSaving;
    // Hold off system sleep while the job runs (#16183); the display may still turn off.
    private readonly SleepInhibitorScope _sleepInhibitor = new(Se.Language.Video.SyncAudioTrackTitle);

    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private string _errorText = string.Empty;

    public Window? Window { get; set; }
    public VideoPlayerControl VideoPlayer { get; internal set; }
    public string? SavedFileName { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string _videoFileName = string.Empty;
    private double _startPositionSeconds;
    private List<int> _streamIndexes = new();
    private double _durationSeconds;
    private CancellationTokenSource? _cancellationTokenSource;

    public SyncAudioTrackViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        VideoPlayer = new VideoPlayerControl(new EmptyVideoPlayer());
    }

    public void Initialize(string videoFileName, double startPositionSeconds)
    {
        _videoFileName = videoFileName;
        _startPositionSeconds = startPositionSeconds;
    }

    /// <summary>
    /// Fills the track list from ffmpeg's stream list: every stream index (for the remux) and the
    /// audio streams (for the picker).
    /// </summary>
    internal void SetMediaInfo(FfmpegMediaInfo2 mediaInfo)
    {
        // Data streams (MOV timecode, SCTE-35, ...) have no codec - "-c copy" cannot mux them and
        // ffmpeg aborts, so they are left out of the copy.
        var copiedTracks = mediaInfo.Tracks.Where(t => !IsDataStream(t)).ToList();
        _streamIndexes = copiedTracks.Select(t => t.StreamIndex).Where(i => i >= 0).ToList();
        if (_streamIndexes.Count != copiedTracks.Count)
        {
            _streamIndexes.Clear(); // not all indexes known - the remux then appends the track
        }

        _durationSeconds = mediaInfo.Duration?.TotalSeconds ?? 0;
        AudioTracks.Clear();
        var audioNumber = 0;
        foreach (var track in mediaInfo.Tracks.Where(t => t.TrackType == FfmpegTrackType.Audio))
        {
            audioNumber++;
            var language = string.IsNullOrEmpty(track.Language) ? string.Empty : track.Language + " - ";
            AudioTracks.Add(new SyncAudioTrackItem
            {
                StreamIndex = track.StreamIndex >= 0 ? track.StreamIndex : audioNumber,
                AudioNumber = audioNumber,
                Display = $"#{audioNumber}: {language}{track.TrackInfo}",
            });
        }

        IsAudioTrackPickerVisible = AudioTracks.Count > 1;
        ErrorText = AudioTracks.Count == 0 ? Se.Language.Video.SyncAudioTrackNoAudio : string.Empty;
        SelectedAudioTrack = AudioTracks.FirstOrDefault();
    }

    private static bool IsDataStream(FfmpegTrackInfo track) =>
        track.TrackType == FfmpegTrackType.Other &&
        track.TypeName is "Data" or "Unknown";

    partial void OnOffsetMsChanged(int value)
    {
        VideoPlayer.VideoPlayer.SetAudioDelay(value / 1000.0);
    }

    partial void OnSelectedAudioTrackChanged(SyncAudioTrackItem? value)
    {
        if (value == null)
        {
            return;
        }

        // mpv numbers its audio tracks 1..n; match on the ffmpeg index when it reports one
        var player = VideoPlayer.VideoPlayer;
        var tracks = player.GetAudioTracks();
        var track = tracks.FirstOrDefault(t => t.FfIndex == value.StreamIndex);
        player.SetAudioTrack(track?.Id ?? value.AudioNumber);
        player.SetAudioDelay(OffsetMs / 1000.0);
    }

    [RelayCommand]
    private void Nudge(string? milliseconds)
    {
        if (int.TryParse(milliseconds, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var ms))
        {
            OffsetMs += ms;
        }
    }

    [RelayCommand]
    private void ResetOffset()
    {
        OffsetMs = 0;
    }

    [RelayCommand]
    private async Task SaveVideo()
    {
        if (Window == null || SelectedAudioTrack == null || IsSaving)
        {
            return;
        }

        var extension = Path.GetExtension(_videoFileName);
        var suggestedName = Path.Combine(Path.GetDirectoryName(_videoFileName) ?? string.Empty,
            Path.GetFileNameWithoutExtension(_videoFileName) + "_synced" + extension);
        var outputFileName = await _fileHelper.PickSaveFile(Window, extension, suggestedName, Se.Language.Video.SyncAudioTrackSaveVideoDotDotDot);
        if (string.IsNullOrEmpty(outputFileName))
        {
            return;
        }

        if (string.Equals(Path.GetFullPath(outputFileName), Path.GetFullPath(_videoFileName), StringComparison.OrdinalIgnoreCase))
        {
            // ffmpeg cannot write over the file it is reading
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.General.CouldNotSaveFileXErrorY, outputFileName, string.Empty),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        VideoPlayer.VideoPlayer.Pause();
        _cancellationTokenSource = new CancellationTokenSource();
        var output = new StringBuilder();
        var progressTracker = new FfmpegProgressTracker(_durationSeconds);
        IsSaving = true;
        ProgressValue = 0;
        ProgressText = Se.Language.Video.SyncAudioTrackSaving;
        int exitCode;
        Process? process = null;
        try
        {
            var dropNegativePackets = OffsetMs < 0 && await Task.Run(FfmpegGenerator.NoiseBitstreamFilterCanDrop);
            process = FfmpegGenerator.ShiftAudioTrack(_videoFileName, outputFileName, _streamIndexes,
                SelectedAudioTrack.StreamIndex, SelectedAudioTrack.AudioNumber, OffsetMs / 1000.0, dropNegativePackets, (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                    {
                        return;
                    }

                    lock (output)
                    {
                        output.AppendLine(e.Data);
                    }

                    if (progressTracker.TryGetNewPercent(e.Data, out var percent))
                    {
                        Dispatcher.UIThread.Post(() => ProgressValue = percent);
                    }
                });
            await process.StartAndWaitAsync(_cancellationTokenSource.Token);
            exitCode = process.ExitCode;
        }
        catch (OperationCanceledException)
        {
            // The cancel kills ffmpeg without waiting for it to exit - until it has, the partial
            // output is still open and (on Windows) cannot be deleted.
            if (process != null)
            {
                await Task.Run(() => WaitForExitNoError(process, 2000));
            }

            TryDelete(outputFileName);
            return;
        }
        finally
        {
            process?.Dispose();
            IsSaving = false;
            ProgressText = string.Empty;
        }

        if (exitCode != 0 || !File.Exists(outputFileName) || new FileInfo(outputFileName).Length == 0)
        {
            string log;
            lock (output)
            {
                log = output.ToString();
            }

            SeLogger.Error($"Sync audio track: ffmpeg failed (exit code {exitCode}){Environment.NewLine}{log}");
            TryDelete(outputFileName);
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.General.CouldNotSaveFileXErrorY, outputFileName, LastLines(log, 6)),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SavedFileName = outputFileName;
        await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.FileSaved, string.Format(Se.Language.General.FileSavedToX, outputFileName), outputFileName, true, true);
        });
    }

    private static string LastLines(string text, int count)
    {
        var lines = text.SplitToLines().Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        return string.Join(Environment.NewLine, lines.Skip(Math.Max(0, lines.Count - count)));
    }

    private static void WaitForExitNoError(Process process, int milliseconds)
    {
        try
        {
            process.WaitForExit(milliseconds);
        }
        catch
        {
            // not started, or already gone
        }
    }

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

    [RelayCommand]
    private void Cancel()
    {
        if (IsSaving)
        {
            _cancellationTokenSource?.Cancel();
            return;
        }

        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }

    internal void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
        IsPreviewNotSupported = !VideoPlayer.VideoPlayer.SupportsAudioDelay;

        _ = Task.Run(() =>
        {
            var mediaInfo = FfmpegMediaInfo2.Parse(_videoFileName);
            Dispatcher.UIThread.Post(() => SetMediaInfo(mediaInfo));
        });

        // A freshly built player is not ready when the window loads
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                await VideoPlayer.Open(_videoFileName, _startPositionSeconds);
                await VideoPlayer.WaitForPlayersReadyAsync();
                if (VideoPlayer.IsDisposed)
                {
                    return;
                }

                OnSelectedAudioTrackChanged(SelectedAudioTrack);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Sync audio track: unable to open " + _videoFileName);
            }
        });
    }

    partial void OnIsSavingChanged(bool value) => _sleepInhibitor.SetActive(value);

    internal void OnClosing()
    {
        _cancellationTokenSource?.Cancel();
        _sleepInhibitor.Dispose();
        VideoPlayer.VideoPlayer.CloseFile();
        UiUtil.SaveWindowPosition(Window);

        // Here, not from Closed: the OpenGL deinit that frees mpv's render context runs before
        // Closed, so disposing there leaked the preview's mpv core (issue #15971).
        VideoPlayer.CloseAndDisposePlayer();
    }
}
