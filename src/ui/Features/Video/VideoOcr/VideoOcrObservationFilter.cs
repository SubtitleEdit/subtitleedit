using Nikse.SubtitleEdit.UiLogic.Ocr.AppleVision;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>
/// Drops OCR observations whose bounding box holds (almost) no pixels above the brightness
/// minimum. Burned-in subtitles are bright by definition - that is what the brightness
/// minimum expresses - while darker scene text inside the scan area (shirt prints, opening
/// credits) is what the engines otherwise prepend to subtitles. This is the observation-level
/// counterpart of the input masking the Paddle path uses; vision engines measured better on
/// natural frames, so for them the filtering happens on the results instead.
/// </summary>
public static class VideoOcrObservationFilter
{
    // Fraction of an observation's box that must clear the brightness minimum. Subtitle
    // glyph cores cover well over a tenth of their box; sub-threshold scene text has ~0.
    private const double MinBrightFraction = 0.01;

    public static List<AppleVisionObservation> FilterByBrightness(
        IReadOnlyList<AppleVisionObservation> observations, SKBitmap bitmap, int brightnessMinimum)
    {
        if (brightnessMinimum <= 0 || observations.Count == 0)
        {
            return observations.ToList();
        }

        // No never-empty fallback here (unlike the Paddle confidence cut): every frame that
        // reaches OCR has bright pixels - the grouper classified it non-blank with the same
        // threshold - so when no observation's box covers any of them, the bright thing on
        // screen is not text, and dropping every observation is the correct answer.
        // The boxes are a small part of the frame - read them from the bitmap's own memory
        // rather than copying the whole frame out (bitmap.Pixels) for each OCR'd frame.
        var pixels = VideoOcrFrameGrouper.GetPixelBytes(bitmap, out var red, out var blue, out _);
        var width = bitmap.Width;
        var height = bitmap.Height;

        var kept = new List<AppleVisionObservation>(observations.Count);
        foreach (var observation in observations)
        {
            if (GetBrightFraction(observation, pixels, red, blue, width, height, brightnessMinimum) >= MinBrightFraction)
            {
                kept.Add(observation);
            }
        }

        return kept;
    }

    /// <summary>
    /// Fraction of the observation's box pixels at or above the brightness minimum. The box
    /// is in Vision's normalized bottom-left-origin coordinates.
    /// </summary>
    internal static double GetBrightFraction(AppleVisionObservation observation, SKColor[] pixels, int width, int height, int brightnessMinimum)
    {
        // SKColor is BGRA in memory.
        return GetBrightFraction(observation, MemoryMarshal.AsBytes(pixels.AsSpan()), 2, 0, width, height, brightnessMinimum);
    }

    /// <param name="pixels">4 bytes per pixel, green at 1, red and blue at the given offsets.</param>
    private static double GetBrightFraction(AppleVisionObservation observation, ReadOnlySpan<byte> pixels, int red, int blue, int width, int height, int brightnessMinimum)
    {
        var x0 = Math.Clamp((int)Math.Floor(observation.Left * width), 0, width - 1);
        var x1 = Math.Clamp((int)Math.Ceiling(observation.Right * width), x0 + 1, width);
        var y0 = Math.Clamp((int)Math.Floor((1.0 - Math.Max(observation.Top, observation.Bottom)) * height), 0, height - 1);
        var y1 = Math.Clamp((int)Math.Ceiling((1.0 - Math.Min(observation.Top, observation.Bottom)) * height), y0 + 1, height);

        long bright = 0;
        long total = 0;
        for (var y = y0; y < y1; y++)
        {
            var row = y * width;
            for (var x = x0; x < x1; x++)
            {
                var o = (row + x) * 4;
                if ((pixels[o + red] * 299 + pixels[o + 1] * 587 + pixels[o + blue] * 114) / 1000 >= brightnessMinimum)
                {
                    bright++;
                }

                total++;
            }
        }

        return total == 0 ? 0 : bright / (double)total;
    }
}
