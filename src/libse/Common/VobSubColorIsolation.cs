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
    /// This pass rebuilds a crisp black-on-white bitmap purely from pixel frequency:
    /// transparent pixels are the background, the most common opaque colour is the main text
    /// body (kept as black), and every other opaque colour (the outline / anti-alias tiers)
    /// collapses into the white background. The result is a clean binary mask that OCR
    /// engines recognise far more reliably. Used by both the seconv CLI
    /// (<c>--no-vobsub-isolate-colors</c> to disable) and the Batch Convert UI.
    /// </summary>
    public static class VobSubColorIsolation
    {
        /// <summary>
        /// Returns a new opaque black-on-white bitmap in which only the dominant opaque colour
        /// (the glyph fill) is kept as foreground. The caller owns the returned bitmap. When the
        /// source has no opaque pixels (a blank / clear frame) a solid white bitmap is returned
        /// so downstream OCR simply sees nothing.
        /// </summary>
        /// <param name="source">Rendered VobSub bitmap; a transparent background is expected.</param>
        /// <param name="alphaThreshold">Pixels with alpha below this are treated as background.</param>
        public static SKBitmap Isolate(SKBitmap source, byte alphaThreshold = 128)
        {
            var width = source.Width;
            var height = source.Height;
            var result = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));

            // Tally every opaque colour. Key on RGB only (alpha already gates the background)
            // so anti-aliasing tiers that share a hue but differ in coverage still merge.
            var counts = new Dictionary<uint, int>();
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
                }
            }

            using var canvas = new SKCanvas(result);
            canvas.Clear(SKColors.White);

            if (counts.Count == 0)
            {
                // No text pixels — hand back a blank white canvas.
                return result;
            }

            // Most frequent opaque colour = the glyph fill (the "second tier" once the
            // transparent background is excluded). Ties resolve to the lowest colour key so the
            // output is deterministic regardless of dictionary iteration order.
            var foreground = 0u;
            var best = -1;
            foreach (var kv in counts)
            {
                if (kv.Value > best || (kv.Value == best && kv.Key < foreground))
                {
                    best = kv.Value;
                    foreground = kv.Key;
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
    }
}
