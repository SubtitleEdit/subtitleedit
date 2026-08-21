using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Globalization;

namespace Tests.Logic;

/// <summary>
/// Guards for the round-8 fast paths on the per-repaint / per-keystroke UI paths: each test
/// sits on the condition the new early return depends on.
/// </summary>
public class HotPathFastPathTests
{
    [Theory]
    [InlineData("")]
    [InlineData("Plain text with no markup at all.")]
    [InlineData("a > b, and 5 = 5")]           // '>' and '=' alone are not a tag
    [InlineData("closing } brace only")]
    [InlineData("stray { brace")]           // a brace is only a tag when a backslash follows
    [InlineData("50% of 3 - 1")]
    public void TokenizeReturnsNoRangesForTextWithoutMarkup(string text)
    {
        Assert.Empty(SubtitleSyntaxTokenizer.Tokenize(text));
    }

    [Theory]
    [InlineData("<i>italic</i>")]
    [InlineData("{" + "\\" + "an8}positioned")]
    [InlineData("stray < bracket")]
    public void TokenizeStillRunsWhenMarkupIsPresent(string text)
    {
        // Not asserting the ranges themselves - only that the fast path does not swallow input
        // the full walk has something to say about. A stray "<" colors itself as a tag start.
        Assert.NotEmpty(SubtitleSyntaxTokenizer.Tokenize(text));
    }

    [Fact]
    public void SingleLineConverterJoinsEveryLineBreakVariant()
    {
        var converter = new TextToSingleLineConverter();

        var crlf = (string)converter.Convert("a\r\nb", typeof(string), null, CultureInfo.InvariantCulture);
        var lf = (string)converter.Convert("a\nb", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.DoesNotContain("\n", crlf);
        Assert.DoesNotContain("\n", lf);
    }

    [Fact]
    public void SingleLineConverterLeavesSingleLineTextAlone()
    {
        var converter = new TextToSingleLineConverter();
        const string text = "One line, nothing to join.";

        Assert.Equal(text, converter.Convert(text, typeof(string), null, CultureInfo.InvariantCulture));
    }
}
