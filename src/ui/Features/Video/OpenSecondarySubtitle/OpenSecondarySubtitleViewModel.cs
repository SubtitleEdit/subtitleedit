using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public partial class OpenSecondarySubtitleViewModel : ObservableObject
{
    [ObservableProperty] private Color _subtitleColor;
    [ObservableProperty] private FontBoxItem _selectedFontBoxType;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private bool _fontBold;
    [ObservableProperty] private ObservableCollection<SubtitleDisplayItem> _paragraphs;
    [ObservableProperty] private int _selectedParagraphIndex = -1;
    [ObservableProperty] private AlignmentItem _selectedFontAlignment;
    [ObservableProperty] private MpvJustifyDisplay _selectedJustify;
    [ObservableProperty] private bool _overrideStyle;
    [ObservableProperty] private bool _doNotShowAgain;

    public ObservableCollection<FontBoxItem> FontBoxTypes { get; }
    public ObservableCollection<AlignmentItem> FontAlignments { get; }
    public ObservableCollection<MpvJustifyDisplay> JustifyItems { get; }
    public VideoPlayerControl VideoPlayerControl { get; set; }
    public ComboBox ComboBoxParagraphs { get; set; }

    // Stable per-dialog style name: regenerating it in every BuildAssaSubtitle call made the
    // serialized preview text differ on every 500 ms tick, forcing a SubReload twice a second
    // (the "flicker" in issue #13425).
    private readonly string _styleName = "Style" + Guid.NewGuid().ToString().Replace("-", string.Empty);
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

    /// <summary>
    /// True when re-styling the second subtitle already on the video player (Video > Edit second
    /// subtitle settings, #15110) rather than opening a new file. Set in Initialize, so the window
    /// can pick its title from it.
    /// </summary>
    public bool IsEditingSettings { get; private set; }

    public OpenSecondarySubtitleViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        SubtitleColor = Colors.White;
        FontBold = Se.Settings.Video.MpvPreviewFontBold;
        FontBoxTypes = new ObservableCollection<FontBoxItem>
        {
            new(FontBoxType.None, Se.Language.General.None),
            new(FontBoxType.OneBox, Se.Language.Video.BurnIn.OneBox),
            new(FontBoxType.BoxPerLine, Se.Language.General.BoxPerLine),
        };
        SelectedFontBoxType = FontBoxTypes[0];
        FontAlignments = new ObservableCollection<AlignmentItem>(AlignmentItem.Alignments);
        SelectedFontAlignment = AlignmentItem.Alignments[1]; // an8 = Top-center
        JustifyItems = new ObservableCollection<MpvJustifyDisplay>(MpvJustifyDisplay.GetAll());
        SelectedJustify = JustifyItems[0]; // auto

        // Start from the saved style instead of the defaults above, so re-opening the dialog to
        // adjust the second subtitle doesn't reset it (#14842). Font size needs the video height,
        // so it's set in Initialize.
        var video = Se.Settings.Video;
        OverrideStyle = video.SecondarySubtitleOverrideStyle;
        if (OverrideStyle)
        {
            SubtitleColor = video.SecondarySubtitleColor.FromHexToColor();
            FontBold = video.SecondarySubtitleFontBold;
            SelectedFontBoxType = FontBoxTypes.FirstOrDefault(p => p.BoxType == video.SecondarySubtitleBoxType) ?? FontBoxTypes[0];
            SelectedFontAlignment = FontAlignments.FirstOrDefault(p => p.Code == video.SecondarySubtitleAlignment) ?? FontAlignments[1];
            SelectedJustify = JustifyItems.FirstOrDefault(p => p.Code == video.SecondarySubtitleJustify) ?? JustifyItems[0];
            DoNotShowAgain = !video.SecondarySubtitleShowDialog;
        }

        Paragraphs = new ObservableCollection<SubtitleDisplayItem>();

        _tempSubtitleFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ass");
        _oldSubtitleText = string.Empty;

        VideoPlayerControl = new VideoPlayerControl(new EmptyVideoPlayer());
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

        var video = Se.Settings.Video;
        video.SecondarySubtitleOverrideStyle = OverrideStyle;
        if (!IsEditingSettings)
        {
            video.SecondarySubtitleShowDialog = !(OverrideStyle && DoNotShowAgain);
        }

        if (OverrideStyle)
        {
            SecondarySubtitleStyler.SaveToSettings(FontSize, GetVideoHeight(), FontBold, SubtitleColor, SelectedFontBoxType.BoxType, SelectedFontAlignment.Code, SelectedJustify.Code);
        }

        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void Initialize(Subtitle secondarySubtitle, Subtitle subtitle, SubtitleFormat subtitleFormat, Logic.Media.FfmpegMediaInfo2? mediaInfo, string? videoFileName, bool isEditingSettings = false)
    {
        IsEditingSettings = isEditingSettings;
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

            var height = GetVideoHeight();
            FontSize = OverrideStyle
                ? SecondarySubtitleStyler.GetFontSizeFromSettings(height)
                : AssaResampler.Resample(AdvancedSubStationAlpha.DefaultHeight, height, Se.Settings.Video.MpvPreviewFontSize);
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
            _mpvPlayer = VideoPlayerControl.VideoPlayer as LibMpvDynamicPlayer;

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
        VideoPlayerControl.CloseAndDisposePlayer();
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/video-player", "secondary-subtitles");
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
        videoPlayer.VideoPlayer.Play();
        await Task.Delay(milliseconds);
        videoPlayer.VideoPlayer.Pause();
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

    private int GetVideoHeight()
    {
        return SecondarySubtitleStyler.GetVideoSize(_mediaInfo).Height;
    }

    private Subtitle BuildAssaSubtitle(bool mergeWithSubtitle)
    {
        var style = SecondarySubtitleStyler.MakeStyle(_styleName, FontSize, FontBold, SubtitleColor, SelectedFontBoxType.BoxType, SelectedFontAlignment.Code);

        var (width, height) = SecondarySubtitleStyler.GetVideoSize(_mediaInfo);
        var secondaryParagraphs = SecondarySubtitleJustifier.Apply(_secondarySubtitle.Paragraphs, style, SelectedJustify.Code, width, height);

        var result = new Subtitle(_secondarySubtitle);
        result.Paragraphs.Clear();
        result.Paragraphs.AddRange(secondaryParagraphs);
        SecondarySubtitleStyler.SetHeader(result, style, width, height);

        if (mergeWithSubtitle)
        {
            if (_subtitleFormat.GetType() == typeof(AdvancedSubStationAlpha))
            {
                result = new Subtitle(_subtitle);
                var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(result.Header);
                styles.Add(style);
                result.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(_subtitle.Header, styles);

                foreach (var p in secondaryParagraphs)
                {
                    p.Extra = style.Name;
                    p.Layer = -1;
                    result.Paragraphs.Add(p);
                }
            }
            else
            {
                var defaultStyle = AdvancedSubStationAlpha.GetSsaStyle("Default", result.Header);
                defaultStyle.FontName = Se.Settings.Video.MpvPreviewFontName;
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