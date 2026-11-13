using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>
/// Builds the inpainting masks for burned-in subtitles from the Video OCR result: one mask per
/// OCR line, taken from the scan area of every frame in the line's (padded) time span.
/// <para>
/// A pixel is subtitle text when it is at least <c>brightnessMinimum</c> bright in at least half
/// of the span's frames - the text stands still while the picture moves, so moving bright
/// background drops out - and, optionally, has a pixel within a few px that is dark on average
/// over the span (the text outline). The mask is that text grown by <c>grow</c> px, which also
/// covers the outline and anti-aliasing.
/// </para>
/// <para>
/// OCR timings are only as exact as its sample rate, so spans are padded. Per frame,
/// <see cref="GetFrameMask"/> applies a line's mask only when most of its text pixels are bright
/// (at least 20% of them) in that very frame; frames inside the padding without the text stay untouched (and serve as
/// clean background for the temporal fill).
/// </para>
/// </summary>
public sealed class BurnedInMaskBuilder
{
    public const int DarkOutlineMaximum = 80;
    private const double StableFraction = 0.5;
    // Lenient on purpose: a frame where the text is fading in or out must still be masked,
    // or the temporal fill copies its faint text as "clean background" into other frames.
    private const double PresentFraction = 0.2;

    private readonly int _videoWidth;
    private readonly int _videoHeight;
    private readonly int _rx;
    private readonly int _ry;
    private readonly int _rw;
    private readonly int _rh;
    private readonly int _brightnessMinimum;
    private readonly bool _requireOutline;
    private readonly int _outlineRadius;
    private readonly int _outlineSteps;
    private readonly int _grow;
    private readonly int _padFrames;
    private readonly int _applyMargin;
    private readonly int _extraWidth;
    private readonly List<LineMask> _lines = new();

    private sealed class LineMask
    {
        public long StartFrame;
        public long EndFrame;
        public int CoreFrames; // frames of the OCR line itself, without the padding
        public long CoreOffset; // first core frame - StartFrame
        public int Frames;
        public ushort[]? BrightCount;
        public uint[]? LumaSum;

        // Final result, in scan-area coordinates: a sample of the text pixels (presence check,
        // indexes into the scan area) and the grown mask as row runs (y, x, length) - compact
        // enough to keep for every line of a film.
        public int[] TextPixels = Array.Empty<int>();
        public int[] MaskRuns = Array.Empty<int>();
        public bool Done;
    }

    /// <param name="spans">Frame spans (inclusive) of the OCR lines, already padded.</param>
    public BurnedInMaskBuilder(int videoWidth, int videoHeight, int regionX, int regionY, int regionWidth, int regionHeight,
        IEnumerable<(long StartFrame, long EndFrame)> spans, int brightnessMinimum, bool requireOutline, int grow, int padFrames = 0,
        int applyMarginFrames = 0, int extraWidth = 0)
    {
        _videoWidth = videoWidth;
        _videoHeight = videoHeight;
        // Even position and size: ffmpeg's crop rounds odd values down for 4:2:0 video, and the
        // decoded frames must have exactly the size the builder reads (an odd scan area height
        // otherwise shifted every frame after the first and garbled all masks).
        _rx = Math.Clamp(regionX, 0, videoWidth - 2) & ~1;
        _ry = Math.Clamp(regionY, 0, videoHeight - 2) & ~1;
        _rw = Math.Max(2, Math.Clamp(regionWidth, 2, videoWidth - _rx) & ~1);
        _rh = Math.Max(2, Math.Clamp(regionHeight, 2, videoHeight - _ry) & ~1);
        _brightnessMinimum = brightnessMinimum > 0 ? brightnessMinimum : 190;
        _requireOutline = requireOutline;
        _outlineRadius = Math.Max(2, (int)Math.Round(videoHeight / 270.0));
        _outlineSteps = Math.Max(3, (int)Math.Round(videoHeight / 60.0));
        _grow = Math.Max(0, grow);
        _padFrames = Math.Max(0, padFrames);
        _applyMargin = Math.Max(0, applyMarginFrames);
        _extraWidth = Math.Max(0, extraWidth);
        foreach (var (start, end) in spans)
        {
            if (end < start)
            {
                continue;
            }

            var first = Math.Max(0, start);
            var coreFirst = Math.Max(first, Math.Min(end, start + _padFrames));
            var coreLast = Math.Max(coreFirst, end - _padFrames);
            _lines.Add(new LineMask
            {
                StartFrame = first,
                EndFrame = end,
                CoreOffset = coreFirst - first,
                CoreFrames = (int)(coreLast - coreFirst + 1),
            });
        }

        _lines.Sort((a, b) => a.StartFrame.CompareTo(b.StartFrame));
    }

    public int RegionX => _rx;
    public int RegionY => _ry;
    public int RegionWidth => _rw;
    public int RegionHeight => _rh;

    /// <summary>Default mask growth for a video height: covers a typical 2-4 px outline at 1080p.</summary>
    public static int DefaultGrow(int videoHeight) => Math.Max(3, (int)Math.Round(videoHeight / 180.0));

    /// <summary>True when the frame is inside any line span (the caller only needs to feed those).</summary>
    public bool NeedsFrame(long frameIndex)
    {
        foreach (var line in _lines)
        {
            if (line.StartFrame > frameIndex)
            {
                break;
            }

            if (frameIndex <= line.EndFrame)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Feeds the 8-bit luma of the scan area (regionWidth x regionHeight, row-major) of one
    /// frame. Frames must come in increasing order; lines whose span ended are finalized.
    /// </summary>
    public void AddRegionFrame(long frameIndex, ReadOnlySpan<byte> regionLuma)
    {
        var n = _rw * _rh;
        foreach (var line in _lines)
        {
            if (line.StartFrame > frameIndex)
            {
                break;
            }

            if (line.Done)
            {
                continue;
            }

            if (frameIndex > line.EndFrame)
            {
                Finalize(line);
                continue;
            }

            // The text pixels come from the line's own frames only: the padding (OCR timing
            // slack) may show the background, the previous or the next line, and counting it
            // let bright background into the "text" (then the per-frame presence check failed).
            if (frameIndex < line.StartFrame + line.CoreOffset || frameIndex >= line.StartFrame + line.CoreOffset + line.CoreFrames)
            {
                continue;
            }

            line.BrightCount ??= new ushort[n];
            line.LumaSum ??= new uint[n];
            for (var i = 0; i < n; i++)
            {
                var v = regionLuma[i];
                line.LumaSum[i] += v;
                if (v >= _brightnessMinimum && line.BrightCount[i] < ushort.MaxValue)
                {
                    line.BrightCount[i]++;
                }
            }

            line.Frames++;
        }
    }

    /// <summary>Finalizes every line (call after the last frame).</summary>
    public void Finish()
    {
        foreach (var line in _lines)
        {
            if (!line.Done)
            {
                Finalize(line);
            }
        }
    }

    /// <summary>Number of lines with a non-empty mask.</summary>
    public int LinesWithText
    {
        get
        {
            var count = 0;
            foreach (var line in _lines)
            {
                if (line.MaskRuns.Length > 0)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// One box per line (in span order) for covering the subtitle instead of inpainting it: the
    /// bounding box of the line's mask plus room for the outline the per-frame growth would add,
    /// or the whole scan area when no text was found for the line.
    /// </summary>
    public IReadOnlyList<(long StartFrame, long EndFrame, int X, int Y, int Width, int Height)> GetLineBoxes()
    {
        var boxes = new List<(long, long, int, int, int, int)>();
        foreach (var line in _lines)
        {
            if (line.MaskRuns.Length == 0)
            {
                boxes.Add((line.StartFrame, line.EndFrame, _rx, _ry, _rw, _rh));
                continue;
            }

            int x0 = int.MaxValue, y0 = int.MaxValue, x1 = 0, y1 = 0;
            for (var r = 0; r < line.MaskRuns.Length; r += 3)
            {
                x0 = Math.Min(x0, _rx + line.MaskRuns[r + 1]);
                y0 = Math.Min(y0, _ry + line.MaskRuns[r]);
                x1 = Math.Max(x1, _rx + line.MaskRuns[r + 1] + line.MaskRuns[r + 2]);
                y1 = Math.Max(y1, _ry + line.MaskRuns[r] + 1);
            }

            var margin = _outlineSteps;
            x0 = Math.Max(0, x0 - margin);
            y0 = Math.Max(0, y0 - margin);
            x1 = Math.Min(_videoWidth, x1 + margin);
            y1 = Math.Min(_videoHeight, y1 + margin);
            boxes.Add((line.StartFrame, line.EndFrame, x0, y0, x1 - x0, y1 - y0));
        }

        return boxes;
    }

    /// <summary>
    /// Writes the full-frame mask (videoWidth x videoHeight, 255 = fill) for a frame, given the
    /// frame's RGB24 pixels for the presence check. Returns false (mask untouched) when no line
    /// with visible text covers the frame. <paramref name="mask"/> must be cleared by the caller.
    /// </summary>
    public bool GetFrameMask(long frameIndex, ReadOnlySpan<byte> rgb, Span<byte> mask)
    {
        // OCR timings can be off by a second or more (sampled frames, merged/split lines), so a
        // line's mask is applied up to applyMarginFrames outside its span - but only on frames
        // where that line's text is actually visible (IsTextPresent).
        var any = false;
        foreach (var line in _lines)
        {
            if (line.StartFrame - _applyMargin > frameIndex)
            {
                break;
            }

            if (frameIndex > line.EndFrame + _applyMargin || line.MaskRuns.Length == 0 || !IsTextPresent(line, rgb))
            {
                continue;
            }

            int x0 = int.MaxValue, y0 = int.MaxValue, x1 = 0, y1 = 0;
            for (var r = 0; r < line.MaskRuns.Length; r += 3)
            {
                var y = _ry + line.MaskRuns[r];
                var x = _rx + line.MaskRuns[r + 1];
                mask.Slice(y * _videoWidth + x, line.MaskRuns[r + 2]).Fill(255);
                x0 = Math.Min(x0, x);
                y0 = Math.Min(y0, y);
                x1 = Math.Max(x1, x + line.MaskRuns[r + 2]);
                y1 = Math.Max(y1, y + 1);
            }

            GrowIntoDarkPixels(rgb, mask, x0, y0, x1, y1);
            any = true;
        }

        return any;
    }

    // Per frame, the mask also takes the text's dark outline / drop shadow in *this* frame. The
    // per-line growth uses the line's average brightness, which misses the outline when the line
    // spans a scene cut, and any outline left on the rim of the hole makes the models fill it
    // dark (a black bar or a grey shadow on a bright background).
    // "Dark" is relative to the local background, estimated around the text with the text and
    // its immediate surroundings left out: a semi-transparent shadow on a bright picture is far
    // above any absolute threshold but still clearly darker than the picture next to it. The
    // mask grows through such pixels until they run out (up to 3x the outline size), through
    // absolutely dark pixels for the outline size; the outline found gets the extra width left
    // and right, then everything a few px all round.
    private const int RelativeDarkDelta = 28;
    private static readonly int[] BackgroundRadii = { 8, 16, 32, 64 };

    private void GrowIntoDarkPixels(ReadOnlySpan<byte> rgb, Span<byte> mask, int x0, int y0, int x1, int y1)
    {
        var steps = _outlineSteps;
        var maxSteps = steps * 3;
        var finalGrow = Math.Max(2, _grow / 2);
        var margin = maxSteps + finalGrow + _extraWidth + 1;
        var context = BackgroundRadii[^1];
        // Work area: the growth margin plus room for the background estimate.
        var wx0 = Math.Max(0, x0 - margin - context);
        var wy0 = Math.Max(0, y0 - margin - context);
        var wx1 = Math.Min(_videoWidth, x1 + margin + context);
        var wy1 = Math.Min(_videoHeight, y1 + margin + context);
        var w = wx1 - wx0;
        var h = wy1 - wy0;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        var n = w * h;
        var original = new bool[n];
        var luma = new int[n];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var i = (wy0 + y) * _videoWidth + wx0 + x;
                original[y * w + x] = mask[i] != 0;
                var o = i * 3;
                luma[y * w + x] = (rgb[o] * 299 + rgb[o + 1] * 587 + rgb[o + 2] * 114) / 1000;
            }
        }

        var background = EstimateBackground(luma, original, w, h, Math.Max(6, steps));

        // Breadth-first growth; dist = steps from the original mask (8-neighbour).
        var dist = new int[n];
        Array.Fill(dist, -1);
        var queue = new Queue<int>();
        for (var i = 0; i < n; i++)
        {
            if (original[i])
            {
                dist[i] = 0;
                queue.Enqueue(i);
            }
        }

        while (queue.Count > 0)
        {
            var i = queue.Dequeue();
            var next = dist[i] + 1;
            if (next > maxSteps)
            {
                continue;
            }

            var cx = i % w;
            var cy = i / w;
            for (var dy = -1; dy <= 1; dy++)
            {
                var yy = cy + dy;
                if (yy < 0 || yy >= h)
                {
                    continue;
                }

                for (var dx = -1; dx <= 1; dx++)
                {
                    var xx = cx + dx;
                    if (xx < 0 || xx >= w)
                    {
                        continue;
                    }

                    var j = yy * w + xx;
                    if (dist[j] >= 0)
                    {
                        continue;
                    }

                    var relativeDark = luma[j] < background[j] - RelativeDarkDelta;
                    var absoluteDark = luma[j] <= DarkOutlineMaximum && next <= steps;
                    if (relativeDark || absoluteDark)
                    {
                        dist[j] = next;
                        queue.Enqueue(j);
                    }
                }
            }
        }

        var grown = new bool[n];
        for (var i = 0; i < n; i++)
        {
            grown[i] = dist[i] >= 0;
        }

        // The line mask already has the extra width; outline found here gets it too.
        if (_extraWidth > 0)
        {
            var added = new bool[n];
            for (var i = 0; i < n; i++)
            {
                added[i] = grown[i] && !original[i];
            }

            var wide = DilateHorizontal(added, w, h, _extraWidth);
            for (var i = 0; i < n; i++)
            {
                grown[i] |= wide[i];
            }
        }

        var result = Dilate(grown, w, h, finalGrow);
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                if (result[y * w + x])
                {
                    mask[(wy0 + y) * _videoWidth + wx0 + x] = 255;
                }
            }
        }
    }

    // Local background brightness per pixel: the mean of the pixels more than `exclude` px from
    // the mask in the smallest box (radius 8..64) with enough of them; the overall median of
    // those pixels where no box has.
    private static int[] EstimateBackground(int[] luma, bool[] mask, int w, int h, int exclude)
    {
        var n = w * h;
        var near = Dilate(mask, w, h, exclude);
        var valid = new int[n];
        var values = new int[n];
        var all = new List<int>();
        for (var i = 0; i < n; i++)
        {
            if (!near[i])
            {
                valid[i] = 1;
                values[i] = luma[i];
                all.Add(luma[i]);
            }
        }

        var fallback = 128;
        if (all.Count > 0)
        {
            all.Sort();
            fallback = all[all.Count / 2];
        }

        var background = new int[n];
        Array.Fill(background, -1);
        var countIntegral = Integral(valid, w, h);
        var sumIntegral = Integral(values, w, h);
        foreach (var r in BackgroundRadii)
        {
            var minimum = r * r / 2;
            var open = false;
            for (var y = 0; y < h; y++)
            {
                var ya = Math.Max(0, y - r);
                var yb = Math.Min(h, y + r + 1);
                for (var x = 0; x < w; x++)
                {
                    var i = y * w + x;
                    if (background[i] >= 0)
                    {
                        continue;
                    }

                    var xa = Math.Max(0, x - r);
                    var xb = Math.Min(w, x + r + 1);
                    var count = BoxSum(countIntegral, w, xa, ya, xb, yb);
                    if (count >= minimum)
                    {
                        background[i] = (int)(BoxSum(sumIntegral, w, xa, ya, xb, yb) / count);
                    }
                    else
                    {
                        open = true;
                    }
                }
            }

            if (!open)
            {
                break;
            }
        }

        for (var i = 0; i < n; i++)
        {
            if (background[i] < 0)
            {
                background[i] = fallback;
            }
        }

        return background;
    }

    private static long[] Integral(int[] values, int w, int h)
    {
        var integral = new long[(w + 1) * (h + 1)];
        for (var y = 0; y < h; y++)
        {
            long row = 0;
            for (var x = 0; x < w; x++)
            {
                row += values[y * w + x];
                integral[(y + 1) * (w + 1) + x + 1] = integral[y * (w + 1) + x + 1] + row;
            }
        }

        return integral;
    }

    private static long BoxSum(long[] integral, int w, int x0, int y0, int x1, int y1)
    {
        var stride = w + 1;
        return integral[y1 * stride + x1] - integral[y0 * stride + x1] - integral[y1 * stride + x0] + integral[y0 * stride + x0];
    }

    private bool IsTextPresent(LineMask line, ReadOnlySpan<byte> rgb)
    {
        var bright = 0;
        foreach (var p in line.TextPixels)
        {
            var o = ((_ry + p / _rw) * _videoWidth + _rx + p % _rw) * 3;
            if ((rgb[o] * 299 + rgb[o + 1] * 587 + rgb[o + 2] * 114) / 1000 >= _brightnessMinimum)
            {
                bright++;
            }
        }

        return bright >= line.TextPixels.Length * PresentFraction;
    }

    private void Finalize(LineMask line)
    {
        line.Done = true;
        if (line.Frames == 0 || line.BrightCount == null || line.LumaSum == null)
        {
            return;
        }

        var n = _rw * _rh;
        var minCount = Math.Max(1, (int)Math.Ceiling(line.Frames * StableFraction));
        var text = new bool[n];
        var any = false;
        for (var i = 0; i < n; i++)
        {
            if (line.BrightCount[i] >= minCount)
            {
                text[i] = true;
                any = true;
            }
        }

        var dark = new bool[n];
        for (var i = 0; i < n; i++)
        {
            dark[i] = line.LumaSum[i] / (uint)line.Frames <= DarkOutlineMaximum;
        }

        if (any && _requireOutline)
        {
            var nearDark = Dilate(dark, _outlineRadius);
            any = false;
            for (var i = 0; i < n; i++)
            {
                text[i] &= nearDark[i];
                any |= text[i];
            }
        }

        line.BrightCount = null;
        line.LumaSum = null;
        if (!any)
        {
            return;
        }

        // The outline / shadow: grow the text through pixels that are dark on average, up to a
        // size relative to the video height. Left out, a thick outline stays as "known" context
        // and the fill comes out as a dark smear in the shape of the text.
        var withOutline = GrowInto(text, dark, _outlineSteps);
        var grown = DilateHorizontal(Dilate(withOutline, _grow), _extraWidth);
        var textCount = 0;
        foreach (var t in text)
        {
            if (t)
            {
                textCount++;
            }
        }

        const int maxSamples = 4000;
        var step = Math.Max(1, textCount / maxSamples);
        var textPixels = new List<int>();
        var k = 0;
        for (var i = 0; i < n; i++)
        {
            if (text[i] && k++ % step == 0)
            {
                textPixels.Add(i);
            }
        }

        var runs = new List<int>();
        for (var y = 0; y < _rh; y++)
        {
            var x = 0;
            while (x < _rw)
            {
                if (!grown[y * _rw + x])
                {
                    x++;
                    continue;
                }

                var start = x;
                while (x < _rw && grown[y * _rw + x])
                {
                    x++;
                }

                runs.Add(y);
                runs.Add(start);
                runs.Add(x - start);
            }
        }

        line.TextPixels = textPixels.ToArray();
        line.MaskRuns = runs.ToArray();
    }

    // Grows `seed` one pixel (8-neighbour) per step, but only into `allowed` pixels.
    private bool[] GrowInto(bool[] seed, bool[] allowed, int steps)
    {
        var current = (bool[])seed.Clone();
        for (var step = 0; step < steps; step++)
        {
            var next = (bool[])current.Clone();
            var changed = false;
            for (var y = 0; y < _rh; y++)
            {
                for (var x = 0; x < _rw; x++)
                {
                    var i = y * _rw + x;
                    if (current[i] || !allowed[i])
                    {
                        continue;
                    }

                    for (var dy = -1; dy <= 1 && !next[i]; dy++)
                    {
                        var yy = y + dy;
                        if (yy < 0 || yy >= _rh)
                        {
                            continue;
                        }

                        for (var dx = -1; dx <= 1; dx++)
                        {
                            var xx = x + dx;
                            if (xx >= 0 && xx < _rw && current[yy * _rw + xx])
                            {
                                next[i] = true;
                                changed = true;
                                break;
                            }
                        }
                    }
                }
            }

            current = next;
            if (!changed)
            {
                break;
            }
        }

        return current;
    }

    // Extra width left and right of the text (user setting): the first and last letters'
    // outline/shadow can reach further sideways than the normal margin covers.
    private bool[] DilateHorizontal(bool[] source, int radius) => DilateHorizontal(source, _rw, _rh, radius);

    private static bool[] DilateHorizontal(bool[] source, int width, int height, int radius)
    {
        if (radius <= 0)
        {
            return source;
        }

        var result = new bool[source.Length];
        for (var y = 0; y < height; y++)
        {
            var last = int.MinValue / 2;
            for (var x = 0; x < width + radius; x++)
            {
                if (x < width && source[y * width + x])
                {
                    last = x;
                }

                var ox = x - radius;
                if (ox >= 0 && x - last <= 2 * radius)
                {
                    result[y * width + ox] = true;
                }
            }
        }

        return result;
    }

    // Square dilation (max filter) of a scan-area bitmap, separable.
    private bool[] Dilate(bool[] source, int radius) => Dilate(source, _rw, _rh, radius);

    private static bool[] Dilate(bool[] source, int width, int height, int radius)
    {
        if (radius <= 0)
        {
            return (bool[])source.Clone();
        }

        var tmp = new bool[source.Length];
        for (var y = 0; y < height; y++)
        {
            var last = int.MinValue / 2;
            for (var x = 0; x < width + radius; x++)
            {
                if (x < width && source[y * width + x])
                {
                    last = x;
                }

                var ox = x - radius;
                if (ox >= 0 && x - last <= 2 * radius)
                {
                    tmp[y * width + ox] = true;
                }
            }
        }

        var result = new bool[source.Length];
        for (var x = 0; x < width; x++)
        {
            var last = int.MinValue / 2;
            for (var y = 0; y < height + radius; y++)
            {
                if (y < height && tmp[y * width + x])
                {
                    last = y;
                }

                var oy = y - radius;
                if (oy >= 0 && y - last <= 2 * radius)
                {
                    result[oy * width + x] = true;
                }
            }
        }

        return result;
    }
}
