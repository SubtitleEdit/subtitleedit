using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

namespace Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;

public partial class SetSyncPointViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphIndex = -1;
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _videoInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public double SyncPosition { get; private set; }
    public VideoPlayerControl VideoPlayerControl { get; set; }
    public AudioVisualizer AudioVisualizer { get; set; }
    public ComboBox ComboBoxSubtitle { get; set; }

    private readonly IWindowService _windowService;

    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private bool _updateAudioVisualizer;

    public SetSyncPointViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Title = string.Empty;
        VideoInfo = string.Empty;
        _videoFileName = string.Empty;
        VideoPlayerControl = new VideoPlayerControl(new VideoPlayerInstanceNone());
        AudioVisualizer = new AudioVisualizer();
        ComboBoxSubtitle = new ComboBox();
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
            UpdateAudioVisualizer(VideoPlayerControl.VideoPlayerInstance, AudioVisualizer, SelectedParagraphIndex);

            if (_updateAudioVisualizer)
            {
                AudioVisualizer.InvalidateVisual();
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
        VideoPlayerControl.Position = Math.Max(0, VideoPlayerControl.Position - 1);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void LeftOneSecondForward()
    {
        VideoPlayerControl.Position = Math.Max(0, VideoPlayerControl.Position + 1);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void LeftHalfSecondBack()
    {
        VideoPlayerControl.Position = Math.Max(0, VideoPlayerControl.Position - 0.5);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void LeftHalfSecondForward()
    {
        VideoPlayerControl.Position = Math.Max(0, VideoPlayerControl.Position + 0.5);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task PlayTwoSecondsAndBackLeft()
    {
        await PlayAndBack(VideoPlayerControl, 2000);
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
        VideoPlayerControl.Position = s.Subtitle.StartTime.TotalSeconds;
        CenterWaveform(VideoPlayerControl, AudioVisualizer);
        _updateAudioVisualizer = true;
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
        videoPlayer.VideoPlayerInstance.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayerInstance.Pause();
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
        VideoPlayerControl.Position = e.PositionInSeconds;
        _updateAudioVisualizer = true;
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();
        VideoPlayerControl.VideoPlayerInstance.CloseFile();
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
        VideoPlayerControl.Position = selected.Subtitle.StartTime.TotalSeconds;
        AudioVisualizer.CurrentVideoPositionSeconds = selected.Subtitle.StartTime.TotalSeconds;
        CenterWaveform(VideoPlayerControl, AudioVisualizer);
        _updateAudioVisualizer = true;
    }

    internal async void OnLoaded()
    {
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
            VideoPlayerControl.Position = Math.Max(0, VideoPlayerControl.Position - 1);
            _updateAudioVisualizer = true;
        }
        else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            VideoPlayerControl.Position += 1;
            _updateAudioVisualizer = true;
        }
        else if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            VideoPlayerControl.Position = Math.Max(0, VideoPlayerControl.Position - 0.5);
            _updateAudioVisualizer = true;
        }
        else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            e.Handled = true;
            VideoPlayerControl.Position += 0.5;
            _updateAudioVisualizer = true;
        }
    }

    internal void AudioVisualizerOnPrimarySingleClicked(object sender, ParagraphNullableEventArgs e)
    {
        VideoPlayerControl.Position = e.Seconds;
    }
}