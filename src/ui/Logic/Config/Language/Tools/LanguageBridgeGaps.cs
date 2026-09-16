namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBridgeGaps
{
    public string BridgeGapsSmallerThan { get; set; }
    public string MinGap { get; set; }
    public string BridgeGapsSmallerThanFrames { get; set; }
    public string MinGapFrames { get; set; }
    public string CalculateBridgeGapsSmallerThanDotDotDot { get; set; }
    public string UseXMsAsBridgeGapsSmallerThan { get; set; }
    public string NumberOfSmallGapsBridgedX { get; set; }
    public string PercentFoPrevious { get; set; }
    public string GapChange { get; set; }

    public LanguageBridgeGaps()
    {
        BridgeGapsSmallerThan = "Bridge gaps smaller than (ms)";
        MinGap = "Minimum gap (ms)";
        BridgeGapsSmallerThanFrames = "Bridge gaps smaller than (frames)";
        MinGapFrames = "Minimum gap (frames)";
        CalculateBridgeGapsSmallerThanDotDotDot = "Calculate bridge gap limit from a frame rate...";
        UseXMsAsBridgeGapsSmallerThan = "Bridge gaps smaller than \"{0}\" milliseconds?";
        NumberOfSmallGapsBridgedX = "Number of small gaps bridged: {0}";
        PercentFoPrevious = "Gap for previous (%)";
        GapChange = "Gap change";
    }
}