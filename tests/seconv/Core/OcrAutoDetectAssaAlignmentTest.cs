using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using SkiaSharp;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// --ocr-auto-detect-assa-alignment (#15136): the recognised text gets the ASSA alignment tag
/// for where its image sits in the video frame, like the OCR window's "Auto-detect ASSA
/// alignment". Tested below the OCR engine, so it runs with none installed.
/// </summary>
public class OcrAutoDetectAssaAlignmentTest
{
    private static readonly SKSizeI FullHd = new(1920, 1080);

    [Fact]
    public void AddWithAlignment_ImageAtTop_AddsAn8()
    {
        var subtitle = new Subtitle();
        using var bitmap = new SKBitmap(800, 100);

        ImageOcrLoader.AddWithAlignment(
            subtitle, "Hello", 1000, 3000, () => bitmap, callerOwnsBitmap: true, new SKPointI(560, 40), FullHd);

        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("{\\an8}Hello", paragraph.Text);
        Assert.Equal(1000, paragraph.StartTime.TotalMilliseconds);
        Assert.Equal(3000, paragraph.EndTime.TotalMilliseconds);
    }

    [Fact]
    public void AddWithAlignment_ImageAtBottomCenter_AddsNoTag()
    {
        var subtitle = new Subtitle();
        using var bitmap = new SKBitmap(800, 100);

        ImageOcrLoader.AddWithAlignment(
            subtitle, "Hello", 1000, 3000, () => bitmap, callerOwnsBitmap: true, new SKPointI(560, 940), FullHd);

        Assert.Equal("Hello", Assert.Single(subtitle.Paragraphs).Text);
    }

    [Fact]
    public void AddWithAlignment_UnknownFrame_KeepsTextAndDoesNotDecode()
    {
        var subtitle = new Subtitle();
        var decoded = false;

        ImageOcrLoader.AddWithAlignment(
            subtitle, "Hello", 1000, 3000, () => { decoded = true; return null; }, callerOwnsBitmap: false,
            new SKPointI(560, 40), default);

        Assert.Equal("Hello", Assert.Single(subtitle.Paragraphs).Text);
        Assert.False(decoded);
    }

    [Fact]
    public void AddWithAlignment_LinesAtTopAndBottom_BecomeTwoParagraphs()
    {
        var subtitle = new Subtitle();
        using var bitmap = new SKBitmap(800, 1000, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        bitmap.Erase(SKColors.Transparent);
        using (var canvas = new SKCanvas(bitmap))
        using (var paint = new SKPaint { Color = SKColors.White })
        {
            canvas.DrawRect(100, 20, 600, 40, paint);
            canvas.DrawRect(100, 940, 600, 40, paint);
        }

        ImageOcrLoader.AddWithAlignment(
            subtitle, "Top" + Environment.NewLine + "Bottom", 1000, 3000, () => bitmap, callerOwnsBitmap: true,
            new SKPointI(560, 40), FullHd);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("{\\an8}Top", subtitle.Paragraphs[0].Text);
        Assert.Equal("{\\an2}Bottom", subtitle.Paragraphs[1].Text);
        Assert.All(subtitle.Paragraphs, p => Assert.Equal(1000, p.StartTime.TotalMilliseconds));
    }

    [Fact]
    public void AddWithAlignment_FreshBitmap_IsDisposed()
    {
        var subtitle = new Subtitle();
        var bitmap = new SKBitmap(800, 100);

        ImageOcrLoader.AddWithAlignment(
            subtitle, "Hello", 1000, 3000, () => bitmap, callerOwnsBitmap: false, new SKPointI(560, 40), FullHd);

        Assert.Equal(IntPtr.Zero, bitmap.Handle);
    }
}
