using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class SetCueTimeHelperTests
{
    private const int MinimumDurationMs = 1000;

    private static SubtitleLineViewModel NewLine(double startMs, double endMs) => new()
    {
        Text = "Hello",
        StartTime = TimeSpan.FromMilliseconds(startMs),
        EndTime = TimeSpan.FromMilliseconds(endMs),
    };

    [Fact]
    public void SetStart_BeforeEnd_KeepsEndFixed()
    {
        var line = NewLine(1000, 3000);

        SetCueTimeHelper.SetStart(line, TimeSpan.FromMilliseconds(1500), MinimumDurationMs);

        Assert.Equal(1500, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(3000, line.EndTime.TotalMilliseconds, 3);
        Assert.Equal(1500, line.Duration.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetStart_PastEnd_StampsStartAndKeepsDuration()
    {
        // #13066: the playhead is past the line's end (the normal case for a freshly
        // inserted line) - the start must still be set instead of the command doing nothing.
        var line = NewLine(1000, 3000);

        SetCueTimeHelper.SetStart(line, TimeSpan.FromMilliseconds(10000), MinimumDurationMs);

        Assert.Equal(10000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(12000, line.EndTime.TotalMilliseconds, 3);
        Assert.Equal(2000, line.Duration.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetStart_AtEnd_StampsStartAndKeepsDuration()
    {
        var line = NewLine(1000, 3000);

        SetCueTimeHelper.SetStart(line, TimeSpan.FromMilliseconds(3000), MinimumDurationMs);

        Assert.Equal(3000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(5000, line.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetStart_PastEndOfZeroDurationLine_UsesMinimumDuration()
    {
        var line = NewLine(0, 0);

        SetCueTimeHelper.SetStart(line, TimeSpan.FromMilliseconds(5000), MinimumDurationMs);

        Assert.Equal(5000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(6000, line.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetEnd_AfterStart_KeepsStartFixed()
    {
        var line = NewLine(1000, 3000);

        SetCueTimeHelper.SetEnd(line, TimeSpan.FromMilliseconds(4000), MinimumDurationMs);

        Assert.Equal(1000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(4000, line.EndTime.TotalMilliseconds, 3);
        Assert.Equal(3000, line.Duration.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetEnd_BeforeStart_StampsEndAndKeepsDuration()
    {
        var line = NewLine(10000, 12000);

        SetCueTimeHelper.SetEnd(line, TimeSpan.FromMilliseconds(5000), MinimumDurationMs);

        Assert.Equal(3000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(5000, line.EndTime.TotalMilliseconds, 3);
        Assert.Equal(2000, line.Duration.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetEnd_BeforeStart_ClampsStartAtZero()
    {
        var line = NewLine(10000, 12000);

        SetCueTimeHelper.SetEnd(line, TimeSpan.FromMilliseconds(500), MinimumDurationMs);

        Assert.Equal(0, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(500, line.EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void SetEnd_BeforeStartOfZeroDurationLine_UsesMinimumDuration()
    {
        var line = NewLine(10000, 10000);

        SetCueTimeHelper.SetEnd(line, TimeSpan.FromMilliseconds(5000), MinimumDurationMs);

        Assert.Equal(4000, line.StartTime.TotalMilliseconds, 3);
        Assert.Equal(5000, line.EndTime.TotalMilliseconds, 3);
    }
}
