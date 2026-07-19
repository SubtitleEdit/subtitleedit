using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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

    // #12620: the returned text must use Environment.NewLine like all other paragraph text - the
    // format writers rely on it (ASSA folds breaks via Replace(Environment.NewLine, "\\N")), so a
    // bare \n left behind by the match normalization was written as a physical newline inside the
    // Dialogue line on Windows, corrupting the saved file. The reporter's repro: an ellipsis
    // replacement on one line of a two-line ASSA paragraph.
    [Fact]
    public void ReplaceNewLineSafe_CrLfText_ReturnsPlatformNewLines()
    {
        var regex = new Regex(RegexUtils.FixNewLine(@"(?:…|\.{4,})"));
        var result = RegexUtils.ReplaceNewLineSafe(regex, "First line…\r\nSecond line", "...");
        Assert.Equal("First line..." + Environment.NewLine + "Second line", result);
    }

    [Fact]
    public void ReplaceNewLineSafe_UntouchedLineBreakInLineFeedText_ReturnsPlatformNewLines()
    {
        var regex = new Regex(RegexUtils.FixNewLine(@"\.{4,}"));
        var result = RegexUtils.ReplaceNewLineSafe(regex, "First line....\nSecond line", "...");
        Assert.Equal("First line..." + Environment.NewLine + "Second line", result);
    }

    // A replacement that introduces a line break (rule splits one line into two) must come out
    // with a platform newline too - the fast path applies here since neither input contains \r.
    [Fact]
    public void ReplaceNewLineSafe_ReplacementIntroducesLineBreak_ReturnsPlatformNewLines()
    {
        var regex = new Regex(RegexUtils.FixNewLine(@" - "));
        var result = RegexUtils.ReplaceNewLineSafe(regex, "First part - second part", RegexUtils.FixNewLine(@"\n"));
        Assert.Equal("First part" + Environment.NewLine + "second part", result);
    }

    [Fact]
    public void ReplaceNewLineSafe_NoLineBreaks_ReturnsTextUnchanged()
    {
        var regex = new Regex("b+");
        var result = RegexUtils.ReplaceNewLineSafe(regex, "abc", "B");
        Assert.Equal("aBc", result);
    }

    // The reporter's end-to-end repro from #12620: replace an ellipsis on one line of a two-line
    // ASSA paragraph, then serialize - the line break must come out as \N, not as a physical
    // newline inside the Dialogue line. ASSA folds breaks via Replace(Environment.NewLine, "\\N"),
    // so this only holds when ReplaceNewLineSafe returns platform newlines.
    [Fact]
    public void ReplaceNewLineSafe_AssaRoundTrip_KeepsBackslashN()
    {
        var regex = new Regex(RegexUtils.FixNewLine(@"(?:…|\.{4,})"));
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First line…" + Environment.NewLine + "Second line", 1000, 4000));

        subtitle.Paragraphs[0].Text = RegexUtils.ReplaceNewLineSafe(regex, subtitle.Paragraphs[0].Text, "...");
        var assa = new AdvancedSubStationAlpha().ToText(subtitle, "title");

        Assert.Contains("First line...\\NSecond line", assa);
    }
}
