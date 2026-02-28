using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustDuration;

public class BinaryAdjustDurationDisplay
{
    public string Name { get; set; }
    public BinaryAdjustDurationType Type { get; set; }
    public bool IsSecondsVisible { get; set; }
    public bool IsPercentVisible { get; set; }
    public bool IsFixedVisible { get; set; }
    public bool IsRecalculateVisible { get; set; }
    
    public BinaryAdjustDurationDisplay()
    {
        Name = string.Empty;
        Type = BinaryAdjustDurationType.Seconds;
        IsSecondsVisible = true;
    }

    public override string ToString()
    {
        return Name; 
    }

    public static List<BinaryAdjustDurationDisplay> ListAll()
    {
        return
        [
            new BinaryAdjustDurationDisplay
            {
                Name = "Seconds",
                Type = BinaryAdjustDurationType.Seconds,
                IsSecondsVisible = true,
                IsPercentVisible = false,
                IsFixedVisible = false,
                IsRecalculateVisible = false
            },
            new BinaryAdjustDurationDisplay
            {
                Name = "Percent",
                Type = BinaryAdjustDurationType.Percent,
                IsSecondsVisible = false,
                IsPercentVisible = true,
                IsFixedVisible = false,
                IsRecalculateVisible = false
            },
            new BinaryAdjustDurationDisplay
            {
                Name = "Fixed",
                Type = BinaryAdjustDurationType.Fixed,
                IsSecondsVisible = false,
                IsPercentVisible = false,
                IsFixedVisible = true,
                IsRecalculateVisible = false
            },
            new BinaryAdjustDurationDisplay
            {
                Name = "Recalculate",
                Type = BinaryAdjustDurationType.Recalculate,
                IsSecondsVisible = false,
                IsPercentVisible = false,
                IsFixedVisible = false,
                IsRecalculateVisible = true
            }
        ];
    }
}
