using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace UITests.Logic;

public class FindServiceTests
{
    // #11956: a regex with \r\n (or \n / \r) must match a line break in the (line-feed) subtitle
    // text. Previously \r\n patterns matched nothing because the text uses \n.
    [Theory]
    [InlineData(@"ear\r\ntwice")]
    [InlineData(@"ear\ntwice")]
    [InlineData(@"ear\rtwice")]
    public void Regex_NewLineEscapes_MatchAcrossLineBreak(string pattern)
    {
        var text = "Two drops in each ear\ntwice a day.";
        var service = new FindService();
        service.Initialize([text], 0, false, FindService.FindMode.RegularExpression);

        Assert.Equal(0, service.FindNext(pattern, [text], 0, 0));
        Assert.Equal(1, service.Count(pattern, [text], false, FindService.FindMode.RegularExpression));
        Assert.Single(service.FindAll(pattern));
    }

    [Fact]
    public void RegexMultilineEndAnchorMatchesCrLfLineEndings()
    {
        var text = "First-\r\nSecond-";
        var service = new FindService();
        service.Initialize([text], 0, false, FindService.FindMode.RegularExpression);

        Assert.Equal(2, service.Count(@"(?m)-$", [text], false, FindService.FindMode.RegularExpression));

        var matches = service.FindAll(@"(?m)-$");
        Assert.Equal(2, matches.Count);
        Assert.Equal(5, matches[0].TextIndex);
        Assert.Equal(14, matches[1].TextIndex);

        Assert.Equal(0, service.FindNext(@"(?m)-$", [text], 0, 6));
        Assert.Equal(14, service.CurrentTextIndex);
    }

    // Regression: regex (^|\n).{N,}($|\n) on a two-line subtitle produces overlapping
    // matches sharing the \n boundary. FindNext must find the second line's match in full;
    // FindPrevious traversing backwards must find the second-line match before the first.
    [Fact]
    public void RegexOverlappingNewlineBoundary_FindNext_FindsSecondLineInFull()
    {
        var line1 = new string('A', 40);  // 40 chars — satisfies .{38,}
        var line2 = new string('B', 40);
        var text = $"{line1}\n{line2}";   // "AAA...\nBBB..."
        var service = new FindService();
        service.Initialize([text], 0, false, FindService.FindMode.RegularExpression);
        var pattern = @"(^|\n).{38,}($|\n)";

        // First match: starts at 0 (via ^), value = "AAA...\n"
        var idx = service.FindNext(pattern, [text], 0, 0);
        Assert.Equal(0, idx);
        var firstIndex = service.CurrentTextIndex;
        var firstLength = service.CurrentTextFound.Length;

        // Second match: searching from end of first match (SelectionEnd = firstIndex + firstLength)
        // must find "BBB..." starting at line1.Length + 1 (first char of line 2), full length.
        idx = service.FindNext(pattern, [text], 0, firstIndex + firstLength);
        Assert.Equal(0, idx);
        Assert.Equal(line1.Length + 1, service.CurrentTextIndex);
        Assert.Equal(line2, service.CurrentTextFound);
    }

    [Fact]
    public void RegexOverlappingNewlineBoundary_FindPrevious_FindsSecondLineBeforeFirst()
    {
        var line1 = new string('A', 40);
        var line2 = new string('B', 40);
        var text = $"{line1}\n{line2}";
        var service = new FindService();
        service.Initialize([text], 0, false, FindService.FindMode.RegularExpression);
        var pattern = @"(^|\n).{38,}($|\n)";

        // Approaching subtitle 0 from the end (startTextIndex = text.Length - 1):
        // must find the second-line match first (it starts later in the text).
        var idx = service.FindPrevious(pattern, [text], 0, text.Length - 1);
        Assert.Equal(0, idx);
        Assert.Equal(line1.Length, service.CurrentTextIndex);  // index of the \n before line2

        // Then from that match's SelectionStart - 1, must find the first-line match.
        idx = service.FindPrevious(pattern, [text], 0, service.CurrentTextIndex - 1);
        Assert.Equal(0, idx);
        Assert.Equal(0, service.CurrentTextIndex);
    }

    [Fact]
    public void Count_DoesNotResetFindPosition()
    {
        var lines = new List<string> { "hello world", "hello again" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive);
        service.FindNext("hello", lines, 0, 0);

        var lineBeforeCount = service.CurrentLineNumber;
        var indexBeforeCount = service.CurrentTextIndex;

        service.Count("hello", lines, false, FindService.FindMode.CaseInsensitive);

        Assert.Equal(lineBeforeCount, service.CurrentLineNumber);
        Assert.Equal(indexBeforeCount, service.CurrentTextIndex);
    }

    // #12484: replacing a line break (\n) with a space must remove the whole break, including the
    // \r of a \r\n pair. The replace path used to run the regex against the raw line, so a \n
    // pattern matched only the \n and left the \r behind, turning "Hello\r\nWorld" into
    // "Hello\r World" (a dangling line break with a space after it).
    [Theory]
    [InlineData("Hello\r\nWorld")]
    [InlineData("Hello\nWorld")]
    [InlineData("Hello\rWorld")]
    public void ReplaceAll_NewLineWithSpace_RemovesWholeBreak(string text)
    {
        var lines = new List<string> { text };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        var count = service.ReplaceAll(@"\n", " ");

        Assert.Equal(1, count);
        Assert.Equal("Hello World", lines[0]);
    }

    // The \r\n and \r escapes in a pattern match a line break too (treated as \n), so replacing
    // any of them with a space merges the two lines regardless of how the rule was written.
    [Theory]
    [InlineData(@"\r\n")]
    [InlineData(@"\r")]
    [InlineData(@"\n")]
    public void ReplaceAll_NewLineEscapeVariants_MergeCrLfLines(string pattern)
    {
        var lines = new List<string> { "Hello\r\nWorld" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        var count = service.ReplaceAll(pattern, " ");

        Assert.Equal(1, count);
        Assert.Equal("Hello World", lines[0]);
    }

    // A line with no match must be left untouched (including its original \r\n line ending),
    // and only matching lines are counted/changed.
    [Fact]
    public void ReplaceAll_NewLine_LeavesNonMatchingLinesUntouched()
    {
        var lines = new List<string> { "single line", "top\r\nbottom" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        var count = service.ReplaceAll(@"\n", " ");

        Assert.Equal(1, count);
        Assert.Equal("single line", lines[0]);
        Assert.Equal("top bottom", lines[1]);
    }
}