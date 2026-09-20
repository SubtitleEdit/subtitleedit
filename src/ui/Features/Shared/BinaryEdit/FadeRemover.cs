using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

/// <summary>
/// BDSup2Sub's "remove fade in/out": authoring tools render a fade as a run of back-to-back
/// captions that share one image and differ only in alpha. Playing that back after editing
/// (or in a player that ignores palette updates) gives a flicker of near-identical lines, so
/// collapse each run into one caption that spans the whole time and uses the most opaque image.
/// </summary>
public static class FadeRemover
{
    /// <summary>Maximum gap between two captions for them to count as one fade.</summary>
    public const int MaxGapMs = 50;

    /// <summary>Positions may drift a pixel or two between the fade steps.</summary>
    private const int MaxPositionDrift = 2;

    /// <summary>Per-channel colour tolerance after undoing premultiplication.</summary>
    private const int ColorTolerance = 16;

    /// <summary>Unpremultiplied colour of very transparent pixels is noise - only compare alpha coverage there.</summary>
    private const int MinAlphaForColorCompare = 32;

    /// <summary>Fraction of pixels that may differ (anti-aliased edges) and still be the same image.</summary>
    private const double MaxMismatchFraction = 0.005;

    /// <summary>
    /// Collapses fade runs in <paramref name="subtitles"/> in place. Removed items have their
    /// bitmaps disposed. Returns the number of items removed.
    /// </summary>
    public static int RemoveFades(IList<BinarySubtitleItem> subtitles)
    {
        var removed = 0;
        var i = 0;
        while (i < subtitles.Count - 1)
        {
            var keep = subtitles[i];
            if (keep.Bitmap == null)
            {
                i++;
                continue;
            }

            using var keepSk = keep.Bitmap.ToSkBitmap();
            var bestOpacity = TotalAlpha(keepSk);
            BinarySubtitleItem? best = null;
            var runEnd = i;

            for (var j = i + 1; j < subtitles.Count; j++)
            {
                var prev = subtitles[j - 1];
                var cur = subtitles[j];
                if (cur.Bitmap == null || !IsAdjacent(prev, cur))
                {
                    break;
                }

                using var curSk = cur.Bitmap.ToSkBitmap();
                if (!IsSameImage(keepSk, curSk))
                {
                    break;
                }

                runEnd = j;
                var opacity = TotalAlpha(curSk);
                if (opacity > bestOpacity)
                {
                    bestOpacity = opacity;
                    best = cur;
                }
            }

            if (runEnd == i)
            {
                i++;
                continue;
            }

            var last = subtitles[runEnd];
            keep.EndTime = last.EndTime;
            if (best != null)
            {
                // Swap the most opaque image into the surviving line (a fade-in starts nearly invisible).
                var old = keep.Bitmap;
                keep.Bitmap = best.Bitmap;
                keep.X = best.X;
                keep.Y = best.Y;
                best.Bitmap = old;
            }

            for (var j = runEnd; j > i; j--)
            {
                var item = subtitles[j];
                subtitles.RemoveAt(j);
                item.Bitmap?.Dispose();
                item.Bitmap = null;
                removed++;
            }

            i++;
        }

        return removed;
    }

    public static bool IsAdjacent(BinarySubtitleItem prev, BinarySubtitleItem cur)
    {
        var gap = Math.Abs((cur.StartTime - prev.EndTime).TotalMilliseconds);
        return gap <= MaxGapMs
               && Math.Abs(prev.X - cur.X) <= MaxPositionDrift
               && Math.Abs(prev.Y - cur.Y) <= MaxPositionDrift;
    }

    /// <summary>
    /// True when the two bitmaps show the same picture, ignoring how transparent it is: same size,
    /// same alpha coverage, same unpremultiplied colours where the pixel is visible enough to judge.
    /// </summary>
    public static bool IsSameImage(SKBitmap a, SKBitmap b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Width == 0 || a.Height == 0)
        {
            return false;
        }

        if (a.ColorType != SKColorType.Bgra8888 || b.ColorType != SKColorType.Bgra8888)
        {
            return false;
        }

        var maxMismatches = (int)(a.Width * a.Height * MaxMismatchFraction);
        var mismatches = 0;

        unsafe
        {
            for (var y = 0; y < a.Height; y++)
            {
                var pa = (byte*)a.GetPixels() + y * a.RowBytes;
                var pb = (byte*)b.GetPixels() + y * b.RowBytes;
                for (var x = 0; x < a.Width; x++, pa += 4, pb += 4)
                {
                    var alphaA = pa[3];
                    var alphaB = pb[3];
                    if ((alphaA == 0) != (alphaB == 0))
                    {
                        if (++mismatches > maxMismatches)
                        {
                            return false;
                        }

                        continue;
                    }

                    if (alphaA < MinAlphaForColorCompare || alphaB < MinAlphaForColorCompare)
                    {
                        continue;
                    }

                    if (!ChannelMatches(pa[0], alphaA, pb[0], alphaB, a.AlphaType, b.AlphaType)
                        || !ChannelMatches(pa[1], alphaA, pb[1], alphaB, a.AlphaType, b.AlphaType)
                        || !ChannelMatches(pa[2], alphaA, pb[2], alphaB, a.AlphaType, b.AlphaType))
                    {
                        if (++mismatches > maxMismatches)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    private static bool ChannelMatches(byte ca, byte alphaA, byte cb, byte alphaB, SKAlphaType typeA, SKAlphaType typeB)
    {
        var va = typeA == SKAlphaType.Premul ? ca * 255 / alphaA : ca;
        var vb = typeB == SKAlphaType.Premul ? cb * 255 / alphaB : cb;
        return Math.Abs(va - vb) <= ColorTolerance;
    }

    private static long TotalAlpha(SKBitmap bitmap)
    {
        if (bitmap.ColorType != SKColorType.Bgra8888)
        {
            return 0;
        }

        long total = 0;
        unsafe
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                var p = (byte*)bitmap.GetPixels() + y * bitmap.RowBytes + 3;
                for (var x = 0; x < bitmap.Width; x++, p += 4)
                {
                    total += *p;
                }
            }
        }

        return total;
    }
}
