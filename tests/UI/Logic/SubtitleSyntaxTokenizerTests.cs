using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class SubtitleSyntaxTokenizerTests
{
    private static SubtitleSyntaxTokenizer.ColoredRange RangeAt(string text, string token)
    {
        var start = text.IndexOf(token, StringComparison.Ordinal);
        Assert.True(start >= 0, $"'{token}' not found in '{text}'");
        var ranges = SubtitleSyntaxTokenizer.Tokenize(text);
        var match = ranges.Where(r => r.Start == start && r.Length == token.Length).ToList();
        Assert.True(match.Count == 1, $"expected exactly one range covering '{token}' in '{text}', got {match.Count}");
        return match[0];
    }

    [Fact]
    public void PlainTextHasNoRanges()
    {
        Assert.Empty(SubtitleSyntaxTokenizer.Tokenize("Hello world."));
    }

    [Fact]
    public void RangesAreSortedAndNonOverlapping()
    {
        var text = "{\\an8}<i>Hello</i> <font color=\"#ff0000\">world</font>{\\i1\\b1}!";
        var ranges = SubtitleSyntaxTokenizer.Tokenize(text);

        Assert.NotEmpty(ranges);
        var position = 0;
        foreach (var range in ranges)
        {
            Assert.True(range.Start >= position, $"range at {range.Start} overlaps or is out of order");
            Assert.True(range.Start + range.Length <= text.Length);
            position = range.Start + range.Length;
        }
    }

    [Fact]
    public void HtmlTagIsColored()
    {
        var range = RangeAt("<i>Hello</i>", "i");
        Assert.Equal(SubtitleSyntaxTokenizer.ElementColor, range.Color);
    }

    [Fact]
    public void AssTagNameAndValueAreColored()
    {
        var text = "{\\fs20}Hi";
        Assert.Equal(SubtitleSyntaxTokenizer.ElementColor, RangeAt(text, "fs").Color);
        Assert.Equal(SubtitleSyntaxTokenizer.ValuesColor, RangeAt(text, "20").Color);
    }

    [Fact]
    public void AssColorTagValueRendersInItsActualColor()
    {
        // &HBBGGRR& - "FF0000" is blue
        var range = RangeAt("{\\c&HFF0000&}Hi", "&HFF0000&");
        Assert.Equal(Color.FromRgb(0, 0, 255), range.Color);
    }

    [Theory]
    [InlineData("1c")]
    [InlineData("2c")]
    [InlineData("3c")]
    [InlineData("4c")]
    public void DigitPrefixedAssColorTagValueRendersInItsActualColor(string tagName)
    {
        // The digit belongs to the tag name (\1c..\4c), and the value gets the parsed
        // color - "0000FF" in &HBBGGRR& is red.
        var text = "{\\" + tagName + "&H0000FF&}Hi";
        Assert.Equal(SubtitleSyntaxTokenizer.ElementColor, RangeAt(text, tagName).Color);
        Assert.Equal(Color.FromRgb(255, 0, 0), RangeAt(text, "&H0000FF&").Color);
    }

    [Fact]
    public void DigitValueStaysOutOfTagName()
    {
        // \b1: "b" is the name, "1" the value - the digit is only part of the name when it
        // starts the tag (\1c), not when it follows one.
        var text = "{\\b1}Hi";
        Assert.Equal(SubtitleSyntaxTokenizer.ElementColor, RangeAt(text, "b").Color);
        Assert.Equal(SubtitleSyntaxTokenizer.ValuesColor, RangeAt(text, "1").Color);
    }

    [Fact]
    public void FontColorAttributeValueRendersInItsActualColor()
    {
        var range = RangeAt("<font color=\"#ff0000\">x</font>", "#ff0000");
        Assert.Equal(Color.FromRgb(255, 0, 0), range.Color);
    }

    [Fact]
    public void NamedFontColorAttributeValueRendersInItsActualColor()
    {
        var range = RangeAt("<font color=\"red\">x</font>", "red");
        Assert.Equal(Color.FromRgb(255, 0, 0), range.Color);
    }

    [Fact]
    public void HtmlCommentIsColored()
    {
        var range = RangeAt("a<!-- note -->b", "<!-- note -->");
        Assert.Equal(SubtitleSyntaxTokenizer.CommentColor, range.Color);
    }

    [Fact]
    public void UnterminatedAssTagIsNotColored()
    {
        Assert.Empty(SubtitleSyntaxTokenizer.Tokenize("{\\i1 no closing brace"));
    }
}
