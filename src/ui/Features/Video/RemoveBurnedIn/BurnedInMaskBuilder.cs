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

    // Per frame, the mask also takes the text's dark outline / drop shadow in *this* frame:
    // it grows through dark pixels (see below), a few px (by video height), then
    // one more px. The per-line growth uses the line's average brightness, which misses the
    // outline when the line spans a scene cut; any outline left on the rim of the hole makes
    // the models fill it dark (a black bar on a bright background).
    private void GrowIntoDarkPixels(ReadOnlySpan<byte> rgb, Span<byte> mask, int x0, int y0, int x1, int y1)
    {
        var steps = _outlineSteps;
        x0 = Math.Max(0, x0 - steps - 1);
        y0 = Math.Max(0, y0 - steps - 1);
        x1 = Math.Min(_videoWidth, x1 + steps + 1);
        y1 = Math.Min(_videoHeight, y1 + steps + 1);
        var w = x1 - x0;
        var h = y1 - y0;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        // "Dark" relative to the background around the text: a semi-transparent shadow on a
        // bright picture is far above an absolute threshold but still clearly darker than its
        // surroundings.
        var cur = new bool[w * h];
        var luma = new int[w * h];
        var background = new List<int>();
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var i = (y0 + y) * _videoWidth + x0 + x;
                cur[y * w + x] = mask[i] != 0;
                var o = i * 3;
                luma[y * w + x] = (rgb[o] * 299 + rgb[o + 1] * 587 + rgb[o + 2] * 114) / 1000;
                if (!cur[y * w + x])
                {
                    background.Add(luma[y * w + x]);
                }
            }
        }

        var darkMaximum = DarkOutlineMaximum;
        if (background.Count > 0)
        {
            background.Sort();
            darkMaximum = Math.Clamp((int)(background[background.Count / 2] * 0.65), DarkOutlineMaximum, 160);
        }

        var dark = new bool[w * h];
        for (var i = 0; i < dark.Length; i++)
        {
            dark[i] = luma[i] <= darkMaximum;
        }

        for (var step = 0; step <= steps; step++)
        {
            var next = (bool[])cur.Clone();
            var last = step == steps; // final step: plain 1 px dilation
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var i = y * w + x;
                    if (cur[i] || (!last && !dark[i]))
                    {
                        continue;
                    }

                    for (var dy = -1; dy <= 1 && !next[i]; dy++)
                    {
                        var yy = y + dy;
                        if (yy < 0 || yy >= h)
                        {
                            continue;
                        }

                        for (var dx = -1; dx <= 1; dx++)
                        {
                            var xx = x + dx;
                            if (xx >= 0 && xx < w && cur[yy * w + xx])
                            {
                                next[i] = true;
                                break;
                            }
                        }
                    }
                }
            }

            cur = next;
        }

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                if (cur[y * w + x])
                {
                    mask[(y0 + y) * _videoWidth + x0 + x] = 255;
                }
            }
        }
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
    private bool[] DilateHorizontal(bool[] source, int radius)
    {
        if (radius <= 0)
        {
            return source;
        }

        var result = new bool[source.Length];
        for (var y = 0; y < _rh; y++)
        {
            var last = int.MinValue / 2;
            for (var x = 0; x < _rw + radius; x++)
            {
                if (x < _rw && source[y * _rw + x])
                {
                    last = x;
                }

                var ox = x - radius;
                if (ox >= 0 && x - last <= 2 * radius)
                {
                    result[y * _rw + ox] = true;
                }
            }
        }

        return result;
    }

    // Square dilation (max filter) of a scan-area bitmap, separable.
    private bool[] Dilate(bool[] source, int radius)
    {
        if (radius <= 0)
        {
            return (bool[])source.Clone();
        }

        var tmp = new bool[source.Length];
        for (var y = 0; y < _rh; y++)
        {
            var last = int.MinValue / 2;
            for (var x = 0; x < _rw + radius; x++)
            {
                if (x < _rw && source[y * _rw + x])
                {
                    last = x;
                }

                var ox = x - radius;
                if (ox >= 0 && x - last <= 2 * radius)
                {
                    tmp[y * _rw + ox] = true;
                }
            }
        }

        var result = new bool[source.Length];
        for (var x = 0; x < _rw; x++)
        {
            var last = int.MinValue / 2;
            for (var y = 0; y < _rh + radius; y++)
            {
                if (y < _rh && tmp[y * _rw + x])
                {
                    last = y;
                }

                var oy = y - radius;
                if (oy >= 0 && y - last <= 2 * radius)
                {
                    result[oy * _rw + x] = true;
                }
            }
        }

        return result;
    }
}
