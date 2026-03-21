using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;

public partial class AssSetBackgroundViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayerControl { get; set; }

    [ObservableProperty] private string _progressText = string.Empty;

    // Padding settings
    [ObservableProperty] private int _paddingLeft = 10;
    [ObservableProperty] private int _paddingRight = 10;
    [ObservableProperty] private int _paddingTop = 5;
    [ObservableProperty] private int _paddingBottom = 5;

    // Style settings
    [ObservableProperty] private Color _boxColor = Color.FromRgb(0, 0, 0);
    [ObservableProperty] private Color _outlineColor = Colors.White;
    [ObservableProperty] private Color _shadowColor = Color.FromRgb(128, 128, 128);
    [ObservableProperty] private int _outlineWidth = 2;
    [ObservableProperty] private int _shadowDistance = 1;
    [ObservableProperty] private int _boxStyleIndex; // 0=square, 1=rounded, 2=circle
    [ObservableProperty] private ObservableCollection<string> _boxStyles = [];
    [ObservableProperty] private int _radius = 10;

    // Fill width
    [ObservableProperty] private bool _fillWidth;
    [ObservableProperty] private int _fillWidthMarginLeft;
    [ObservableProperty] private int _fillWidthMarginRight;

    private Subtitle _subtitle = new();
    private Subtitle _previewSubtitle = new();
    private Paragraph _previewParagraph = new Paragraph();
    private List<SubtitleLineViewModel> _selectedItems;
    private int _videoWidth = 1920;
    private int _videoHeight = 1080;
    private readonly Random _random = new();
    private string? _videoFileName;
    private readonly string _tempSubtitleFileName;
    private readonly SubtitleFormat _assaFormat;
    private LibMpvDynamicPlayer? _mpvPlayer;
    private bool _isSubtitleLoaded;
    private string _oldSubtitleText = string.Empty;
    private DispatcherTimer _positionTimer = new();
    private SkiaExt.TrimResult? _trimResult;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public Subtitle ResultSubtitle => _subtitle;

    public AssSetBackgroundViewModel()
    {
        BoxStyles.Add(Se.Language.Assa.ProgressBarSquareCorners);
        BoxStyles.Add(Se.Language.Assa.ProgressBarRoundedCorners);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxCircle);

        VideoPlayerControl = new VideoPlayerControl(new VideoPlayerInstanceNone());

        _assaFormat = new AdvancedSubStationAlpha();
        _tempSubtitleFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ass");
        _selectedItems = new List<SubtitleLineViewModel>();
        LoadSettings();
    }

    private void LoadSettings()
    {
        PaddingLeft = Se.Settings.Assa.BackgroundBoxesPaddingLeft;
        PaddingRight = Se.Settings.Assa.BackgroundBoxesPaddingRight;
        PaddingTop = Se.Settings.Assa.BackgroundBoxesPaddingTop;
        PaddingBottom = Se.Settings.Assa.BackgroundBoxesPaddingBottom;

        FillWidth = Se.Settings.Assa.BackgroundBoxesFillWidth;
        FillWidthMarginLeft = Se.Settings.Assa.BackgroundBoxesFillWidthMarginLeft;
        FillWidthMarginRight = Se.Settings.Assa.BackgroundBoxesFillWidthMarginRight;

        BoxStyleIndex = Se.Settings.Assa.BackgroundBoxesStyleIndex;
        Radius = Se.Settings.Assa.BackgroundBoxesStyleRadius;

        BoxColor = Se.Settings.Assa.BackgroundBoxesBoxColor.FromHexToColor();
        ShadowColor = Se.Settings.Assa.BackgroundBoxesShadowColor.FromHexToColor();
        OutlineColor = Se.Settings.Assa.BackgroundBoxesOutlineColor.FromHexToColor();
    }

    private void SaveSettings()
    {
        Se.Settings.Assa.BackgroundBoxesPaddingLeft = PaddingLeft;
        Se.Settings.Assa.BackgroundBoxesPaddingRight = PaddingRight;
        Se.Settings.Assa.BackgroundBoxesPaddingTop = PaddingTop;
        Se.Settings.Assa.BackgroundBoxesPaddingBottom = PaddingBottom;

        Se.Settings.Assa.BackgroundBoxesFillWidth = FillWidth;
        Se.Settings.Assa.BackgroundBoxesFillWidthMarginLeft = FillWidthMarginLeft;
        Se.Settings.Assa.BackgroundBoxesFillWidthMarginRight = FillWidthMarginRight;

        Se.Settings.Assa.BackgroundBoxesStyleIndex = BoxStyleIndex;
        Se.Settings.Assa.BackgroundBoxesStyleRadius = Radius;

        Se.Settings.Assa.BackgroundBoxesBoxColor = BoxColor.FromColorToHex();
        Se.Settings.Assa.BackgroundBoxesShadowColor = ShadowColor.FromColorToHex();
        Se.Settings.Assa.BackgroundBoxesOutlineColor = OutlineColor.FromColorToHex();
    }

    public void Initialize(
        Subtitle subtitle,
        List<SubtitleLineViewModel> selectedItems,
        int videoWidth,
        int videoHeight,
        string? videoFileName)
    {
        _subtitle = new Subtitle(subtitle, false);
        _selectedItems = selectedItems;
        _videoWidth = videoWidth > 0 ? videoWidth : 1920;
        _videoHeight = videoHeight > 0 ? videoHeight : 1080;
        _videoFileName = videoFileName;

        _previewParagraph = _selectedItems.First().ToParagraph(_assaFormat);
        _previewSubtitle = new Subtitle(subtitle, false);
        _previewSubtitle.Paragraphs.Clear();
        _previewSubtitle.Paragraphs.Add(_previewParagraph);

        if (string.IsNullOrWhiteSpace(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                await VideoPlayerControl.Open(videoFileName);
            }
        });
    }

    [RelayCommand]
    private async Task Ok()
    {
        _positionTimer.Stop();

        var backgroundTask = Task.Run(async () => await ApplyBackgroundBoxes(), _cancellationTokenSource.Token);
        SaveSettings();
        await backgroundTask;

        OkPressed = true;
        Close();
    }

    private Lock _addSubtitleLock = new Lock();

    private async Task ApplyBackgroundBoxes()
    {
        if (string.IsNullOrEmpty(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        // Generate unique style name
        var boxStyleName = GenerateUniqueStyleName();
        var styleBoxBg = MakeBoxStyle(boxStyleName);
        _subtitle.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(_subtitle.Header, styleBoxBg);

        // Generate background boxes for each paragraph
        var count = 0;
        var total = _selectedItems.Count;

        await Task.Run(() =>
        {
            Parallel.ForEach(_selectedItems, p =>
            {
                var currentCount = Interlocked.Increment(ref count);
                Dispatcher.UIThread.Post(() => ProgressText = string.Format(Se.Language.Assa.GeneratingBackgroundBoxXOfY, currentCount, total));

                var previewSubtitle = new Subtitle();
                previewSubtitle.Header = _subtitle.Header;
                previewSubtitle.Footer = _subtitle.Footer;
                previewSubtitle.Paragraphs.Add(p.ToParagraph(_assaFormat));

                var previewScreenshotFileName = FfmpegGenerator.GetScreenShotWithSubtitle(previewSubtitle, _videoWidth, _videoHeight);

                // one retry
                if (string.IsNullOrEmpty(previewScreenshotFileName) || !File.Exists(previewScreenshotFileName))
                {
                    previewScreenshotFileName = FfmpegGenerator.GetScreenShotWithSubtitle(previewSubtitle, _videoWidth, _videoHeight);
                }

                // skip if still not valid
                if (string.IsNullOrEmpty(previewScreenshotFileName) || !File.Exists(previewScreenshotFileName))
                {
                    return;
                }

                var skBitmap = SKBitmap.Decode(previewScreenshotFileName);
                var trimResult = skBitmap.TrimTransparentPixels();

                var left = FillWidth ? FillWidthMarginLeft : trimResult.Left - PaddingLeft;
                var right = FillWidth ? (_videoWidth - FillWidthMarginRight) : (trimResult.Left + trimResult.TrimmedBitmap.Width + PaddingRight);
                var top = trimResult.Top - PaddingTop;
                var bottom = trimResult.Top + trimResult.TrimmedBitmap.Height + PaddingBottom;
                var boxDrawing = GenerateBackgroundBox(left, right, top, bottom);
                var boxParagraph = MakeBoxParagraph(boxStyleName, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, boxDrawing);

                lock (_addSubtitleLock)
                {
                    _subtitle.InsertParagraphInCorrectTimeOrder(boxParagraph);
                }
            });
        });

        _subtitle.Renumber();
    }

    private static Paragraph MakeBoxParagraph(string boxStyleName, double startMs, double endMs, string boxDrawing)
    {
        return new Paragraph(boxDrawing, startMs, endMs)
        {
            Layer = -1000,
            Extra = boxStyleName,
        };
    }

    private SsaStyle MakeBoxStyle(string boxStyleName)
    {

        // Add style
        return new SsaStyle
        {
            Alignment = "7",
            Name = boxStyleName,
            MarginLeft = 0,
            MarginRight = 0,
            MarginVertical = 0,
            Primary = SKColor.Parse($"#{BoxColor.A:X2}{BoxColor.R:X2}{BoxColor.G:X2}{BoxColor.B:X2}"),
            Secondary = SKColor.Parse($"#{ShadowColor.A:X2}{ShadowColor.R:X2}{ShadowColor.G:X2}{ShadowColor.B:X2}"),
            Tertiary = SKColor.Parse($"#{ShadowColor.A:X2}{ShadowColor.R:X2}{ShadowColor.G:X2}{ShadowColor.B:X2}"),
            Background = SKColor.Parse($"#{ShadowColor.A:X2}{ShadowColor.R:X2}{ShadowColor.G:X2}{ShadowColor.B:X2}"),
            Outline = SKColor.Parse($"#{OutlineColor.A:X2}{OutlineColor.R:X2}{OutlineColor.G:X2}{OutlineColor.B:X2}"),
            ShadowWidth = ShadowDistance,
            OutlineWidth = OutlineWidth,
        };
    }

    private string GenerateBackgroundBox(int left, int right, int top, int bottom)
    {
        var width = right - left;
        var height = bottom - top;

        if (BoxStyleIndex == 1 && Radius > 0) // Rounded corners
        {
            var r = Math.Min(Radius, Math.Min(width / 2, height / 2));
            return
                $"{{\\p1}}m {left + r} {top} l {right - r} {top} b {right} {top} {right} {top + r} {right} {top + r} l {right} {bottom - r} b {right} {bottom} {right - r} {bottom} {right - r} {bottom} l {left + r} {bottom} b {left} {bottom} {left} {bottom - r} {left} {bottom - r} l {left} {top + r} b {left} {top} {left + r} {top} {left + r} {top}{{\\p0}}";
        }
        else if (BoxStyleIndex == 2) // Circle
        {
            var centerX = (left + right) / 2;
            var centerY = (top + bottom) / 2;
            var radiusX = width / 2;
            var radiusY = height / 2;

            // Simplified circle approximation using bezier curves
            var kappa = 0.5522847498;
            var ox = (int)(radiusX * kappa);
            var oy = (int)(radiusY * kappa);

            return
                $"{{\\p1}}m {centerX} {centerY - radiusY} b {centerX + ox} {centerY - radiusY} {centerX + radiusX} {centerY - oy} {centerX + radiusX} {centerY} b {centerX + radiusX} {centerY + oy} {centerX + ox} {centerY + radiusY} {centerX} {centerY + radiusY} b {centerX - ox} {centerY + radiusY} {centerX - radiusX} {centerY + oy} {centerX - radiusX} {centerY} b {centerX - radiusX} {centerY - oy} {centerX - ox} {centerY - radiusY} {centerX} {centerY - radiusY}{{\\p0}}";
        }

        // Square corners (default)
        return $"{{\\p1}}m {left} {top} l {right} {top} {right} {bottom} {left} {bottom}{{\\p0}}";
    }

    private string GenerateUniqueStyleName()
    {
        var baseName = "SE-box-bg";
        var styleNames = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);

        if (!styleNames.Contains(baseName))
        {
            return baseName;
        }

        var tryCount = 0;
        while (tryCount < 100)
        {
            var name = $"SE-box-bg{_random.Next(1000, 9999)}";
            if (!styleNames.Contains(name))
            {
                return name;
            }

            tryCount++;
        }

        return $"SE-box-bg{Guid.NewGuid():N}";
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void StartPreviewTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _positionTimer.Tick += (s, e) =>
        {
            if (_mpvPlayer == null)
            {
                return;
            }

            var previewSubtitle = GeneratePreviewSubtitle();
            var text = _assaFormat.ToText(previewSubtitle, string.Empty);
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

    private Subtitle GeneratePreviewSubtitle()
    {
        var previewSubtitle = new Subtitle(_subtitle, false);
        previewSubtitle.Paragraphs.Clear();
        previewSubtitle.Paragraphs.Add(_previewParagraph);

        if (_trimResult == null)
        {
            var previewScreenshotFileName = FfmpegGenerator.GetScreenShotWithSubtitle(previewSubtitle, _videoWidth, _videoHeight);
            var skBitmap = SKBitmap.Decode(previewScreenshotFileName);
            _trimResult = skBitmap.TrimTransparentPixels();
        }

        // Temporarily apply background boxes
        var boxStyleName = "SE-box-bg-preview";
        var styleBoxBg = MakeBoxStyle(boxStyleName);
        previewSubtitle.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(previewSubtitle.Header, styleBoxBg);

        var p = _previewParagraph;

        var left = FillWidth ? FillWidthMarginLeft : _trimResult.Left - PaddingLeft;
        var right = FillWidth ? (_videoWidth - FillWidthMarginRight) : (_trimResult.Left + _trimResult.TrimmedBitmap.Width + PaddingRight);
        var top = _trimResult.Top - PaddingTop;
        var bottom = _trimResult.Top + _trimResult.TrimmedBitmap.Height + PaddingBottom;
        var boxDrawing = GenerateBackgroundBox(left, right, top, bottom);
        var boxParagraph = MakeBoxParagraph(boxStyleName, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, boxDrawing);

        previewSubtitle.InsertParagraphInCorrectTimeOrder(boxParagraph);

        return previewSubtitle;
    }

    internal async void OnLoaded()
    {
        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        await VideoPlayerControl.WaitForPlayersReadyAsync();
        VideoPlayerControl.HideVideoControls();
        VideoPlayerControl.Position = _previewParagraph.StartTime.TotalSeconds + (_previewParagraph.Duration.TotalSeconds / 2.0);
        _mpvPlayer = VideoPlayerControl.VideoPlayerInstance as LibMpvDynamicPlayer;
        StartPreviewTimer();
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();
        _cancellationTokenSource.Cancel();
        VideoPlayerControl.VideoPlayerInstance.CloseFile();
        try
        {
            File.Delete(_tempSubtitleFileName);
        }
        catch
        {
            // ignore
        }
    }
}