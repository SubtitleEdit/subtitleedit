using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

public partial class AssaApplyAdvancedEffectViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<IAdvancedEffectDisplay> _overrideTags;
    [ObservableProperty] private IAdvancedEffectDisplay? _selectedOverrideTag;
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphIndex = -1;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;
    [ObservableProperty] private string _selectionInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle UpdatedSubtitle { get; set; }
    public VideoPlayerControl VideoPlayerControl { get; set; }
    public ComboBox ComboBoxLeft { get; set; }

    private readonly string _tempSubtitleFileName;
    private readonly SubtitleFormat _assaFormat;
    private LibMpvDynamicPlayer? _mpvPlayer;
    private bool _isSubtitleLoaded;
    private string _oldSubtitleText;
    private WavePeakData2? _wavePeaks;
    private Subtitle _subtitle;
    private string? _videoFileName;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private List<SubtitleLineViewModel> _subtitleLines = new List<SubtitleLineViewModel>();
    private List<SubtitleLineViewModel> _selectedSubtitleLines = new List<SubtitleLineViewModel>();
    private FfmpegMediaInfo2? _mediaInfo;
    private AudioVisualizer? _audioVisualizer;
    private HashSet<Guid> _selectedSubtitleLineIds = [];
    private int _firstSelectedIndex;

    public AssaApplyAdvancedEffectViewModel()
    {
        OverrideTags = new ObservableCollection<IAdvancedEffectDisplay>(AdvancedEffectDisplayFactory.List());
        _videoFileName = string.Empty;
        VideoPlayerControl = new VideoPlayerControl(new VideoPlayerInstanceNone());
        ComboBoxLeft = new ComboBox();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();
        _assaFormat = new AdvancedSubStationAlpha();
        _oldSubtitleText = string.Empty;
        SelectionInfo = string.Empty;
        AdjustAll = true;
        UpdatedSubtitle = new Subtitle();
        _subtitle = new Subtitle();

        // Toggle play/pause on surface click
        VideoPlayerControl.SurfacePointerPressed += (_, __) => VideoPlayerControl.TogglePlayPause();

        SelectedOverrideTag = OverrideTags[0];

        _tempSubtitleFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ass");
    }

    public void Initialize(
        Subtitle subtitle,
        List<SubtitleLineViewModel> paragraphs,
        List<SubtitleLineViewModel> selectedParagraphs,
        string? videoFileName,
        FfmpegMediaInfo2? mediaInfo,
        AudioVisualizer? audioVisualizer)
    {
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(paragraphs.Select(p => new SubtitleDisplayItem(p)));
        _wavePeaks = audioVisualizer?.WavePeaks;
        _subtitle = subtitle;
        _videoFileName = videoFileName;
        _subtitleLines = paragraphs;
        _selectedSubtitleLines = selectedParagraphs;
        _mediaInfo = mediaInfo;
        _audioVisualizer = audioVisualizer;
        _selectedSubtitleLineIds = new HashSet<Guid>(selectedParagraphs.Select(x => x.Id));
        _firstSelectedIndex = _subtitleLines.Count > 0
            ? Paragraphs.IndexOf(Paragraphs.First(p => p.Subtitle.Id == _subtitleLines[0].Id))
            : 0;

        if (audioVisualizer == null || audioVisualizer.WavePeaks == null || audioVisualizer.WavePeaks.Peaks.Count == 0)
        {
            // remove audio visualizer related effects if no audio visualizer data is available
            var i = OverrideTags.Count - 1;
            while (i >= 0)
            {
                var effect = OverrideTags[i];
                if (effect.UsesAudio)
                {
                    OverrideTags.RemoveAt(i);
                }
                i--;
            }
        }

        if (selectedParagraphs.Count > 1)
        {
            AdjustSelectedLines = true;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayerControl.Open(videoFileName);
            }

            if (_subtitleLines.Count > 10)
            {
                SelectionInfo = string.Format(Se.Language.General.SelectedlinesX, _selectedSubtitleLines.Count);
            }
            else
            {
                SelectionInfo = string.Format(Se.Language.General.SelectedlinesX, string.Join(", ", _selectedSubtitleLines.Select(s => s.Number)));
            }

            StartTitleTimer();
        });
    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(750) };
        _positionTimer.Tick += async (s, e) =>
        {
            if (_mpvPlayer == null)
            {
                return;
            }

            _positionTimer.Stop();
            try
            {
                var (subtitle, text) = await Task.Run(() =>
                {
                    var built = BuildPreviewSubtitle();
                    return (built, _assaFormat.ToText(built, string.Empty));
                });

                if (_oldSubtitleText == text)
                {
                    return;
                }

                _oldSubtitleText = text;
                await File.WriteAllTextAsync(_tempSubtitleFileName, text);
                if (!_isSubtitleLoaded)
                {
                    _isSubtitleLoaded = true;
                    _mpvPlayer.SubAdd(_tempSubtitleFileName);
                }
                else
                {
                    _mpvPlayer.SubReload();
                }

                UpdatedSubtitle = subtitle;
            }
            finally
            {
                _positionTimer.Start();
            }
        };

        _positionTimer.Start();
    }

    private Subtitle BuildPreviewSubtitle()
    {
        var effect = SelectedOverrideTag;
        var result = new Subtitle();
        result.Header = _subtitle.Header;

        if (effect == null || Paragraphs.Count == 0)
        {
            foreach (var item in Paragraphs)
            {
                result.Paragraphs.Add(item.Subtitle.ToParagraph(_assaFormat));
            }
            return result;
        }

        var toAffect = new List<SubtitleLineViewModel>(Paragraphs.Count);
        var affectedIds = new HashSet<Guid>(Paragraphs.Count);
        var idx = 0;

        foreach (var item in Paragraphs)
        {
            var shouldAffect = AdjustAll
                || (AdjustSelectedLines && _selectedSubtitleLineIds.Contains(item.Subtitle.Id))
                || (AdjustSelectedLinesAndForward && idx >= _firstSelectedIndex);

            if (shouldAffect)
            {
                toAffect.Add(item.Subtitle);
                affectedIds.Add(item.Subtitle.Id);
            }
            idx++;
        }

        var transformed = effect.ApplyEffect(
            toAffect,
            _mediaInfo?.Dimension.Width ?? 1920,
            _mediaInfo?.Dimension.Height ?? 1080,
            _wavePeaks);

        foreach (var item in Paragraphs)
        {
            if (!affectedIds.Contains(item.Subtitle.Id))
            {
                result.Paragraphs.Add(item.Subtitle.ToParagraph(_assaFormat));
            }
        }

        foreach (var line in transformed)
        {
            result.Paragraphs.Add(line.ToParagraph(_assaFormat));
        }

        result.Paragraphs.Sort((a, b) =>
            a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));

        return result;
    }

    [RelayCommand]
    private void Ok()
    {
        UpdatedSubtitle = BuildPreviewSubtitle();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task PlayAndBack()
    {
        if (SelectedParagraphIndex <= 0)
        {
            await PlayAndBack(VideoPlayerControl, 3000);
        }

        var selected = Paragraphs[SelectedParagraphIndex];
        VideoPlayerControl.Position = selected.Subtitle.StartTime.TotalSeconds;
        await PlayAndBack(VideoPlayerControl, (int)selected.Subtitle.Duration.TotalMilliseconds);
    }

    private async Task PlayAndBack(VideoPlayerControl videoPlayer, int milliseconds)
    {
        var originalPosition = videoPlayer.Position;
        videoPlayer.VideoPlayerInstance.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayerInstance.Pause();
        videoPlayer.Position = originalPosition;
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();
        VideoPlayerControl.VideoPlayerInstance.CloseFile();
        try
        {
            if (File.Exists(_tempSubtitleFileName))
            {
                File.Delete(_tempSubtitleFileName);
            }
        }
        catch
        {
            // ignore
        }

        UiUtil.SaveWindowPosition(Window);
    }

    internal async void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        // Wait a bit for video players to finish opening the file (or until they report a duration)
        await VideoPlayerControl.WaitForPlayersReadyAsync();

        Dispatcher.UIThread.Post(() =>
        {
            _mpvPlayer = VideoPlayerControl.VideoPlayerInstance as LibMpvDynamicPlayer;

            if (Paragraphs.Count == 0)
            {
                return;
            }

            if (_selectedSubtitleLines.Count == 0)
            {
                SelectedParagraphIndex = 0;
                return;
            }

            var firstSelected = _selectedSubtitleLines[0];
            SelectedParagraphIndex = Paragraphs.IndexOf(Paragraphs.First(p => p.Subtitle.Id == firstSelected.Id));
            VideoPlayerControl.Position = firstSelected.StartTime.TotalSeconds;
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
            UiUtil.ShowHelp("features/assa-apply-advanced-effects");
        }
    }

    internal void ComboBoxParagraphsChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectedParagraphIndex <= 0)
        {
            return;
        }

        var selected = Paragraphs[SelectedParagraphIndex];
        VideoPlayerControl.Position = selected.Subtitle.StartTime.TotalSeconds;
    }
}