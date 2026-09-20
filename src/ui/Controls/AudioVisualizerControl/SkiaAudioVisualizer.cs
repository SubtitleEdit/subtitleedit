using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>
/// Experimental <see cref="AudioVisualizer"/> that paints with SkiaSharp directly instead of
/// recording Avalonia geometry and text.
/// <para>
/// Everything except drawing is inherited - hit testing, dragging, snapping, events - so it is a
/// drop-in replacement. <see cref="Render"/> only copies the view state into a
/// <see cref="SkiaWaveformFrame"/> on the UI thread (no per-pixel work, no geometry, no text
/// shaping) and hands it to a custom draw operation; <see cref="SkiaWaveformRenderer"/> then
/// builds and rasterizes the whole scene on the render thread.
/// </para>
/// <para>
/// Differences from the classic renderer: the waveform is sampled per device pixel (sharper on
/// high DPI screens) and takes the real min/max of all peaks under a column when zoomed out
/// instead of interpolating one sample; the fancy style is a gradient filled envelope with a
/// continuous amplitude color and glow; the spectrogram is drawn straight from its tiles with
/// sub-pixel scrolling instead of being copied into a new bitmap every frame.
/// </para>
/// <para>
/// The background, grid and waveform are cached together in an offscreen layer anchored to a 256
/// device pixel block, so scrolling inside that block is a blit (see SkiaWaveformRenderer). The
/// cursor, the paragraphs and the classic style's selection are drawn over it on every frame.
/// </para>
/// Turned on by the waveform setting "Use experimental fast renderer"
/// (<see cref="SeWaveform.UseSkiaRenderer"/>); the control to build is chosen in InitWaveform.
/// </summary>
public class SkiaAudioVisualizer : AudioVisualizer
{
    public static bool UseSkiaRenderer => Se.Settings.Waveform.UseSkiaRenderer;

    private readonly SkiaWaveformRenderer _renderer = new();
    private readonly ConcurrentQueue<SkiaWaveformFrame> _framePool = new();

    private readonly Dictionary<int, string> _numberLabels = new(512);

    // The selected paragraphs inside the view, probed once per visible paragraph - a set, because
    // with "select all" on a large subtitle a list lookup is millions of compares per frame.
    private readonly HashSet<SubtitleLineViewModel> _selectedInView = new();
    private readonly Dictionary<long, string> _timeLabels = new(256);
    private double _timeLabelsVideoOffsetMs = double.NaN;

    private string? _textColorSource;
    private SKColor _textColor = SKColors.White;
    private string? _fontNameSource;
    private string _fontName = string.Empty;

    private SpectrogramData2? _spectrogramSource;
    private readonly Dictionary<int, SKImage> _spectrogramImages = new();

    public override void Render(DrawingContext context)
    {
        var drawOperation = CreateDrawOperation();
        if (drawOperation != null)
        {
            context.Custom(drawOperation);
        }

        RaiseRendered();
    }

    /// <summary>
    /// The UI-thread half of a frame: snapshots the view into a draw operation. Disposing the
    /// operation (the compositor does it when the next frame replaces it) returns its frame to the pool.
    /// </summary>
    internal SkiaWaveformDrawOperation? CreateDrawOperation()
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0)
        {
            return null;
        }

        if (!_framePool.TryDequeue(out var frame))
        {
            frame = new SkiaWaveformFrame();
        }

        FillFrame(frame, width, height);
        return new SkiaWaveformDrawOperation(new Rect(0, 0, width, height), frame, _renderer, _framePool);
    }

    private void FillFrame(SkiaWaveformFrame f, double width, double height)
    {
        var peaks = WavePeaks;
        var settings = Se.Settings;

        f.Width = (float)width;
        f.Height = (float)height;
        f.Peaks = peaks;
        f.SampleRate = peaks?.SampleRate ?? 0;
        f.HighestPeak = Math.Max(1, peaks?.HighestPeak ?? 1);
        f.StartSeconds = StartPositionSeconds;
        f.ZoomFactor = ZoomFactor;
        f.VerticalZoomFactor = VerticalZoomFactor;
        f.EndSeconds = f.PixelsPerSecond > 0 ? f.StartSeconds + width / f.PixelsPerSecond : f.StartSeconds;
        f.DisplayMode = GetDisplayMode();
        f.DrawStyle = WaveformDrawStyle;
        f.WaveformHeight = f.DisplayMode == WaveformDisplayMode.WaveformAndSpectrogram
            ? (float)(height * WaveformHeightPercentage / 100.0)
            : f.Height;

        // The color properties have no default; the dialogs that never set them get the same
        // fallbacks the classic renderer's initial pens use.
        f.BackgroundColor = ToSkColor(WaveformBackgroundColor, new SKColor(70, 70, 70, 90));
        f.WaveformColor = ToSkColor(WaveformColor, new SKColor(144, 238, 144, 150));
        f.SelectedColor = ToSkColor(WaveformSelectedColor, new SKColor(254, 10, 10, 210));
        f.FancyHighColor = ToSkColor(WaveformFancyHighColor, SKColors.Orange);
        f.CursorColor = ToSkColor(WaveformCursorColor, SKColors.Cyan);
        f.ShotChangeColor = ToSkColor(WaveformShotChangeColor, SKColors.AntiqueWhite);
        f.ParagraphLeftColor = ToSkColor(WaveformParagraphLeftColor, new SKColor(0, 255, 0, 60));
        f.ParagraphRightColor = ToSkColor(WaveformParagraphRightColor, new SKColor(255, 0, 0, 100));
        f.ParagraphBackgroundColor = ToSkColor(ParagraphBackground, SKColors.Transparent);
        f.ParagraphSelectedBackgroundColor = ToSkColor(ParagraphSelectedBackground, SKColors.Transparent);
        f.TextColor = GetTextColor(settings.Waveform.WaveformTextColor);

        f.FontName = GetFontName(settings.Appearance.FontName);
        f.FontSize = (float)settings.Waveform.WaveformTextFontSize;
        f.FontBold = settings.Waveform.WaveformTextFontBold;
        f.UnwrapText = settings.Waveform.WaveformUnwrapText;

        f.DrawGridLines = DrawGridLines;
        f.FrameMode = settings.General.UseFrameMode;
        f.FrameRate = settings.General.CurrentFrameRate;

        f.IsFocused = IsFocused;
        f.HintText = peaks == null && ShowClickToGenerateHint && !string.IsNullOrEmpty(ClickToGenerateText)
            ? ClickToGenerateText
            : null;

        if (f.SampleRate == 0)
        {
            return;
        }

        AddTimeLabels(f, settings.General.CurrentVideoOffsetInMs);
        AddSelectedRanges(f);
        AddParagraphs(f, settings.Waveform.WaveformShowNumberAndDuration, settings.Waveform.WaveformShowCps);
        AddOriginalSubtitleCues(f);
        AddShotChanges(f);
        AddChapters(f);
        AddSpectrogram(f);

        f.CursorSeconds = CurrentVideoPositionSeconds;
        f.CursorOnShotChange = f.CursorSeconds >= 0 && GetShotChangeIndex(f.CursorSeconds) >= 0;

        var newSelection = NewSelectionParagraph;
        if (newSelection != null)
        {
            f.HasNewSelection = true;
            f.NewSelectionStartSeconds = newSelection.StartTime.TotalSeconds;
            f.NewSelectionEndSeconds = newSelection.EndTime.TotalSeconds;
            var durationMs = (newSelection.EndTime - newSelection.StartTime).TotalMilliseconds;
            f.NewSelectionLabel = durationMs >= 10 ? new TimeCode(durationMs).ToShortDisplayString() : null;
        }
    }

    private void AddTimeLabels(SkiaWaveformFrame f, double videoOffsetMs)
    {
        // Labels are formatted here (GetDisplayTime reads settings) and cached per whole second,
        // so a playing waveform does not allocate a string per label per frame.
        if (!videoOffsetMs.Equals(_timeLabelsVideoOffsetMs) || _timeLabels.Count > 8000)
        {
            _timeLabels.Clear();
            _timeLabelsVideoOffsetMs = videoOffsetMs;
        }

        var labelEverySecond = f.PixelsPerSecond > 38;
        var first = (long)Math.Floor(f.StartSeconds) - 1;
        var last = (long)Math.Ceiling(f.EndSeconds) + 1;
        for (var second = Math.Max(0, first); second <= last; second++)
        {
            if (!labelEverySecond && second % 5 != 0)
            {
                continue;
            }

            if (!_timeLabels.TryGetValue(second, out var label))
            {
                label = GetDisplayTime(second);
                _timeLabels[second] = label;
            }

            f.TimeLabels.Add(new SkiaTimeLabel(second, label));
        }
    }

    private void AddSelectedRanges(SkiaWaveformFrame f)
    {
        _selectedInView.Clear();
        var selection = AllSelectedParagraphs;
        if (selection == null)
        {
            return;
        }

        for (var i = 0; i < selection.Count; i++)
        {
            var p = selection[i];
            var start = p.StartTime.TotalSeconds;
            var end = p.EndTime.TotalSeconds;
            if (end >= f.StartSeconds && start <= f.EndSeconds && end > start)
            {
                f.SelectedRanges.Add((start, end));
                _selectedInView.Add(p);
            }
        }

        f.SelectedRanges.Sort(static (a, b) => a.Start.CompareTo(b.Start));
    }

    private void AddParagraphs(SkiaWaveformFrame f, bool showNumberAndDuration, bool showCps)
    {
        var paragraphs = DisplayableParagraphs;
        var showOriginal = ShowOriginalTextInWaveform;
        var audioLengthProvider = ParagraphAudioLengthProvider;
        var n = f.PixelsPerSecond;

        f.WorkingTextHeight = IsOriginalSubtitleOverlayVisible ? f.Height / 2 : f.Height;

        for (var i = 0; i < paragraphs.Count; i++)
        {
            var p = paragraphs[i];
            var start = p.StartTime.TotalSeconds;
            var end = p.EndTime.TotalSeconds;
            if (end < f.StartSeconds || start > f.EndSeconds)
            {
                continue;
            }

            var prepared = GetPreparedParagraphText(!ShowParagraphText ? string.Empty : showOriginal ? p.OriginalText : p.Text);
            var item = new SkiaParagraph
            {
                StartSeconds = start,
                EndSeconds = end,
                IsSelected = _selectedInView.Contains(p),
                Lines = prepared.Lines,
                Unwrapped = prepared.Unwrapped,
                RightToLeft = prepared.RightToLeft,
                AudioSeconds = audioLengthProvider?.Invoke(p) ?? 0,
            };

            if (showNumberAndDuration && n > 15)
            {
                item.NumberLabel = GetNumberLabel(p.Number);
                item.NumberAndDurationLabel = n > 51 ? GetCachedNumberAndDurationLabel(p) : null;
            }

            if (showCps && n > 99 && p.Duration.TotalMilliseconds > 0)
            {
                item.CpsLabel = GetCachedCpsLabel(showOriginal ? p.OriginalCharactersPerSecond : p.CharactersPerSecond);
            }

            f.Paragraphs.Add(item);
        }
    }

    private void AddOriginalSubtitleCues(SkiaWaveformFrame f)
    {
        if (!IsOriginalSubtitleOverlayVisible)
        {
            return;
        }

        var cues = OriginalSubtitleCues;
        var startIndex = FindFirstIndexAfterTime(OriginalSubtitleCueMaxEnds, f.StartSeconds, static maxEnd => maxEnd);
        var lastStart = -1d;
        var count = 0;
        for (var i = startIndex; i < cues.Count && count < 250; i++)
        {
            var cue = cues[i];
            if (cue.StartSeconds > f.EndSeconds)
            {
                break;
            }

            if (cue.EndSeconds < f.StartSeconds ||
                (count > 200 && (cue.EndSeconds - cue.StartSeconds < 0.00001 || cue.StartSeconds - lastStart < 0.09)))
            {
                continue;
            }

            lastStart = cue.StartSeconds;
            count++;
            var prepared = GetPreparedParagraphText(cue.Text);
            f.OriginalCues.Add(new SkiaParagraph
            {
                StartSeconds = cue.StartSeconds,
                EndSeconds = cue.EndSeconds,
                Lines = prepared.Lines,
                Unwrapped = prepared.Unwrapped,
                RightToLeft = prepared.RightToLeft,
            });
        }
    }

    private void AddShotChanges(SkiaWaveformFrame f)
    {
        var shotChanges = ShotChanges;
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return;
        }

        var low = 0;
        var high = shotChanges.Count;
        while (low < high)
        {
            var mid = low + (high - low) / 2;
            if (shotChanges[mid] < f.StartSeconds)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        for (var i = low; i < shotChanges.Count && shotChanges[i] <= f.EndSeconds; i++)
        {
            f.ShotChanges.Add(shotChanges[i]);
        }
    }

    private void AddChapters(SkiaWaveformFrame f)
    {
        var chapters = Chapters;
        var flagSeconds = 170 / f.PixelsPerSecond; // a chapter left of the view can own a visible flag
        for (var i = 0; i < chapters.Count; i++)
        {
            var chapter = chapters[i];
            if (chapter.Seconds > f.EndSeconds)
            {
                break;
            }

            if (chapter.Seconds + flagSeconds < f.StartSeconds)
            {
                continue;
            }

            var next = i + 1 < chapters.Count ? chapters[i + 1].Seconds : double.MaxValue;
            f.Chapters.Add(new SkiaChapter(chapter.Seconds, next, chapter.Title));
        }
    }

    private void AddSpectrogram(SkiaWaveformFrame f)
    {
        var spectrogram = GetSpectrogram();
        if (f.DisplayMode == WaveformDisplayMode.OnlyWaveform || spectrogram?.Images == null ||
            spectrogram.Images.Count == 0 || spectrogram.SampleDuration <= 0 || spectrogram.ImageWidth <= 0)
        {
            return;
        }

        // The tiles are SKBitmaps the control disposes on the UI thread when the spectrogram is
        // replaced, so the render thread gets immutable SKImage copies - made lazily for the tiles
        // actually on screen, and dropped (never disposed under a frame still using them) when
        // the spectrogram changes.
        if (!ReferenceEquals(spectrogram, _spectrogramSource) || _spectrogramImages.Count > 32)
        {
            _spectrogramImages.Clear();
            _spectrogramSource = spectrogram;
        }

        var secondsPerImage = spectrogram.SampleDuration * spectrogram.ImageWidth;
        var first = Math.Max(0, (int)Math.Floor(f.StartSeconds / secondsPerImage));
        var last = Math.Min(spectrogram.Images.Count - 1, (int)Math.Floor(f.EndSeconds / secondsPerImage));
        if (last - first > 64)
        {
            return;
        }

        for (var index = first; index <= last; index++)
        {
            if (!_spectrogramImages.TryGetValue(index, out var image))
            {
                var bitmap = spectrogram.Images[index];
                if (bitmap == null)
                {
                    continue;
                }

                image = SKImage.FromBitmap(bitmap);
                _spectrogramImages[index] = image;
            }

            f.SpectrogramImages.Add((index, image));
        }

        f.SpectrogramSampleDuration = spectrogram.SampleDuration;
        f.SpectrogramImageWidth = spectrogram.ImageWidth;
    }

    private string GetNumberLabel(int number)
    {
        if (!_numberLabels.TryGetValue(number, out var label))
        {
            if (_numberLabels.Count > 8000)
            {
                _numberLabels.Clear();
            }

            label = "#" + number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _numberLabels[number] = label;
        }

        return label;
    }

    private SKColor GetTextColor(string hex)
    {
        if (!ReferenceEquals(hex, _textColorSource))
        {
            _textColorSource = hex;
            _textColor = ToSkColor(hex.FromHexToColor(), SKColors.White);
        }

        return _textColor;
    }

    private string GetFontName(string appearanceFontName)
    {
        // UiUtil.GetDefaultFontName scans the system fonts when no UI font is set - not per frame.
        if (_fontName.Length == 0 || !ReferenceEquals(appearanceFontName, _fontNameSource))
        {
            _fontNameSource = appearanceFontName;
            _fontName = UiUtil.GetDefaultFontName();
        }

        return _fontName;
    }

    private static SKColor ToSkColor(Color color, SKColor fallback) =>
        color == default ? fallback : new SKColor(color.R, color.G, color.B, color.A);
}

/// <summary>
/// Everything one waveform frame draws, copied on the UI thread so the render thread never touches
/// view models, settings or Avalonia properties. Pooled: a frame goes back to its control's pool
/// when the draw operation that owns it is disposed.
/// </summary>
internal sealed class SkiaWaveformFrame
{
    public float Width;
    public float Height;
    public WavePeakData2? Peaks;
    public int SampleRate;
    public int HighestPeak;
    public double StartSeconds;
    public double EndSeconds;
    public double ZoomFactor;
    public double VerticalZoomFactor;
    public double PixelsPerSecond => SampleRate * ZoomFactor;
    public WaveformDisplayMode DisplayMode;
    public WaveformDrawStyle DrawStyle;
    public float WaveformHeight;
    public float WorkingTextHeight;

    public SKColor BackgroundColor;
    public SKColor WaveformColor;
    public SKColor SelectedColor;
    public SKColor FancyHighColor;
    public SKColor CursorColor;
    public SKColor ShotChangeColor;
    public SKColor ParagraphLeftColor;
    public SKColor ParagraphRightColor;
    public SKColor ParagraphBackgroundColor;
    public SKColor ParagraphSelectedBackgroundColor;
    public SKColor TextColor;

    public string FontName = string.Empty;
    public float FontSize;
    public bool FontBold;
    public bool UnwrapText;

    public bool DrawGridLines;
    public bool FrameMode;
    public double FrameRate;
    public bool IsFocused;
    public string? HintText;

    public double CursorSeconds = -1;
    public bool CursorOnShotChange;

    public bool HasNewSelection;
    public double NewSelectionStartSeconds;
    public double NewSelectionEndSeconds;
    public string? NewSelectionLabel;

    public double SpectrogramSampleDuration;
    public int SpectrogramImageWidth;

    public readonly List<SkiaTimeLabel> TimeLabels = new(128);
    public readonly List<(double Start, double End)> SelectedRanges = new(16);
    public readonly List<SkiaParagraph> Paragraphs = new(64);
    public readonly List<SkiaParagraph> OriginalCues = new();
    public readonly List<double> ShotChanges = new(32);
    public readonly List<SkiaChapter> Chapters = new();
    public readonly List<(int Index, SKImage Image)> SpectrogramImages = new(4);

    /// <summary>Copies what the cached scenery layer draws: background, grid lines and the waveform.</summary>
    public void CopyViewStateTo(SkiaWaveformFrame target)
    {
        target.Width = Width;
        target.Height = Height;
        target.Peaks = Peaks;
        target.SampleRate = SampleRate;
        target.HighestPeak = HighestPeak;
        target.StartSeconds = StartSeconds;
        target.EndSeconds = EndSeconds;
        target.ZoomFactor = ZoomFactor;
        target.VerticalZoomFactor = VerticalZoomFactor;
        target.DisplayMode = DisplayMode;
        target.DrawStyle = DrawStyle;
        target.WaveformHeight = WaveformHeight;
        target.BackgroundColor = BackgroundColor;
        target.WaveformColor = WaveformColor;
        target.SelectedColor = SelectedColor;
        target.FancyHighColor = FancyHighColor;
        target.DrawGridLines = DrawGridLines;
        target.FrameMode = FrameMode;
        target.FrameRate = FrameRate;
        target.SelectedRanges.Clear();
        target.SelectedRanges.AddRange(SelectedRanges);
    }

    public void Clear()
    {
        Peaks = null;
        SampleRate = 0;
        HintText = null;
        CursorSeconds = -1;
        CursorOnShotChange = false;
        HasNewSelection = false;
        NewSelectionLabel = null;
        TimeLabels.Clear();
        SelectedRanges.Clear();
        Paragraphs.Clear();
        OriginalCues.Clear();
        ShotChanges.Clear();
        Chapters.Clear();
        SpectrogramImages.Clear();
    }
}

internal struct SkiaParagraph
{
    public double StartSeconds;
    public double EndSeconds;
    public bool IsSelected;
    public IReadOnlyList<string> Lines;
    public string Unwrapped;
    public bool RightToLeft;
    public string? NumberLabel;
    public string? NumberAndDurationLabel;
    public string? CpsLabel;
    public double AudioSeconds;
}

internal readonly record struct SkiaTimeLabel(double Seconds, string Text);

internal readonly record struct SkiaChapter(double Seconds, double NextSeconds, string? Title);

internal sealed class SkiaWaveformDrawOperation : ICustomDrawOperation
{
    private readonly SkiaWaveformRenderer _renderer;
    private readonly ConcurrentQueue<SkiaWaveformFrame> _pool;
    private SkiaWaveformFrame? _frame;

    public SkiaWaveformDrawOperation(Rect bounds, SkiaWaveformFrame frame, SkiaWaveformRenderer renderer, ConcurrentQueue<SkiaWaveformFrame> pool)
    {
        Bounds = bounds;
        _frame = frame;
        _renderer = renderer;
        _pool = pool;
    }

    public Rect Bounds { get; }

    public bool HitTest(Point p) => Bounds.Contains(p);

    public bool Equals(ICustomDrawOperation? other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null || _frame == null)
        {
            return;
        }

        using var lease = leaseFeature.Lease();
        RenderTo(lease.SkCanvas);
    }

    internal void RenderTo(SKCanvas canvas)
    {
        var frame = _frame;
        if (frame == null)
        {
            return;
        }

        lock (frame)
        {
            // Disposed (and possibly handed back out) while waiting for the lock.
            if (ReferenceEquals(_frame, frame))
            {
                _renderer.Render(canvas, frame);
            }
        }
    }

    public void Dispose()
    {
        var frame = Interlocked.Exchange(ref _frame, null);
        if (frame == null)
        {
            return;
        }

        lock (frame)
        {
            frame.Clear();
        }

        if (_pool.Count < 4)
        {
            _pool.Enqueue(frame);
        }
    }
}
