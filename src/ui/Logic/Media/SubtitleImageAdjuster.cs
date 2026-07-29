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

        // Brightness, contrast and gamma are each a pure function of a single channel byte,
        // so the whole chain collapses into one 256-entry lookup table.
        var gammaLookup = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            gammaLookup[i] = (byte)Math.Clamp(Math.Pow(i / 255.0, 1.0 / gamma) * 255.0, 0, 255);
        }

        var channelLookup = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            var value = (byte)Math.Clamp(i + brightnessAdjust, 0, 255);
            value = (byte)Math.Clamp(((value - 128) * contrastAdjust) + 128, 0, 255);
            channelLookup[i] = gammaLookup[value];
        }

        unsafe
        {
            var originalPixels = (uint*)originalBitmap.GetPixels();
            var adjustedPixels = (uint*)adjustedBitmap.GetPixels();
            var pixelCount = originalBitmap.Width * originalBitmap.Height;

            for (int index = 0; index < pixelCount; index++)
            {
                var pixel = originalPixels[index];
                var alpha = pixel & 0xFF000000u;

                // Skip transparent pixels
                if (alpha == 0)
                {
                    adjustedPixels[index] = pixel;
                    continue;
                }

                var r = channelLookup[(byte)(pixel >> 16)];
                var g = channelLookup[(byte)(pixel >> 8)];
                var b = channelLookup[(byte)pixel];

                adjustedPixels[index] = alpha | ((uint)r << 16) | ((uint)g << 8) | b;
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

        // The new alpha is a pure function of the old alpha byte - one 256-entry lookup table.
        var alphaLookup = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            var newAlpha = Math.Clamp(i + alphaAdjustment, 0, 255);
            alphaLookup[i] = newAlpha < transparencyThreshold ? (byte)0 : (byte)newAlpha;
        }

        unsafe
        {
            var originalPixels = (uint*)originalBitmap.GetPixels();
            var adjustedPixels = (uint*)adjustedBitmap.GetPixels();
            var pixelCount = originalBitmap.Width * originalBitmap.Height;

            for (int index = 0; index < pixelCount; index++)
            {
                var pixel = originalPixels[index];
                var a = (byte)(pixel >> 24);

                // Skip if already fully transparent
                if (a == 0)
                {
                    adjustedPixels[index] = pixel;
                    continue;
                }

                adjustedPixels[index] = (pixel & 0x00FFFFFFu) | ((uint)alphaLookup[a] << 24);
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

        // Each output channel is a pure function of total = r+g+b (0..765) - three lookup tables
        // replace the per-pixel double math.
        var totalLookupR = new byte[766];
        var totalLookupG = new byte[766];
        var totalLookupB = new byte[766];
        for (int total = 0; total < 766; total++)
        {
            totalLookupR[total] = (byte)Math.Min(255, redPercent * total / 100);
            totalLookupG[total] = (byte)Math.Min(255, greenPercent * total / 100);
            totalLookupB[total] = (byte)Math.Min(255, bluePercent * total / 100);
        }

        unsafe
        {
            var srcPixels = (uint*)originalBitmap.GetPixels();
            var dstPixels = (uint*)adjustedBitmap.GetPixels();
            var pixelCount = originalBitmap.Width * originalBitmap.Height;

            for (int i = 0; i < pixelCount; i++)
            {
                var pixel = srcPixels[i];

                var a = (byte)(pixel >> 24);
                var pr = (byte)(pixel >> 16);
                var pg = (byte)(pixel >> 8);
                var pb = (byte)pixel;

                int total = pr + pg + pb;
                if (total > 100 && a > 0)
                {
                    pr = totalLookupR[total];
                    pg = totalLookupG[total];
                    pb = totalLookupB[total];
                }

                dstPixels[i] = (uint)((a << 24) | (pr << 16) | (pg << 8) | pb);
            }
        }

        return adjustedBitmap;
    }
}
