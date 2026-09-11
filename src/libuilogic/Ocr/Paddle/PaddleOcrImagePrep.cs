using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

/// <summary>How a subtitle bitmap is prepared before PaddleOCR sees it.</summary>
public static class PaddleOcrImagePrep
{
    /// <summary>Border width the OCR window and seconv both pad with.</summary>
    public const int DefaultBorderSize = 10;

    /// <summary>File name a batch input gets inside the input folder.</summary>
    public static string InputFileName(int index) => index.ToString("0000") + ".png";

    /// <summary>The stem PaddleOCR derives its result file name from, for <see cref="InputFileName"/>.</summary>
    public static string InputStem(int index) => index.ToString("0000");

    /// <summary>
    /// Pads the bitmap with an inner border of <paramref name="innerColor"/> inside an outer
    /// border of <paramref name="outerColor"/>. Subtitle bitmaps are text that runs to the very
    /// edge of the image; the detector wants margin around a text box, and without it the first
    /// and last glyphs of a line are regularly clipped or missed entirely.
    /// </summary>
    public static SKBitmap CreateDoubleBorder(
        SKBitmap source, int borderSize, SKColor innerColor, SKColor outerColor)
    {
        var totalBorder = borderSize * 2;
        var finalWidth = source.Width + totalBorder * 2;
        var finalHeight = source.Height + totalBorder * 2;

        var result = new SKBitmap(finalWidth, finalHeight);
        using var canvas = new SKCanvas(result);

        canvas.Clear(outerColor);

        using var paint = new SKPaint { Color = innerColor };
        canvas.DrawRect(borderSize, borderSize, finalWidth - borderSize * 2, finalHeight - borderSize * 2, paint);

        canvas.DrawBitmap(source, totalBorder, totalBorder);

        return result;
    }

    /// <summary>
    /// The standard preparation: a black inner border on transparent, as both OCR paths use.
    /// </summary>
    public static SKBitmap PrepareForOcr(SKBitmap source) =>
        CreateDoubleBorder(source, DefaultBorderSize, SKColors.Black, new SKColor(0, 0, 0, 0));

    /// <summary>
    /// Writes a blank prepared image. A batch numbers its inputs densely so a result file maps
    /// straight back to its line, so an index whose bitmap could not be decoded still needs a
    /// file - it just has nothing on it.
    /// </summary>
    public static void WriteBlankPng(string path)
    {
        using var blank = new SKBitmap(1, 1, true);
        WritePreparedPng(blank, path);
    }

    /// <summary>Writes a prepared bitmap to <paramref name="path"/> as PNG.</summary>
    public static void WritePreparedPng(SKBitmap source, string path)
    {
        using var prepared = PrepareForOcr(source);
        using var image = SKImage.FromBitmap(prepared);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }
}
