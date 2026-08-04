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
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;

public partial class SetSyncPointViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphIndex = -1;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _videoInfo;
    [ObservableProperty] private TimeSpan _syncPointTimeCode;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public double SyncPosition { get; private set; }
    public VideoPlayerControl VideoPlayerControl { get; set; }
    public AudioVisualizer AudioVisualizer { get; set; }
    public ComboBox ComboBoxSubtitle { get; set; }
    public TimeCodeUpDown TimeCodeUpDownSyncPoint { get; set; }

    private readonly IWindowService _windowService;

    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private bool _updateAudioVisualizer;
    private bool _updateTimeCodeFromVideo;
    private bool _timeCodeUpDownFocused;

    public SetSyncPointViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Title = string.Empty;
        VideoInfo = string.Empty;
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
        SetVideoInFo(videoFileName);
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _videoFileName = videoFileName;
        _subtitleLines = paragraphs;

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayerControl.Open(videoFileName);
            }

            if (audioVisualizer != null)
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
            var isEditingTimeCode = TimeCodeUpDownSyncPoint.IsKeyboardFocusWithin && !VideoPlayerControl.IsPlaying;
            if (!isEditingTimeCode)
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

    /// <summary>
    /// Single entry point for moving the video - keeps the sync point time code box in sync
    /// with the video position, also while the box has focus (where the timer leaves it alone).
    /// </summary>
    private void SetVideoPosition(double seconds)
    {
        VideoPlayerControl.Position = Math.Max(0, seconds);
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
        if (_updateTimeCodeFromVideo)
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
        SetVideoPosition(VideoPlayerControl.Position - 1);
    }

    [RelayCommand]
    private void LeftOneSecondForward()
    {
        SetVideoPosition(VideoPlayerControl.Position + 1);
    }

    [RelayCommand]
    private void LeftHalfSecondBack()
    {
        SetVideoPosition(VideoPlayerControl.Position - 0.5);
    }

    [RelayCommand]
    private void LeftHalfSecondForward()
    {
        SetVideoPosition(VideoPlayerControl.Position + 0.5);
    }

    [RelayCommand]
    private async Task PlayTwoSecondsAndBackLeft()
    {
        await PlayAndBack(VideoPlayerControl, 2000);
        UpdateTimeCodeFromVideoPosition();
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

        SelectedParagraphIndex = Paragraphs.IndexOf(s);
        SetVideoPosition(s.Subtitle.StartTime.TotalSeconds);
        CenterWaveform(VideoPlayerControl, AudioVisualizer);
    }

    [RelayCommand]
    private void Ok()
    {
        SyncPosition = VideoPlayerControl.Position;
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

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        await VideoPlayerControl.WaitForPlayersReadyAsync();

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
            VideoPlayerControl.TogglePlayPause();
        }
        else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            SetVideoPosition(VideoPlayerControl.Position - 1);
        }
        else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            SetVideoPosition(VideoPlayerControl.Position + 1);
        }
        else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            SetVideoPosition(VideoPlayerControl.Position - 0.5);
        }
        else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            SetVideoPosition(VideoPlayerControl.Position + 0.5);
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