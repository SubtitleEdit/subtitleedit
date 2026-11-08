using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

public class ImageBaselineFinderTests
{
    private const int GlyphWidth = 10;
    private const int GlyphGap = 6;
    private const int XHeight = 14;
    private const int Descender = 6;
    private const int Outline = 3;

    /// <summary>
    /// One line of white block "glyphs" with a dark outline around the image. Each entry is the
    /// glyph's (top offset, height) relative to the x-height top.
    /// </summary>
    private static SKBitmap MakeLine(params (int Top, int Height)[] glyphs)
    {
        var width = glyphs.Length * (GlyphWidth + GlyphGap) + Outline * 2;
        var height = XHeight + Descender + Outline * 2;
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        bitmap.Erase(SKColors.Transparent);
        using var canvas = new SKCanvas(bitmap);
        using var outline = new SKPaint { Color = SKColors.Black };
        using var fill = new SKPaint { Color = SKColors.White };
        for (var i = 0; i < glyphs.Length; i++)
        {
            var left = Outline + i * (GlyphWidth + GlyphGap);
            var top = Outline + glyphs[i].Top;
            canvas.DrawRect(left - Outline, top - Outline, GlyphWidth + Outline * 2, glyphs[i].Height + Outline * 2, outline);
            canvas.DrawRect(left, top, GlyphWidth, glyphs[i].Height, fill);
        }

        return bitmap;
    }

    private static readonly (int, int) Letter = (0, XHeight);
    private static readonly (int, int) DescenderLetter = (0, XHeight + Descender);
    private static readonly (int, int) Comma = (XHeight - 2, 5);

    [Fact]
    public void NoDescender_ReturnsZero()
    {
        using var bitmap = MakeLine(Letter, Letter, Letter);
        Assert.Equal(0, ImageBaselineFinder.GetDescent(bitmap, 12));
    }

    [Fact]
    public void OneDescender_ReturnsDescenderDepth()
    {
        using var bitmap = MakeLine(DescenderLetter, Letter, Letter, Letter);
        Assert.Equal(Descender, ImageBaselineFinder.GetDescent(bitmap, 12));
    }

    [Fact]
    public void MostlyDescenders_StillFindsBaseline()
    {
        // "gypsy": a median of the glyph bottoms would land on the descenders.
        using var bitmap = MakeLine(DescenderLetter, DescenderLetter, DescenderLetter, Letter, DescenderLetter);
        Assert.Equal(Descender, ImageBaselineFinder.GetDescent(bitmap, 12));
    }

    [Fact]
    public void Comma_CountsAsHangingBelow()
    {
        using var bitmap = MakeLine(Letter, Letter, Comma);
        Assert.Equal(3, ImageBaselineFinder.GetDescent(bitmap, 12));
    }

    [Fact]
    public void OnlyDescenders_ReturnsZero()
    {
        using var bitmap = MakeLine(DescenderLetter);
        Assert.Equal(0, ImageBaselineFinder.GetDescent(bitmap, 12));
    }

    [Fact]
    public void RenderedText_DescenderLineHangsLower()
    {
        using var abc = RenderText("abc");
        using var jabc = RenderText("jabc");

        Assert.Equal(0, ImageBaselineFinder.GetDescent(abc, 12));
        var descent = ImageBaselineFinder.GetDescent(jabc, 12);
        Assert.InRange(descent, 6, 16);
    }

    [Fact]
    public void RenderedText_TwoLines_UsesLastLine()
    {
        using var bitmap = RenderText("jabc", "abc");
        Assert.Equal(0, ImageBaselineFinder.GetDescent(bitmap, 12));
    }

    private static SKBitmap RenderText(params string[] lines)
    {
        using var typeface = SKTypeface.FromFamilyName("Arial") ?? SKTypeface.Default;
        using var font = new SKFont(typeface, 48);
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        var lineHeight = 64;
        var bitmap = new SKBitmap(400, lineHeight * lines.Length + 10, SKColorType.Bgra8888, SKAlphaType.Premul);
        bitmap.Erase(SKColors.Transparent);
        using var canvas = new SKCanvas(bitmap);
        for (var i = 0; i < lines.Length; i++)
        {
            canvas.DrawText(lines[i], 10, 48 + i * lineHeight, font, paint);
        }

        return bitmap;
    }
}
