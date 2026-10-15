using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Image preparation for the vision-model OCR engines (llama.cpp and Ollama, UI and seconv).
/// The subtitle is centered in a black square with a 20% margin - GLM-OCR drops line breaks
/// without the square (#16031). The models do not downscale, so a 1280x240 Video OCR crop becomes
/// a 1536 square of ~3000 image tokens; an optional cap on the square's side shrinks that (#16043).
/// </summary>
public static class AiOcrImagePadding
{
    /// <param name="source">The subtitle image.</param>
    /// <param name="maxSide">Largest side of the returned square in pixels; 0 or less = no cap.</param>
    public static SKBitmap PadToSquare(SKBitmap source, int maxSide)
    {
        var longest = Math.Max(source.Width, source.Height);
        var margin = (int)(longest * 0.2);
        var side = longest + margin;
        var scale = 1f;
        if (maxSide > 0 && side > maxSide)
        {
            scale = maxSide / (float)side;
            side = maxSide;
        }

        var info = new SKImageInfo(side, side, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var square = new SKBitmap(info);
        using var canvas = new SKCanvas(square);
        canvas.Clear(SKColors.Black);
        using var image = SKImage.FromBitmap(source);
        var width = source.Width * scale;
        var height = source.Height * scale;
        var left = (side - width) / 2f;
        var top = (side - height) / 2f;

        // Mipmaps keep thin glyph strokes when shrinking by more than half.
        var sampling = scale < 1f
            ? new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)
            : new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
        canvas.DrawImage(image, new SKRect(left, top, left + width, top + height), sampling, null);
        canvas.Flush();
        return square;
    }
}
