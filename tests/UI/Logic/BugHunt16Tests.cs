using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Guard tests for the 2026-08-27 bug hunt (sweep 16): a syntax highlighter that threw out of
/// Render on half-typed markup, and containers offered for a codec that cannot go in them.
/// </summary>
public class BugHunt16Tests
{
    [Theory]
    // The opening quote as the very last character: IndexOf found no closing quote, valueEnd
    // landed on Length, and the colon search was handed a count of -1.
    [InlineData("<font color=\"")]
    [InlineData("Hello <font color=\"")]
    [InlineData("<font face=\"")]
    [InlineData("<font color=\"#ff0000\">ok</font>")]
    [InlineData("<i>plain</i>")]
    [InlineData("")]
    public void SubRipHighlighter_HalfTypedAttribute_DoesNotThrow(string line)
    {
        var highlighter = new SubRipSourceSyntaxHighlighting();
        var styler = new SourceSyntaxLineStyler();

        var exception = Record.Exception(() => highlighter.HighlightLine(line, styler));

        Assert.Null(exception);
    }

    [Fact]
    public void SubRipHighlighter_UnterminatedQuoteMidLine_DoesNotThrow()
    {
        var highlighter = new SubRipSourceSyntaxHighlighting();
        var styler = new SourceSyntaxLineStyler();

        Assert.Null(Record.Exception(() => highlighter.HighlightLine("<font color=\"red>text", styler)));
    }
}
