using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

public class SubtitleLineViewModelTimeTests
{
    private static SubtitleLineViewModel NewLine(double startMs, double endMs) => new()
    {
        Text = "Hello",
        StartTime = TimeSpan.FromMilliseconds(startMs),
        EndTime = TimeSpan.FromMilliseconds(endMs),
    };

    [Fact]
    public void StartTime_PlainSetter_KeepsEndFixed_AndChangesDuration()
    {
        var line = NewLine(1000, 3000);

        line.StartTime = TimeSpan.FromMilliseconds(1500);

        Assert.Equal(1500, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(3000, line.EndTime.TotalMilliseconds, 3); // unchanged
        Assert.Equal(1500, line.Duration.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetStartTimeOnly_KeepsEndFixed()
    {
        var line = NewLine(1000, 3000);

        line.SetStartTimeOnly(TimeSpan.FromMilliseconds(1500));

        Assert.Equal(1500, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(3000, line.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetStartTimeKeepDuration_ShiftsEndByTheSameAmount()
    {
        var line = NewLine(1000, 3000);

        line.SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(1500));

        Assert.Equal(1500, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(3500, line.EndTime.TotalMilliseconds, 3); // moved by +500
        Assert.Equal(2000, line.Duration.TotalMilliseconds, 3); // preserved
    }

    [Fact]
    public void SetStartTimeKeepDuration_NegativeShift_PreservesDuration()
    {
        var line = NewLine(5000, 7000);

        line.SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(2000));

        Assert.Equal(2000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(4000, line.EndTime.TotalMilliseconds, 3);
        Assert.Equal(2000, line.Duration.TotalMilliseconds, 3);
    }

    [Fact]
    public void StartTimeKeepDurationProperty_DelegatesToMethod()
    {
        // The property exists for the start-time editor binding; assigning it
        // must behave the same as SetStartTimeKeepDuration (move the whole line).
        var line = NewLine(1000, 3000);

        line.StartTimeKeepDuration = TimeSpan.FromMilliseconds(1500);

        Assert.Equal(1500, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(3500, line.EndTime.TotalMilliseconds, 3);
        Assert.Equal(2000, line.Duration.TotalMilliseconds, 3);
    }
}
