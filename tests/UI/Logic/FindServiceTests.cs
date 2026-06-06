using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class FindServiceTests
{
    [Fact]
    public void RegexMultilineEndAnchorMatchesCrLfLineEndings()
    {
        var text = "First-\r\nSecond-";
        var service = new FindService();
        service.Initialize([text], 0, false, FindService.FindMode.RegularExpression);

        Assert.Equal(2, service.Count(@"(?m)-$"));

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
}