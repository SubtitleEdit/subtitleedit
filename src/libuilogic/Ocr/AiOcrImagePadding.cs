using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// How a vision-model OCR engine wants the subtitle image framed.
/// </summary>
public enum AiOcrPadding
{
    /// <summary>Centered in a black square with a 20% margin - GLM-OCR drops line breaks without it (#16031).</summary>
    Square,

    /// <summary>A thin black border that keeps the aspect ratio - far fewer image tokens on wide strips.</summary>
    Border,
}

/// <summary>
/// Image preparation for the llama.cpp vision-model OCR engine (UI and seconv). The models do
/// not downscale, so the square turns a 1280x240 Video OCR crop into a 1536x1536 image of
/// ~3000 image tokens, while a border keeps it near 1300x260.
/// </summary>
public static class AiOcrImagePadding
{
    public static SKBitmap Pad(SKBitmap source, AiOcrPadding padding)
    {
        return padding == AiOcrPadding.Border
            ? PadWithBorder(source)
            : PadToSquare(source);
    }

    public static SKBitmap PadToSquare(SKBitmap source)
    {
        var longest = Math.Max(source.Width, source.Height);
        var side = longest + (int)(longest * 0.2);
        return Draw(source, side, side);
    }

    /// <summary>
    /// A black border of 10% of the image height (at least 10 px) on every side.
    /// </summary>
    public static SKBitmap PadWithBorder(SKBitmap source)
    {
        var border = Math.Max(10, (int)(source.Height * 0.1));
        return Draw(source, source.Width + 2 * border, source.Height + 2 * border);
    }

    private static SKBitmap Draw(SKBitmap source, int width, int height)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var padded = new SKBitmap(info);
        using var canvas = new SKCanvas(padded);
        canvas.Clear(SKColors.Black);
        using var image = SKImage.FromBitmap(source);
        var left = (width - source.Width) / 2f;
        var top = (height - source.Height) / 2f;
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
        canvas.DrawImage(image, new SKRect(left, top, left + source.Width, top + source.Height), sampling, null);
        canvas.Flush();
        return padded;
    }
}
