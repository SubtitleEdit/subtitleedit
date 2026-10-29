using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Ytt;

/// <summary>
/// Editor for the head of a YouTube timed text file: pens (text styles), window positions and
/// window styles. Lines point to them by id, so ids never change here - a new entry gets the next
/// free id and a deleted one just stops being written (its lines fall back to the player default).
/// </summary>
public partial class YttStylesViewModel : ObservableObject, IClosingCleanup
{
    public const int TabPens = 0;
    public const int TabPositions = 1;
    public const int TabWindowStyles = 2;

    public static string[] EdgeTypeNames => new[]
    {
        Se.Language.File.Ytt.EdgeNone,
        Se.Language.File.Ytt.EdgeHardShadow,
        Se.Language.File.Ytt.EdgeBevel,
        Se.Language.File.Ytt.EdgeGlow,
        Se.Language.File.Ytt.EdgeSoftShadow,
    };

    public static string[] OffsetNames => new[]
    {
        Se.Language.File.Ytt.Subscript,
        Se.Language.File.Ytt.Normal,
        Se.Language.File.Ytt.Superscript,
    };

    public static string[] FontStyleNames => new[] { Se.Language.File.Ytt.Default }.Concat(YttPen.FontStyleNames.Skip(1)).ToArray();

    public static string[] AnchorPointNames => new[]
    {
        Se.Language.General.TopLeft,
        Se.Language.General.TopCenter,
        Se.Language.General.TopRight,
        Se.Language.General.MiddleLeft,
        Se.Language.General.MiddleCenter,
        Se.Language.General.MiddleRight,
        Se.Language.General.BottomLeft,
        Se.Language.General.BottomCenter,
        Se.Language.General.BottomRight,
    };

    public static string[] JustifyNames => new[]
    {
        Se.Language.File.Ytt.Default,
        Se.Language.File.Ytt.Left,
        Se.Language.File.Ytt.Right,
        Se.Language.File.Ytt.Center,
    };

    public static string[] PrintDirectionNames => new[]
    {
        Se.Language.File.Ytt.Default,
        Se.Language.File.Ytt.LeftToRight,
        Se.Language.File.Ytt.RightToLeft,
        Se.Language.File.Ytt.VerticalUpright,
        Se.Language.File.Ytt.VerticalRotated,
    };

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<YttPenDisplay> _pens;
    [ObservableProperty] private ObservableCollection<YttPositionDisplay> _positions;
    [ObservableProperty] private ObservableCollection<YttWindowStyleDisplay> _windowStyles;
    [ObservableProperty] private YttPenDisplay? _selectedPen;
    [ObservableProperty] private YttPositionDisplay? _selectedPosition;
    [ObservableProperty] private YttWindowStyleDisplay? _selectedWindowStyle;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private Bitmap? _imagePreview;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The rewritten header, valid after OK.</summary>
    public string Header { get; private set; }

    private Subtitle _subtitle;
    private readonly System.Timers.Timer _timerUpdatePreview;
    private volatile bool _isClosing;
    private volatile bool _previewDirty = true;

    public YttStylesViewModel()
    {
        _title = string.Empty;
        _pens = new ObservableCollection<YttPenDisplay>();
        _positions = new ObservableCollection<YttPositionDisplay>();
        _windowStyles = new ObservableCollection<YttWindowStyleDisplay>();
        _subtitle = new Subtitle();
        Header = string.Empty;

        _timerUpdatePreview = new System.Timers.Timer(250);
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    public void Initialize(Subtitle subtitle, string fileName, int? selectedPenId, int tabIndex = TabPens)
    {
        Title = UiUtil.FormatTitleWithFileName(Se.Language.File.Ytt.StylesTitleX, fileName);
        _subtitle = subtitle;
        var head = YttHead.Parse(subtitle.Header);

        // id 0 is the placeholder written for YouTube's players - not something to edit
        foreach (var pen in head.Pens.Where(p => p.Id != 0).OrderBy(p => p.Id))
        {
            AddPen(new YttPenDisplay(pen));
        }

        foreach (var wp in head.WindowPositions.Where(p => p.Id != 0).OrderBy(p => p.Id))
        {
            AddPosition(new YttPositionDisplay(wp));
        }

        foreach (var ws in head.WindowStyles.Where(p => p.Id != 0).OrderBy(p => p.Id))
        {
            AddWindowStyle(new YttWindowStyleDisplay(ws));
        }

        UpdateUsages();
        SelectedPen = Pens.FirstOrDefault(p => p.Id == selectedPenId) ?? Pens.FirstOrDefault();
        SelectedPosition = Positions.FirstOrDefault();
        SelectedWindowStyle = WindowStyles.FirstOrDefault();
        SelectedTabIndex = tabIndex;
        _timerUpdatePreview.Start();
    }

    private void AddPen(YttPenDisplay pen)
    {
        pen.PropertyChanged += (_, _) => _previewDirty = true;
        Pens.Add(pen);
    }

    private void AddPosition(YttPositionDisplay position)
    {
        position.PropertyChanged += (_, _) => _previewDirty = true;
        Positions.Add(position);
    }

    private void AddWindowStyle(YttWindowStyleDisplay windowStyle)
    {
        windowStyle.PropertyChanged += (_, _) => _previewDirty = true;
        WindowStyles.Add(windowStyle);
    }

    partial void OnSelectedPenChanged(YttPenDisplay? value) => _previewDirty = true;
    partial void OnSelectedPositionChanged(YttPositionDisplay? value) => _previewDirty = true;
    partial void OnSelectedTabIndexChanged(int value) => _previewDirty = true;

    private void UpdateUsages()
    {
        foreach (var pen in Pens)
        {
            var id = pen.IdText;
            var spanRegex = new Regex("<s\\b[^>]*\\bp=\"" + id + "\"", RegexOptions.CultureInvariant);
            pen.UsageCount = _subtitle.Paragraphs.Count(p => p.Style == id || (p.Text.Contains("<s") && spanRegex.IsMatch(p.Text)));
        }

        foreach (var position in Positions)
        {
            position.UsageCount = _subtitle.Paragraphs.Count(p => p.Region == position.IdText);
        }

        foreach (var windowStyle in WindowStyles)
        {
            windowStyle.UsageCount = _subtitle.Paragraphs.Count(p => p.Effect == windowStyle.IdText);
        }
    }

    // Ids are never reused within the dialog: lines may still point to a deleted id until OK.
    private int _maxPenId;
    private int _maxPositionId;
    private int _maxWindowStyleId;

    private int NextPenId() => _maxPenId = Math.Max(_maxPenId, Pens.Select(p => p.Id).DefaultIfEmpty(0).Max()) + 1;
    private int NextPositionId() => _maxPositionId = Math.Max(_maxPositionId, Positions.Select(p => p.Id).DefaultIfEmpty(0).Max()) + 1;
    private int NextWindowStyleId() => _maxWindowStyleId = Math.Max(_maxWindowStyleId, WindowStyles.Select(p => p.Id).DefaultIfEmpty(0).Max()) + 1;

    [RelayCommand]
    private void New()
    {
        switch (SelectedTabIndex)
        {
            case TabPositions:
                var position = new YttPositionDisplay(new YttWindowPosition { Id = NextPositionId(), AnchorPoint = 7, Horizontal = 50, Vertical = 100 });
                AddPosition(position);
                SelectedPosition = position;
                break;
            case TabWindowStyles:
                var windowStyle = new YttWindowStyleDisplay(new YttWindowStyle { Id = NextWindowStyleId(), Justify = 2 });
                AddWindowStyle(windowStyle);
                SelectedWindowStyle = windowStyle;
                break;
            default:
                var pen = new YttPenDisplay(new YttPen { Id = NextPenId() });
                AddPen(pen);
                SelectedPen = pen;
                break;
        }
    }

    [RelayCommand]
    private void Duplicate()
    {
        switch (SelectedTabIndex)
        {
            case TabPositions when SelectedPosition != null:
                var position = SelectedPosition.CloneWithId(NextPositionId());
                AddPosition(position);
                SelectedPosition = position;
                break;
            case TabWindowStyles when SelectedWindowStyle != null:
                var windowStyle = SelectedWindowStyle.CloneWithId(NextWindowStyleId());
                AddWindowStyle(windowStyle);
                SelectedWindowStyle = windowStyle;
                break;
            case TabPens when SelectedPen != null:
                var pen = SelectedPen.CloneWithId(NextPenId());
                AddPen(pen);
                SelectedPen = pen;
                break;
        }
    }

    [RelayCommand]
    private void Delete()
    {
        switch (SelectedTabIndex)
        {
            case TabPositions when SelectedPosition != null:
                _maxPositionId = Math.Max(_maxPositionId, SelectedPosition.Id);
                SelectedPosition = RemoveAndSelectNext(Positions, SelectedPosition);
                break;
            case TabWindowStyles when SelectedWindowStyle != null:
                _maxWindowStyleId = Math.Max(_maxWindowStyleId, SelectedWindowStyle.Id);
                SelectedWindowStyle = RemoveAndSelectNext(WindowStyles, SelectedWindowStyle);
                break;
            case TabPens when SelectedPen != null:
                _maxPenId = Math.Max(_maxPenId, SelectedPen.Id);
                SelectedPen = RemoveAndSelectNext(Pens, SelectedPen);
                break;
        }
    }

    private static T? RemoveAndSelectNext<T>(ObservableCollection<T> list, T item) where T : class
    {
        var index = list.IndexOf(item);
        list.Remove(item);
        return list.Count == 0 ? null : list[Math.Min(index, list.Count - 1)];
    }

    [RelayCommand]
    private void Ok()
    {
        // keep the head's placeholders (id 0) as they were
        var old = YttHead.Parse(_subtitle.Header);
        var head = new YttHead();
        head.Pens.AddRange(old.Pens.Where(p => p.Id == 0));
        head.WindowPositions.AddRange(old.WindowPositions.Where(p => p.Id == 0));
        head.WindowStyles.AddRange(old.WindowStyles.Where(p => p.Id == 0));
        head.Pens.AddRange(Pens.Select(p => p.ToPen()));
        head.WindowPositions.AddRange(Positions.Select(p => p.ToWindowPosition()));
        head.WindowStyles.AddRange(WindowStyles.Select(p => p.ToWindowStyle()));
        Header = head.ToHeader();
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    private void TimerUpdatePreviewElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

        _timerUpdatePreview.Stop();
        if (_previewDirty)
        {
            _previewDirty = false;
            Dispatcher.UIThread.Post(UpdatePreview);
        }

        // OnClosingCleanup may have disposed the timer while this handler ran (#12739).
        if (!_isClosing)
        {
            _timerUpdatePreview.Start();
        }
    }

    private void UpdatePreview()
    {
        if (_isClosing)
        {
            return;
        }

        // The pen preview shows the selected pen, the other tabs the selected position with the
        // first pen in use.
        var pen = SelectedPen?.ToPen() ?? new YttPen();
        var position = SelectedTabIndex == TabPositions && SelectedPosition != null
            ? SelectedPosition.ToWindowPosition()
            : new YttWindowPosition();
        var text = SelectedTabIndex == TabPositions && SelectedPosition != null
            ? string.Format(Se.Language.File.Ytt.WindowPositionX, SelectedPosition.Id)
            : SelectedTabIndex == TabPens && SelectedPen != null
                ? string.Format(Se.Language.File.Ytt.PenX, SelectedPen.Id)
                : Se.Language.General.Preview;
        if (SelectedTabIndex != TabPens)
        {
            pen = new YttPen();
        }

        using var frame = RenderPreview(pen, position, text);
        ImagePreview = frame.ToAvaloniaBitmap();
    }

    /// <summary>
    /// A 16:9 frame at YouTube's 720p reference size (text is 32 px there) with the text drawn in
    /// the pen at the window position - both scaled the way the player scales them.
    /// </summary>
    internal static SKBitmap RenderPreview(YttPen pen, YttWindowPosition position, string text)
    {
        const int width = 1280;
        const int height = 720;
        var fontPixels = (float)(32 * YttPen.GetRealScale(pen.Size));
        if (pen.Offset is 0 or 2)
        {
            fontPixels /= 2;
        }

        var foreOpacity = (byte)Math.Clamp(pen.ForeOpacity ?? YttPen.DefaultForeOpacity, 0, 255);
        var fore = YttText.FromHtml(pen.ForeColor ?? YttPen.DefaultForeColor, SKColors.White).WithAlpha(foreOpacity);
        var back = YttText.FromHtml(pen.BackColor ?? YttPen.DefaultBackColor, SKColors.Black)
            .WithAlpha((byte)Math.Clamp(pen.BackOpacity ?? YttPen.DefaultBackOpacity, 0, 255));
        var edge = YttText.FromHtml(pen.EdgeColor ?? YttPen.DefaultEdgeColor, SKColors.Black)
            .WithAlpha(pen.EdgeColor == null ? foreOpacity : (byte)255);
        var outline = pen.EdgeType == 3 ? 2.5f : 0;
        var shadow = pen.EdgeType switch { 1 => 2.5f, 2 => 1f, 4 => 2f, _ => 0f };
        var fontName = FontHelper.GetSkiaFontNameFromLibAssaFontName(YttPen.GetFontName(pen.FontStyle));
        var displayText = pen.FontStyle == 7 ? text.ToUpperInvariant() : text;

        using var textBitmap = TextToImageGenerator.GenerateImageWithPadding(
            displayText,
            fontName,
            fontPixels,
            pen.Bold == true,
            fore,
            pen.EdgeType == 3 ? edge : SKColors.Transparent,
            shadow > 0 ? edge : SKColors.Transparent,
            back,
            outline,
            shadow,
            cornerRadius: 0,
            padding: (int)Math.Round(fontPixels / 4),
            isItalic: pen.Italic == true,
            isUnderline: pen.Underline == true);

        var frame = new SKBitmap(width, height);
        using var canvas = new SKCanvas(frame);
        canvas.Clear(new SKColor(40, 40, 40));
        using (var safeArea = new SKPaint { Color = new SKColor(80, 80, 80), Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new[] { 10f, 8f }, 0) })
        {
            // the 2%-98% area YouTube squeezes all positions into
            canvas.DrawRect(width * 0.02f, height * 0.02f, width * 0.96f, height * 0.96f, safeArea);
        }

        var anchorPoint = position.AnchorPoint is >= 0 and <= 8 ? position.AnchorPoint.Value : YttWindowPosition.DefaultAnchorPoint;
        var x = (float)(YttWindowPosition.GetRealPercent(position.Horizontal ?? YttWindowPosition.DefaultHorizontal) / 100 * width);
        var y = (float)(YttWindowPosition.GetRealPercent(position.Vertical ?? YttWindowPosition.DefaultVertical) / 100 * height);
        x -= (anchorPoint % 3) * textBitmap.Width / 2f;
        y -= (anchorPoint / 3) * textBitmap.Height / 2f;
        canvas.DrawBitmap(textBitmap, x, y);

        using (var marker = new SKPaint { Color = new SKColor(255, 80, 0), IsAntialias = true })
        {
            canvas.DrawCircle(
                (float)(YttWindowPosition.GetRealPercent(position.Horizontal ?? YttWindowPosition.DefaultHorizontal) / 100 * width),
                (float)(YttWindowPosition.GetRealPercent(position.Vertical ?? YttWindowPosition.DefaultVertical) / 100 * height),
                6,
                marker);
        }

        return frame;
    }

    public void OnClosingCleanup()
    {
        _isClosing = true;
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/ytt-styles");
        }
    }
}
