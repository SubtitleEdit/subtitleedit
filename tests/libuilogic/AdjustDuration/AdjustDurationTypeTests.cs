using Nikse.SubtitleEdit.UiLogic.AdjustDuration;

namespace LibUiLogicTests.AdjustDuration;

public class AdjustDurationTypeTests
{
    [Fact]
    public void Enum_HasFourMembers_InExpectedOrder()
    {
        Assert.Equal(0, (int)AdjustDurationType.Seconds);
        Assert.Equal(1, (int)AdjustDurationType.Percent);
        Assert.Equal(2, (int)AdjustDurationType.Fixed);
        Assert.Equal(3, (int)AdjustDurationType.Recalculate);
    }
}
