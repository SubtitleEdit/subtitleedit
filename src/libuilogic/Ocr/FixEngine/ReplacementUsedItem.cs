namespace Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

public class ReplacementUsedItem
{
    public string From { get; set; }
    public string To { get; set; }
    public int LineIndex { get; set; }

    public ReplacementUsedItem()
    {
        From = string.Empty;
        To = string.Empty;
        LineIndex = -1;
    }

    public ReplacementUsedItem(string from, string to, int lineLineIndex)
    {
        From = from;
        To = to;
        LineIndex = lineLineIndex;
    }

    public override string ToString()
    {
        var from = From.Replace("\r\n", "·").Replace("\n", "·");
        var to = To.Replace("\r\n", "·").Replace("\n", "·");
        return $"#{LineIndex + 1}: {from} → {to}";
    }
}
