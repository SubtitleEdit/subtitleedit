using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;

public partial class MergeTwoSubtitlesViewModel : ObservableObject
{
    public const string OutputFormatSubRip = "SubRip";
    public const string OutputFormatAssa = "Assa";

    [ObservableProperty] private ObservableCollection<MergeTwoSubtitlesDisplayItem> _items1;
    [ObservableProperty] private ObservableCollection<MergeTwoSubtitlesDisplayItem> _items2;
    [ObservableProperty] private MergeTwoSubtitlesDisplayItem? _selectedItem1;
    [ObservableProperty] private MergeTwoSubtitlesDisplayItem? _selectedItem2;
    [ObservableProperty] private string _file1Display;
    [ObservableProperty] private string _file2Display;

    [ObservableProperty] private ObservableCollection<OutputFormatItem> _outputFormats;
    [ObservableProperty] private OutputFormatItem _selectedOutputFormat;
    [ObservableProperty] private bool _isAssaSelected;

    [ObservableProperty] private ObservableCollection<string> _fontNames;

    // Style 1
    [ObservableProperty] private string _fontName1;
    [ObservableProperty] private int _fontSize1;
    [ObservableProperty] private bool _bold1;
    [ObservableProperty] private bool _italic1;
    [ObservableProperty] private Color _primaryColor1;
    [ObservableProperty] private Color _outlineColor1;
    [ObservableProperty] private decimal _outlineWidth1;
    [ObservableProperty] private decimal _shadowWidth1;
    [ObservableProperty] private bool _alignTop1;

    // Style 2
    [ObservableProperty] private string _fontName2;
    [ObservableProperty] private int _fontSize2;
    [ObservableProperty] private bool _bold2;
    [ObservableProperty] private bool _italic2;
    [ObservableProperty] private Color _primaryColor2;
    [ObservableProperty] private Color _outlineColor2;
    [ObservableProperty] private decimal _outlineWidth2;
    [ObservableProperty] private decimal _shadowWidth2;
    [ObservableProperty] private bool _alignTop2;

    [ObservableProperty] private Bitmap? _imagePreview;
    [ObservableProperty] private bool _isMergeEnabled;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle ResultSubtitle { get; private set; }
    public SubtitleFormat ResultFormat { get; private set; }

    private readonly IFileHelper _fileHelper;
    private Subtitle _subtitle1;
    private Subtitle _subtitle2;
    private bool _loaded;

    public MergeTwoSubtitlesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Items1 = new ObservableCollection<MergeTwoSubtitlesDisplayItem>();
        Items2 = new ObservableCollection<MergeTwoSubtitlesDisplayItem>();
        File1Display = string.Empty;
        File2Display = string.Empty;

        _subtitle1 = new Subtitle();
        _subtitle2 = new Subtitle();
        ResultSubtitle = new Subtitle();
        ResultFormat = new SubRip();

        OutputFormats = new ObservableCollection<OutputFormatItem>
        {
            new(OutputFormatSubRip, Se.Language.Tools.MergeTwoSubtitles.OutputFormatSubRip),
            new(OutputFormatAssa, Se.Language.Tools.MergeTwoSubtitles.OutputFormatAssa),
        };

        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());

        var s = Se.Settings.Tools;
        var format = OutputFormats.FirstOrDefault(p => p.Id == s.MergeTwoSubtitlesOutputFormat) ?? OutputFormats[1];
        SelectedOutputFormat = format;
        IsAssaSelected = format.Id == OutputFormatAssa;

        FontName1 = ResolveFontName(s.MergeTwoSubtitlesFontName1);
        FontSize1 = s.MergeTwoSubtitlesFontSize1;
        Bold1 = s.MergeTwoSubtitlesBold1;
        Italic1 = s.MergeTwoSubtitlesItalic1;
        PrimaryColor1 = s.MergeTwoSubtitlesPrimaryColor1.FromHexToColor();
        OutlineColor1 = s.MergeTwoSubtitlesOutlineColor1.FromHexToColor();
        OutlineWidth1 = s.MergeTwoSubtitlesOutlineWidth1;
        ShadowWidth1 = s.MergeTwoSubtitlesShadowWidth1;
        AlignTop1 = s.MergeTwoSubtitlesAlignTop1;

        FontName2 = ResolveFontName(s.MergeTwoSubtitlesFontName2);
        FontSize2 = s.MergeTwoSubtitlesFontSize2;
        Bold2 = s.MergeTwoSubtitlesBold2;
        Italic2 = s.MergeTwoSubtitlesItalic2;
        PrimaryColor2 = s.MergeTwoSubtitlesPrimaryColor2.FromHexToColor();
        OutlineColor2 = s.MergeTwoSubtitlesOutlineColor2.FromHexToColor();
        OutlineWidth2 = s.MergeTwoSubtitlesOutlineWidth2;
        ShadowWidth2 = s.MergeTwoSubtitlesShadowWidth2;
        AlignTop2 = s.MergeTwoSubtitlesAlignTop2;

        _loaded = true;
    }

    private string ResolveFontName(string? name)
    {
        if (!string.IsNullOrEmpty(name) && FontNames.Contains(name))
        {
            return name;
        }

        return FontNames.FirstOrDefault() ?? "Arial";
    }

    public void Initialize(IList<SubtitleLineViewModel> currentLines, bool hasOriginal)
    {
        var sub1 = new Subtitle();
        foreach (var line in currentLines)
        {
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                continue;
            }

            sub1.Paragraphs.Add(new Paragraph(line.Text, line.StartTime.TotalMilliseconds, line.EndTime.TotalMilliseconds));
        }

        var sub2 = new Subtitle();
        if (hasOriginal)
        {
            foreach (var line in currentLines)
            {
                if (string.IsNullOrWhiteSpace(line.OriginalText))
                {
                    continue;
                }

                sub2.Paragraphs.Add(new Paragraph(line.OriginalText, line.StartTime.TotalMilliseconds, line.EndTime.TotalMilliseconds));
            }
        }

        SetSubtitle1(sub1, Se.Language.Tools.MergeTwoSubtitles.LoadFromCurrentText);
        SetSubtitle2(sub2, hasOriginal
            ? Se.Language.Tools.MergeTwoSubtitles.LoadFromCurrentTranslation
            : string.Empty);

        UpdateMergeEnabled();
        UpdatePreview();
    }

    private void SetSubtitle1(Subtitle subtitle, string display)
    {
        _subtitle1 = subtitle;
        File1Display = display;
        Items1.Clear();
        var index = 1;
        foreach (var p in subtitle.Paragraphs)
        {
            Items1.Add(new MergeTwoSubtitlesDisplayItem
            {
                Number = index++,
                StartTime = p.StartTime.TimeSpan,
                EndTime = p.EndTime.TimeSpan,
                Text = p.Text,
            });
        }
    }

    private void SetSubtitle2(Subtitle subtitle, string display)
    {
        _subtitle2 = subtitle;
        File2Display = display;
        Items2.Clear();
        var index = 1;
        foreach (var p in subtitle.Paragraphs)
        {
            Items2.Add(new MergeTwoSubtitlesDisplayItem
            {
                Number = index++,
                StartTime = p.StartTime.TimeSpan,
                EndTime = p.EndTime.TimeSpan,
                Text = p.Text,
            });
        }
    }

    [RelayCommand]
    private async Task LoadFile1()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = LoadSubtitleFile(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "Unable to read subtitle: " + fileName);
            return;
        }

        SetSubtitle1(subtitle, Path.GetFileName(fileName));
        UpdateMergeEnabled();
    }

    [RelayCommand]
    private async Task LoadFile2()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = LoadSubtitleFile(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "Unable to read subtitle: " + fileName);
            return;
        }

        SetSubtitle2(subtitle, Path.GetFileName(fileName));
        UpdateMergeEnabled();
    }

    private static Subtitle? LoadSubtitleFile(string fileName)
    {
        var sub = Subtitle.Parse(fileName);
        if (sub == null || sub.Paragraphs.Count == 0)
        {
            return null;
        }

        return sub;
    }

    private void UpdateMergeEnabled()
    {
        IsMergeEnabled = _subtitle1.Paragraphs.Count > 0 && _subtitle2.Paragraphs.Count > 0;
    }

    [RelayCommand]
    private void Ok()
    {
        if (_subtitle1.Paragraphs.Count == 0 || _subtitle2.Paragraphs.Count == 0)
        {
            return;
        }

        SaveSettings();
        BuildResult();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private void SaveSettings()
    {
        var s = Se.Settings.Tools;
        s.MergeTwoSubtitlesOutputFormat = SelectedOutputFormat.Id;
        s.MergeTwoSubtitlesFontName1 = FontName1;
        s.MergeTwoSubtitlesFontSize1 = FontSize1;
        s.MergeTwoSubtitlesBold1 = Bold1;
        s.MergeTwoSubtitlesItalic1 = Italic1;
        s.MergeTwoSubtitlesPrimaryColor1 = PrimaryColor1.FromColorToHex();
        s.MergeTwoSubtitlesOutlineColor1 = OutlineColor1.FromColorToHex();
        s.MergeTwoSubtitlesOutlineWidth1 = OutlineWidth1;
        s.MergeTwoSubtitlesShadowWidth1 = ShadowWidth1;
        s.MergeTwoSubtitlesAlignTop1 = AlignTop1;
        s.MergeTwoSubtitlesFontName2 = FontName2;
        s.MergeTwoSubtitlesFontSize2 = FontSize2;
        s.MergeTwoSubtitlesBold2 = Bold2;
        s.MergeTwoSubtitlesItalic2 = Italic2;
        s.MergeTwoSubtitlesPrimaryColor2 = PrimaryColor2.FromColorToHex();
        s.MergeTwoSubtitlesOutlineColor2 = OutlineColor2.FromColorToHex();
        s.MergeTwoSubtitlesOutlineWidth2 = OutlineWidth2;
        s.MergeTwoSubtitlesShadowWidth2 = ShadowWidth2;
        s.MergeTwoSubtitlesAlignTop2 = AlignTop2;
        Se.SaveSettings();
    }

    private void BuildResult()
    {
        if (SelectedOutputFormat.Id == OutputFormatSubRip)
        {
            ResultSubtitle = BuildSubRipMerge(_subtitle1, _subtitle2);
            ResultFormat = new SubRip();
        }
        else
        {
            ResultSubtitle = BuildAssaMerge(_subtitle1, _subtitle2);
            ResultFormat = new AdvancedSubStationAlpha();
        }

        ResultSubtitle.Renumber();
    }

    private static Subtitle BuildSubRipMerge(Subtitle sub1, Subtitle sub2)
    {
        var result = new Subtitle();
        var used2 = new HashSet<int>();

        for (var i1 = 0; i1 < sub1.Paragraphs.Count; i1++)
        {
            var p1 = sub1.Paragraphs[i1];
            var matchIndex = FindOverlapping(p1, sub2, used2);
            if (matchIndex >= 0)
            {
                used2.Add(matchIndex);
                var p2 = sub2.Paragraphs[matchIndex];
                var combined = new Paragraph(
                    p1.Text + Environment.NewLine + p2.Text,
                    p1.StartTime.TotalMilliseconds,
                    p1.EndTime.TotalMilliseconds);
                result.Paragraphs.Add(combined);
            }
            else
            {
                result.Paragraphs.Add(new Paragraph(p1));
            }
        }

        for (var i2 = 0; i2 < sub2.Paragraphs.Count; i2++)
        {
            if (used2.Contains(i2))
            {
                continue;
            }

            result.Paragraphs.Add(new Paragraph(sub2.Paragraphs[i2]));
        }

        result.Paragraphs.Sort((a, b) => a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));
        return result;
    }

    private static int FindOverlapping(Paragraph p, Subtitle other, HashSet<int> used)
    {
        for (var i = 0; i < other.Paragraphs.Count; i++)
        {
            if (used.Contains(i))
            {
                continue;
            }

            var q = other.Paragraphs[i];
            if (p.StartTime.TotalMilliseconds < q.EndTime.TotalMilliseconds &&
                q.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
            {
                return i;
            }
        }

        return -1;
    }

    private Subtitle BuildAssaMerge(Subtitle sub1, Subtitle sub2)
    {
        var style1 = BuildSsaStyle("Style1", FontName1, FontSize1, Bold1, Italic1,
            PrimaryColor1, OutlineColor1, OutlineWidth1, ShadowWidth1, AlignTop1);
        var style2 = BuildSsaStyle("Style2", FontName2, FontSize2, Bold2, Italic2,
            PrimaryColor2, OutlineColor2, OutlineWidth2, ShadowWidth2, AlignTop2);

        var header = AdvancedSubStationAlpha.DefaultHeader;
        header = AdvancedSubStationAlpha.UpdateOrAddStyle(header, style1);
        header = AdvancedSubStationAlpha.UpdateOrAddStyle(header, style2);
        // Remove the leftover "Default" style
        header = RemoveDefaultStyleFromHeader(header);

        var result = new Subtitle { Header = header };
        foreach (var p in sub1.Paragraphs)
        {
            // Different layer per track so the two streams don't collide/stack against each other when shown at the same time
            var copy = new Paragraph(p) { Extra = "Style1", Layer = 0 };
            result.Paragraphs.Add(copy);
        }

        foreach (var p in sub2.Paragraphs)
        {
            var copy = new Paragraph(p) { Extra = "Style2", Layer = 1 };
            result.Paragraphs.Add(copy);
        }

        result.Paragraphs.Sort((a, b) => a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));
        return result;
    }

    private static string RemoveDefaultStyleFromHeader(string header)
    {
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
        var keep = styles.Where(p => !string.Equals(p.Name, "Default", StringComparison.OrdinalIgnoreCase)).ToList();
        if (keep.Count == styles.Count || keep.Count == 0)
        {
            return header;
        }

        return AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, keep);
    }

    private static SsaStyle BuildSsaStyle(string name, string fontName, int fontSize, bool bold, bool italic,
        Color primary, Color outline, decimal outlineWidth, decimal shadowWidth, bool alignTop)
    {
        return new SsaStyle
        {
            Name = name,
            FontName = fontName,
            FontSize = fontSize,
            Bold = bold,
            Italic = italic,
            Primary = ToSkColor(primary),
            Secondary = SKColors.Yellow,
            Outline = ToSkColor(outline),
            Background = SKColors.Black,
            OutlineWidth = outlineWidth,
            ShadowWidth = shadowWidth,
            Alignment = alignTop ? "8" : "2",
            BorderStyle = "1",
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
            ScaleX = 100,
            ScaleY = 100,
            Spacing = 0,
            Angle = 0,
        };
    }

    private static SKColor ToSkColor(Color c) => new(c.R, c.G, c.B, c.A);

    internal void OnPropertyChangedFromUi()
    {
        if (!_loaded)
        {
            return;
        }

        IsAssaSelected = SelectedOutputFormat?.Id == OutputFormatAssa;
        UpdatePreview();
    }

    partial void OnSelectedOutputFormatChanged(OutputFormatItem value) => OnPropertyChangedFromUi();
    partial void OnSelectedItem1Changed(MergeTwoSubtitlesDisplayItem? value) => UpdatePreview();
    partial void OnSelectedItem2Changed(MergeTwoSubtitlesDisplayItem? value) => UpdatePreview();
    partial void OnFontName1Changed(string value) => UpdatePreview();
    partial void OnFontSize1Changed(int value) => UpdatePreview();
    partial void OnBold1Changed(bool value) => UpdatePreview();
    partial void OnItalic1Changed(bool value) => UpdatePreview();
    partial void OnPrimaryColor1Changed(Color value) => UpdatePreview();
    partial void OnOutlineColor1Changed(Color value) => UpdatePreview();
    partial void OnOutlineWidth1Changed(decimal value) => UpdatePreview();
    partial void OnShadowWidth1Changed(decimal value) => UpdatePreview();
    partial void OnAlignTop1Changed(bool value) => UpdatePreview();
    partial void OnFontName2Changed(string value) => UpdatePreview();
    partial void OnFontSize2Changed(int value) => UpdatePreview();
    partial void OnBold2Changed(bool value) => UpdatePreview();
    partial void OnItalic2Changed(bool value) => UpdatePreview();
    partial void OnPrimaryColor2Changed(Color value) => UpdatePreview();
    partial void OnOutlineColor2Changed(Color value) => UpdatePreview();
    partial void OnOutlineWidth2Changed(decimal value) => UpdatePreview();
    partial void OnShadowWidth2Changed(decimal value) => UpdatePreview();
    partial void OnAlignTop2Changed(bool value) => UpdatePreview();

    private void UpdatePreview()
    {
        if (!_loaded)
        {
            return;
        }

        const int width = 480;
        const int height = 200;

        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(bitmap))
        {
            DrawCheckerboard(canvas, width, height);

            var sample1 = SelectedItem1?.Text;
            if (string.IsNullOrEmpty(sample1))
            {
                sample1 = Items1.FirstOrDefault()?.Text ?? "Subtitle 1 example";
            }

            var sample2 = SelectedItem2?.Text;
            if (string.IsNullOrEmpty(sample2))
            {
                sample2 = Items2.FirstOrDefault()?.Text ?? "Subtitle 2 example";
            }

            DrawText(canvas, width, height, sample1!, FontName1, FontSize1, Bold1, Italic1,
                PrimaryColor1, OutlineColor1, OutlineWidth1, ShadowWidth1, AlignTop1);
            DrawText(canvas, width, height, sample2!, FontName2, FontSize2, Bold2, Italic2,
                PrimaryColor2, OutlineColor2, OutlineWidth2, ShadowWidth2, AlignTop2);
        }

        ImagePreview = bitmap.ToAvaloniaBitmap();
    }

    private static void DrawCheckerboard(SKCanvas canvas, int width, int height)
    {
        const int rectangleSize = 9;
        using var lightBrush = new SKPaint { Color = new SKColor(245, 245, 245), Style = SKPaintStyle.Fill };
        using var darkBrush = new SKPaint { Color = new SKColor(211, 211, 211), Style = SKPaintStyle.Fill };
        for (var y = 0; y < height; y += rectangleSize)
        {
            for (var x = 0; x < width; x += rectangleSize)
            {
                var darker = (y / rectangleSize + x / rectangleSize) % 2 == 0;
                canvas.DrawRect(new SKRect(x, y, x + rectangleSize, y + rectangleSize), darker ? darkBrush : lightBrush);
            }
        }
    }

    private static void DrawText(SKCanvas canvas, int width, int height, string text, string fontName, int fontSize,
        bool bold, bool italic, Color primary, Color outline, decimal outlineWidth, decimal shadowWidth,
        bool alignTop)
    {
        var lines = SplitPreviewLines(text);
        if (lines.Length == 0)
        {
            return;
        }

        var weight = bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        var slant = italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        using var typeface = SKTypeface.FromFamilyName(fontName, weight, SKFontStyleWidth.Normal, slant)
                             ?? SKTypeface.Default;
        using var font = new SKFont(typeface, fontSize);

        using var fillPaint = new SKPaint
        {
            Color = ToSkColor(primary),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        var metrics = font.Metrics;
        var lineHeight = metrics.Descent - metrics.Ascent;

        float blockTop;
        if (alignTop)
        {
            blockTop = -metrics.Ascent + 10;
        }
        else
        {
            blockTop = height - 10 - lineHeight * lines.Length - metrics.Ascent;
        }

        SKPaint? shadowPaint = null;
        SKPaint? outlinePaint = null;
        try
        {
            if (shadowWidth > 0)
            {
                shadowPaint = new SKPaint
                {
                    Color = new SKColor(0, 0, 0, 200),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                };
            }

            if (outlineWidth > 0)
            {
                outlinePaint = new SKPaint
                {
                    Color = ToSkColor(outline),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = (float)outlineWidth * 2f,
                    StrokeJoin = SKStrokeJoin.Round,
                };
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var textWidth = font.MeasureText(line);
                var x = (width - textWidth) / 2f;
                var y = blockTop + i * lineHeight;

                if (shadowPaint != null)
                {
                    for (var s = 1; s <= (int)shadowWidth; s++)
                    {
                        canvas.DrawText(line, x + s, y + s, font, shadowPaint);
                    }
                }

                if (outlinePaint != null)
                {
                    canvas.DrawText(line, x, y, font, outlinePaint);
                }

                canvas.DrawText(line, x, y, font, fillPaint);
            }
        }
        finally
        {
            shadowPaint?.Dispose();
            outlinePaint?.Dispose();
        }
    }

    private static string[] SplitPreviewLines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<string>();
        }

        var clean = HtmlUtil.RemoveHtmlTags(text, true)
            .Replace("\\N", "\n", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal);

        return clean.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}

public class OutputFormatItem
{
    public string Id { get; }
    public string Display { get; }

    public OutputFormatItem(string id, string display)
    {
        Id = id;
        Display = display;
    }

    public override string ToString() => Display;
}
