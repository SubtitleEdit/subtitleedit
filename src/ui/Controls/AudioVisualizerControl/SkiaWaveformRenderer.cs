using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>How the scroll-anchored scenery layer (background, grid, waveform) is cached.</summary>
internal enum SkiaWaveformCacheMode
{
    /// <summary>No cache - rebuild and redraw the scenery every frame.</summary>
    None,

    /// <summary>Record the scenery into an <see cref="SKPicture"/> and replay it translated.</summary>
    Picture,

    /// <summary>Rasterize the scenery into an offscreen surface and blit it translated.</summary>
    Image,
}

/// <summary>
/// Render-thread painter for <see cref="SkiaAudioVisualizer"/>: draws one
/// <see cref="SkiaWaveformFrame"/> onto the leased Skia canvas. Owns every buffer, paint and text
/// blob it uses and reuses them across frames, so a steady frame allocates nothing.
/// <para>
/// Coordinates are DIPs (the leased canvas is already scaled); anything that must look crisp is
/// snapped to device pixels via <see cref="_scale"/>. The waveform is built with one point per
/// device pixel column and drawn in a couple of calls - a filled envelope path for the classic
/// style, Gouraud-shaded triangle strips for the fancy style's gradient and glow - so its cost is
/// one tight loop over the peaks, independent of how loud the audio is.
/// </para>
/// </summary>
internal sealed class SkiaWaveformRenderer
{
    private const float ChapterFlagHeight = 15;
    private const float ChapterFlagMaxWidth = 170;
    private const float ChapterFlagPadding = 5;
    private const float TimeLineTextOffset = 14;
    private static readonly SKColor ChapterColor = new(0xC0, 0x8A, 0xDF);
    private static readonly SKColor GridColor = new(169, 169, 169, 64);
    private static readonly SKColor CenterLineColor = new(169, 169, 169, 140);
    private static readonly SKColor TimeLineTickColor = new(128, 128, 128);
    private static readonly SKColor ShotChangeStartColor = new(0, 100, 0, 175);
    private static readonly SKColor ShotChangeEndColor = new(110, 10, 10, 175);
    private static readonly SKColor AudioLengthFitsColor = new(70, 190, 110, 190);
    private static readonly SKColor AudioLengthOverrunColor = new(235, 70, 70, 220);
    private static readonly int[] FrameStepCandidates = { 1, 2, 5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 };

    private readonly Lock _lock = new();
    private float _scale = 1;

    private readonly SKPaint _fill = new() { Style = SKPaintStyle.Fill, IsAntialias = false };
    private readonly SKPaint _roundFill = new() { Style = SKPaintStyle.Fill, IsAntialias = true };
    private readonly SKPaint _hairline = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 0, IsAntialias = false };
    private readonly SKPaint _dash = new() { Style = SKPaintStyle.Stroke, IsAntialias = false };
    private readonly SKPaint _vertices = new() { Color = SKColors.White };
    private readonly SKPaint _image = new() { IsAntialias = false };
    private readonly SKPaint _text = new() { IsAntialias = true };
    private readonly SKPath _path = new();
    private readonly SKPath _wavePath = new();
    private readonly SKPathEffect _dashShotChange = SKPathEffect.CreateDash(new[] { 4f, 4f }, 0);
    private readonly SKPathEffect _dashCursor = SKPathEffect.CreateDash(new[] { 3f, 3f }, 1.5f);
    private readonly SkiaTextCache _textCache = new();

    // Triangle strips, one vertex pair per device pixel column: [top, middle] and [middle, bottom]
    // for the waveform, [outer, edge] for the glow. Sized to the column count and reused.
    private SKPoint[] _upperPoints = Array.Empty<SKPoint>();
    private SKPoint[] _lowerPoints = Array.Empty<SKPoint>();
    private SKPoint[] _upperGlowPoints = Array.Empty<SKPoint>();
    private SKPoint[] _lowerGlowPoints = Array.Empty<SKPoint>();
    private SKColor[] _upperColors = Array.Empty<SKColor>();
    private SKColor[] _lowerColors = Array.Empty<SKColor>();
    private SKColor[] _upperGlowColors = Array.Empty<SKColor>();
    private SKColor[] _lowerGlowColors = Array.Empty<SKColor>();

    // Scroll-anchored cache of everything that depends only on the view: the background, the grid
    // and the waveform. It is anchored to a 256 device pixel block and built one block wider than
    // the control, so scrolling inside the block - what center-mode playback does on every frame -
    // replays the cache translated instead of re-running the per-column peak scan and the fill.
    // Only LayerKey state rebuilds it; the cursor, the paragraphs and (in classic style) the
    // selection are drawn over it every frame.
    private const int CachePadDevicePixels = 256;

    internal static SkiaWaveformCacheMode CacheMode = SkiaWaveformCacheMode.Image;

    private readonly SkiaWaveformFrame _layerFrame = new();
    private SKSurface? _layerSurface;
    private SKImage? _layerImage;
    private SKPicture? _layerPicture;
    private LayerKey _layerKey;
    private object? _layerPeaks;
    private bool _layerValid;
    private bool _wavePathValid;
    private float _layerWidth;
    private float _layerHeight;
    private int _layerSurfaceWidth;
    private int _layerSurfaceHeight;
    private static readonly SKSamplingOptions NearestSampling = new(SKFilterMode.Nearest);
    private readonly SKPaint _layerPaint = new() { IsAntialias = false };

    private readonly HashSet<int> _paragraphStartXs = new();
    private readonly HashSet<int> _paragraphEndXs = new();

    public void Render(SKCanvas canvas, SkiaWaveformFrame f)
    {
        lock (_lock)
        {
            var scale = canvas.TotalMatrix.ScaleX;
            _scale = scale > 0 ? scale : 1;
            _textCache.SetFont(f.FontName, f.FontBold);

            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, f.Width, f.Height));
            try
            {
                DrawScenery(canvas, f);

                if (f.SampleRate > 0)
                {
                    DrawSpectrogram(canvas, f);
                    DrawTimeLine(canvas, f);
                    DrawParagraphs(canvas, f);
                    DrawOriginalCues(canvas, f);
                    DrawShotChanges(canvas, f);
                    DrawChapters(canvas, f);
                    DrawCursor(canvas, f);
                    DrawNewSelection(canvas, f);
                }

                if (f.HintText != null)
                {
                    var hint = _textCache.Get(f.HintText, 14);
                    DrawText(canvas, hint, (f.Width - hint.Width) / 2, (f.Height - hint.Height) / 2, new SKColor(220, 220, 220));
                }

                if (f.IsFocused)
                {
                    var inset = 0.5f / _scale;
                    _hairline.Color = f.SelectedColor;
                    canvas.DrawRect(new SKRect(inset, inset, f.Width - inset, f.Height - inset), _hairline);
                }
            }
            finally
            {
                canvas.Restore();
            }
        }
    }

    private float X(SkiaWaveformFrame f, double seconds) => (float)((seconds - f.StartSeconds) * f.PixelsPerSecond);

    private float Snap(double x) => (float)(Math.Round(x * _scale) / _scale);

    private void FillRect(SKCanvas canvas, float left, float top, float width, float height, SKColor color)
    {
        if (width <= 0 || height <= 0 || color.Alpha == 0)
        {
            return;
        }

        _fill.Color = color;
        canvas.DrawRect(SKRect.Create(left, top, width, height), _fill);
    }

    /// <summary>A crisp full-height vertical line of <paramref name="thickness"/> DIPs, at least one device pixel.</summary>
    private void VerticalLine(SKCanvas canvas, double x, float top, float bottom, float thickness, SKColor color)
    {
        var width = Math.Max(1 / _scale, Snap(thickness));
        FillRect(canvas, Snap(x - width / 2), top, width, bottom - top, color);
    }

    private void DrawText(SKCanvas canvas, SkiaTextCache.ShapedText text, float x, float top, SKColor color)
    {
        if (text.Blob == null)
        {
            return;
        }

        _text.Color = color;
        canvas.DrawText(text.Blob, x, top + text.Baseline, _text);
    }

    /// <summary>
    /// Background, grid and waveform, from the cache when it is still valid, followed by the
    /// classic style's selection overlay (which is never part of the cache, so dragging a selected
    /// line's times never rebuilds the waveform).
    /// </summary>
    private void DrawScenery(SKCanvas canvas, SkiaWaveformFrame f)
    {
        var devicePixelsPerSecond = f.PixelsPerSecond * _scale;
        var startColumn = f.StartSeconds * devicePixelsPerSecond;
        var anchorColumn = devicePixelsPerSecond > 0
            ? Math.Floor(startColumn / CachePadDevicePixels) * CachePadDevicePixels
            : 0;

        // Whole device pixels, so the blit lands on the same grid the scenery was built on.
        var offset = devicePixelsPerSecond > 0 ? (float)(Math.Floor(startColumn - anchorColumn) / _scale) : 0;

        PrepareLayerFrame(f, anchorColumn, devicePixelsPerSecond);

        canvas.Save();
        canvas.Translate(-offset, 0);
        if (CacheMode == SkiaWaveformCacheMode.None || !EnsureLayer(f, anchorColumn))
        {
            DrawSceneryInto(canvas, anchorColumn);
        }
        else if (CacheMode == SkiaWaveformCacheMode.Picture)
        {
            canvas.DrawPicture(_layerPicture);
        }
        else
        {
            // An opaque background means the blit owns every pixel it covers, so it can overwrite
            // instead of blending - the layer is the first thing drawn inside the control.
            _layerPaint.BlendMode = f.BackgroundColor.Alpha == 255 ? SKBlendMode.Src : SKBlendMode.SrcOver;
            canvas.DrawImage(_layerImage, new SKRect(0, 0, _layerImage!.Width, _layerImage.Height),
                new SKRect(0, 0, _layerWidth, _layerHeight), NearestSampling, _layerPaint);
        }

        canvas.Restore();

        if (f.DrawStyle == WaveformDrawStyle.Classic && f.Peaks != null && f.DisplayMode != WaveformDisplayMode.OnlySpectrogram)
        {
            DrawClassicSelection(canvas, f, offset);
        }
    }

    /// <summary>The same view one anchor block to the left and one block wider - what the cache holds.</summary>
    private void PrepareLayerFrame(SkiaWaveformFrame f, double anchorColumn, double devicePixelsPerSecond)
    {
        f.CopyViewStateTo(_layerFrame);
        _layerWidth = f.Width + CachePadDevicePixels / _scale;
        _layerHeight = f.Height;
        _layerFrame.Width = _layerWidth;
        _layerFrame.StartSeconds = devicePixelsPerSecond > 0 ? anchorColumn / devicePixelsPerSecond : f.StartSeconds;
    }

    private void DrawSceneryInto(SKCanvas canvas, double anchorColumn)
    {
        var f = _layerFrame;
        FillRect(canvas, 0, 0, f.Width, f.Height, f.BackgroundColor);
        DrawGridLines(canvas, f);
        if (f.SampleRate > 0)
        {
            DrawWaveform(canvas, f, anchorColumn);
        }
    }

    /// <summary>Returns false when the scenery has to be drawn straight onto the canvas instead.</summary>
    private bool EnsureLayer(SkiaWaveformFrame f, double anchorColumn)
    {
        var key = new LayerKey(
            _layerWidth, _layerHeight, f.WaveformHeight, _scale, anchorColumn, f.ZoomFactor,
            f.VerticalZoomFactor, f.SampleRate, f.HighestPeak, (int)f.DisplayMode, (int)f.DrawStyle,
            ColorKey(f.WaveformColor), ColorKey(f.FancyHighColor), ColorKey(f.SelectedColor),
            ColorKey(f.BackgroundColor), f.DrawGridLines, f.FrameMode, f.FrameRate, SelectionKey(f));

        var cached = CacheMode == SkiaWaveformCacheMode.Picture ? _layerPicture != null : _layerImage != null;
        if (_layerValid && cached && ReferenceEquals(_layerPeaks, f.Peaks) && _layerKey.Equals(key))
        {
            return true;
        }

        _layerKey = key;
        _layerPeaks = f.Peaks;
        _layerValid = false;

        if (CacheMode == SkiaWaveformCacheMode.Picture)
        {
            using var recorder = new SKPictureRecorder();
            var recording = recorder.BeginRecording(new SKRect(0, 0, _layerWidth, _layerHeight));
            DrawSceneryInto(recording, anchorColumn);
            _layerPicture?.Dispose();
            _layerPicture = recorder.EndRecording();
            _layerValid = _layerPicture != null;
            return _layerValid;
        }

        var width = Math.Max(1, (int)Math.Ceiling(_layerWidth * _scale));
        var height = Math.Max(1, (int)Math.Ceiling(_layerHeight * _scale));
        if (_layerSurface == null || _layerSurfaceWidth != width || _layerSurfaceHeight != height)
        {
            _layerSurface?.Dispose();
            _layerSurface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
            _layerSurfaceWidth = width;
            _layerSurfaceHeight = height;
        }

        if (_layerSurface == null)
        {
            return false;
        }

        // Before drawing: the snapshot shares the surface's pixels copy-on-write, so redrawing
        // while it is still alive clones the whole layer.
        _layerImage?.Dispose();
        _layerImage = null;

        var layerCanvas = _layerSurface.Canvas;
        layerCanvas.Clear(SKColors.Transparent);
        layerCanvas.Save();
        layerCanvas.Scale(_scale);
        DrawSceneryInto(layerCanvas, anchorColumn);
        layerCanvas.Restore();
        _layerImage = _layerSurface.Snapshot();
        _layerValid = _layerImage != null;
        return _layerValid;
    }

    private static uint ColorKey(SKColor c) => ((uint)c.Alpha << 24) | ((uint)c.Red << 16) | ((uint)c.Green << 8) | c.Blue;

    /// <summary>
    /// Classic style paints the selection over the cached waveform, so its cache is selection
    /// independent; fancy style bakes the selected color into the columns and has to rebuild.
    /// </summary>
    private static long SelectionKey(SkiaWaveformFrame f)
    {
        if (f.DrawStyle == WaveformDrawStyle.Classic)
        {
            return 0;
        }

        long hash = 17;
        foreach (var (start, end) in f.SelectedRanges)
        {
            hash = hash * 31 + BitConverter.DoubleToInt64Bits(start);
            hash = hash * 31 + BitConverter.DoubleToInt64Bits(end);
        }

        return hash;
    }

    private readonly record struct LayerKey(
        float Width, float Height, float WaveformHeight, float Scale, double AnchorColumn,
        double ZoomFactor, double VerticalZoomFactor, int SampleRate, int HighestPeak,
        int DisplayMode, int DrawStyle, uint WaveformColor, uint FancyHighColor, uint SelectedColor,
        uint BackgroundColor, bool DrawGridLines, bool FrameMode, double FrameRate, long SelectionHash);

    private void DrawGridLines(SKCanvas canvas, SkiaWaveformFrame f)
    {
        // A zero zoom would never advance the line loops below.
        var pps = f.PixelsPerSecond;
        if (!f.DrawGridLines || (f.SampleRate > 0 && pps <= 0))
        {
            return;
        }

        double stepPixels;
        if (f.SampleRate == 0)
        {
            stepPixels = 10;
            for (var x = 0d; x < f.Width; x += stepPixels)
            {
                AddVertical(x);
            }
        }
        else if (f.FrameMode && f.FrameRate >= 1)
        {
            var pixelsPerFrame = pps / f.FrameRate;
            var framesPerStep = PickFramesPerStep(pixelsPerFrame, 8);
            stepPixels = framesPerStep * pixelsPerFrame;
            var startFrame = Math.Max(0, (long)Math.Floor(f.StartSeconds * f.FrameRate + 1e-6));
            for (var frame = startFrame - startFrame % framesPerStep; ; frame += framesPerStep)
            {
                var x = X(f, frame / f.FrameRate);
                if (x >= f.Width)
                {
                    break;
                }

                AddVertical(x);
            }
        }
        else
        {
            var interval = f.ZoomFactor >= 0.4 ? 0.1 : 1.0;
            stepPixels = interval * pps;
            for (var i = (long)Math.Floor(f.StartSeconds / interval); ; i++)
            {
                var x = X(f, i * interval);
                if (x >= f.Width)
                {
                    break;
                }

                AddVertical(x);
            }
        }

        if (stepPixels >= 1)
        {
            for (var y = stepPixels; y < f.Height; y += stepPixels)
            {
                FillRect(canvas, 0, Snap(y), f.Width, 1 / _scale, GridColor);
            }
        }

        // One filled device-pixel rect per line: measured ~4x cheaper than the same lines as a
        // hairline path on the CPU rasterizer, and crisper (no anti-aliased half coverage).
        void AddVertical(double x) => FillRect(canvas, Snap(x), 0, 1 / _scale, f.Height, GridColor);
    }

    private static int PickFramesPerStep(double pixelsPerFrame, double minPixelGap)
    {
        foreach (var candidate in FrameStepCandidates)
        {
            if (pixelsPerFrame * candidate >= minPixelGap)
            {
                return candidate;
            }
        }

        return FrameStepCandidates[^1];
    }

    private void DrawWaveform(SKCanvas canvas, SkiaWaveformFrame f, double originColumn)
    {
        _wavePathValid = false;
        var peakData = f.Peaks;
        if (peakData == null || f.DisplayMode == WaveformDisplayMode.OnlySpectrogram)
        {
            return;
        }

        var peaks = peakData.AsSpan();
        var fancy = f.DrawStyle != WaveformDrawStyle.Classic;
        var columns = (int)Math.Ceiling(f.Width * _scale) + 1;
        EnsureStripCapacity(columns * 2, fancy);

        var halfHeight = f.WaveformHeight / 2;
        var yScale = f.VerticalZoomFactor / f.HighestPeak * halfHeight;
        var height = f.Height;

        // Sample on the absolute device pixel grid, so scrolling by a fraction of a pixel moves
        // the picture instead of re-sampling it (no shimmer in center mode). Zoomed out, a column
        // covers several peaks and takes their real min/max instead of interpolating one of them.
        var devicePixelsPerSecond = f.PixelsPerSecond * _scale;
        var columnOffset = (float)(f.StartSeconds * devicePixelsPerSecond - originColumn);
        var peaksPerColumn = f.SampleRate / devicePixelsPerSecond;
        var inverseScale = 1 / _scale;

        double highestPeak = f.HighestPeak;
        var lowThreshold = highestPeak * 0.3;
        var mediumThreshold = highestPeak * 0.6;
        var inverseMediumRange = 1 / (mediumThreshold - lowThreshold);
        var inverseHighRange = 1 / (highestPeak - mediumThreshold);
        var glowPixels = 3f;

        var waveformColor = f.WaveformColor;
        var selectedColor = f.SelectedColor;
        var highColor = f.FancyHighColor;

        var ranges = f.SelectedRanges;
        var rangeIndex = 0;
        var drawn = 0;
        var anyGlow = false;

        for (var column = 0; column < columns; column++)
        {
            var peakPosition = (originColumn + column) * peaksPerColumn;
            var i0 = (int)peakPosition;
            if (i0 < 0 || i0 + 1 >= peaks.Length)
            {
                break;
            }

            double max;
            double min;
            if (peaksPerColumn <= 1)
            {
                var weight1 = peakPosition - i0;
                var weight0 = 1 - weight1;
                max = peaks[i0].Max * weight0 + peaks[i0 + 1].Max * weight1;
                min = peaks[i0].Min * weight0 + peaks[i0 + 1].Min * weight1;
            }
            else
            {
                var i1 = Math.Min(peaks.Length, Math.Max(i0 + 1, (int)(peakPosition + peaksPerColumn)));
                int localMax = short.MinValue;
                int localMin = short.MaxValue;
                for (var i = i0; i < i1; i++)
                {
                    var peak = peaks[i];
                    if (peak.Max > localMax)
                    {
                        localMax = peak.Max;
                    }

                    if (peak.Min < localMin)
                    {
                        localMin = peak.Min;
                    }
                }

                max = localMax;
                min = localMin;
            }

            var top = (float)Math.Clamp(halfHeight - max * yScale, 0, height);
            var bottom = (float)Math.Clamp(halfHeight - min * yScale, 0, height);
            if (bottom < top)
            {
                (top, bottom) = (bottom, top);
            }

            if (bottom - top < 1)
            {
                bottom = top + 1;
            }

            var x = (column - columnOffset) * inverseScale;
            var v = column * 2;

            if (!fancy)
            {
                // One solid colour for the whole envelope; the selected colour is painted over it
                // afterwards, clipped to the selected ranges, exactly as the Avalonia renderer
                // re-strokes its cached geometry.
                _upperPoints[v] = new SKPoint(x, top);
                _upperPoints[v + 1] = new SKPoint(x, bottom);
                drawn = column + 1;
                continue;
            }

            var seconds = (originColumn + column) / devicePixelsPerSecond;
            while (rangeIndex < ranges.Count && ranges[rangeIndex].End < seconds)
            {
                rangeIndex++;
            }

            var isSelected = rangeIndex < ranges.Count && ranges[rangeIndex].Start <= seconds;

            var amplitude = Math.Max(Math.Abs(max), Math.Abs(min));
            var baseColor = isSelected ? selectedColor : waveformColor;
            SKColor full;
            byte glowAlpha = 0;
            if (amplitude < lowThreshold)
            {
                full = baseColor;
            }
            else if (amplitude < mediumThreshold)
            {
                full = Lerp(baseColor, highColor, (float)((amplitude - lowThreshold) * inverseMediumRange));
            }
            else
            {
                var t = (float)Math.Min(1, (amplitude - mediumThreshold) * inverseHighRange);
                full = highColor.WithAlpha((byte)(highColor.Alpha + t * (255 - highColor.Alpha)));
                glowAlpha = (byte)(90 * t);
                anyGlow |= glowAlpha > 0;
            }

            var edge = full.WithAlpha((byte)(full.Alpha * 0.3f));
            var middle = Math.Clamp(halfHeight, top, bottom);
            _upperPoints[v] = new SKPoint(x, top);
            _upperPoints[v + 1] = new SKPoint(x, middle);
            _upperColors[v] = edge;
            _upperColors[v + 1] = full;
            _lowerPoints[v] = new SKPoint(x, middle);
            _lowerPoints[v + 1] = new SKPoint(x, bottom);
            _lowerColors[v] = full;
            _lowerColors[v + 1] = edge;

            var glow = full.WithAlpha(glowAlpha);
            var clear = full.WithAlpha(0);
            _upperGlowPoints[v] = new SKPoint(x, top - glowPixels);
            _upperGlowPoints[v + 1] = new SKPoint(x, top);
            _upperGlowColors[v] = clear;
            _upperGlowColors[v + 1] = glow;
            _lowerGlowPoints[v] = new SKPoint(x, bottom);
            _lowerGlowPoints[v + 1] = new SKPoint(x, bottom + glowPixels);
            _lowerGlowColors[v] = glow;
            _lowerGlowColors[v + 1] = clear;
            drawn = column + 1;
        }

        if (drawn == 0)
        {
            return;
        }

        // The strips keep a fixed length (so the arrays are reused); columns past the end of the
        // audio collapse onto the last real vertex and draw nothing.
        if (!fancy)
        {
            // A filled envelope polygon: measured ~2.5x faster than the same columns as a
            // Gouraud-shaded triangle strip on the CPU rasterizer, and with one column per device
            // pixel the two are visually identical.
            BuildEnvelopePath(drawn);
            _wavePathValid = true;
            _fill.Color = waveformColor;
            canvas.DrawPath(_wavePath, _fill);
            return;
        }

        CollapseTail(_upperPoints, drawn * 2);
        CollapseTail(_lowerPoints, drawn * 2);
        HorizontalLine(canvas, halfHeight, f.Width, CenterLineColor);
        if (anyGlow)
        {
            CollapseTail(_upperGlowPoints, drawn * 2);
            CollapseTail(_lowerGlowPoints, drawn * 2);
            canvas.DrawVertices(SKVertexMode.TriangleStrip, _upperGlowPoints, _upperGlowColors, _vertices);
            canvas.DrawVertices(SKVertexMode.TriangleStrip, _lowerGlowPoints, _lowerGlowColors, _vertices);
        }

        canvas.DrawVertices(SKVertexMode.TriangleStrip, _upperPoints, _upperColors, _vertices);
        canvas.DrawVertices(SKVertexMode.TriangleStrip, _lowerPoints, _lowerColors, _vertices);
    }

    /// <summary>Top edge left to right, bottom edge back again - the filled waveform envelope.</summary>
    private void BuildEnvelopePath(int columns)
    {
        _wavePath.Rewind();
        _wavePath.MoveTo(_upperPoints[0]);
        for (var i = 1; i < columns; i++)
        {
            _wavePath.LineTo(_upperPoints[i * 2]);
        }

        for (var i = columns - 1; i >= 0; i--)
        {
            _wavePath.LineTo(_upperPoints[i * 2 + 1]);
        }

        _wavePath.Close();
    }

    /// <summary>
    /// Classic style's selected colour: re-fill the envelope clipped to each selected range.
    /// Overlapping ranges are merged first so a shared column is painted exactly once.
    /// </summary>
    private void DrawClassicSelection(SKCanvas canvas, SkiaWaveformFrame f, float offset)
    {
        var ranges = f.SelectedRanges;
        if (ranges.Count == 0 || !_wavePathValid)
        {
            return;
        }

        _fill.Color = SourceOver(f.SelectedColor, f.WaveformColor);
        float mergedLeft = 0;
        float mergedRight = 0;
        var hasMerged = false;
        for (var i = 0; i < ranges.Count; i++)
        {
            var left = Math.Max(0, X(f, ranges[i].Start));
            var right = Math.Min(f.Width, X(f, ranges[i].End));
            if (right <= left)
            {
                continue;
            }

            if (hasMerged && left <= mergedRight)
            {
                mergedRight = Math.Max(mergedRight, right);
                continue;
            }

            if (hasMerged)
            {
                FillEnvelopeClipped(canvas, mergedLeft, mergedRight, f.Height, offset);
            }

            mergedLeft = left;
            mergedRight = right;
            hasMerged = true;
        }

        if (hasMerged)
        {
            FillEnvelopeClipped(canvas, mergedLeft, mergedRight, f.Height, offset);
        }
    }

    /// <summary>The clip is in view coordinates; the envelope path lives in the cache's anchor space.</summary>
    private void FillEnvelopeClipped(SKCanvas canvas, float left, float right, float height, float offset)
    {
        canvas.Save();
        canvas.ClipRect(new SKRect(left, 0, right, height));
        canvas.Translate(-offset, 0);
        canvas.DrawPath(_wavePath, _fill);
        canvas.Restore();
    }

    private void HorizontalLine(SKCanvas canvas, float y, float width, SKColor color) =>
        FillRect(canvas, 0, Snap(y), width, 1 / _scale, color);

    private void EnsureStripCapacity(int vertexCount, bool fancy)
    {
        if (_upperPoints.Length != vertexCount)
        {
            _upperPoints = new SKPoint[vertexCount];
            _upperColors = new SKColor[vertexCount];
        }

        if (fancy && _lowerPoints.Length != vertexCount)
        {
            _lowerPoints = new SKPoint[vertexCount];
            _lowerColors = new SKColor[vertexCount];
            _upperGlowPoints = new SKPoint[vertexCount];
            _upperGlowColors = new SKColor[vertexCount];
            _lowerGlowPoints = new SKPoint[vertexCount];
            _lowerGlowColors = new SKColor[vertexCount];
        }
    }

    private static void CollapseTail(SKPoint[] points, int used)
    {
        var last = points[used - 1];
        for (var i = used; i < points.Length; i++)
        {
            points[i] = last;
        }
    }

    private static SKColor Lerp(SKColor a, SKColor b, float t) => new(
        (byte)(a.Red + t * (b.Red - a.Red)),
        (byte)(a.Green + t * (b.Green - a.Green)),
        (byte)(a.Blue + t * (b.Blue - a.Blue)),
        (byte)(a.Alpha + t * (b.Alpha - a.Alpha)));

    /// <summary>What the classic renderer's selected pen stroked over the waveform pen looks like.</summary>
    private static SKColor SourceOver(SKColor source, SKColor destination)
    {
        var sa = source.Alpha / 255f;
        var da = destination.Alpha / 255f;
        var outAlpha = sa + da * (1 - sa);
        if (outAlpha <= 0)
        {
            return SKColors.Transparent;
        }

        byte Channel(byte s, byte d) => (byte)Math.Clamp((s * sa + d * da * (1 - sa)) / outAlpha, 0, 255);
        return new SKColor(
            Channel(source.Red, destination.Red),
            Channel(source.Green, destination.Green),
            Channel(source.Blue, destination.Blue),
            (byte)(outAlpha * 255));
    }

    private void DrawSpectrogram(SKCanvas canvas, SkiaWaveformFrame f)
    {
        if (f.SpectrogramImages.Count == 0)
        {
            return;
        }

        var top = f.DisplayMode == WaveformDisplayMode.WaveformAndSpectrogram ? f.WaveformHeight : 0;
        var secondsPerImage = f.SpectrogramSampleDuration * f.SpectrogramImageWidth;
        var sampling = new SKSamplingOptions(SKFilterMode.Linear);
        foreach (var (index, image) in f.SpectrogramImages)
        {
            var left = X(f, index * secondsPerImage);
            var right = X(f, index * secondsPerImage + image.Width * f.SpectrogramSampleDuration);
            canvas.DrawImage(image, new SKRect(0, 0, image.Width, image.Height), new SKRect(left, top, right, f.Height), sampling, _image);
        }
    }

    private void DrawTimeLine(SKCanvas canvas, SkiaWaveformFrame f)
    {
        var pps = f.PixelsPerSecond;
        var height = f.Height;
        var labelSize = Math.Max(1, f.FontSize - 1);
        foreach (var label in f.TimeLabels)
        {
            var x = X(f, label.Seconds);
            if (x < -200 || x > f.Width)
            {
                continue;
            }

            VerticalLine(canvas, x, height - 10, height, 1, TimeLineTickColor);
            var text = _textCache.Get(label.Text, labelSize);
            DrawText(canvas, text, x + 2, Math.Max(0, height - text.Height - 2), f.TextColor);
        }

        if (pps > 64)
        {
            for (var second = Math.Floor(f.StartSeconds); second <= f.EndSeconds; second++)
            {
                VerticalLine(canvas, X(f, second + 0.5), height - 5, height, 1, TimeLineTickColor);
            }
        }
    }

    private void DrawParagraphs(SKCanvas canvas, SkiaWaveformFrame f)
    {
        foreach (var p in f.Paragraphs)
        {
            var left = X(f, p.StartSeconds);
            var right = X(f, p.EndSeconds);
            var width = right - left;
            if (width <= 5)
            {
                continue;
            }

            FillRect(canvas, left, 0, width, f.Height, p.IsSelected ? f.ParagraphSelectedBackgroundColor : f.ParagraphBackgroundColor);
            VerticalLine(canvas, left + 0.5, 0, f.Height, 1, f.ParagraphLeftColor);
            VerticalLine(canvas, right - 0.5, 0, f.Height, 1, f.ParagraphRightColor);

            canvas.Save();
            canvas.ClipRect(SKRect.Create(left + 1, 0, width - 3, f.WorkingTextHeight));
            DrawParagraphText(canvas, f, p, left + 3, TimeLineTextOffset, 255);
            canvas.Restore();

            canvas.Save();
            canvas.ClipRect(SKRect.Create(left + 1, 0, width - 3, f.Height));
            DrawParagraphFooter(canvas, f, p, left, width);
            canvas.Restore();

            DrawAudioLength(canvas, f, p, left, right);
        }
    }

    private void DrawParagraphText(SKCanvas canvas, SkiaWaveformFrame f, SkiaParagraph p, float x, float y, byte alpha)
    {
        var color = f.TextColor.WithAlpha((byte)(f.TextColor.Alpha * alpha / 255));
        if (f.UnwrapText)
        {
            DrawText(canvas, _textCache.Get(p.Unwrapped, f.FontSize, p.RightToLeft), x, y, color);
            return;
        }

        foreach (var line in p.Lines)
        {
            var text = _textCache.Get(line, f.FontSize, p.RightToLeft);
            DrawText(canvas, text, x, y, color);
            y += text.Height;
            if (y > f.Height)
            {
                break;
            }
        }
    }

    private void DrawParagraphFooter(SKCanvas canvas, SkiaWaveformFrame f, SkiaParagraph p, float left, float width)
    {
        const float padding = 3;
        SkiaTextCache.ShapedText? baseLine = null;
        if (p.NumberAndDurationLabel != null)
        {
            var withDuration = _textCache.Get(p.NumberAndDurationLabel, f.FontSize);
            baseLine = withDuration.Width >= width - padding - 1 && p.NumberLabel != null
                ? _textCache.Get(p.NumberLabel, f.FontSize)
                : withDuration;
        }
        else if (p.NumberLabel != null)
        {
            baseLine = _textCache.Get(p.NumberLabel, f.FontSize);
        }

        var y = f.Height - TimeLineTextOffset;
        var x = left + padding;
        if (baseLine != null)
        {
            y -= baseLine.Height;
            DrawText(canvas, baseLine, x, y, f.TextColor);
        }

        if (p.CpsLabel != null)
        {
            var cps = _textCache.Get(p.CpsLabel, f.FontSize);
            DrawText(canvas, cps, x, y - cps.Height, f.TextColor);
        }
    }

    private void DrawAudioLength(SKCanvas canvas, SkiaWaveformFrame f, SkiaParagraph p, float left, float right)
    {
        if (p.AudioSeconds <= 0)
        {
            return;
        }

        const float barHeight = 4;
        var audioRight = X(f, p.StartSeconds + p.AudioSeconds);
        var y = f.WorkingTextHeight - barHeight - 1;
        var fitsRight = Math.Min(audioRight, right - 1);
        if (fitsRight > left + 1)
        {
            FillRect(canvas, left + 1, y, fitsRight - left - 1, barHeight, AudioLengthFitsColor);
        }

        if (audioRight > right)
        {
            FillRect(canvas, right - 1, y, audioRight - right + 1, barHeight, AudioLengthOverrunColor);
        }
    }

    private void DrawOriginalCues(SKCanvas canvas, SkiaWaveformFrame f)
    {
        if (f.OriginalCues.Count == 0)
        {
            return;
        }

        var top = f.Height / 2;
        var height = f.Height - top;
        static SKColor Half(SKColor c) => c.WithAlpha((byte)(c.Alpha / 2));
        foreach (var cue in f.OriginalCues)
        {
            var left = X(f, cue.StartSeconds);
            var right = X(f, cue.EndSeconds);
            if (right - left <= 5)
            {
                continue;
            }

            FillRect(canvas, left, top, right - left, height, Half(f.ParagraphBackgroundColor));
            VerticalLine(canvas, left + 0.5, top, f.Height, 1, Half(f.ParagraphLeftColor));
            VerticalLine(canvas, right - 0.5, top, f.Height, 1, Half(f.ParagraphRightColor));
            canvas.Save();
            canvas.ClipRect(SKRect.Create(left + 1, top, right - left - 3, height));
            DrawParagraphText(canvas, f, cue, left + 3, top + TimeLineTextOffset, 128);
            canvas.Restore();
        }
    }

    private void DrawShotChanges(SKCanvas canvas, SkiaWaveformFrame f)
    {
        if (f.ShotChanges.Count == 0)
        {
            return;
        }

        // Same "shot change sits on a paragraph edge / the cursor" highlighting as the classic
        // renderer, compared on whole DIP positions like it does.
        _paragraphStartXs.Clear();
        _paragraphEndXs.Clear();
        foreach (var p in f.Paragraphs)
        {
            _paragraphStartXs.Add((int)Math.Round(X(f, p.StartSeconds)));
            _paragraphEndXs.Add((int)Math.Round(X(f, p.EndSeconds)));
        }

        var cursorX = f.CursorSeconds >= 0 ? (int)Math.Round(X(f, f.CursorSeconds)) : int.MinValue;
        foreach (var seconds in f.ShotChanges)
        {
            var x = X(f, seconds);
            var rounded = (int)Math.Round(x);
            if (rounded <= 0 || x >= f.Width)
            {
                continue;
            }

            if (rounded == cursorX)
            {
                VerticalLine(canvas, rounded, 0, f.Height, 2, f.ShotChangeColor);
                VerticalLine(canvas, rounded, 0, f.Height, 1, f.CursorColor);
            }
            else if (_paragraphStartXs.Contains(rounded) || _paragraphEndXs.Contains(rounded))
            {
                VerticalLine(canvas, rounded, 0, f.Height, 2, f.ShotChangeColor);
                _dash.StrokeWidth = 2;
                _dash.PathEffect = _dashShotChange;
                _dash.Color = _paragraphStartXs.Contains(rounded) ? ShotChangeStartColor : ShotChangeEndColor;
                canvas.DrawLine(Snap(rounded), 0, Snap(rounded), f.Height, _dash);
            }
            else
            {
                VerticalLine(canvas, rounded, 0, f.Height, 1, f.ShotChangeColor);
            }
        }
    }

    private void DrawChapters(SKCanvas canvas, SkiaWaveformFrame f)
    {
        foreach (var chapter in f.Chapters)
        {
            var x = X(f, chapter.Seconds);
            if (x >= 0)
            {
                VerticalLine(canvas, x, 0, f.Height, 1.5f, ChapterColor.WithAlpha(217));
            }

            if (string.IsNullOrEmpty(chapter.Title))
            {
                continue;
            }

            var available = chapter.NextSeconds == double.MaxValue ? float.MaxValue : X(f, chapter.NextSeconds) - x;
            if (available < 12)
            {
                continue;
            }

            var text = _textCache.Get(chapter.Title, 10);
            var flag = SKRect.Create(x, 0, Math.Min(Math.Min(ChapterFlagMaxWidth, text.Width + ChapterFlagPadding * 2), available), ChapterFlagHeight);
            _roundFill.Color = ChapterColor.WithAlpha(230);
            canvas.DrawRoundRect(flag, 3, 3, _roundFill);
            canvas.Save();
            canvas.ClipRect(flag);
            DrawText(canvas, text, x + ChapterFlagPadding, (ChapterFlagHeight - text.Height) / 2, SKColors.Black);
            canvas.Restore();
        }
    }

    private void DrawCursor(SKCanvas canvas, SkiaWaveformFrame f)
    {
        if (f.CursorSeconds < 0)
        {
            return;
        }

        var x = X(f, f.CursorSeconds);
        if (x < 0 || x >= f.Width)
        {
            return;
        }

        if (!f.CursorOnShotChange)
        {
            VerticalLine(canvas, Math.Max(x, 0.5f), 0, f.Height, 1, f.CursorColor);
            return;
        }

        var snapped = Snap(Math.Max(x, 0.75f));
        _dash.StrokeWidth = 1.5f;
        _dash.PathEffect = _dashCursor;
        _dash.Color = SKColors.LightCyan;
        canvas.DrawLine(snapped, 0, snapped, f.Height, _dash);
    }

    private void DrawNewSelection(SKCanvas canvas, SkiaWaveformFrame f)
    {
        if (!f.HasNewSelection)
        {
            return;
        }

        var left = X(f, f.NewSelectionStartSeconds);
        var right = X(f, f.NewSelectionEndSeconds);
        var width = right - left;
        if (right < 0 || left > f.Width)
        {
            return;
        }

        FillRect(canvas, left, 0, width, f.Height, f.ParagraphBackgroundColor);
        if (f.NewSelectionLabel == null || width <= 5)
        {
            return;
        }

        var text = _textCache.Get(f.NewSelectionLabel, f.FontSize);
        if (text.Width >= width - 4)
        {
            return;
        }

        DrawText(canvas, text, left + (width - text.Width) / 2, f.Height - TimeLineTextOffset - text.Height, f.TextColor);
    }
}

/// <summary>
/// Shaped, positioned text blobs for the render thread, keyed by string and size. Shaping goes
/// through HarfBuzz (<see cref="SKShaper"/>), so ligatures and right to left scripts come out in
/// visual order; a string the UI font cannot show falls back to a system font that has its first
/// missing character. (Mixed-direction bidi reordering is not done.)
/// </summary>
internal sealed class SkiaTextCache
{
    internal sealed class ShapedText
    {
        public SKTextBlob? Blob;
        public float Width;
        public float Baseline;
        public float Height;
    }

    private readonly Dictionary<(string Text, float Size, bool RightToLeft), ShapedText> _texts = new(1024);
    private readonly Dictionary<(SKTypeface Typeface, float Size), SKFont> _fonts = new();
    private readonly Dictionary<SKTypeface, SKShaper> _shapers = new();
    private readonly Dictionary<int, SKTypeface> _fallbacks = new();
    private SKTypeface _typeface = SKTypeface.Default;
    private string? _familyName;
    private bool _bold;

    public void SetFont(string familyName, bool bold)
    {
        if (familyName == _familyName && bold == _bold)
        {
            return;
        }

        Clear();
        _familyName = familyName;
        _bold = bold;
        _typeface = SKTypeface.FromFamilyName(familyName,
            bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright) ?? SKTypeface.Default;
    }

    /// <summary>
    /// The shaped glyphs for one line of text. <paramref name="rightToLeft"/> is the paragraph
    /// direction: HarfBuzz shapes a buffer in one direction and does no bidi reordering, so a
    /// right to left line with Latin words or digits is split into directional runs first (see
    /// <see cref="SkiaBidiRuns"/>) and each run is shaped with an explicit direction. A left to
    /// right line without right to left letters keeps the single shaping pass.
    /// </summary>
    public ShapedText Get(string text, float size, bool rightToLeft = false)
    {
        if (_texts.TryGetValue((text, size, rightToLeft), out var shaped))
        {
            return shaped;
        }

        if (_texts.Count > 4000)
        {
            foreach (var old in _texts.Values)
            {
                old.Blob?.Dispose();
            }

            _texts.Clear();
        }

        var font = GetFont(PickTypeface(text), size);
        font.GetFontMetrics(out var metrics);
        shaped = new ShapedText
        {
            Baseline = -metrics.Ascent,
            Height = metrics.Descent - metrics.Ascent + metrics.Leading,
        };

        if (!string.IsNullOrWhiteSpace(text))
        {
            if (!rightToLeft && !SkiaBidiRuns.HasStrongRightToLeft(text))
            {
                ShapeSingleRun(text, font, shaped);
            }
            else
            {
                ShapeBidiRuns(text, size, rightToLeft, shaped);
            }
        }

        _texts[(text, size, rightToLeft)] = shaped;
        return shaped;
    }

    private void ShapeSingleRun(string text, SKFont font, ShapedText shaped)
    {
        var result = GetShaper(font.Typeface).Shape(text, font);
        var count = result.Codepoints.Length;
        if (count == 0)
        {
            return;
        }

        using var builder = new SKTextBlobBuilder();
        AppendRun(builder, font, result);
        shaped.Blob = builder.Build();
        shaped.Width = result.Width;
    }

    private void ShapeBidiRuns(string text, float size, bool rightToLeft, ShapedText shaped)
    {
        var runs = SkiaBidiRuns.Split(text, rightToLeft);
        using var builder = new SKTextBlobBuilder();
        var x = 0f;
        var any = false;
        foreach (var run in runs)
        {
            var font = GetFont(PickTypeface(run.Text), size);
            using var buffer = new HarfBuzzSharp.Buffer();
            buffer.AddUtf16(run.Text);
            buffer.Direction = run.RightToLeft ? HarfBuzzSharp.Direction.RightToLeft : HarfBuzzSharp.Direction.LeftToRight;
            buffer.GuessSegmentProperties();
            var result = GetShaper(font.Typeface).Shape(buffer, x, 0, font);
            if (result.Codepoints.Length > 0)
            {
                AppendRun(builder, font, result);
                any = true;
            }

            x += result.Width;
        }

        if (any)
        {
            shaped.Blob = builder.Build();
        }

        shaped.Width = x;
    }

    private static void AppendRun(SKTextBlobBuilder builder, SKFont font, SKShaper.Result result)
    {
        var count = result.Codepoints.Length;
        var run = builder.AllocatePositionedRun(font, count);
        var glyphs = run.Glyphs;
        var positions = run.Positions;
        for (var i = 0; i < count; i++)
        {
            glyphs[i] = (ushort)result.Codepoints[i];
            positions[i] = result.Points[i];
        }
    }

    private SKShaper GetShaper(SKTypeface typeface)
    {
        if (!_shapers.TryGetValue(typeface, out var shaper))
        {
            shaper = new SKShaper(typeface);
            _shapers[typeface] = shaper;
        }

        return shaper;
    }

    private SKTypeface PickTypeface(string text)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            if (rune.Value < 0x80 || System.Text.Rune.IsWhiteSpace(rune) || System.Text.Rune.IsControl(rune) || _typeface.ContainsGlyph(rune.Value))
            {
                continue;
            }

            if (!_fallbacks.TryGetValue(rune.Value, out var fallback))
            {
                fallback = SKFontManager.Default.MatchCharacter(_familyName ?? string.Empty,
                    _bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, null, rune.Value) ?? _typeface;
                _fallbacks[rune.Value] = fallback;
            }

            return fallback;
        }

        return _typeface;
    }

    private SKFont GetFont(SKTypeface typeface, float size)
    {
        if (!_fonts.TryGetValue((typeface, size), out var font))
        {
            font = new SKFont(typeface, size) { Subpixel = true, Edging = SKFontEdging.Antialias };
            _fonts[(typeface, size)] = font;
        }

        return font;
    }

    private void Clear()
    {
        foreach (var text in _texts.Values)
        {
            text.Blob?.Dispose();
        }

        _texts.Clear();
        _fonts.Clear();
        _shapers.Clear();
        _fallbacks.Clear();
    }
}
