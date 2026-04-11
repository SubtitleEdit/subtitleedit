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
using System.Text;
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
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxSpikes);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxBubbles);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxWave);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxHexagon);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxTornPaper);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxCloud);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxTornPaperDouble);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxStarburst);
        BoxStyles.Add(Se.Language.Assa.BackgroundBoxScroll);

        VideoPlayerControl = new VideoPlayerControl(new NoopVideoPlayer());

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
        else if (BoxStyleIndex == 3) // Spikes
        {
            return GenerateSpikesBox(left, right, top, bottom);
        }
        else if (BoxStyleIndex == 4) // Bubbles
        {
            return GenerateBubblesBox(left, right, top, bottom);
        }

        else if (BoxStyleIndex == 5) // Wave
        {
            return GenerateWaveBox(left, right, top, bottom);
        }
        else if (BoxStyleIndex == 6) // Hexagon
        {
            return GenerateHexagonBox(left, right, top, bottom);
        }
        else if (BoxStyleIndex == 7) // Torn paper
        {
            return GenerateTornPaperBox(left, right, top, bottom);
        }
        else if (BoxStyleIndex == 8) // Cloud
        {
            return GenerateCloudBox(left, right, top, bottom);
        }
        else if (BoxStyleIndex == 9) // Torn paper (top & bottom)
        {
            return GenerateTornPaperDoubleBox(left, right, top, bottom);
        }
        else if (BoxStyleIndex == 10) // Starburst
        {
            return GenerateStarburstBox(left, right, top, bottom);
        }
        else if (BoxStyleIndex == 11) // Scroll / Parchment
        {
            return GenerateScrollBox(left, right, top, bottom);
        }

        // Square corners (default)
        return $"{{\\p1}}m {left} {top} l {right} {top} {right} {bottom} {left} {bottom}{{\\p0}}";
    }

    private static string GenerateWaveBox(int left, int right, int top, int bottom)
    {
        const int amplitude = 6;
        var halfWaves = Math.Max(4, Math.Min(12, (right - left) / 30));
        var segW = (right - left) / (double)halfWaves;

        var sb = new StringBuilder();
        sb.Append($"m {left} {top} ");

        // Top edge: alternating up/down bezier bumps (left to right)
        for (var i = 0; i < halfWaves; i++)
        {
            var x = left + i * segW;
            var xEnd = left + (i + 1) * segW;
            var yCtrl = i % 2 == 0 ? top - amplitude : top + amplitude;
            sb.Append($"b {(int)(x + segW / 3)} {yCtrl} {(int)(x + 2 * segW / 3)} {yCtrl} {(int)xEnd} {top} ");
        }

        // Right edge
        sb.Append($"l {right} {bottom} ");

        // Bottom edge: mirror wave pattern (right to left)
        for (var i = halfWaves - 1; i >= 0; i--)
        {
            var x = left + i * segW;
            var yCtrl = i % 2 == 0 ? bottom + amplitude : bottom - amplitude;
            sb.Append($"b {(int)(x + 2 * segW / 3)} {yCtrl} {(int)(x + segW / 3)} {yCtrl} {(int)x} {bottom} ");
        }

        return $"{{\\p1}}{sb.ToString().TrimEnd()}{{\\p0}}";
    }

    private static string GenerateHexagonBox(int left, int right, int top, int bottom)
    {
        var cy = (top + bottom) / 2;
        var chamfer = Math.Min((bottom - top) / 2, (right - left) / 5);
        return $"{{\\p1}}m {left + chamfer} {top} l {right - chamfer} {top} {right} {cy} {right - chamfer} {bottom} {left + chamfer} {bottom} {left} {cy}{{\\p0}}";
    }

    private static string GenerateTornPaperBox(int left, int right, int top, int bottom)
    {
        var rng = new Random((left * 397) ^ (top * 613) ^ right);
        const int maxTearH = 22;
        var count = Math.Max(3, (right - left) / 28);
        var segW = (right - left) / (double)count;
        var sb = new StringBuilder();
        sb.Append($"m {left} {top} ");
        for (var i = 0; i < count; i++)
        {
            var tipX = (int)(left + i * segW + segW * (0.2 + rng.NextDouble() * 0.6));
            var tipY = top - rng.Next(8, maxTearH + 1);
            var endX = i == count - 1 ? right : (int)(left + (i + 1) * segW);
            sb.Append($"l {tipX} {tipY} {endX} {top} ");
        }
        sb.Append($"l {right} {bottom} {left} {bottom}");
        return $"{{\\p1}}{sb.ToString().TrimEnd()}{{\\p0}}";
    }

    private static string GenerateCloudBox(int left, int right, int top, int bottom)
    {
        const double k = 0.5523;
        var w = right - left;
        var h = bottom - top;

        // Single target radius keeps all bumps roughly the same size on every edge.
        // Clamp so tiny boxes still look cloud-like and large boxes don't get huge bumps.
        var targetR = Math.Clamp(Math.Min(w, h) / 4.0, 14.0, 48.0);

        // Snap each edge's count to the nearest integer, then recompute the exact radius
        // so bumps fill the edge perfectly with no gaps.
        var hCount = Math.Max(2, (int)Math.Round(w / (2.0 * targetR)));
        var hR = w / (2.0 * hCount);

        var vCount = Math.Max(1, (int)Math.Round(h / (2.0 * targetR)));
        var vR = h / (2.0 * vCount);

        var sb = new StringBuilder();
        var curX = (double)left;
        var curY = (double)top;
        sb.Append($"m {left} {top} ");

        // Top edge: bell-curved bump heights — taller in the middle, shorter at the ends
        for (var i = 0; i < hCount; i++)
        {
            var t = hCount > 1 ? (double)i / (hCount - 1) : 0.5;
            var bell = 1.0 - 4.0 * (t - 0.5) * (t - 0.5); // 1 at centre, 0 at edges
            var bumpH = hR * (0.6 + 0.9 * bell);            // 0.6×hR at edges ? 1.5×hR at centre
            sb.Append($"b {(int)curX} {(int)(top - bumpH * k)} {(int)(curX + hR * (1 - k))} {(int)(top - bumpH)} {(int)(curX + hR)} {(int)(top - bumpH)} ");
            sb.Append($"b {(int)(curX + hR * (1 + k))} {(int)(top - bumpH)} {(int)(curX + 2 * hR)} {(int)(top - bumpH * k)} {(int)(curX + 2 * hR)} {top} ");
            curX += 2 * hR;
        }

        // Right edge: uniform bumps rightward (top ? bottom)
        curY = top;
        for (var i = 0; i < vCount; i++)
        {
            sb.Append($"b {(int)(right + vR * k)} {(int)curY} {(int)(right + vR)} {(int)(curY + vR * (1 - k))} {(int)(right + vR)} {(int)(curY + vR)} ");
            sb.Append($"b {(int)(right + vR)} {(int)(curY + vR * (1 + k))} {(int)(right + vR * k)} {(int)(curY + 2 * vR)} {right} {(int)(curY + 2 * vR)} ");
            curY += 2 * vR;
        }

        // Bottom edge: uniform bumps downward (right ? left)
        curX = right;
        for (var i = 0; i < hCount; i++)
        {
            sb.Append($"b {(int)curX} {(int)(bottom + hR * k)} {(int)(curX - hR * (1 - k))} {(int)(bottom + hR)} {(int)(curX - hR)} {(int)(bottom + hR)} ");
            sb.Append($"b {(int)(curX - hR * (1 + k))} {(int)(bottom + hR)} {(int)(curX - 2 * hR)} {(int)(bottom + hR * k)} {(int)(curX - 2 * hR)} {bottom} ");
            curX -= 2 * hR;
        }

        // Left edge: uniform bumps leftward (bottom ? top)
        curY = bottom;
        for (var i = 0; i < vCount; i++)
        {
            sb.Append($"b {(int)(left - vR * k)} {(int)curY} {(int)(left - vR)} {(int)(curY - vR * (1 - k))} {(int)(left - vR)} {(int)(curY - vR)} ");
            sb.Append($"b {(int)(left - vR)} {(int)(curY - vR * (1 + k))} {(int)(left - vR * k)} {(int)(curY - 2 * vR)} {left} {(int)(curY - 2 * vR)} ");
            curY -= 2 * vR;
        }

        return $"{{\\p1}}{sb.ToString().TrimEnd()}{{\\p0}}";
    }

    private static string GenerateTornPaperDoubleBox(int left, int right, int top, int bottom)
    {
        var rng = new Random((left * 397) ^ (top * 613) ^ right);
        const int maxTearH = 22;
        var count = Math.Max(3, (right - left) / 28);
        var segW = (right - left) / (double)count;
        var sb = new StringBuilder();
        sb.Append($"m {left} {top} ");
        for (var i = 0; i < count; i++)
        {
            var tipX = (int)(left + i * segW + segW * (0.2 + rng.NextDouble() * 0.6));
            var tipY = top - rng.Next(8, maxTearH + 1);
            var endX = i == count - 1 ? right : (int)(left + (i + 1) * segW);
            sb.Append($"l {tipX} {tipY} {endX} {top} ");
        }
        sb.Append($"l {right} {bottom} ");
        for (var i = count - 1; i >= 0; i--)
        {
            var tipX = (int)(left + i * segW + segW * (0.2 + rng.NextDouble() * 0.6));
            var tipY = bottom + rng.Next(8, maxTearH + 1);
            var endX = i == 0 ? left : (int)(left + i * segW);
            sb.Append($"l {tipX} {tipY} {endX} {bottom} ");
        }
        return $"{{\\p1}}{sb.ToString().TrimEnd()}{{\\p0}}";
    }

    private static string GenerateSpikesBox(int left, int right, int top, int bottom)
    {
        const int spikeH = 8;
        var count = Math.Max(2, (right - left) / 20);
        var spikeW = (right - left) / (double)count;

        var sb = new StringBuilder();
        sb.Append($"m {left} {top} ");

        // Top edge: spikes pointing up (left to right)
        for (var i = 0; i < count; i++)
        {
            var tipX = (int)(left + i * spikeW + spikeW / 2.0);
            var endX = (int)(left + (i + 1) * spikeW);
            sb.Append($"l {tipX} {top - spikeH} {endX} {top} ");
        }

        // Right edge
        sb.Append($"l {right} {bottom} ");

        // Bottom edge: spikes pointing down (right to left)
        for (var i = count - 1; i >= 0; i--)
        {
            var tipX = (int)(left + i * spikeW + spikeW / 2.0);
            var endX = (int)(left + i * spikeW);
            sb.Append($"l {tipX} {bottom + spikeH} {endX} {bottom} ");
        }

        return $"{{\\p1}}{sb.ToString().TrimEnd()}{{\\p0}}";
    }

    private static string GenerateBubblesBox(int left, int right, int top, int bottom)
    {
        const double k = 0.5523;
        var count = Math.Max(2, (right - left) / 18);
        var d = (right - left) / (double)count;
        var r = d / 2.0;

        var sb = new StringBuilder();
        sb.Append($"m {left} {top} ");

        // Top edge: bubbles pointing up (left to right)
        for (var i = 0; i < count; i++)
        {
            var bx = left + i * d;
            sb.Append($"b {(int)bx} {(int)(top - r * k)} {(int)(bx + r * (1 - k))} {(int)(top - r)} {(int)(bx + r)} {(int)(top - r)} ");
            sb.Append($"b {(int)(bx + r * (1 + k))} {(int)(top - r)} {(int)(bx + 2 * r)} {(int)(top - r * k)} {(int)(bx + 2 * r)} {top} ");
        }

        // Right edge
        sb.Append($"l {right} {bottom} ");

        // Bottom edge: bubbles pointing down (right to left)
        for (var i = count - 1; i >= 0; i--)
        {
            var bx = left + i * d;
            sb.Append($"b {(int)(bx + 2 * r)} {(int)(bottom + r * k)} {(int)(bx + r * (1 + k))} {(int)(bottom + r)} {(int)(bx + r)} {(int)(bottom + r)} ");
            sb.Append($"b {(int)(bx + r * (1 - k))} {(int)(bottom + r)} {(int)bx} {(int)(bottom + r * k)} {(int)bx} {bottom} ");
        }

        return $"{{\\p1}}{sb.ToString().TrimEnd()}{{\\p0}}";
    }

    private static string GenerateStarburstBox(int left, int right, int top, int bottom)
    {
        var cx = (left + right) / 2.0;
        var cy = (top + bottom) / 2.0;
        var a = (right - left) / 2.0;
        var b = (bottom - top) / 2.0;

        var baseSpikeOut = Math.Clamp(Math.Min(a, b) * 0.55, 8.0, 28.0);
        var rng = new Random((left * 397) ^ (top * 613) ^ right);

        // Random spike count per subtitle: 9–20 spikes
        var numSpikes = rng.Next(9, 21);
        var numPoints = numSpikes * 2;

        var sb = new StringBuilder();
        for (var i = 0; i < numPoints; i++)
        {
            var angle = i * 2.0 * Math.PI / numPoints - Math.PI / 2.0;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var ellipseR = (a * b) / Math.Sqrt(b * b * cos * cos + a * a * sin * sin);
            double r;
            if (i % 2 == 0) // outer spike: very wide random range
            {
                r = ellipseR + baseSpikeOut * (0.1 + rng.NextDouble() * 2.4);
            }
            else // inner valley: wide random depth
            {
                r = ellipseR * (0.50 + rng.NextDouble() * 0.40);
            }
            var px = (int)(cx + r * cos);
            var py = (int)(cy + r * sin);
            sb.Append(i == 0 ? $"m {px} {py} " : $"l {px} {py} ");
        }

        return $"{{\\p1}}{sb.ToString().TrimEnd()}{{\\p0}}";
    }

    private static string GenerateScrollBox(int left, int right, int top, int bottom)
    {
        var h = bottom - top;
        var curve = Math.Max(10, Math.Min(30, h / 2));
        var q = h / 4;

        // Left and right edges are S-curves (bezier), top and bottom are straight lines
        return $"{{\\p1}}m {left} {top} " +
               $"l {right} {top} " +
               $"b {right + curve} {top + q} {right - curve} {bottom - q} {right} {bottom} " +
               $"l {left} {bottom} " +
               $"b {left - curve} {bottom - q} {left + curve} {top + q} {left} {top}{{\\p0}}";
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
        _mpvPlayer = VideoPlayerControl.VideoPlayer as LibMpvDynamicPlayer;
        StartPreviewTimer();
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();
        _cancellationTokenSource.Cancel();
        VideoPlayerControl.VideoPlayer.CloseFile();
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
