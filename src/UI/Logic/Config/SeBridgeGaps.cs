namespace Nikse.SubtitleEdit.Logic.Config;

public class SeBridgeGaps
{
    public int BridgeGapsSmallerThanMs { get; set; }
    public int MinGapMs { get; set; }
    public int PercentForLeft { get; set; } 

    public SeBridgeGaps()
    {
        BridgeGapsSmallerThanMs = 2000;
        MinGapMs = 24;
        PercentForLeft = 100;
    }
}