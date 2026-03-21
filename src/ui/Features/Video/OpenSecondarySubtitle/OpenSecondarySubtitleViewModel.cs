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
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public partial class OpenSecondarySubtitleViewModel : ObservableObject
{
    [ObservableProperty] private Color _subtitleColor;
    [ObservableProperty] private FontBoxItem _selectedFontBoxType;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphIndex = -1;
    [ObservableProperty] private AlignmentItem _selectedFontAlignment;

    public ObservableCollection<FontBoxItem> FontBoxTypes { get; }
    public ObservableCollection<AlignmentItem> FontAlignments { get; }
    public VideoPlayerControl VideoPlayerControl { get; set; }
    public ComboBox ComboBoxParagraphs { get; set; }

    private Subtitle _secondarySubtitle = new Subtitle();
    private Subtitle _subtitle = new Subtitle();
    private SubtitleFormat _subtitleFormat = new SubRip();
    private SubtitleFormat _assaFormat = new AdvancedSubStationAlpha();
    private readonly IWindowService _windowService;
    private string? _videoFileName;
    private FfmpegMediaInfo2? _mediaInfo;
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

        SubtitleColor = Colors.White;
        FontBoxTypes = new ObservableCollection<FontBoxItem>
        {
            new(FontBoxType.None, Se.Language.General.None),
            new(FontBoxType.OneBox, Se.Language.Video.BurnIn.OneBox),
            new(FontBoxType.BoxPerLine, Se.Language.Video.BurnIn.BoxPerLine),
        };
        SelectedFontBoxType = FontBoxTypes[0];
        FontAlignments = new ObservableCollection<AlignmentItem>(AlignmentItem.Alignments);
        SelectedFontAlignment = AlignmentItem.Alignments[1]; // an8 = Top-center
        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();

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
        ResultSubtitle = BuildAssaSubtitle(false);
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
        _mediaInfo = mediaInfo;

        Paragraphs = new ObservableCollection<SubtitleDisplayItem>(
            secondarySubtitle.Paragraphs.Select(p => new SubtitleDisplayItem(new SubtitleLineViewModel(p, _assaFormat))));

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayerControl.Open(videoFileName);
            }

            var height = mediaInfo?.Dimension.Height ?? 1080;
            FontSize = AssaResampler.Resample(AdvancedSubStationAlpha.DefaultHeight, height, Se.Settings.Video.MpvPreviewFontSize);
        });
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

            var subtitle = BuildAssaSubtitle(true);
            var text = _assaFormat.ToText(subtitle, string.Empty);
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
        if (SelectedFontBoxType.BoxType == FontBoxType.OneBox)
        {
            return "4";
        }

        if (SelectedFontBoxType.BoxType == FontBoxType.BoxPerLine)
        {
            return "3";
        }

        return "1";
    }

    private string GetAlignment()
    {
        return SelectedFontAlignment.Code;
    }

    private Subtitle BuildAssaSubtitle(bool mergeWithSubtitle)
    {
        var style = new SsaStyle
        {
            Name = "Style" + Guid.NewGuid().ToString().Replace("-", string.Empty),
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

        if (!mergeWithSubtitle && _subtitleFormat.GetType() != typeof(AdvancedSubStationAlpha))
        {
            style.FontSize = Se.Settings.Video.MpvPreviewFontSize;
        }

        style.Outline = new SkiaSharp.SKColor(style.Outline.Red, style.Outline.Green, style.Outline.Blue, SubtitleColor.A);
        style.Background = new SkiaSharp.SKColor(style.Background.Red, style.Background.Green, style.Background.Blue, SubtitleColor.A);

        var result = new Subtitle(_secondarySubtitle);
        result.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            new List<SsaStyle> { style });

        var width = _mediaInfo?.Dimension.Width ?? 1920;
        var height = _mediaInfo?.Dimension.Height ?? 1080;
        result.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + width.ToString(CultureInfo.InvariantCulture), "[Script Info]", result.Header);
        result.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + height.ToString(CultureInfo.InvariantCulture), "[Script Info]", result.Header);

        if (mergeWithSubtitle)
        {
            if (_subtitleFormat.GetType() == typeof(AdvancedSubStationAlpha))
            {
                result = new Subtitle(_subtitle);
                var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(result.Header);
                styles.Add(style);
                result.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(_subtitle.Header, styles);

                foreach (var p in _secondarySubtitle.Paragraphs)
                {
                    p.Extra = style.Name;
                    p.Layer = -1;
                    result.Paragraphs.Add(p);
                }
            }
            else
            {
                var defaultStyle = AdvancedSubStationAlpha.GetSsaStyle("Default", result.Header);
                defaultStyle.FontSize = AssaResampler.Resample(AdvancedSubStationAlpha.DefaultHeight, height, Se.Settings.Video.MpvPreviewFontSize);
                defaultStyle.Bold = Se.Settings.Video.MpvPreviewFontBold;
                result.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(result.Header, defaultStyle);

                foreach (var p in _subtitle.Paragraphs)
                {
                    p.Layer = 1;
                    p.Extra = "Default";
                    result.Paragraphs.Add(p);
                }
            }
        }
        else
        {
            foreach (var p in result.Paragraphs)
            {
                p.Extra = style.Name;
            }
        }

        return result;
    }
}