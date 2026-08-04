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

    // #13053: in translator mode the original text column must be searched too - SE 4 searched
    // both columns, SE 5 only looked at the main text.
    [Fact]
    public void FindNext_FindsMatchInOriginalTextOnly()
    {
        var lines = new List<string> { "Paráda.", "- No tak, no tak." };
        var originals = new List<string> { "Great.", "- Come on, come on." };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive, originals);

        var idx = service.FindNext("come", lines, 0, 0, originals);

        Assert.Equal(1, idx);
        Assert.True(service.CurrentMatchInOriginal);
        Assert.Equal(2, service.CurrentTextIndex);
    }

    // Within a line the main text is searched before the original text.
    [Fact]
    public void FindNext_SearchesMainTextBeforeOriginalText()
    {
        var lines = new List<string> { "hello there" };
        var originals = new List<string> { "hello again" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive, originals);

        var idx = service.FindNext("hello", lines, 0, 0, originals);
        Assert.Equal(0, idx);
        Assert.False(service.CurrentMatchInOriginal);

        // Continue from the match in the main text - the next hit is in the original text
        // of the same line.
        idx = service.FindNext("hello", lines, 0, service.CurrentTextIndex + 5, originals, false);
        Assert.Equal(0, idx);
        Assert.True(service.CurrentMatchInOriginal);
        Assert.Equal(0, service.CurrentTextIndex);
    }

    // Resuming from a match in the original column must not find the same line's main text again.
    [Fact]
    public void FindNext_ResumingFromOriginal_SkipsMainTextOfSameLine()
    {
        var lines = new List<string> { "hello there", "nothing here" };
        var originals = new List<string> { "hello again", "hello once more" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive, originals);

        var idx = service.FindNext("hello", lines, 0, 1, originals, true);

        Assert.Equal(1, idx);
        Assert.True(service.CurrentMatchInOriginal);
    }

    // Backwards the columns are visited in reverse order: original text first, then main text.
    [Fact]
    public void FindPrevious_SearchesOriginalTextBeforeMainText()
    {
        var lines = new List<string> { "hello there", "no match here" };
        var originals = new List<string> { "hello again", "no match either" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive, originals);

        var idx = service.FindPrevious("hello", lines, 1, lines[1].Length - 1, originals);
        Assert.Equal(0, idx);
        Assert.True(service.CurrentMatchInOriginal);

        idx = service.FindPrevious("hello", lines, 0, service.CurrentTextIndex - 1, originals, true);
        Assert.Equal(0, idx);
        Assert.False(service.CurrentMatchInOriginal);
    }

    [Fact]
    public void Count_IncludesOriginalTexts()
    {
        var lines = new List<string> { "hello world", "nothing" };
        var originals = new List<string> { "hello again", "hello once more" };
        var service = new FindService();

        var count = service.Count("hello", lines, false, FindService.FindMode.CaseInsensitive, originals);

        Assert.Equal(3, count);
    }

    [Fact]
    public void ReplaceAll_ReplacesInOriginalTexts()
    {
        var lines = new List<string> { "hello world" };
        var originals = new List<string> { "hello again" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive, originals);

        var count = service.ReplaceAll("hello", "hi");

        Assert.Equal(2, count);
        Assert.Equal("hi world", lines[0]);
        Assert.Equal("hi again", originals[0]);
    }

    // No original subtitle loaded - nothing changes for the plain single-column case.
    [Fact]
    public void FindNext_WithoutOriginals_NeverReportsMatchInOriginal()
    {
        var lines = new List<string> { "hello world" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive);

        var idx = service.FindNext("world", lines, 0, 0);

        Assert.Equal(0, idx);
        Assert.False(service.CurrentMatchInOriginal);
    }
}