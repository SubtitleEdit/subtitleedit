using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Pixel adjustments for image-based subtitles (brightness/contrast/gamma, alpha,
/// re-coloring). Shared by the binary-edit adjust dialogs and batch convert.
/// All methods take a premultiplied bitmap and return a new straight-alpha bitmap.
/// </summary>
public static class SubtitleImageAdjuster
{
    public static SKBitmap AdjustBrightness(SKBitmap premultipliedBitmap, float brightness, float contrast, float gamma)
    {
        // Work in straight alpha: premultiplied color already carries the pixel's alpha, so
        // brightening it would scale with transparency and could push RGB above A (halos).
        using var originalBitmap = premultipliedBitmap.ToUnpremultiplied();
        var adjustedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

        // Normalize values for calculations
        var brightnessAdjust = brightness; // -100 to 100
        var contrastAdjust = (contrast + 100) / 100.0f; // Convert -100 to 100 range to 0 to 2 multiplier

        // Build lookup table for gamma correction
        var gammaLookup = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            gammaLookup[i] = (byte)Math.Clamp(Math.Pow(i / 255.0, 1.0 / gamma) * 255.0, 0, 255);
        }

        unsafe
        {
            var originalPixels = originalBitmap.GetPixels();
            var adjustedPixels = adjustedBitmap.GetPixels();

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    var index = y * originalBitmap.Width + x;
                    var pixel = ((uint*)originalPixels)[index];

                    var a = (byte)((pixel >> 24) & 0xFF);
                    var r = (byte)((pixel >> 16) & 0xFF);
                    var g = (byte)((pixel >> 8) & 0xFF);
                    var b = (byte)(pixel & 0xFF);

                    // Skip transparent pixels
                    if (a == 0)
                    {
                        ((uint*)adjustedPixels)[index] = pixel;
                        continue;
                    }

                    // Apply brightness
                    r = (byte)Math.Clamp(r + brightnessAdjust, 0, 255);
                    g = (byte)Math.Clamp(g + brightnessAdjust, 0, 255);
                    b = (byte)Math.Clamp(b + brightnessAdjust, 0, 255);

                    // Apply contrast
                    r = (byte)Math.Clamp(((r - 128) * contrastAdjust) + 128, 0, 255);
                    g = (byte)Math.Clamp(((g - 128) * contrastAdjust) + 128, 0, 255);
                    b = (byte)Math.Clamp(((b - 128) * contrastAdjust) + 128, 0, 255);

                    // Apply gamma
                    r = gammaLookup[r];
                    g = gammaLookup[g];
                    b = gammaLookup[b];

                    // Reconstruct pixel
                    var adjustedPixel = (uint)((a << 24) | (r << 16) | (g << 8) | b);
                    ((uint*)adjustedPixels)[index] = adjustedPixel;
                }
            }
        }

        return adjustedBitmap;
    }

    public static SKBitmap AdjustAlpha(SKBitmap premultipliedBitmap, float alphaAdjustment, byte transparencyThreshold)
    {
        // Work in straight alpha: changing A while leaving premultiplied R, G and B alone
        // would produce RGB > A, which Skia renders as clipped, over-bright edges.
        using var originalBitmap = premultipliedBitmap.ToUnpremultiplied();
        var adjustedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

        unsafe
        {
            var originalPixels = originalBitmap.GetPixels();
            var adjustedPixels = adjustedBitmap.GetPixels();

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    var index = y * originalBitmap.Width + x;
                    var pixel = ((uint*)originalPixels)[index];

                    var a = (byte)((pixel >> 24) & 0xFF);
                    var r = (byte)((pixel >> 16) & 0xFF);
                    var g = (byte)((pixel >> 8) & 0xFF);
                    var b = (byte)(pixel & 0xFF);

                    // Skip if already fully transparent
                    if (a == 0)
                    {
                        ((uint*)adjustedPixels)[index] = pixel;
                        continue;
                    }

                    // Apply alpha adjustment (additive)
                    float newAlpha = a + alphaAdjustment;

                    // Clamp to valid range
                    newAlpha = Math.Clamp(newAlpha, 0, 255);

                    // Apply transparency threshold
                    if (newAlpha < transparencyThreshold)
                    {
                        newAlpha = 0;
                    }

                    a = (byte)newAlpha;

                    // Reconstruct pixel
                    var adjustedPixel = (uint)((a << 24) | (r << 16) | (g << 8) | b);
                    ((uint*)adjustedPixels)[index] = adjustedPixel;
                }
            }
        }

        return adjustedBitmap;
    }

    public static SKBitmap Colorize(SKBitmap premultipliedBitmap, byte r, byte g, byte b)
    {
        var redPercent = r * 100.0 / 255;
        var greenPercent = g * 100.0 / 255;
        var bluePercent = b * 100.0 / 255;

        // Work in straight alpha: the "total" brightness test below is meaningless on
        // premultiplied color, where a faint anti-aliased pixel falls under the threshold
        // and keeps its original color, leaving a fringe around the recolored text.
        using var originalBitmap = premultipliedBitmap.ToUnpremultiplied();
        var adjustedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

        unsafe
        {
            var srcPixels = originalBitmap.GetPixels();
            var dstPixels = adjustedBitmap.GetPixels();

            for (int i = 0; i < originalBitmap.Width * originalBitmap.Height; i++)
            {
                var pixel = ((uint*)srcPixels)[i];

                var a = (byte)((pixel >> 24) & 0xFF);
                var pr = (byte)((pixel >> 16) & 0xFF);
                var pg = (byte)((pixel >> 8) & 0xFF);
                var pb = (byte)(pixel & 0xFF);

                int total = pr + pg + pb;
                if (total > 100 && a > 0)
                {
                    pr = (byte)Math.Min(255, redPercent * total / 100);
                    pg = (byte)Math.Min(255, greenPercent * total / 100);
                    pb = (byte)Math.Min(255, bluePercent * total / 100);
                }

                ((uint*)dstPixels)[i] = (uint)((a << 24) | (pr << 16) | (pg << 8) | pb);
            }
        }

        return adjustedBitmap;
    }
}
