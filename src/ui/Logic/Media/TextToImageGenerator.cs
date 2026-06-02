using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class TextToImageGenerator
{
    public static SKBitmap GenerateImage(
        string text,
        string fontName,
        float fontSize,
        bool isBold,
        SKColor textColor,
        SKColor outlineColor,
        SKColor shadowColor,
        SKColor backgroundColor,
        float outlineWidth,
        float shadowWidth,
        float kerning,
        float cornerRadius = 1.0f// Parameter for rounded corners
        )
    {
        if (kerning == 0)
        {
            return GenerateImage(text, fontName, fontSize, isBold, textColor, outlineColor, shadowColor, backgroundColor, outlineWidth, shadowWidth, cornerRadius);
        }

        // Measure the total width of the text
        float xPosition = 0;
        using var typeface = SKTypeface.FromFamilyName(fontName, isBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        var paint = new SKPaint
        {
            Color = textColor,
            IsAntialias = true,
        };

        // Create a new SKBitmap with sufficient dimensions
        using var tempPaint = paint.Clone();
        SKRect textBounds = new SKRect();
        var font = new SKFont
        {
            Typeface = typeface,
            Size = fontSize,
            Edging = SKFontEdging.SubpixelAntialias,
            Subpixel = true,
            BaselineSnap = true,
            Hinting = SKFontHinting.Full,
        };
        font.MeasureText(text, out textBounds);

        //float baseline = -textBounds.Top;
        font.GetFontMetrics(out var metrics);
        float baseline = -metrics.Ascent;
        //tempPaint.MeasureText(text, ref textBounds);

        var bitmapWidth = (int)(textBounds.Width + kerning * text.Length);
        var bitmapHeight = (int)(textBounds.Height + -metrics.Ascent);

        var spaceWidth = font.MeasureText(".");

        var bitmap = new SKBitmap(bitmapWidth, bitmapHeight);
        using (var canvas = new SKCanvas(bitmap))
        {
            // Clear the canvas
            canvas.Clear(SKColors.Transparent);

            // Iterate over each glyph in the text
            foreach (var glyph in text)
            {
                if (glyph == ' ')
                {
                    xPosition += spaceWidth + kerning;
                    continue;
                }

                string singleGlyph = glyph.ToString();

                // Measure the width of the current glyph
                //float glyphWidth = tempPaint.MeasureText(singleGlyph);
                font.MeasureText(singleGlyph, out textBounds);
                var glyphWidth = textBounds.Width;

                // Draw the glyph at the current position
                canvas.DrawText(singleGlyph, xPosition, baseline, SKTextAlign.Left, font, paint);

                // Update the position for the next glyph
                xPosition += glyphWidth + kerning;
            }
        }

        return bitmap;
    }

    public static SKBitmap GenerateImageWithPadding(
     string text,
     string fontName,
     float fontSize,
     bool isBold,
     SKColor textColor,
     SKColor outlineColor,
     SKColor shadowColor,
     SKColor backgroundColor,
     float outlineWidth,
     float shadowWidth,
     float cornerRadius = 1.0f, // Parameter for rounded corners
     int padding = 20,
     bool isItalic = false,
     bool isUnderline = false,
     bool isStrikeout = false
 )
    {
        outlineWidth *= 1.7f; // factor to match ASSA

        using var typeface = SKTypeface.FromFamilyName(fontName, isBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, isItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
        using var paint = new SKPaint
        {
            IsAntialias = true
        };

        using var font = new SKFont(typeface, fontSize);
        if (isItalic)
        {
            font.SkewX = -0.25f; // synthetic slant so italic shows even when the font has no italic face
        }

        font.MeasureText(text, out var textBounds, paint);

        var width = (int)(textBounds.Width + padding * 2 + outlineWidth * 2 + shadowWidth);
        var height = (int)(textBounds.Height + padding * 2 + outlineWidth * 2 + shadowWidth);

        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(backgroundColor);

        var x = padding + outlineWidth;
        var y = height - padding - outlineWidth;

        // Create a path for the text
        using var textPath = font.GetTextPath(text, new SKPoint(x, y));

        // Draw shadow (includes both text and outline with rounded corners)
        if (shadowWidth > 0)
        {
            DrawTextWithRoundedOutline(x + shadowWidth, y + shadowWidth, shadowColor, shadowColor, textPath, x, y, cornerRadius, outlineWidth, paint, canvas);
        }

        // Draw main text with rounded outline
        DrawTextWithRoundedOutline(x, y, textColor, outlineColor, textPath, x, y, cornerRadius, outlineWidth, paint, canvas);

        DrawUnderlineAndStrikeout(canvas, font, textColor, x, y, textBounds.Width, fontSize, isUnderline, isStrikeout);

        return bitmap;
    }

    private static void DrawUnderlineAndStrikeout(SKCanvas canvas, SKFont font, SKColor color, float x, float baselineY, float textWidth, float fontSize, bool isUnderline, bool isStrikeout)
    {
        if (!isUnderline && !isStrikeout)
        {
            return;
        }

        var metrics = font.Metrics;
        using var linePaint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true };

        if (isUnderline)
        {
            var thickness = metrics.UnderlineThickness ?? Math.Max(1f, fontSize / 14f);
            var position = metrics.UnderlinePosition ?? fontSize * 0.1f;
            canvas.DrawRect(x, baselineY + position, textWidth, thickness, linePaint);
        }

        if (isStrikeout)
        {
            var thickness = metrics.StrikeoutThickness ?? Math.Max(1f, fontSize / 14f);
            var position = metrics.StrikeoutPosition ?? -fontSize * 0.3f;
            canvas.DrawRect(x, baselineY + position, textWidth, thickness, linePaint);
        }
    }

    /// <summary>
    /// Places a generated text bitmap onto a video-frame-proportioned canvas, positioned by the
    /// CEA/ASSA numpad alignment (1-9) and the margins, so the preview reflects the style layout.
    /// </summary>
    public static SKBitmap ComposeOnPreviewFrame(SKBitmap textBitmap, int alignment, int marginLeft, int marginRight, int marginVertical)
    {
        var frameWidth = Math.Max(640, textBitmap.Width + 160);
        var frameHeight = Math.Max((int)Math.Round(frameWidth * 9.0 / 16.0), textBitmap.Height + 60);

        var frame = new SKBitmap(frameWidth, frameHeight);
        using var canvas = new SKCanvas(frame);
        canvas.Clear(new SKColor(40, 40, 40)); // represent the video frame
        using (var border = new SKPaint { Color = new SKColor(90, 90, 90), Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
        {
            canvas.DrawRect(1, 1, frameWidth - 2, frameHeight - 2, border);
        }

        // numpad alignment: 1/4/7 left, 2/5/8 center, 3/6/9 right; 1/2/3 bottom, 4/5/6 middle, 7/8/9 top
        var isLeft = alignment == 1 || alignment == 4 || alignment == 7;
        var isRight = alignment == 3 || alignment == 6 || alignment == 9;
        var isTop = alignment >= 7;
        var isMiddle = alignment >= 4 && alignment <= 6;

        float x;
        if (isLeft)
        {
            x = marginLeft;
        }
        else if (isRight)
        {
            x = frameWidth - textBitmap.Width - marginRight;
        }
        else
        {
            x = (frameWidth - textBitmap.Width) / 2f;
        }

        float yy;
        if (isTop)
        {
            yy = marginVertical;
        }
        else if (isMiddle)
        {
            yy = (frameHeight - textBitmap.Height) / 2f;
        }
        else
        {
            yy = frameHeight - textBitmap.Height - marginVertical;
        }

        x = Math.Max(0, Math.Min(x, frameWidth - textBitmap.Width));
        yy = Math.Max(0, Math.Min(yy, frameHeight - textBitmap.Height));

        canvas.DrawBitmap(textBitmap, x, yy);
        return frame;
    }

    private static void DrawTextWithRoundedOutline(float xPos, float yPos, SKColor fillColor, SKColor strokeColor, SKPath textPath, float x, float y, float cornerRadius, float outlineWidth, SKPaint paint, SKCanvas canvas)
    {
        // Translate the path to the current position
        using var translatedPath = new SKPath();
        textPath.Transform(SKMatrix.CreateTranslation(xPos - x, yPos - y), translatedPath);

        // Set the path effect for rounded corners
        using var pathEffect = SKPathEffect.CreateCorner(cornerRadius);

        // Draw the outline
        if (outlineWidth > 0)
        {
            paint.Color = strokeColor;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = outlineWidth;
            paint.PathEffect = pathEffect; // Apply rounded corners to the paint
            canvas.DrawPath(translatedPath, paint);
        }

        // Draw the fill
        paint.Color = fillColor;
        paint.Style = SKPaintStyle.Fill;
        paint.PathEffect = null; // Remove path effect for the fill
        canvas.DrawPath(translatedPath, paint);
    }

    public static SKBitmap AddShadowToBitmap(SKBitmap originalBitmap, int shadowWidth, SKColor shadowColor)
    {
        var offset = 2;

        // Calculate new dimensions
        var newWidth = originalBitmap.Width + shadowWidth + offset;
        var newHeight = originalBitmap.Height + shadowWidth + offset;

        // Create a new bitmap with increased size
        using var surface = SKSurface.Create(new SKImageInfo(newWidth, newHeight));
        var canvas = surface.Canvas;

        // Clear the canvas with transparent color
        canvas.Clear(SKColors.Transparent);

        // Draw the shadow
        using (var paint = new SKPaint
        {
            Color = shadowColor,
            Style = SKPaintStyle.Fill
        })
        {
            // Draw bottom shadow
            canvas.DrawRect(0 + offset, originalBitmap.Height, newWidth, shadowWidth, paint);

            // Draw right shadow
            canvas.DrawRect(originalBitmap.Width, 0 + offset, shadowWidth + offset, originalBitmap.Height, paint);
        }

        // Draw the original bitmap
        canvas.DrawBitmap(originalBitmap, 0, 0);

        // Create a new bitmap from the surface
        using (var image = surface.Snapshot())
        {
            return SKBitmap.FromImage(image);
        }
    }
}
