using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.FindText;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public partial class VisualSyncViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphLeftIndex = -1;
    [ObservableProperty] private int _selectedParagraphRightIndex = -1;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _videoInfo;
    [ObservableProperty] private string _adjustInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayerControlLeft { get; set; }
    public VideoPlayerControl VideoPlayerControlRight { get; set; }
    public AudioVisualizer AudioVisualizerLeft { get; set; }
    public AudioVisualizer AudioVisualizerRight { get; set; }
    public ComboBox ComboBoxLeft { get; set; }
    public ComboBox ComboBoxRight { get; set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;

    // One per player: each keeps the temp subtitle file it handed to its own player, so a shared
    // instance would have the two players overwriting each other's file.
    private readonly IVideoPreviewSubtitle _previewSubtitleLeft;
    private readonly IVideoPreviewSubtitle _previewSubtitleRight;

    private string? _videoFileName;
    private string? _wavePeaksVideoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private VideoPreviewSubtitleContext _previewContext = VideoPreviewSubtitleContext.Default;
    private bool _updateAudioVisualizer;
    private double _lastManualOffsetSeconds;
    private double _lastManualSpeedFactor = 1.0;

    public VisualSyncViewModel(
        IWindowService windowService,
        IFileHelper fileHelper,
        IVideoPreviewSubtitle previewSubtitleLeft,
        IVideoPreviewSubtitle previewSubtitleRight)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _previewSubtitleLeft = previewSubtitleLeft;
        _previewSubtitleRight = previewSubtitleRight;

        Title = string.Empty;
        VideoInfo = string.Empty;
        AdjustInfo = string.Empty;
        _videoFileName = string.Empty;
        VideoPlayerControlLeft = new VideoPlayerControl(new EmptyVideoPlayer());
        VideoPlayerControlRight = new VideoPlayerControl(new EmptyVideoPlayer());
        AudioVisualizerLeft = new AudioVisualizer();
        AudioVisualizerRight = new AudioVisualizer();
        ComboBoxLeft = new ComboBox();
        ComboBoxRight = new ComboBox();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();

        // Toggle play/pause on surface click
        VideoPlayerControlLeft.SurfacePointerPressed += (_, __) => VideoPlayerControlLeft.TogglePlayPause();
        VideoPlayerControlRight.SurfacePointerPressed += (_, __) => VideoPlayerControlRight.TogglePlayPause();
    }

    public void AudioVisualizerLeft_OnPrimarySingleClicked(object sender, ParagraphNullableEventArgs e)
    {
        VideoPlayerControlLeft.Position = e.Seconds;
    }

    public void AudioVisualizerRight_OnPrimarySingleClicked(object sender, ParagraphNullableEventArgs e)
    {
        VideoPlayerControlRight.Position = e.Seconds;
    }

    public void Initialize(
        List<SubtitleLineViewModel> paragraphs,
        string? videoFileName,
        string? subtitleFileName,
        VideoPreviewSubtitleContext previewContext,
        AudioVisualizer? audioVisualizer,
        int audioTrackId = -1)
    {
        // No video handed down from the main window - look for one next to the subtitle file, the
        // way SE4's visual sync did. Failing that the dialog is not a dead end: "Open video file..."
        // is there to pick one. The video stays local to this dialog either way - only time codes
        // are reported back, so a video found on disk cannot walk over the "auto open video file"
        // setting in the main window.
        if (string.IsNullOrEmpty(videoFileName) &&
            !string.IsNullOrEmpty(subtitleFileName) &&
            FindVideoFileName.TryFindVideoFileName(subtitleFileName, out var foundVideoFileName))
        {
            videoFileName = foundVideoFileName;
        }

        SetVideoInFo(videoFileName);
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _videoFileName = videoFileName;
        _subtitleLines = paragraphs;

        // Carried in so the subtitle drawn on the two videos looks like the one on the main
        // window's video.
        _previewContext = previewContext;

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = OpenPlayersAsync(videoFileName, audioTrackId);
            }

            // An audio visualizer without peaks is just an empty box - only show it when the main
            // window actually has a waveform to lend us.
            if (audioVisualizer?.WavePeaks != null)
            {
                AudioVisualizerLeft.WavePeaks = audioVisualizer.WavePeaks;
                AudioVisualizerRight.WavePeaks = audioVisualizer.WavePeaks;
                IsAudioVisualizerVisible = true;
                _wavePeaksVideoFileName = videoFileName;
            }
            StartTitleTimer();
            _updateAudioVisualizer = true;
        });
    }

    // Opens both preview players and, once each video is loaded, applies the audio track the user
    // selected in the main window. Without this Visual Sync always played the default track (#11952).
    private async Task OpenPlayersAsync(string videoFileName, int audioTrackId)
    {
        await Task.WhenAll(
            VideoPlayerControlLeft.Open(videoFileName),
            VideoPlayerControlRight.Open(videoFileName));

        if (audioTrackId <= 0)
        {
            return;
        }

        if (VideoPlayerControlLeft.VideoPlayer is LibMpvDynamicPlayer mpvLeft)
        {
            mpvLeft.SetAudioTrack(audioTrackId);
        }

        if (VideoPlayerControlRight.VideoPlayer is LibMpvDynamicPlayer mpvRight)
        {
            mpvRight.SetAudioTrack(audioTrackId);
        }
    }

    private void SetVideoInFo(string? videoFileName)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            VideoInfo = Se.Language.General.NoVideoLoaded;
            return;
        }

        _ = Task.Run(() =>
        {
            var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
            if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 } && mediaInfo.Duration != null)
            {
                VideoInfo = string.Format(Se.Language.General.FileNameX, videoFileName) + Environment.NewLine +
                            string.Format(Se.Language.Sync.ResolutionXDurationYFrameRateZ,
                                $"{mediaInfo.Dimension.Width}x{mediaInfo.Dimension.Height}",
                                mediaInfo.Duration.ToShortDisplayString(),
                                mediaInfo.FramesRateNonNormalized);
                return;
            }

            VideoInfo = Se.Language.General.NoVideoLoaded;
        });

    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _positionTimer.Tick += (s, e) =>
        {
            UpdateAudioVisualizer(VideoPlayerControlLeft.VideoPlayer, AudioVisualizerLeft, SelectedParagraphLeftIndex);
            UpdateAudioVisualizer(VideoPlayerControlRight.VideoPlayer, AudioVisualizerRight, SelectedParagraphRightIndex);

            RefreshPreviewSubtitles();

            if (_updateAudioVisualizer)
            {
                AudioVisualizerLeft.InvalidateVisual();
                AudioVisualizerRight.InvalidateVisual();
                _updateAudioVisualizer = false;
            }
        };
        _positionTimer.Start();
    }

    /// <summary>
    /// Both players get the whole subtitle, so each shows whatever line belongs at the frame it is
    /// parked on - which is the comparison visual sync is for (discussion #13767).
    /// </summary>
    internal void RefreshPreviewSubtitles()
    {
        _previewSubtitleLeft.Refresh(VideoPlayerControlLeft.VideoPlayer, BuildPreviewSubtitle, _previewContext);
        _previewSubtitleRight.Refresh(VideoPlayerControlRight.VideoPlayer, BuildPreviewSubtitle, _previewContext);
    }

    /// <summary>
    /// The lines as they stand right now - "Sync" adjusts them in place, so the subtitle on the
    /// videos follows the new time codes just as SE4's did.
    /// </summary>
    private Subtitle BuildPreviewSubtitle()
    {
        var subtitle = new Subtitle { Header = _previewContext.Header };
        foreach (var p in Paragraphs)
        {
            subtitle.Paragraphs.Add(p.Subtitle.ToParagraph(_previewContext.Format));
        }

        return subtitle;
    }

    private void UpdateAudioVisualizer(
        IVideoPlayer vp,
        AudioVisualizer av,
        int selectedParagraphIndex)
    {
        SubtitleDisplayItem? selectedParagraph = selectedParagraphIndex < 0
            ? null
            : Paragraphs[selectedParagraphIndex];

        var subtitle = _subtitleLines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var firstSelectedIndex = -1;

        var mediaPlayerSeconds = vp.Position;
        var startPos = mediaPlayerSeconds - 0.01;
        if (startPos < 0)
        {
            startPos = 0;
        }

        av.CurrentVideoPositionSeconds = vp.Position;
        var isPlaying = vp.IsPlaying;

        var selectedSubtitles = new List<SubtitleLineViewModel>
        {
            selectedParagraph?.Subtitle ??  new  SubtitleLineViewModel(),
        };

        if ((isPlaying || !av.IsScrolling) && (mediaPlayerSeconds > av.EndPositionSeconds ||
                                               mediaPlayerSeconds < av.StartPositionSeconds))
        {
            av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0,
                selectedSubtitles);
        }
        else
        {
            av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                selectedSubtitles);
        }
    }

    [RelayCommand]
    private void LeftOneSecondBack()
    {
        VideoPlayerControlLeft.Position = Math.Max(0, VideoPlayerControlLeft.Position - 1);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void LeftOneSecondForward()
    {
        VideoPlayerControlLeft.Position = Math.Min(VideoPlayerControlLeft.Duration, VideoPlayerControlLeft.Position + 1);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void RightOneSecondBack()
    {
        VideoPlayerControlRight.Position = Math.Max(0, VideoPlayerControlRight.Position - 1);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void RightOneSecondForward()
    {
        VideoPlayerControlRight.Position = Math.Min(VideoPlayerControlRight.Duration, VideoPlayerControlRight.Position + 1);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void LeftHalfSecondBack()
    {
        VideoPlayerControlLeft.Position = Math.Max(0, VideoPlayerControlLeft.Position - 0.5);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void LeftHalfSecondForward()
    {
        VideoPlayerControlLeft.Position = Math.Min(VideoPlayerControlLeft.Duration, VideoPlayerControlLeft.Position + 0.5);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void RightHalfSecondBack()
    {
        VideoPlayerControlRight.Position = Math.Max(0, VideoPlayerControlRight.Position - 0.5);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void RightHalfSecondForward()
    {
        VideoPlayerControlRight.Position = Math.Min(VideoPlayerControlRight.Duration, VideoPlayerControlRight.Position + 0.5);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task PlayTwoSecondsAndBackLeft()
    {
        await PlayAndBack(VideoPlayerControlLeft, 2000);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task PlayTwoSecondsAndBackRight()
    {
        await PlayAndBack(VideoPlayerControlRight, 2000);
        _updateAudioVisualizer = true;
    }

    private void CenterWaveform(VideoPlayerControl videoPlayerControl, AudioVisualizer audioVisualizer)
    {
        audioVisualizer.StartPositionSeconds = Math.Max(0, videoPlayerControl.Position - 0.5);
    }

    [RelayCommand]
    private async Task FindTextLeft()
    {
        var result = await _windowService.ShowDialogAsync<FindTextWindow, FindTextViewModel>(Window!, vm =>
        {
            vm.Initialize(_subtitleLines, string.Format(Se.Language.General.FindTextX, Se.Language.Sync.StartScene));
        });

        if (!result.OkPressed || result.SelectedSubtitle == null)
        {
            return;
        }

        var s = Paragraphs.FirstOrDefault(p => p.Subtitle == result.SelectedSubtitle);
        if (s == null)
        {
            return;
        }

        SelectedParagraphLeftIndex = Paragraphs.IndexOf(s);
        VideoPlayerControlLeft.Position = s.Subtitle.StartTime.TotalSeconds;
        CenterWaveform(VideoPlayerControlLeft, AudioVisualizerLeft);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task FindTextRight()
    {
        var result = await _windowService.ShowDialogAsync<FindTextWindow, FindTextViewModel>(Window!, vm =>
        {
            vm.Initialize(_subtitleLines, string.Format(Se.Language.General.FindTextX, Se.Language.Sync.EndScene));
        });

        if (!result.OkPressed || result.SelectedSubtitle == null)
        {
            return;
        }

        var s = Paragraphs.FirstOrDefault(p => p.Subtitle == result.SelectedSubtitle);
        if (s == null)
        {
            return;
        }

        SelectedParagraphRightIndex = Paragraphs.IndexOf(s);
        VideoPlayerControlRight.Position = s.Subtitle.StartTime.TotalSeconds;
        CenterWaveform(VideoPlayerControlRight, AudioVisualizerRight);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task Sync()
    {
        if (SelectedParagraphLeftIndex < 0 || SelectedParagraphRightIndex < 0)
        {
            return;
        }

        // Video player current start and end position.
        double videoPlayerCurrentStartPos = VideoPlayerControlLeft.Position;
        double videoPlayerCurrentEndPos = VideoPlayerControlRight.Position;

        // Subtitle start and end time in seconds.
        double subStart = Paragraphs[SelectedParagraphLeftIndex].Subtitle.StartTime.TotalSeconds;
        double subEnd = Paragraphs[SelectedParagraphRightIndex].Subtitle.StartTime.TotalSeconds;

        // Validate: End time must be greater than start time.
        if (!(videoPlayerCurrentEndPos > videoPlayerCurrentStartPos && subEnd > subStart))
        {
            await MessageBox.Show(Window!, Title, Se.Language.Sync.StartSceneMustComeBeforeEndScene);
            return;
        }

        double subDiff = subEnd - subStart;
        double realDiff = videoPlayerCurrentEndPos - videoPlayerCurrentStartPos;

        // speed factor
        double factor = realDiff / subDiff;

        // adjust to starting position
        double adjust = videoPlayerCurrentStartPos - subStart * factor;

        SetAdjustInfo(factor, adjust);
        ApplySync(factor, adjust);
    }

    [RelayCommand]
    private async Task ManualSync()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ManualSyncWindow, ManualSyncViewModel>(Window!, vm =>
        {
            vm.Initialize(new ObservableCollection<SubtitleLineViewModel>(_subtitleLines), _lastManualOffsetSeconds, _lastManualSpeedFactor);
        });

        if (!result.OkPressed)
        {
            return;
        }

        _lastManualOffsetSeconds = result.OffsetSeconds;
        _lastManualSpeedFactor = result.SpeedFactor;

        SetAdjustInfo(result.SpeedFactor, result.OffsetSeconds);
        ApplySync(result.SpeedFactor, result.OffsetSeconds);
    }

    internal void ApplySync(double factor, double adjust)
    {
        if (Math.Abs(factor) < 0.000001)
        {
            return;
        }

        foreach (var p in Paragraphs)
        {
            p.Subtitle.Adjust(factor, adjust);
            p.UpdateText();
        }

        // fix overlapping time codes
        for (var i = 0; i < Paragraphs.Count - 1; i++)
        {
            var current = Paragraphs[i].Subtitle;
            var next = Paragraphs[i + 1].Subtitle;
            if (current.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
            {
                var newEndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - 1);
                if (newEndTime < current.StartTime)
                {
                    continue;
                }

                current.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - 1);
            }
        }

        // The time codes moved, so the subtitle on both videos has to be pushed again.
        _previewSubtitleLeft.Invalidate();
        _previewSubtitleRight.Invalidate();

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private void SetAdjustInfo(double factor, double adjust)
    {
        AdjustInfo = string.Empty;
        if (Math.Abs(adjust) > 0.001 || Math.Abs(1 - factor) > 0.001)
        {
            AdjustInfo = string.Format("*{0:0.000}, {1:+0.000;-0.000}", factor, adjust);
        }
    }

    private async Task PlayAndBack(VideoPlayerControl videoPlayer, int milliseconds)
    {
        var originalPosition = videoPlayer.Position;
        videoPlayer.VideoPlayer.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayer.Pause();
        videoPlayer.Position = originalPosition;
    }

    private bool IsLeftFocused()
    {
        return AudioVisualizerLeft.IsFocused ||
               VideoPlayerControlLeft.IsFocused ||
               ComboBoxLeft.IsFocused;
    }
    private bool IsRightFocused()
    {
        return AudioVisualizerRight.IsFocused ||
               VideoPlayerControlRight.IsFocused ||
               ComboBoxRight.IsFocused;
    }

    public void AudioVisualizerLeftPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        VideoPlayerControlLeft.Position = e.PositionInSeconds;
        _updateAudioVisualizer = true;
    }

    public void AudioVisualizerRightPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        VideoPlayerControlRight.Position = e.PositionInSeconds;
        _updateAudioVisualizer = true;
    }

    internal void OnClosing()
    {
        UiUtil.SaveWindowPosition(Window);
        _positionTimer.Stop();
        VideoPlayerControlLeft.VideoPlayer.CloseFile();
        VideoPlayerControlRight.VideoPlayer.CloseFile();

        // Deletes the temp subtitle files handed to the two players.
        _previewSubtitleLeft.Reset();
        _previewSubtitleRight.Reset();
    }

    [RelayCommand]
    private void GoToLeftSubtitle()
    {
        var selectedIndex = SelectedParagraphLeftIndex;
        if (selectedIndex < 0)
        {
            return;
        }

        var selected = Paragraphs[selectedIndex];
        VideoPlayerControlLeft.Position = selected.Subtitle.StartTime.TotalSeconds;
        AudioVisualizerLeft.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        CenterWaveform(VideoPlayerControlLeft, AudioVisualizerLeft);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void GoToRightSubtitle()
    {
        var selectedIndex = SelectedParagraphRightIndex;
        if (selectedIndex < 0)
        {
            return;
        }

        var selected = Paragraphs[selectedIndex];
        VideoPlayerControlRight.Position = selected.Subtitle.StartTime.TotalSeconds;
        AudioVisualizerRight.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        CenterWaveform(VideoPlayerControlRight, AudioVisualizerRight);
        _updateAudioVisualizer = true;
    }

    internal async void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);

        // Only the video needs waiting for - the start/end scenes still have to be picked when
        // there is none, so the combo boxes are not left empty while the user finds a video.
        if (!string.IsNullOrEmpty(_videoFileName))
        {
            // Wait a bit for video players to finish opening the file (or until they report a duration)
            await VideoPlayerControlLeft.WaitForPlayersReadyAsync();
            await VideoPlayerControlRight.WaitForPlayersReadyAsync();
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (Paragraphs.Count == 0)
            {
                return;
            }

            SelectedParagraphLeftIndex = 0;
            SelectedParagraphRightIndex = Paragraphs.Count - 1;
            GoToLeftSubtitle();
            GoToRightSubtitle();
        });
    }

    /// <summary>
    /// Opens a video from inside the dialog, so entering visual sync without one is not a dead end
    /// - SE4 offered the same button. The video stays local to this dialog; only time codes are
    /// reported back.
    /// </summary>
    [RelayCommand]
    private async Task OpenVideoFile()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenVideoFile(Window, Se.Language.General.OpenVideoFileTitle);
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return;
        }

        _videoFileName = fileName;
        SetVideoInFo(fileName);

        // The lent waveform belongs to the video the dialog was opened with - keeping it
        // under a different video would have the user syncing against the wrong peaks.
        if (!string.Equals(fileName, _wavePeaksVideoFileName, StringComparison.OrdinalIgnoreCase))
        {
            AudioVisualizerLeft.WavePeaks = null;
            AudioVisualizerRight.WavePeaks = null;
            IsAudioVisualizerVisible = false;
        }

        await OpenPlayersAsync(fileName, -1);
        await VideoPlayerControlLeft.WaitForPlayersReadyAsync();
        await VideoPlayerControlRight.WaitForPlayersReadyAsync();

        // The external subtitle went with the old file - it has to be added to the new one from
        // scratch, not reloaded into a track that is no longer there.
        _previewSubtitleLeft.Reset();
        _previewSubtitleRight.Reset();

        // Land the players on the scenes already picked in the combo boxes (OnLoaded seeds them
        // to the first/last line even without a video), instead of both sitting at zero.
        Dispatcher.UIThread.Post(() =>
        {
            GoToLeftSubtitle();
            GoToRightSubtitle();
            _updateAudioVisualizer = true;
        });
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/visual-sync");
        }

        if (IsLeftFocused())
        {
            if (e.Key == Key.Space || (e.Key == Key.P && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
            {
                e.Handled = true;
                VideoPlayerControlLeft.TogglePlayPause();
            }
            else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                e.Handled = true;
                VideoPlayerControlLeft.Position = Math.Max(0, VideoPlayerControlLeft.Position - 1);
                _updateAudioVisualizer = true;
            }
            else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                e.Handled = true;
                VideoPlayerControlLeft.Position += 1;
                _updateAudioVisualizer = true;
            }
            else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                e.Handled = true;
                VideoPlayerControlLeft.Position = Math.Max(0, VideoPlayerControlLeft.Position - 0.5);
                _updateAudioVisualizer = true;
            }
            else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                e.Handled = true;
                VideoPlayerControlLeft.Position += 0.5;
                _updateAudioVisualizer = true;
            }
            else if ((e.Key == Key.Add || e.Key == Key.OemPlus) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                e.Handled = true;
                WaveformVerticalZoomIn(AudioVisualizerLeft);
            }
            else if ((e.Key == Key.Subtract || e.Key == Key.OemMinus) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                e.Handled = true;
                WaveformVerticalZoomOut(AudioVisualizerLeft);
            }
        }
        else if (IsRightFocused())
        {
            if (e.Key == Key.Space || (e.Key == Key.P && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
            {
                e.Handled = true;
                VideoPlayerControlRight.TogglePlayPause();
            }
            else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                e.Handled = true;
                VideoPlayerControlRight.Position = Math.Max(0, VideoPlayerControlRight.Position - 1);
                _updateAudioVisualizer = true;
            }
            else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                e.Handled = true;
                VideoPlayerControlRight.Position += 1;
                _updateAudioVisualizer = true;
            }
            else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                e.Handled = true;
                VideoPlayerControlRight.Position = Math.Max(0, VideoPlayerControlRight.Position - 0.5);
                _updateAudioVisualizer = true;
            }
            else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                e.Handled = true;
                VideoPlayerControlRight.Position += 0.5;
                _updateAudioVisualizer = true;
            }
            else if ((e.Key == Key.Add || e.Key == Key.OemPlus) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                e.Handled = true;
                WaveformVerticalZoomIn(AudioVisualizerRight);
            }
            else if ((e.Key == Key.Subtract || e.Key == Key.OemMinus) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                e.Handled = true;
                WaveformVerticalZoomOut(AudioVisualizerRight);
            }
        }
    }

    /// <summary>
    /// Mirrors the main window's waveform vertical zoom (Shift +/-) for whichever pane has focus:
    /// scales that waveform's amplitude in place instead of resizing the split panel, so zooming
    /// in does not eat into the video area (#14419 comment).
    /// </summary>
    private void WaveformVerticalZoomIn(AudioVisualizer audioVisualizer)
    {
        if (!IsAudioVisualizerVisible)
        {
            return;
        }

        audioVisualizer.VerticalZoomFactor = Math.Max(Math.Min(audioVisualizer.VerticalZoomFactor - 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
    }

    private void WaveformVerticalZoomOut(AudioVisualizer audioVisualizer)
    {
        if (!IsAudioVisualizerVisible)
        {
            return;
        }

        audioVisualizer.VerticalZoomFactor = Math.Max(Math.Min(audioVisualizer.VerticalZoomFactor + 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
    }
}