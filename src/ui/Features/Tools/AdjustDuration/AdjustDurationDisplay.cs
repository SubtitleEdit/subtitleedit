using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

public class AdjustDurationDisplay
{
    public string Name { get; set; }
    public AdjustDurationType Type { get; set; }
    public bool IsSecondsVisible { get; set; }
    public bool IsPercentVisible { get; set; }
    public bool IsFixedVisible { get; set; }
    public bool IsRecalculateVisible { get; set; }
    
    public AdjustDurationDisplay()
    {
        Name = string.Empty;
        Type = AdjustDurationType.Seconds;
        IsSecondsVisible = true;
    }

    public override string ToString()
    {
        return Name; 
    }

    public static List<AdjustDurationDisplay> ListAll()
    {
        return
        [
            new AdjustDurationDisplay
            {
                Name = "Seconds",
                Type = AdjustDurationType.Seconds,
                IsSecondsVisible = true,
                IsPercentVisible = false,
                IsFixedVisible = false,
                IsRecalculateVisible = false
            },
            new AdjustDurationDisplay
            {
                Name = "Percent",
                Type = AdjustDurationType.Percent,
                IsSecondsVisible = false,
                IsPercentVisible = true,
                IsFixedVisible = false,
                IsRecalculateVisible = false
            },
            new AdjustDurationDisplay
            {
                Name = "Fixed",
                Type = AdjustDurationType.Fixed,
                IsSecondsVisible = false,
                IsPercentVisible = false,
                IsFixedVisible = true,
                IsRecalculateVisible = false
            },
            new AdjustDurationDisplay
            {
                Name = "Recalculate",
                Type = AdjustDurationType.Recalculate,
                IsSecondsVisible = false,
                IsPercentVisible = false,
                IsFixedVisible = false,
                IsRecalculateVisible = true
            }
        ];
    }
}