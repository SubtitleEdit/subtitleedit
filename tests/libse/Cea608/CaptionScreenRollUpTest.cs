using Nikse.SubtitleEdit.Core.Cea608;

namespace LibSETests.Cea608;

public class CaptionScreenRollUpTest
{
    private static void WriteRow(CaptionScreen screen, int row, string text)
    {
        screen.CurrentRow = row;
        foreach (var c in text)
        {
            screen.InsertChar(c);
        }
    }

    private static string RowText(CaptionScreen screen, int row)
    {
        var chars = screen.Rows[row].Chars;
        var s = string.Empty;
        foreach (var c in chars)
        {
            s += c.Uchar;
        }

        return s.TrimEnd();
    }

    [Fact]
    public void RollUpAtTheBottomRowScrollsTheWindow()
    {
        var screen = new CaptionScreen();
        screen.SetRollUpRows(2);
        WriteRow(screen, 13, "one");
        WriteRow(screen, 14, "two");

        screen.RollUp();

        Assert.Equal("two", RowText(screen, 13));
        Assert.True(screen.Rows[14].IsEmpty());
    }

    // A PAC can place the roll-up base row anywhere on screen. Only the roll-up window may move -
    // appending the cleared row at the bottom scrolled every row below the window as well.
    [Fact]
    public void RollUpAboveTheBottomRowLeavesTheRowsBelowAlone()
    {
        var screen = new CaptionScreen();
        screen.SetRollUpRows(2);
        WriteRow(screen, 10, "one");
        WriteRow(screen, 11, "two");
        WriteRow(screen, 14, "bottom");

        screen.CurrentRow = 11;
        screen.RollUp();

        Assert.Equal("two", RowText(screen, 10));
        Assert.True(screen.Rows[11].IsEmpty());
        Assert.Equal("bottom", RowText(screen, 14));
        Assert.Equal(15, screen.Rows.Length);
    }
}
