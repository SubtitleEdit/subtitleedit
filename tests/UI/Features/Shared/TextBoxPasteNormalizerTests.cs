using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

namespace UITests.Features.Shared;

/// <summary>
/// Text copied from a PDF keeps the PDF's line wrapping, often with no space at the breaks, so a
/// paste into the subtitle text box is joined and auto-broken again (#17137).
/// </summary>
public class TextBoxPasteNormalizerTests
{
    private const string PdfText = "This is a sentence copied from a PDF\nfile that keeps the line breaks of\nthe page it was copied from.";

    [Fact]
    public void LooksHardWrapped_PdfParagraph_True()
    {
        Assert.True(TextBoxPasteNormalizer.LooksHardWrapped(PdfText, 2));
    }

    [Fact]
    public void LooksHardWrapped_PdfParagraphCrLf_True()
    {
        Assert.True(TextBoxPasteNormalizer.LooksHardWrapped(PdfText.Replace("\n", "\r\n"), 2));
    }

    [Fact]
    public void LooksHardWrapped_ParagraphSeparator_True()
    {
        Assert.True(TextBoxPasteNormalizer.LooksHardWrapped("one line\u2029two line\u2029three line", 2));
    }

    [Theory]
    [InlineData("Hello there.\nHow are you?")] // a normal two-line subtitle
    [InlineData("One line only")]
    [InlineData("-Hi.\n-Hello.\n-How are you?")] // dialog
    [InlineData("<i>-Hi.</i>\n<i>-Hello.</i>\n<i>-Bye.</i>")] // dialog in tags
    [InlineData("First paragraph\n\nSecond paragraph\nmore")] // blank line
    [InlineData("1\n00:00:01,000 --> 00:00:02,000\nHello")] // SubRip
    public void LooksHardWrapped_SubtitleText_False(string text)
    {
        Assert.False(TextBoxPasteNormalizer.LooksHardWrapped(text, 2));
    }

    [Fact]
    public void LooksHardWrapped_ThreeLinesWithLimitThree_False()
    {
        Assert.False(TextBoxPasteNormalizer.LooksHardWrapped("a b\nc d\ne f", 3));
    }

    [Fact]
    public void Compose_EmptyBox_JoinsAndAutoBreaks()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak(string.Empty, 0, 0, PdfText, "en");

        Assert.DoesNotContain("PDF\nfile", result.Replace("\r\n", "\n"));
        Assert.Contains("PDF file", result.Replace(Environment.NewLine, " "));
        Assert.Contains("of the page", result.Replace(Environment.NewLine, " "));
    }

    [Fact]
    public void Compose_AtCaretInWord_KeepsSpaces()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak("Hello world", 5, 5, "big\nwide", "en");

        Assert.Equal("Hello big wide world", result);
    }

    [Fact]
    public void Compose_ReplacesSelection()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak("Hello cruel world", 6, 11, "kind\nand\nwarm", "en");

        Assert.Equal("Hello kind and warm world", result);
    }

    [Fact]
    public void Compose_ReversedSelection_ReplacesSelection()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak("Hello cruel world", 11, 6, "nice", "en");

        Assert.Equal("Hello nice world", result);
    }

    [Fact]
    public void Compose_WhitespaceOnlyPaste_LeavesTextAlone()
    {
        Assert.Equal("Hello", TextBoxPasteNormalizer.ComposePasteAndAutoBreak("Hello", 5, 5, " \n ", "en"));
    }

    private const string JapanesePdfText = "今日はとても暑いので、\n外に出かけるのはやめて\n家で映画を見ることにしました。";

    [Fact]
    public void Compose_Japanese_JoinsWithoutSpaces()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak(string.Empty, 0, 0, JapanesePdfText, "ja");

        Assert.DoesNotContain(" ", result);
        Assert.Equal("今日はとても暑いので、外に出かけるのはやめて家で映画を見ることにしました。", result.Replace(Environment.NewLine, string.Empty));
    }

    [Fact]
    public void Compose_JapaneseAfterJapaneseText_NoSpaceAtTheJoin()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak("前の文。", 4, 4, JapanesePdfText, "ja");

        Assert.DoesNotContain(" ", result);
        Assert.StartsWith("前の文。今日は", result);
    }

    [Fact]
    public void Compose_Chinese_JoinsWithoutSpaces()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak(string.Empty, 0, 0, "我们今天去公园\n散步，然后去\n吃晚饭。", "zh");

        Assert.Equal("我们今天去公园散步，然后去吃晚饭。", result.Replace(Environment.NewLine, string.Empty));
    }

    [Fact]
    public void Compose_Thai_JoinsWithoutSpaces()
    {
        var result = TextBoxPasteNormalizer.ComposePasteAndAutoBreak(string.Empty, 0, 0, "วันนี้อากาศ\nร้อนมาก\nจริงๆ", "th");

        Assert.Equal("วันนี้อากาศร้อนมากจริงๆ", result.Replace(Environment.NewLine, string.Empty));
    }

    [Theory]
    [InlineData("big\nwide\nworld", "big wide world")]
    [InlineData("big \n wide", "big wide")]
    [InlineData("日本\n語", "日本語")]
    [InlineData("日本 \n 語", "日本語")]
    [InlineData("Tokyo\n東京", "Tokyo東京")]
    [InlineData("나 이제\n가야 해.", "나 이제 가야 해.")] // Korean is written with spaces
    [InlineData("<i>big</i>\n<i>wide</i>", "<i>big wide</i>")]
    public void UnbreakLine_SpacesOnlyBetweenSpacedScripts(string input, string expected)
    {
        Assert.Equal(expected, TextBoxPasteNormalizer.UnbreakLine(input));
    }

    [Fact]
    public void NormalizeBreakChars_ConvertsUnknownBreaks()
    {
        Assert.Equal("a\nb\nc\nd\ne", TextBoxPasteNormalizer.NormalizeBreakChars("a\u2029b\u0085c\vd\fe"));
    }
}
