using SkiaSharp;

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
     int padding = 20 
 )
    {
        outlineWidth *= 1.7f; // factor to match ASSA

        using var typeface = SKTypeface.FromFamilyName(fontName, isBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var paint = new SKPaint
        {
            IsAntialias = true
        };

        using var font = new SKFont(typeface, fontSize);
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

        return bitmap;
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
