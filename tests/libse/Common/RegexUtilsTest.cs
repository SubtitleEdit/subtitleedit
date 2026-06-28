using Nikse.SubtitleEdit.Core.Common;
using System.Text.RegularExpressions;
using Xunit;

namespace LibSETests.Common;

public class RegexUtilsTest
{
    // #11956: \r\n, \n and \r in a regex pattern all normalize to a single line-feed, so a cross-line
    // rule matches the line-feed text v5 uses, regardless of which escape the user typed.
    [Theory]
    [InlineData(@"ear\r\ntwice", "ear\ntwice")]
    [InlineData(@"ear\ntwice", "ear\ntwice")]
    [InlineData(@"ear\rtwice", "ear\ntwice")]
    public void FixNewLine_NormalizesEscapesToLineFeed(string pattern, string expected)
    {
        Assert.Equal(expected, RegexUtils.FixNewLine(pattern));
    }

    [Fact]
    public void ReplaceNewLineSafe_CrLfPattern_MatchesLineFeedText()
    {
        var regex = new Regex(RegexUtils.FixNewLine(@"ear\r\ntwice"));
        var result = RegexUtils.ReplaceNewLineSafe(regex, "each ear\ntwice a day", "ear twice");
        Assert.Equal("each ear twice a day", result);
    }

    [Fact]
    public void ReplaceNewLineSafe_LineFeedPattern_MatchesCrLfText()
    {
        var regex = new Regex(RegexUtils.FixNewLine(@"ear\ntwice"));
        var result = RegexUtils.ReplaceNewLineSafe(regex, "each ear\r\ntwice a day", "ear twice");
        Assert.Equal("each ear twice a day", result);
    }

    [Fact]
    public void CountNewLineSafe_CountsMatchAcrossLineBreak()
    {
        var regex = new Regex(RegexUtils.FixNewLine(@"a\r\nb"));
        Assert.Equal(1, RegexUtils.CountNewLineSafe(regex, "xa\nbx"));
    }
}
