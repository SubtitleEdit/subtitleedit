namespace Nikse.SubtitleEdit.Logic.Config;

public class SeApplyDurationLimits
{
    public bool DoNotExtendPastShotChange { get; set; }
    
    public SeApplyDurationLimits()
    {
        DoNotExtendPastShotChange = true;
    }
}