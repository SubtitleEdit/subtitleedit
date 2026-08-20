namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Row math for the EBU STL teletext workflow (1-based rows, row 23 is the bottom row).
/// </summary>
public static class TeletextRowHelper
{
    public const int BottomRow = 23;

    /// <summary>
    /// The teletext row a bottom-anchored subtitle with the given number of text lines starts on:
    /// with double height, 23 for one line and 21 for two.
    /// </summary>
    public static int GetBottomStartRow(int lineCount, bool doubleHeight)
    {
        var rowsPerLine = doubleHeight ? 2 : 1;
        return BottomRow - rowsPerLine * (lineCount - 1);
    }

    /// <summary>
    /// When a text edit changes the number of lines of a bottom-anchored subtitle, returns the row
    /// that keeps it bottom-anchored (23 &lt;-&gt; 21 with double height). Returns null when the
    /// row must be left alone: the subtitle was not on the bottom row for its previous line count
    /// (i.e. it was intentionally positioned), the line count did not change, or the new row would
    /// leave the page.
    /// </summary>
    public static int? GetAdjustedBottomRow(string marginV, int oldLineCount, int newLineCount, bool doubleHeight)
    {
        if (oldLineCount == newLineCount || oldLineCount < 1 || newLineCount < 1)
        {
            return null;
        }

        if (!int.TryParse(marginV, out var row) || row != GetBottomStartRow(oldLineCount, doubleHeight))
        {
            return null;
        }

        var newRow = GetBottomStartRow(newLineCount, doubleHeight);
        return newRow >= 1 ? newRow : null;
    }
}
