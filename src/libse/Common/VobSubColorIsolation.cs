using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Histogram-based colour isolation for VobSub OCR bitmaps (issue #12293).
    ///
    /// A VobSub subpicture is a strict 4-colour indexed image: one palette entry for the
    /// transparent background, one for the main glyph fill, and one or two for the
    /// anti-aliased outline / emphasis. When such a bitmap is composited for OCR the outline
    /// colours bleed into the fill and the recogniser melts adjacent glyphs together
    /// ("Yuri" → "Yurl"). Because DVD authoring tools assign the palette indices
    /// unpredictably from disc to disc, there is no fixed colour to strip in an unattended
    /// batch run.
    ///
    /// This pass rebuilds a crisp black-on-white bitmap from plane topology: transparent
    /// pixels are the background, and of the opaque colours the one that is most *interior*
    /// — the smallest fraction of its pixels bordering the transparent background — is the
    /// glyph fill (kept as black); every other opaque colour (the outline / anti-alias
    /// tiers, which by construction wrap the fill and face the background) collapses into
    /// the white background. The result is a clean binary mask that OCR engines recognise
    /// far more reliably. Used by both the seconv CLI (<c>--no-vobsub-isolate-colors</c> to
    /// disable) and the Batch Convert UI.
    /// </summary>
    public static class VobSubColorIsolation
    {
        /// <summary>
        /// Returns a new opaque black-on-white bitmap in which only the glyph-fill colour
        /// plane is kept as foreground. The fill is identified as the opaque colour with the
        /// lowest background-adjacency ratio — with a real palette the outline can hold more
        /// pixels than the fill (issue #12772 part 3), so frequency alone picks the wrong
        /// plane, but the fill is always the innermost one. The caller owns the returned
        /// bitmap. When the source has no opaque pixels (a blank / clear frame) a solid white
        /// bitmap is returned so downstream OCR simply sees nothing.
        /// </summary>
        /// <param name="source">Rendered VobSub bitmap; a transparent background is expected.</param>
        /// <param name="alphaThreshold">Pixels with alpha below this are treated as background.</param>
        public static SKBitmap Isolate(SKBitmap source, byte alphaThreshold = 128)
        {
            var width = source.Width;
            var height = source.Height;
            var result = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));

            // Tally every opaque colour and how many of its pixels touch the transparent
            // background (4-neighbourhood; the bitmap edge counts as background). Key on RGB
            // only (alpha already gates the background) so anti-aliasing tiers that share a
            // hue but differ in coverage still merge.
            var counts = new Dictionary<uint, int>();
            var borderCounts = new Dictionary<uint, int>();
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var c = source.GetPixel(x, y);
                    if (c.Alpha < alphaThreshold)
                    {
                        continue;
                    }
                    var key = (uint)((c.Red << 16) | (c.Green << 8) | c.Blue);
                    counts[key] = counts.TryGetValue(key, out var n) ? n + 1 : 1;

                    var touchesBackground =
                        x == 0 || source.GetPixel(x - 1, y).Alpha < alphaThreshold ||
                        x == width - 1 || source.GetPixel(x + 1, y).Alpha < alphaThreshold ||
                        y == 0 || source.GetPixel(x, y - 1).Alpha < alphaThreshold ||
                        y == height - 1 || source.GetPixel(x, y + 1).Alpha < alphaThreshold;
                    if (touchesBackground)
                    {
                        borderCounts[key] = borderCounts.TryGetValue(key, out var b) ? b + 1 : 1;
                    }
                }
            }

            using var canvas = new SKCanvas(result);
            canvas.Clear(SKColors.White);

            if (counts.Count == 0)
            {
                // No text pixels — hand back a blank white canvas.
                return result;
            }

            // The glyph fill is the most interior plane: lowest share of its pixels facing
            // the transparent background. The outline faces the background on (at least) its
            // outer edge, and the anti-alias tier is the boundary itself, so both score
            // higher. Planes too small to be the text body (speckle, stray anti-alias
            // colours) are ignored unless nothing bigger exists. Ties resolve to the lowest
            // colour key so the output is deterministic regardless of dictionary order.
            var opaqueTotal = 0;
            foreach (var kv in counts)
            {
                opaqueTotal += kv.Value;
            }
            var minPlane = Math.Max(16, opaqueTotal / 20);

            var foreground = 0u;
            var bestRatio = double.MaxValue;
            var considered = 0;
            foreach (var kv in counts)
            {
                if (kv.Value < minPlane)
                {
                    continue;
                }
                considered++;
                var ratio = borderCounts.TryGetValue(kv.Key, out var b) ? (double)b / kv.Value : 0.0;
                if (ratio < bestRatio || (Math.Abs(ratio - bestRatio) < 0.0001 && kv.Key < foreground))
                {
                    bestRatio = ratio;
                    foreground = kv.Key;
                }
            }

            if (considered == 0)
            {
                // Every plane was speckle-sized; fall back to the most frequent colour.
                var best = -1;
                foreach (var kv in counts)
                {
                    if (kv.Value > best || (kv.Value == best && kv.Key < foreground))
                    {
                        best = kv.Value;
                        foreground = kv.Key;
                    }
                }
            }

            using var paint = new SKPaint { Color = SKColors.Black };
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var c = source.GetPixel(x, y);
                    if (c.Alpha >= alphaThreshold &&
                        (uint)((c.Red << 16) | (c.Green << 8) | c.Blue) == foreground)
                    {
                        canvas.DrawPoint(x, y, paint);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Brightness-keyed binarization for PGS / DVB-sub OCR bitmaps (issue #12291) — the
        /// same preparation the GUI OCR uses (<c>NikseBitmap.MakeBlackAndWhiteForOcr</c>):
        /// bright, sufficiently opaque pixels (the glyph fill, incl. coloured fills like
        /// yellow) become black, everything else — the dark outline, the darker half of the
        /// antialias ramp, and the transparent background — becomes white. Unlike
        /// <see cref="Isolate"/>, this does not assume an indexed palette, so it is robust
        /// for antialiased PGS where no single exact RGB dominates. Without it, compositing
        /// white-fill/black-outline glyphs onto an opaque white OCR canvas leaves hollow
        /// outline rings that Tesseract reads as punctuation soup on some entries.
        /// </summary>
        /// <param name="source">Rendered PGS/DVB bitmap; a transparent background is expected.</param>
        /// <param name="brightnessThreshold">Min brightness (max of R/G/B) for a pixel to count as text.</param>
        /// <param name="alphaThreshold">Pixels with alpha below this are treated as background.</param>
        public static SKBitmap BinarizeForOcr(SKBitmap source, int brightnessThreshold = 90, byte alphaThreshold = 100)
        {
            var width = source.Width;
            var height = source.Height;
            var result = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));

            using var canvas = new SKCanvas(result);
            canvas.Clear(SKColors.White);

            using var paint = new SKPaint { Color = SKColors.Black };
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var c = source.GetPixel(x, y);
                    if (c.Alpha < alphaThreshold)
                    {
                        continue;
                    }

                    var brightness = c.Red > c.Green
                        ? (c.Red > c.Blue ? c.Red : c.Blue)
                        : (c.Green > c.Blue ? c.Green : c.Blue);
                    if (brightness >= brightnessThreshold)
                    {
                        canvas.DrawPoint(x, y, paint);
                    }
                }
            }

            return result;
        }
    }
}
