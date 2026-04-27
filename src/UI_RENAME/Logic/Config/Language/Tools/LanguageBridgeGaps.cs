namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBridgeGaps
{
    public string Title { get; set; }
    public string BridgeGapsSmallerThan { get; set; }
    public string MinGap { get; set; }
    public string NumberOfSmallGapsBridgedX { get; set; }
    public string PercentFoPrevious { get; set; }
    public string GapChange { get; set; }

    public LanguageBridgeGaps()
    {
        Title = "Bridge gaps";
        BridgeGapsSmallerThan = "Bridge gaps smaller than (ms)";
        MinGap = "Minimum gap (ms)";
        NumberOfSmallGapsBridgedX = "Number of small gaps bridged: {0}";
        PercentFoPrevious = "Gap for previous (%)";
        GapChange = "Gap change";
    }
}