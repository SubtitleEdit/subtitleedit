namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageSplitBreakLongLines
{
    public string Title { get; set; }
    public string SplitLongLines { get; set; }
    public string RebalanceLongLines { get; set; }
    public string SplitLongLine { get; set; }
    public string RebalanceLongLine { get; set; }
    public string SplitIntoXLines { get; set; }
    public string LinesSplitX { get; set; }
    public string LinesSplitXLinesRebalancedY { get; set; }

    public LanguageSplitBreakLongLines()
    {
        Title = "Split/rebalance long lines";
        SplitLongLines = "Split long lines (to multiple lines)";
        RebalanceLongLines = "Rebalance long lines";
        SplitLongLine = "Split long line";
        RebalanceLongLine = "Rebalance long line";
        SplitIntoXLines = "Split into {0} lines: '{1}' → '{2}...'";
        LinesSplitX = "Lines split: {0}";
        LinesSplitXLinesRebalancedY = "Lines split: {0}, lines rebalanced: {1}";
    }
}