using Nikse.SubtitleEdit.Features.Tools.ApplyMinGap;
using Xunit;

namespace UITests.Features.Tools.ApplyMinGap;

public class ApplyMinGapViewModelTests
{
    [Theory]
    [InlineData(200.0, 200.0, false)]
    [InlineData(199.0, 200.0, false)]
    [InlineData(195.0, 200.0, false)]
    [InlineData(190.0, 200.0, false)]
    [InlineData(189.9, 200.0, true)]
    [InlineData(180.0, 200.0, true)]
    public void NeedsGapAdjustment_UsesTenMillisecondTolerance(
        double currentGapMs,
        double minimumGapMs,
        bool expected)
    {
        var actual = ApplyMinGapViewModel.NeedsGapAdjustment(
            currentGapMs,
            minimumGapMs);

        Assert.Equal(expected, actual);
    }
}
