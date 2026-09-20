using Nikse.SubtitleEdit.Features.Ocr;
using SkiaSharp;

namespace UITests.Features.Ocr.Engines;

/// <summary>
/// Discussion #12929: SE5 fed Tesseract the binarized subtitle with the glyphs touching the image
/// edge and misread "in" as "In", "to" as "(o", "What" as "\What". SE4 padded the image by 10 px
/// first; the retry passes stretch it and must not invent digits.
/// </summary>
public class TesseractOcrPrepareImageTests
{
    private static SKBitmap MakeSubtitle(int width, int height)
    {
        // Grey text pixel (Blu-ray subtitles are often Y=140 grey) in the top-left corner, rest transparent.
        var bmp = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        bmp.Erase(SKColors.Transparent);
        bmp.SetPixel(0, 0, new SKColor(144, 144, 144, 255));
        return bmp;
    }

    [Fact]
    public void PrepareImage_AddsWhiteMarginAroundBlackText()
    {
        using var bmp = MakeSubtitle(20, 10);

        using var prepared = TesseractOcr.PrepareImage(bmp);

        Assert.Equal(20 + 2 * TesseractOcr.Margin, prepared.Width);
        Assert.Equal(10 + 2 * TesseractOcr.Margin, prepared.Height);
        Assert.Equal(SKColors.White, prepared.GetPixel(0, 0));
        Assert.Equal(SKColors.White, prepared.GetPixel(prepared.Width - 1, prepared.Height - 1));
        Assert.Equal(SKColors.Black, prepared.GetPixel(TesseractOcr.Margin, TesseractOcr.Margin));
    }

    [Fact]
    public void PrepareImage_StretchesAfterPadding()
    {
        using var bmp = MakeSubtitle(20, 10);

        using var prepared = TesseractOcr.PrepareImage(bmp, 3, 2);

        Assert.Equal((20 + 2 * TesseractOcr.Margin) * 3, prepared.Width);
        Assert.Equal((10 + 2 * TesseractOcr.Margin) * 2, prepared.Height);
        Assert.Equal(SKColors.White, prepared.GetPixel(0, 0));
    }

    [Fact]
    public void MergeRetryUnknownWords_TakesOnlyTheUnknownWordFromTheRetry()
    {
        var merged = TesseractOcr.MergeRetryUnknownWords(
            "In the 18 months since my mother diedq,",
            "In the 718 months since my mother died,",
            new[] { "diedq" });

        Assert.Equal("In the 18 months since my mother died,", merged);
    }

    [Fact]
    public void MergeRetryUnknownWords_KeepsKnownWordsFromTheFirstPass()
    {
        var merged = TesseractOcr.MergeRetryUnknownWords("It was lime to stop", "It was time to stap", new[] { "lime" });

        Assert.Equal("It was time to stop", merged);
    }

    [Fact]
    public void MergeRetryUnknownWords_MatchesUnknownWordsWhole_NotAsSubstrings()
    {
        // A lone "l" (misread "I") is the most common unknown word, and nearly every token contains
        // an "l" - it must only claim the token that IS "l", not "fell".
        var merged = TesseractOcr.MergeRetryUnknownWords("l fell 18 times", "I fall 718 times", new[] { "l" });

        Assert.Equal("I fell 18 times", merged);
    }

    [Theory]
    [InlineData("<i>diedq,</i>", "<i>died,</i>", "diedq", "<i>died,</i>")]
    [InlineData("didn'tq.", "didn't.", "didn'tq", "didn't.")]
    public void MergeRetryUnknownWords_MatchesThroughPunctuationAndTags(string firstPass, string retry, string unknown, string expected)
    {
        Assert.Equal(expected, TesseractOcr.MergeRetryUnknownWords(firstPass, retry, new[] { unknown }));
    }

    [Fact]
    public void MergeRetryUnknownWords_KeepsLineBreaks()
    {
        var merged = TesseractOcr.MergeRetryUnknownWords("-Yeanh.\n-You've got nothing.", "-Yeah.\n-You've got nothing.", new[] { "Yeanh" });

        Assert.Equal("-Yeah.\n-You've got nothing.", merged);
    }

    [Theory]
    [InlineData("In the 18 months since my mother diedq,", "In the 718 months since mother died,", "diedq")] // token counts differ
    [InlineData("In the 18 months since my mother died,", "In the 718 months since my mother died,", "")] // nothing unknown
    [InlineData("It was lime to stop", "It was lime to stop", "lime")] // retry identical
    public void MergeRetryUnknownWords_RefusesWhenPassesDoNotLineUp(string firstPass, string retry, string unknown)
    {
        var unknownWords = unknown.Length == 0 ? System.Array.Empty<string>() : new[] { unknown };

        Assert.Null(TesseractOcr.MergeRetryUnknownWords(firstPass, retry, unknownWords));
    }

    [Theory]
    [InlineData("In the 18 months", "In the 718 months", true)]
    [InlineData("-700 miles an houir.", "-700 miles an hour.", false)]
    [InlineData("Onh, great.", "Oh, great.", false)]
    [InlineData("", "I...", false)]
    public void RetryIntroducesDigit(string firstPass, string retry, bool expected)
    {
        Assert.Equal(expected, TesseractOcr.RetryIntroducesDigit(firstPass, retry));
    }
}
