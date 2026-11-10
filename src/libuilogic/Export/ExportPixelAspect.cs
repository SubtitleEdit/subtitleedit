using System.Globalization;
using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Sample (pixel) aspect ratio of an SD frame. DVD and SD Blu-ray store 720x576 (PAL) or
/// 720x480 (NTSC) with non-square pixels, and the player stretches each pixel by the SAR
/// when it shows the frame. A subtitle drawn with square pixels would then come out too wide
/// (16:9, PAL 4:3) or too narrow (NTSC 4:3), so the finished bitmap is pre-scaled
/// horizontally by 1/SAR - after the player applies the SAR the glyphs have their normal
/// proportions. Values are the "full 720 width" ones DVD players and ffmpeg use.
/// </summary>
public static class ExportPixelAspect
{
    /// <summary>PAL 16:9: 720x576 shown as 1024x576.</summary>
    public const string Pal16x9 = "64:45";

    /// <summary>PAL 4:3: 720x576 shown as 768x576.</summary>
    public const string Pal4x3 = "16:15";

    /// <summary>NTSC 16:9: 720x480 shown as 853.3x480.</summary>
    public const string Ntsc16x9 = "32:27";

    /// <summary>NTSC 4:3: 720x480 shown as 640x480.</summary>
    public const string Ntsc4x3 = "8:9";

    /// <summary>
    /// "64:45" (or "64/45", or a plain number) as a ratio; 1 (square pixels) for an empty,
    /// unreadable or out-of-range value.
    /// </summary>
    public static double Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 1;
        }

        var parts = value.Split(':', '/');
        double ratio;
        if (parts.Length == 2 &&
            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var numerator) &&
            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var denominator) &&
            denominator > 0)
        {
            ratio = numerator / denominator;
        }
        else if (parts.Length != 1 || !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out ratio))
        {
            return 1;
        }

        return ratio is >= 0.25 and <= 4 ? ratio : 1;
    }

    /// <summary>
    /// <paramref name="bitmap"/> scaled horizontally by 1/<paramref name="pixelAspectRatio"/>,
    /// or the same bitmap when the pixels are (nearly) square.
    /// </summary>
    public static SKBitmap Apply(SKBitmap bitmap, double pixelAspectRatio)
    {
        if (pixelAspectRatio <= 0 || Math.Abs(pixelAspectRatio - 1) < 0.001 || bitmap.Width <= 1)
        {
            return bitmap;
        }

        var width = Math.Max(1, (int)Math.Round(bitmap.Width / pixelAspectRatio, MidpointRounding.AwayFromZero));
        if (width == bitmap.Width)
        {
            return bitmap;
        }

        var scaled = new SKBitmap(width, bitmap.Height, bitmap.ColorType, bitmap.AlphaType);
        using var canvas = new SKCanvas(scaled);
        using var image = SKImage.FromBitmap(bitmap);
        using var paint = new SKPaint { IsAntialias = true };
        canvas.Clear(SKColors.Transparent);
        canvas.DrawImage(
            image,
            new SKRect(0, 0, bitmap.Width, bitmap.Height),
            new SKRect(0, 0, width, bitmap.Height),
            new SKSamplingOptions(SKCubicResampler.Mitchell),
            paint);
        return scaled;
    }
}
