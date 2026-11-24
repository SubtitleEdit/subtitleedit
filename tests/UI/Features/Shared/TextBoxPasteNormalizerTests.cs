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

    [Fact]
    public void NormalizeBreakChars_ConvertsUnknownBreaks()
    {
        Assert.Equal("a\nb\nc\nd\ne", TextBoxPasteNormalizer.NormalizeBreakChars("a\u2029b\u0085c\vd\fe"));
    }
}
