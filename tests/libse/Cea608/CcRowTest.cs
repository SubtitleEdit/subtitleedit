using Nikse.SubtitleEdit.Core.Cea608;

namespace LibSETests.Cea608;

public class CcRowTest
{
    // A malformed PAC indent could push Position out of the 32-column range and index
    // Chars[-1] / Chars[32]; the setter now clamps.
    [Theory]
    [InlineData(-5, 0)]
    [InlineData(0, 0)]
    [InlineData(31, 31)]
    [InlineData(32, 31)]
    [InlineData(100, 31)]
    public void PositionIsClampedToColumnRange(int set, int expected)
    {
        var row = new CcRow { Position = set };
        Assert.Equal(expected, row.Position);
    }

    [Fact]
    public void InsertCharAtLastColumnDoesNotThrow()
    {
        var row = new CcRow { Position = 31 };
        row.InsertChar('A');
        Assert.Equal(31, row.Position);
    }

    [Fact]
    public void MoveCursorPastLastColumnClampsAndSetsPenState()
    {
        var row = new CcRow { Position = 30 };
        row.MoveCursor(10);
        Assert.Equal(31, row.Position);
    }

    [Fact]
    public void MoveCursorBeforeFirstColumnClamps()
    {
        var row = new CcRow { Position = 0 };
        row.MoveCursor(-3);
        Assert.Equal(0, row.Position);
    }
}
