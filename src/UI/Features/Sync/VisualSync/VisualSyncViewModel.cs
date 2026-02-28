using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
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

    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private bool _updateAudioVisualizer;

    public VisualSyncViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Title = string.Empty;
        VideoInfo = string.Empty;
        AdjustInfo = string.Empty;
        _videoFileName = string.Empty;
        VideoPlayerControlLeft = new VideoPlayerControl(new VideoPlayerInstanceNone());
        VideoPlayerControlRight = new VideoPlayerControl(new VideoPlayerInstanceNone());
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
                _ = VideoPlayerControlLeft.Open(videoFileName);
                _ = VideoPlayerControlRight.Open(videoFileName);
            }

            if (audioVisualizer != null)
            {
                AudioVisualizerLeft.WavePeaks = audioVisualizer.WavePeaks;
                AudioVisualizerRight.WavePeaks = audioVisualizer.WavePeaks;
                IsAudioVisualizerVisible = true;
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
            UpdateAudioVisualizer(VideoPlayerControlLeft.VideoPlayerInstance, AudioVisualizerLeft, SelectedParagraphLeftIndex);
            UpdateAudioVisualizer(VideoPlayerControlRight.VideoPlayerInstance, AudioVisualizerRight, SelectedParagraphRightIndex);

            if (_updateAudioVisualizer)
            {
                AudioVisualizerLeft.InvalidateVisual();
                AudioVisualizerRight.InvalidateVisual();
                _updateAudioVisualizer = false;
            }
        };
        _positionTimer.Start();
    }

    private void UpdateAudioVisualizer(
        IVideoPlayerInstance vp,
        AudioVisualizer av,
        int selectedParagraphIndex)
    {
        SubtitleDisplayItem? selectedParagraph = selectedParagraphIndex < 0
            ? null
            : Paragraphs[selectedParagraphIndex];

        var subtitle = new ObservableCollection<SubtitleLineViewModel>();
        var orderedList = _subtitleLines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var firstSelectedIndex = -1;
        for (var i = 0; i < orderedList.Count; i++)
        {
            var dp = orderedList[i];
            subtitle.Add(dp);
        }

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
        VideoPlayerControlLeft.Position = Math.Max(0, VideoPlayerControlLeft.Position + 1);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void RightOneSecondBack()
    {
        VideoPlayerControlRight.Position = Math.Min(VideoPlayerControlRight.Duration, VideoPlayerControlRight.Position - 1);
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
        VideoPlayerControlLeft.Position = Math.Max(0, VideoPlayerControlLeft.Position + 0.5);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void RightHalfSecondBack()
    {
        VideoPlayerControlRight.Position = Math.Min(VideoPlayerControlRight.Duration, VideoPlayerControlRight.Position - 0.5);
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

        SetSyncFactorLabel(videoPlayerCurrentStartPos, videoPlayerCurrentEndPos);

        double subDiff = subEnd - subStart;
        double realDiff = videoPlayerCurrentEndPos - videoPlayerCurrentStartPos;

        // speed factor
        double factor = realDiff / subDiff;

        // adjust to starting position
        double adjust = videoPlayerCurrentStartPos - subStart * factor;

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

    private void SetSyncFactorLabel(double videoPlayerCurrentStartPos, double videoPlayerCurrentEndPos)
    {
        if (string.IsNullOrWhiteSpace(_videoFileName) || SelectedParagraphLeftIndex < 0 || SelectedParagraphRightIndex < 0)
        {
            return;
        }

        AdjustInfo = string.Empty;
        if (videoPlayerCurrentEndPos > videoPlayerCurrentStartPos)
        {
            double subStart = Paragraphs[SelectedParagraphLeftIndex].Subtitle.StartTime.TotalSeconds;
            double subEnd = Paragraphs[SelectedParagraphRightIndex].Subtitle.StartTime.TotalSeconds;

            double subDiff = subEnd - subStart;
            double realDiff = videoPlayerCurrentEndPos - videoPlayerCurrentStartPos;

            // speed factor
            double factor = realDiff / subDiff;

            // adjust to starting position
            double adjust = videoPlayerCurrentStartPos - subStart * factor;

            if (Math.Abs(adjust) > 0.001 || (Math.Abs(1 - factor)) > 0.001)
            {
                AdjustInfo = string.Format("*{0:0.000}, {1:+0.000;-0.000}", factor, adjust);
            }
        }
    }

    private async Task PlayAndBack(VideoPlayerControl videoPlayer, int milliseconds)
    {
        var originalPosition = videoPlayer.Position;
        videoPlayer.VideoPlayerInstance.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayerInstance.Pause();
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
        _positionTimer.Stop();
        VideoPlayerControlLeft.VideoPlayerInstance.CloseFile();
        VideoPlayerControlRight.VideoPlayerInstance.CloseFile();
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
        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        // Wait a bit for video players to finish opening the file (or until they report a duration)
        await VideoPlayerControlLeft.WaitForPlayersReadyAsync();
        await VideoPlayerControlRight.WaitForPlayersReadyAsync();

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
        }
    }
}