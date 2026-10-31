using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

// "Auto-detect ASSA alignment": the place of a subtitle image in the video frame decides the
// {\anN} tag. Shared by the OCR window and seconv's --ocr-auto-detect-assa-alignment (#15136).
public class OcrAssaAlignmentTests
{
    [Theory]
    [InlineData(0.1, 0.9, "an1")]
    [InlineData(0.5, 0.9, "an2")]
    [InlineData(0.9, 0.9, "an3")]
    [InlineData(0.1, 0.5, "an4")]
    [InlineData(0.5, 0.5, "an5")]
    [InlineData(0.9, 0.5, "an6")]
    [InlineData(0.1, 0.1, "an7")]
    [InlineData(0.5, 0.1, "an8")]
    [InlineData(0.9, 0.1, "an9")]
    public void GetAssaPositionFromScreen_MapsThreeByThreeGrid(double relativeX, double relativeY, string expected)
    {
        Assert.Equal(expected, OcrAssaAlignment.GetAssaPositionFromScreen(relativeX, relativeY));
    }

    [Fact]
    public void Detect_ImageAtTopCenter_AddsAn8()
    {
        using var bitmap = new SKBitmap(800, 100);

        var result = OcrAssaAlignment.Detect(bitmap, 560, 40, 1920, 1080, "Hello", writeAn2Tag: false);

        Assert.True(result.AlignmentAdded);
        Assert.Equal("{\\an8}Hello", result.Text);
    }

    [Fact]
    public void Detect_ImageAtBottomCenter_AddsNothing()
    {
        using var bitmap = new SKBitmap(800, 100);

        var result = OcrAssaAlignment.Detect(bitmap, 560, 940, 1920, 1080, "Hello", writeAn2Tag: false);

        Assert.False(result.AlignmentAdded);
        Assert.Equal("Hello", result.Text);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public void Detect_UnknownScreenSize_AddsNothing(int screenWidth, int screenHeight)
    {
        using var bitmap = new SKBitmap(800, 100);

        // Sources without a frame size report 0x0 or -1x-1; -1 used to score as top-left.
        var result = OcrAssaAlignment.Detect(bitmap, 560, 40, screenWidth, screenHeight, "Hello", writeAn2Tag: false);

        Assert.False(result.AlignmentAdded);
        Assert.Equal("Hello", result.Text);
    }

    [Fact]
    public void Detect_TallImageWithLineAtTopAndBottom_TagsEachLine()
    {
        // One image spanning the frame: a line of "text" at the top and one at the bottom.
        using var bitmap = new SKBitmap(800, 1000, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        bitmap.Erase(SKColors.Transparent);
        using (var canvas = new SKCanvas(bitmap))
        using (var paint = new SKPaint { Color = SKColors.White })
        {
            canvas.DrawRect(100, 20, 600, 40, paint);
            canvas.DrawRect(100, 940, 600, 40, paint);
        }

        var result = OcrAssaAlignment.Detect(bitmap, 560, 40, 1920, 1080, "Top\nBottom", writeAn2Tag: false);

        Assert.True(result.AlignmentAdded);
        Assert.Equal("{\\an8}Top\n{\\an2}Bottom", result.Text);
    }

    private static SKBitmap MakeTallImage(params int[] lineTops)
    {
        var bitmap = new SKBitmap(800, 1000, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        bitmap.Erase(SKColors.Transparent);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { Color = SKColors.White };
        foreach (var lineTop in lineTops)
        {
            canvas.DrawRect(100, lineTop, 600, 40, paint);
        }

        return bitmap;
    }

    // Texts of 40+ characters used to skip the per-line tags and land mid-screen as {\an5} (#16501).
    [Fact]
    public void Detect_TallImageWithLongTextAtTopAndBottom_TagsEachLine()
    {
        using var bitmap = MakeTallImage(20, 940);

        var result = OcrAssaAlignment.Detect(
            bitmap, 560, 40, 1920, 1080, "\"Earth Science Prep Room\"\nWe should be getting back now.", writeAn2Tag: false);

        Assert.True(result.AlignmentAdded);
        Assert.Equal("{\\an8}\"Earth Science Prep Room\"\n{\\an2}We should be getting back now.", result.Text);
    }

    // The lines used to be spread evenly over the image, which put a middle line at {\an5}.
    [Theory]
    [InlineData(new[] { 20, 880, 940 }, "A\nB\nC", "{\\an8}A\n{\\an2}B\nC")]
    [InlineData(new[] { 20, 80, 940 }, "A\nB\nC", "{\\an8}A\nB\n{\\an2}C")]
    [InlineData(new[] { 20, 80, 880, 940 }, "A\nB\nC\nD", "{\\an8}A\nB\n{\\an2}C\nD")]
    public void Detect_TallImageWithBlocksOfLines_TagsEachBlock(int[] lineTops, string text, string expected)
    {
        using var bitmap = MakeTallImage(lineTops);

        var result = OcrAssaAlignment.Detect(bitmap, 560, 40, 1920, 1080, text, writeAn2Tag: false);

        Assert.True(result.AlignmentAdded);
        Assert.Equal(expected, result.Text);
    }

    [Fact]
    public void Detect_OcrFoundMoreLinesThanTheImage_KeepsFirstAndLastInPlace()
    {
        using var bitmap = MakeTallImage(20, 940);

        var result = OcrAssaAlignment.Detect(bitmap, 560, 40, 1920, 1080, "A\nB\nC", writeAn2Tag: false);

        Assert.StartsWith("{\\an8}A", result.Text);
        Assert.EndsWith("{\\an2}C", result.Text);
    }

    // One block of many lines is one subtitle, however tall.
    [Fact]
    public void Detect_TallImageWithOneBlockOfLines_TagsItAsOne()
    {
        using var bitmap = MakeTallImage(20, 100, 180, 260, 340, 420, 500, 580);

        var result = OcrAssaAlignment.Detect(bitmap, 560, 40, 1920, 1080, "A\nB\nC\nD\nE\nF\nG\nH", writeAn2Tag: false);

        Assert.Equal("{\\an5}A\nB\nC\nD\nE\nF\nG\nH", result.Text);
    }

    // Auto-break must leave these alone: it moved words between the top and the bottom (#16501).
    [Theory]
    [InlineData(new[] { 20, 940 }, true)]
    [InlineData(new[] { 20, 80, 940 }, true)]
    [InlineData(new[] { 20, 100, 180, 260, 340, 420, 500, 580 }, false)]
    public void HasSeparatedBlocks_TallImage_GoesByTheGap(int[] lineTops, bool expected)
    {
        using var bitmap = MakeTallImage(lineTops);

        Assert.Equal(expected, OcrAssaAlignment.HasSeparatedBlocks(bitmap, 1920, 1080));
    }

    [Fact]
    public void HasSeparatedBlocks_NormalImage_IsFalse()
    {
        using var bitmap = new SKBitmap(800, 100);

        Assert.False(OcrAssaAlignment.HasSeparatedBlocks(bitmap, 1920, 1080));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public void HasSeparatedBlocks_UnknownScreenSize_IsFalse(int screenWidth, int screenHeight)
    {
        using var bitmap = MakeTallImage(20, 940);

        Assert.False(OcrAssaAlignment.HasSeparatedBlocks(bitmap, screenWidth, screenHeight));
    }

    [Fact]
    public void HasSeparatedBlocks_FrameSizedImage_GoesByTheInk()
    {
        using var bitmap = new SKBitmap(720, 576, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        bitmap.Erase(SKColors.Transparent);
        using (var canvas = new SKCanvas(bitmap))
        using (var paint = new SKPaint { Color = SKColors.White })
        {
            canvas.DrawRect(100, 30, 500, 30, paint);
            canvas.DrawRect(100, 510, 500, 30, paint);
        }

        Assert.True(OcrAssaAlignment.HasSeparatedBlocks(bitmap, 720, 576));
    }

    [Fact]
    public void SplitTextByAlignmentGroups_BlockOfLines_StaysTogether()
    {
        var groups = OcrAssaAlignment.SplitTextByAlignmentGroups("{\\an8}A\nB\n{\\an2}C");

        Assert.Equal(["{\\an8}A\nB", "{\\an2}C"], groups);
    }

    // DVB subtitles: the bitmap is the whole frame and the position is where its text is.
    // Adding the two scored every line as right/bottom ({\an3}) or right/middle ({\an6}).
    [Theory]
    [InlineData(200, 490, "Hello")]        // bottom centre
    [InlineData(200, 30, "{\\an8}Hello")]   // top centre
    [InlineData(20, 260, "{\\an4}Hello")]   // left middle, a narrow sign
    public void Detect_FrameSizedImage_GoesByTheInk(int inkLeft, int inkTop, string expected)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(720, 576, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(SKColors.Transparent);
        var inkWidth = inkLeft == 20 ? 120 : 320;
        using (var canvas = new SKCanvas(bitmap))
        using (var paint = new SKPaint { Color = SKColors.White })
        {
            canvas.DrawRect(inkLeft, inkTop, inkWidth, 40, paint);
        }

        var result = OcrAssaAlignment.Detect(bitmap, inkLeft, inkTop, 720, 576, "Hello", writeAn2Tag: false);

        Assert.Equal(expected, result.Text);
    }

    [Fact]
    public void Detect_FrameSizedImageFullOfInk_IsCentered()
    {
        using var bitmap = new SKBitmap(new SKImageInfo(720, 576, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(SKColors.White);

        var result = OcrAssaAlignment.Detect(bitmap, 0, 0, 720, 576, "Hello", writeAn2Tag: false);

        Assert.Equal("{\\an5}Hello", result.Text);
    }

    [Fact]
    public void SplitTextByAlignmentGroups_DifferentTags_SplitsPerAlignment()
    {
        var groups = OcrAssaAlignment.SplitTextByAlignmentGroups("{\\an8}Top\n{\\an2}Bottom");

        Assert.Equal(new[] { "{\\an8}Top", "{\\an2}Bottom" }, groups);
    }

    [Fact]
    public void SplitTextByAlignmentGroups_OneTag_KeepsText()
    {
        var groups = OcrAssaAlignment.SplitTextByAlignmentGroups("{\\an8}One\nTwo");

        Assert.Equal(new[] { "{\\an8}One\nTwo" }, groups);
    }
}
