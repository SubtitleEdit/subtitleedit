using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustDuration;
using System.Collections.Generic;

namespace UITests.Features.Shared.BinaryEdit;

public class BinaryAdjustDurationViewModelTests
{
    [Fact]
    public void AdjustDuration_Percent_SetsDurationToPercentageLikeTheMainDialog()
    {
        // The binary edit dialog shares its saved percent value with the main "Adjust
        // durations" dialog, so both must interpret it the same way: the duration is SET
        // to the percentage of the original (this used to ADD it, doubling up).
        var vm = new BinaryAdjustDurationViewModel
        {
            SelectedAdjustType = new BinaryAdjustDurationDisplay { Type = BinaryAdjustDurationType.Percent },
            AdjustPercent = 120,
        };
        var items = new List<BinarySubtitleItem>
        {
            new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)),
        };

        vm.AdjustDuration(items);

        // 2 s * 120% = 2.4 s => end at 3.4 s
        Assert.Equal(TimeSpan.FromMilliseconds(3400), items[0].EndTime);
    }

    [Fact]
    public void AdjustDuration_NegativeSeconds_DoesNotPushEndBeforeStart()
    {
        var vm = new BinaryAdjustDurationViewModel
        {
            SelectedAdjustType = new BinaryAdjustDurationDisplay { Type = BinaryAdjustDurationType.Seconds },
            AdjustSeconds = -10,
        };
        var items = new List<BinarySubtitleItem>
        {
            new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)),
        };

        vm.AdjustDuration(items);

        // Clamped to a 100 ms minimum duration instead of ending before it starts
        Assert.Equal(TimeSpan.FromMilliseconds(1100), items[0].EndTime);
    }
}
