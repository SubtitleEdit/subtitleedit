using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// The 23 <-> 21 auto-adjust for bottom-anchored teletext subtitles: inserting a line break in a
// bottom single-liner must move it to the two-line bottom row and back, while intentionally
// positioned subtitles keep their row.
public class TeletextRowHelperTests
{
    [Theory]
    [InlineData(1, true, 23)]
    [InlineData(2, true, 21)]
    [InlineData(3, true, 19)]
    [InlineData(1, false, 23)]
    [InlineData(2, false, 22)]
    public void GetBottomStartRow_ComputesRowFromLineCount(int lineCount, bool doubleHeight, int expected)
    {
        Assert.Equal(expected, TeletextRowHelper.GetBottomStartRow(lineCount, doubleHeight));
    }

    [Fact]
    public void GetAdjustedBottomRow_MovesBottomSingleLineToTwoLineRow()
    {
        Assert.Equal(21, TeletextRowHelper.GetAdjustedBottomRow("23", 1, 2, doubleHeight: true));
    }

    [Fact]
    public void GetAdjustedBottomRow_MovesBackWhenLineBreakIsRemoved()
    {
        Assert.Equal(23, TeletextRowHelper.GetAdjustedBottomRow("21", 2, 1, doubleHeight: true));
    }

    [Fact]
    public void GetAdjustedBottomRow_LeavesIntentionallyPositionedRowAlone()
    {
        // Row 5 is not the bottom row for a single-line subtitle, so the manual position wins.
        Assert.Null(TeletextRowHelper.GetAdjustedBottomRow("5", 1, 2, doubleHeight: true));
    }

    [Fact]
    public void GetAdjustedBottomRow_LeavesUnchangedLineCountAlone()
    {
        Assert.Null(TeletextRowHelper.GetAdjustedBottomRow("23", 1, 1, doubleHeight: true));
    }

    [Fact]
    public void GetAdjustedBottomRow_LeavesMissingRowAlone()
    {
        Assert.Null(TeletextRowHelper.GetAdjustedBottomRow(string.Empty, 1, 2, doubleHeight: true));
    }

    [Fact]
    public void GetAdjustedBottomRow_DoesNotLeaveThePage()
    {
        // 12 double-height lines would start above row 1 - leave the row untouched.
        Assert.Null(TeletextRowHelper.GetAdjustedBottomRow("23", 1, 13, doubleHeight: true));
    }

    // Batch tools (split long lines) keep the bottom edge of the text where it was, for any row.
    [Theory]
    [InlineData("21", 2, 1, true, 23)]
    [InlineData("19", 2, 1, true, 21)]
    [InlineData("20", 2, 1, true, 22)]
    [InlineData("23", 1, 2, true, 21)]
    [InlineData("21", 1, 2, true, 19)]
    [InlineData("22", 1, 2, true, 20)]
    [InlineData("23", 1, 2, false, 22)]
    public void GetRowKeepingBottomEdge_MovesStartRowByRowsGainedOrLost(string marginV, int oldLines, int newLines, bool doubleHeight, int expected)
    {
        Assert.Equal(expected, TeletextRowHelper.GetRowKeepingBottomEdge(marginV, oldLines, newLines, doubleHeight));
    }

    [Theory]
    [InlineData("23", 1, 1)] // line count unchanged
    [InlineData("", 1, 2)] // no row
    [InlineData("abc", 1, 2)] // not a row
    [InlineData("40", 1, 2)] // ASSA-style pixel margin, not a teletext row
    [InlineData("23", 2, 1)] // would leave the page at the bottom
    [InlineData("2", 1, 2)] // would leave the page at the top
    public void GetRowKeepingBottomEdge_LeavesRowAlone(string marginV, int oldLines, int newLines)
    {
        Assert.Null(TeletextRowHelper.GetRowKeepingBottomEdge(marginV, oldLines, newLines, doubleHeight: true));
    }
}
