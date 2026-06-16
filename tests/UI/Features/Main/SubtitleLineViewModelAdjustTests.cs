using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

public class SubtitleLineViewModelAdjustTests
{
    [Fact]
    public void Adjust_ScalesBothStartAndEnd()
    {
        var line = new SubtitleLineViewModel
        {
            Text = "Hello",
            StartTime = TimeSpan.FromMilliseconds(1000),
            EndTime = TimeSpan.FromMilliseconds(3000),
        };

        line.Adjust(2.0, 0.0);

        Assert.Equal(2000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(6000, line.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void Adjust_AppliesOffsetInSeconds()
    {
        var line = new SubtitleLineViewModel
        {
            Text = "Hello",
            StartTime = TimeSpan.FromMilliseconds(1000),
            EndTime = TimeSpan.FromMilliseconds(3000),
        };

        line.Adjust(1.0, 1.5); // shift by 1.5 seconds

        Assert.Equal(2500, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(4500, line.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void Adjust_GrowingFactorKeepsDurationConsistentAndEndAfterStart()
    {
        // Growing the timeline pushes the new start past the old end. Updating start
        // and end separately briefly exposed start > end to the bound editor, which
        // clamped the negative duration and corrupted the end time. Verify the end
        // stays correct (after start) and the duration matches end - start.
        var line = new SubtitleLineViewModel
        {
            Text = "Hello",
            StartTime = TimeSpan.FromMilliseconds(1000),
            EndTime = TimeSpan.FromMilliseconds(1500),
        };

        line.Adjust(3.0, 0.0);

        Assert.Equal(3000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(4500, line.EndTime.TotalMilliseconds, 3);
        Assert.True(line.EndTime > line.StartTime);
        Assert.Equal((line.EndTime - line.StartTime).TotalMilliseconds, line.Duration.TotalMilliseconds, 3);
    }
}
