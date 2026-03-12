using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public partial class OpenSecondarySubtitleViewModel : ObservableObject
{
    [ObservableProperty] private bool _alignmentAn1;
    [ObservableProperty] private bool _alignmentAn2;
    [ObservableProperty] private bool _alignmentAn3;
    [ObservableProperty] private bool _alignmentAn4;
    [ObservableProperty] private bool _alignmentAn5;
    [ObservableProperty] private bool _alignmentAn6;
    [ObservableProperty] private bool _alignmentAn7;
    [ObservableProperty] private bool _alignmentAn8;
    [ObservableProperty] private bool _alignmentAn9;
    [ObservableProperty] private Color _subtitleColor;
    [ObservableProperty] private FontBoxItem _selectedFontBoxType;
    [ObservableProperty] private decimal _fontSize;
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphIndex = -1;

    public ObservableCollection<FontBoxItem> FontBoxTypes { get; }
    public VideoPlayerControl VideoPlayerControl { get; set; }
    public ComboBox ComboBoxParagraphs { get; set; }

    private Subtitle _secondarySubtitle = new Subtitle();
    private Subtitle _subtitle = new Subtitle();
    private SubtitleFormat _subtitleFormat = new SubRip();
    private readonly IWindowService _windowService;
    private string? _videoFileName;
    private readonly string _tempSubtitleFileName;
    private LibMpvDynamicPlayer? _mpvPlayer;
    private bool _isSubtitleLoaded;
    private string _oldSubtitleText;
    private DispatcherTimer _positionTimer = new DispatcherTimer();

    public Subtitle? ResultSubtitle { get; private set; }
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public OpenSecondarySubtitleViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _alignmentAn2 = true;
        _subtitleColor = Colors.White;
        _fontSize = 20;
        FontBoxTypes = new ObservableCollection<FontBoxItem>
        {
            new(FontBoxType.None, Se.Language.General.None),
            new(FontBoxType.OneBox, Se.Language.Video.BurnIn.OneBox),
            new(FontBoxType.BoxPerLine, Se.Language.Video.BurnIn.BoxPerLine),
        };
        _selectedFontBoxType = FontBoxTypes[0];
        _paragraphs = new ObservableCollection<SubtitleDisplayItem>();
        _tempSubtitleFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ass");
        _oldSubtitleText = string.Empty;
        VideoPlayerControl = new VideoPlayerControl(new VideoPlayerInstanceNone());
        ComboBoxParagraphs = new ComboBox();
        VideoPlayerControl.SurfacePointerPressed += (_, _) => VideoPlayerControl.TogglePlayPause();
    }

    [RelayCommand]
    private async Task ChooseColor()
    {
        if (Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            Window, viewModel => { viewModel.SelectedColor = SubtitleColor; });

        if (vm.OkPressed)
        {
            SubtitleColor = vm.SelectedColor;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        ResultSubtitle = BuildAssaSubtitle();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void Initialize(Subtitle secondarySubtitle, Subtitle subtitle, SubtitleFormat subtitleFormat, Logic.Media.FfmpegMediaInfo2? mediaInfo, string? videoFileName)
    {
        _secondarySubtitle = secondarySubtitle;
        _subtitle = subtitle;
        _subtitleFormat = subtitleFormat;
        _videoFileName = videoFileName;

        var assaFormat = new AdvancedSubStationAlpha();
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(
            secondarySubtitle.Paragraphs.Select(p => new SubtitleDisplayItem(new SubtitleLineViewModel(p, assaFormat))));

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayerControl.Open(videoFileName);
            }
        });
    }

    public void BoxTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    public void ComboBoxParagraphsChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectedParagraphIndex < 0 || SelectedParagraphIndex >= Paragraphs.Count)
        {
            return;
        }

        var selected = Paragraphs[SelectedParagraphIndex];
        VideoPlayerControl.Position = selected.Subtitle.StartTime.TotalSeconds;
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
            _mpvPlayer = VideoPlayerControl.VideoPlayerInstance as LibMpvDynamicPlayer;

            if (Paragraphs.Count > 0)
            {
                SelectedParagraphIndex = 0;
                VideoPlayerControl.Position = Paragraphs[0].Subtitle.StartTime.TotalSeconds;
            }

            StartSubtitleTimer();
        });
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    [RelayCommand]
    private async Task PlayAndBack()
    {
        if (SelectedParagraphIndex < 0 || SelectedParagraphIndex >= Paragraphs.Count)
        {
            await PlayAndBackVideo(VideoPlayerControl, 3000);
            return;
        }

        var selected = Paragraphs[SelectedParagraphIndex];
        VideoPlayerControl.Position = selected.Subtitle.StartTime.TotalSeconds;
        await PlayAndBackVideo(VideoPlayerControl, (int)selected.Subtitle.Duration.TotalMilliseconds);
    }

    private static async Task PlayAndBackVideo(VideoPlayerControl videoPlayer, int milliseconds)
    {
        var originalPosition = videoPlayer.Position;
        videoPlayer.VideoPlayerInstance.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayerInstance.Pause();
        videoPlayer.Position = originalPosition;
    }

    private void StartSubtitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _positionTimer.Tick += (_, _) =>
        {
            if (_mpvPlayer == null)
            {
                return;
            }

            var subtitle = BuildAssaSubtitle();
            var format = new AdvancedSubStationAlpha();
            var text = format.ToText(subtitle, string.Empty);
            if (_oldSubtitleText == text)
            {
                return;
            }

            File.WriteAllText(_tempSubtitleFileName, text);
            if (!_isSubtitleLoaded)
            {
                _isSubtitleLoaded = true;
                _mpvPlayer.SubAdd(_tempSubtitleFileName);
            }
            else
            {
                _mpvPlayer.SubReload();
            }

            _oldSubtitleText = text;
        };
        _positionTimer.Start();
    }

    private string GetBorderStyle()
    {
        if (SelectedFontBoxType.BoxType == FontBoxType.OneBox) return "4";
        if (SelectedFontBoxType.BoxType == FontBoxType.BoxPerLine) return "3";
        return "1";
    }

    private string GetAlignment()
    {
        if (AlignmentAn1) return "1";
        if (AlignmentAn3) return "3";
        if (AlignmentAn4) return "4";
        if (AlignmentAn5) return "5";
        if (AlignmentAn6) return "6";
        if (AlignmentAn7) return "7";
        if (AlignmentAn8) return "8";
        if (AlignmentAn9) return "9";
        return "2";
    }

    private Subtitle BuildAssaSubtitle()
    {
        var style = new SsaStyle
        {
            Name = "Default",
            FontName = "Arial",
            FontSize = FontSize,
            Primary = SubtitleColor.ToSKColor(),
            Outline = Colors.Black.ToSKColor(),
            Background = Colors.Black.ToSKColor(),
            Secondary = Colors.Yellow.ToSKColor(),
            Alignment = GetAlignment(),
            OutlineWidth = 2,
            ShadowWidth = 1,
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
            BorderStyle = GetBorderStyle(),
            ScaleX = 100,
            ScaleY = 100,
        };

        var format = new AdvancedSubStationAlpha();
        var tempSub = new Subtitle();
        var tempText = format.ToText(tempSub, string.Empty);
        var tempLines = tempText.SplitToLines();
        format.LoadSubtitle(tempSub, tempLines, string.Empty);

        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            tempSub.Header,
            new List<SsaStyle> { style });

        var result = new Subtitle(_secondarySubtitle);
        result.Header = header;

        return result;
    }
}