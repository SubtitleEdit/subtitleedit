using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAdjustDisplayDurations
{
    public double AdjustDurationSeconds { get; set; }
    public double AdjustDurationFixed { get; set; }
    public int AdjustDurationPercent { get; set; } 
    public string AdjustDurationLast { get; set; } 
    public bool AdjustDurationExtendOnly { get; set; }
    public bool AdjustDurationExtendEnforceDurationLimits { get; set; }
    public bool AdjustDurationExtendCheckShotChanges { get; set; } 
    public double AdjustDurationMaximumCps { get; set; }
    public double AdjustDurationOptimalCps { get; set; }

    public SeAdjustDisplayDurations()
    {
        AdjustDurationSeconds = 0.1;
        AdjustDurationFixed = 3;
        AdjustDurationPercent = 120;
        AdjustDurationOptimalCps = 17;
        AdjustDurationMaximumCps = 25;
        AdjustDurationLast = AdjustDurationType.Seconds.ToString();
    }
}