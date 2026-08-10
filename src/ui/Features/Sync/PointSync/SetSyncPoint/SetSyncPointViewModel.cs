using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.FindText;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;

public partial class SetSyncPointViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphIndex = -1;
    [ObservableProperty] private bool _isAudioVisualizerVisible;

    /// <summary>
    /// Whether the dialog shows its video half at all. With no video there is nothing to scrub, so
    /// the player, the waveform and the playback buttons come off and the dialog is just the line
    /// picker and the sync point time code (issue #13341).
    /// </summary>
    [ObservableProperty] private bool _isVideoVisible;

    [ObservableProperty] private string _title;
    [ObservableProperty] private string _videoInfo;
    [ObservableProperty] private TimeSpan _syncPointTimeCode;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public double SyncPosition { get; private set; }

    /// <summary>
    /// The video in use when the dialog closed - either the one passed in, one found next to the
    /// subtitle file, or one the user opened here. The caller adopts it so the next sync point
    /// (and the main window) reuse it.
    /// </summary>
    public string VideoFileName { get; private set; }

    public VideoPlayerControl VideoPlayerControl { get; set; }
    public AudioVisualizer AudioVisualizer { get; set; }
    public ComboBox ComboBoxSubtitle { get; set; }
    public TimeCodeUpDown TimeCodeUpDownSyncPoint { get; set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;

    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private bool _updateAudioVisualizer;
    private bool _updateTimeCodeFromVideo;
    private bool _timeCodeUpDownFocused;

    public SetSyncPointViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        Title = string.Empty;
        VideoInfo = string.Empty;
        VideoFileName = string.Empty;
        _videoFileName = string.Empty;
        VideoPlayerControl = new VideoPlayerControl(new EmptyVideoPlayer());
        AudioVisualizer = new AudioVisualizer();
        ComboBoxSubtitle = new ComboBox();
        TimeCodeUpDownSyncPoint = new TimeCodeUpDown();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();

        // Toggle play/pause on surface click
        VideoPlayerControl.SurfacePointerPressed += (_, __) => VideoPlayerControl.TogglePlayPause();
    }

    public void Initialize(
        List<SubtitleLineViewModel> paragraphs,
        SubtitleLineViewModel? selectedSubtitle,
        string? videoFileName,
        string? subtitleFileName,
        AudioVisualizer? audioVisualizer)
    {
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _subtitleLines = paragraphs;

        // Only a video the caller already had, or one the user picks here, is reported back - a
        // video merely found on disk must not travel up and open in the main window, which would
        // walk over the "auto open video file" setting.
        VideoFileName = videoFileName ?? string.Empty;

        // No video handed down from the main window - look for one next to the subtitle file, the
        // way the old "Set sync point" did. Failing that the dialog still works: the time code can
        // be typed, and "Open video file..." is there to pick one.
        if (string.IsNullOrEmpty(videoFileName) &&
            !string.IsNullOrEmpty(subtitleFileName) &&
            FindVideoFileName.TryFindVideoFileName(subtitleFileName, out var foundVideoFileName))
        {
            videoFileName = foundVideoFileName;
        }

        _videoFileName = videoFileName;
        IsVideoVisible = HasVideo;
        SetVideoInFo(videoFileName);

        // Seed the sync point with the line's own start time - a video, when there is one, takes
        // over from here (OnLoaded). Without this the box would sit at 00:00:00.000 with no video,
        // which reads as a broken dialog (issue #13341).
        if (selectedSubtitle != null)
        {
            SyncPointTimeCode = selectedSubtitle.StartTime;
        }
        else if (paragraphs.Count > 0)
        {
            SyncPointTimeCode = paragraphs[0].StartTime;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(_videoFileName))
            {
                _ = VideoPlayerControl.Open(_videoFileName);
            }

            // An audio visualizer without peaks is just an empty box - only show it when the main
            // window actually has a waveform to lend us.
            if (audioVisualizer?.WavePeaks != null)
            {
                AudioVisualizer.WavePeaks = audioVisualizer.WavePeaks;
                IsAudioVisualizerVisible = true;
            }

            if (selectedSubtitle != null)
            {
                var idx = paragraphs.IndexOf(selectedSubtitle);
                if (idx >= 0)
                {
                    SelectedParagraphIndex = idx;
                }
            }

            StartTitleTimer();
            _updateAudioVisualizer = true;
        });
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
            UpdateAudioVisualizer(VideoPlayerControl.VideoPlayer, AudioVisualizer, SelectedParagraphIndex);

            // Follow the video position - but leave the time code alone while the user is typing
            // in it (a running video moves on its own, so then the box should still follow).
            // With no video there is nothing to follow and the box is the only input, so the
            // player must not be allowed to reset it to zero (issue #13341).
            var isEditingTimeCode = TimeCodeUpDownSyncPoint.IsKeyboardFocusWithin && !VideoPlayerControl.IsPlaying;
            if (!isEditingTimeCode && HasVideo)
            {
                UpdateTimeCodeFromVideoPosition();
            }

            if (_updateAudioVisualizer)
            {
                AudioVisualizer.InvalidateVisual();
                _updateAudioVisualizer = false;
            }
        };
        _positionTimer.Start();
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

    private bool HasVideo => !string.IsNullOrEmpty(_videoFileName);

    /// <summary>
    /// Where the sync point currently sits. The video owns this while one is loaded; otherwise
    /// the time code box does, so the nudge buttons keep working without a video.
    /// </summary>
    private double CurrentPositionSeconds => HasVideo
        ? VideoPlayerControl.Position
        : SyncPointTimeCode.TotalSeconds;

    /// <summary>
    /// Single entry point for moving the video - keeps the sync point time code box in sync
    /// with the video position, also while the box has focus (where the timer leaves it alone).
    /// With no video the box is authoritative, so it is set directly instead.
    /// </summary>
    private void SetVideoPosition(double seconds)
    {
        seconds = Math.Max(0, seconds);
        if (!HasVideo)
        {
            SyncPointTimeCode = TimeSpan.FromSeconds(seconds);
            return;
        }

        VideoPlayerControl.Position = seconds;
        UpdateTimeCodeFromVideoPosition();
        _updateAudioVisualizer = true;
    }

    private void UpdateTimeCodeFromVideoPosition()
    {
        var position = TimeSpan.FromSeconds(Math.Max(0, VideoPlayerControl.Position));
        if (Math.Abs((position - SyncPointTimeCode).TotalMilliseconds) < 1)
        {
            return;
        }

        _updateTimeCodeFromVideo = true;
        SyncPointTimeCode = position;
        _updateTimeCodeFromVideo = false;
    }

    partial void OnSyncPointTimeCodeChanged(TimeSpan value)
    {
        if (_updateTimeCodeFromVideo || !HasVideo)
        {
            return;
        }

        // The user typed/spun a new time code - move the video there
        VideoPlayerControl.Position = Math.Max(0, value.TotalSeconds);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void LeftOneSecondBack()
    {
        SetVideoPosition(CurrentPositionSeconds - 1);
    }

    [RelayCommand]
    private void LeftOneSecondForward()
    {
        SetVideoPosition(CurrentPositionSeconds + 1);
    }

    [RelayCommand]
    private void LeftHalfSecondBack()
    {
        SetVideoPosition(CurrentPositionSeconds - 0.5);
    }

    [RelayCommand]
    private void LeftHalfSecondForward()
    {
        SetVideoPosition(CurrentPositionSeconds + 0.5);
    }

    [RelayCommand]
    private async Task PlayTwoSecondsAndBackLeft()
    {
        if (!HasVideo)
        {
            return;
        }

        await PlayAndBack(VideoPlayerControl, 2000);
        UpdateTimeCodeFromVideoPosition();
        _updateAudioVisualizer = true;
    }

    private void CenterWaveform(VideoPlayerControl videoPlayerControl, AudioVisualizer audioVisualizer)
    {
        audioVisualizer.StartPositionSeconds = Math.Max(0, videoPlayerControl.Position - 0.5);
    }

    /// <summary>
    /// Opens a video from inside the dialog, so entering point sync without one is not a dead end
    /// (issue #13341). The caller adopts <see cref="VideoFileName"/> when the dialog closes.
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

        var syncPoint = SyncPointTimeCode;
        VideoFileName = fileName;
        SetVideoInFo(fileName);

        // Open at the time code the user already had, rather than snapping the sync point back to
        // the start of the newly opened file. It has to be passed to Open: the position slider is
        // clamped to Duration, which is only polled once the player is running, so seeking right
        // after the open would land at zero.
        await VideoPlayerControl.Open(fileName, Math.Max(0, syncPoint.TotalSeconds));
        await VideoPlayerControl.WaitForPlayersReadyAsync();

        // Only now does the player own the sync point - handing it over any earlier would let the
        // timer copy the still-zero position into the time code while the file is loading.
        _videoFileName = fileName;
        IsVideoVisible = true;
        UpdateTimeCodeFromVideoPosition();
        CenterWaveform(VideoPlayerControl, AudioVisualizer);
        _updateAudioVisualizer = true;
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

        SelectedParagraphIndex = Paragraphs.IndexOf(s);
        SetVideoPosition(s.Subtitle.StartTime.TotalSeconds);
        CenterWaveform(VideoPlayerControl, AudioVisualizer);
    }

    [RelayCommand]
    private void Ok()
    {
        // The time code box is the result, not the player - the player is only one of the ways to
        // drive it, and there may not be one at all (issue #13341).
        SyncPosition = Math.Max(0, SyncPointTimeCode.TotalSeconds);
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
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
        return AudioVisualizer.IsFocused ||
               VideoPlayerControl.IsFocused ||
               ComboBoxSubtitle.IsFocused;
    }

    public void AudioVisualizerLeftPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        SetVideoPosition(e.PositionInSeconds);
    }

    internal void OnClosing()
    {
        UiUtil.SaveWindowPosition(Window);
        _positionTimer.Stop();
        VideoPlayerControl.VideoPlayer.CloseFile();
    }

    [RelayCommand]
    private void GoToLeftSubtitle()
    {
        var selectedIndex = SelectedParagraphIndex;
        if (selectedIndex < 0)
        {
            return;
        }

        var selected = Paragraphs[selectedIndex];
        SetVideoPosition(selected.Subtitle.StartTime.TotalSeconds);
        AudioVisualizer.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        CenterWaveform(VideoPlayerControl, AudioVisualizer);
    }

    internal async void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);

        // Only the video needs waiting for - the sync point still has to be seeded from the
        // selected line when there is none, or it would stay at zero (issue #13341).
        if (HasVideo)
        {
            await VideoPlayerControl.WaitForPlayersReadyAsync();
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (Paragraphs.Count == 0)
            {
                return;
            }

            if (SelectedParagraphIndex < 0 || SelectedParagraphIndex >= Paragraphs.Count)
            {
                SelectedParagraphIndex = 0;
            }

            GoToLeftSubtitle();
        });
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }

        if (e.Key == Key.Space || (e.Key == Key.P && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            e.Handled = true;
            if (HasVideo)
            {
                VideoPlayerControl.TogglePlayPause();
            }
        }
        else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            SetVideoPosition(CurrentPositionSeconds - 1);
        }
        else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            SetVideoPosition(CurrentPositionSeconds + 1);
        }
        else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            SetVideoPosition(CurrentPositionSeconds - 0.5);
        }
        else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            SetVideoPosition(CurrentPositionSeconds + 0.5);
        }
    }

    /// <summary>
    /// Tunnel-stage KeyUp twin of <see cref="OnKeyDownHandler"/>. Avalonia's Button raises OnClick
    /// from OnKeyUp on Space whenever the button is focused - it does not check that the button also
    /// saw the KeyDown - so a handled KeyDown alone still let a focused button click on Space release.
    /// </summary>
    internal void OnKeyUpHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Puts the caret in the sync point time code box, so the window has a focused element for
    /// key handling (and the time code can be typed right away) without arming a button.
    /// </summary>
    internal void FocusTimeCodeUpDown()
    {
        if (_timeCodeUpDownFocused)
        {
            return; // only on first activation - do not steal focus back on every re-activation
        }

        _timeCodeUpDownFocused = true;

        Dispatcher.UIThread.Post(() =>
        {
            // The inner text box is the element that actually takes keyboard focus
            var textBox = TimeCodeUpDownSyncPoint.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
            if (textBox != null)
            {
                textBox.Focus();
                return;
            }

            TimeCodeUpDownSyncPoint.Focus();
        });
    }

    internal void AudioVisualizerOnPrimarySingleClicked(object sender, ParagraphNullableEventArgs e)
    {
        SetVideoPosition(e.Seconds);
    }
}