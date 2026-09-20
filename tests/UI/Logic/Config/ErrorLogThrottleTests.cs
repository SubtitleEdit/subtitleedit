using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class ErrorLogThrottleTests
{
    private const string Error = "Cannot access a disposed object.\nLibMpvDynamicPlayer method called after disposal";

    [Fact]
    public void ATimerFiringTheSameError_IsCappedPerWindow()
    {
        var throttle = new ErrorLogThrottle();
        var written = 0;

        // One minute of a 16 ms timer.
        for (var ms = 0L; ms < ErrorLogThrottle.WindowMilliseconds; ms += 16)
        {
            if (throttle.ShouldLog(Error, ms, out _, out _))
            {
                written++;
            }
        }

        Assert.Equal(ErrorLogThrottle.MaxEntriesPerWindow, written);
    }

    [Fact]
    public void TheLastEntryOfAWindow_IsFlagged()
    {
        var throttle = new ErrorLogThrottle();

        for (var i = 1; i <= ErrorLogThrottle.MaxEntriesPerWindow; i++)
        {
            Assert.True(throttle.ShouldLog(Error, i, out _, out var isLast));
            Assert.Equal(i == ErrorLogThrottle.MaxEntriesPerWindow, isLast);
        }
    }

    [Fact]
    public void TheNextWindow_LogsAgain_AndReportsWhatWasDropped()
    {
        var throttle = new ErrorLogThrottle();
        const int calls = 100;
        for (var i = 0; i < calls; i++)
        {
            throttle.ShouldLog(Error, i, out _, out _);
        }

        Assert.True(throttle.ShouldLog(Error, ErrorLogThrottle.WindowMilliseconds, out var suppressedBefore, out _));
        Assert.Equal(calls - ErrorLogThrottle.MaxEntriesPerWindow, suppressedBefore);

        // Reported once, not on every entry that follows.
        Assert.True(throttle.ShouldLog(Error, ErrorLogThrottle.WindowMilliseconds + 1, out suppressedBefore, out _));
        Assert.Equal(0, suppressedBefore);
    }

    [Fact]
    public void DifferentErrors_DoNotThrottleEachOther()
    {
        var throttle = new ErrorLogThrottle();
        for (var i = 0; i < 100; i++)
        {
            throttle.ShouldLog(Error, i, out _, out _);
        }

        Assert.True(throttle.ShouldLog("Another error", 100, out _, out _));
    }

    [Fact]
    public void ManyDistinctErrors_AreAllLogged_AndTheFloodStaysCapped()
    {
        var throttle = new ErrorLogThrottle();
        for (var i = 0; i < ErrorLogThrottle.MaxEntriesPerWindow; i++)
        {
            throttle.ShouldLog(Error, 1000, out _, out _);
        }

        // More distinct errors than are tracked: the oldest are forgotten, the newer flood is not.
        for (var i = 0; i < ErrorLogThrottle.MaxTrackedErrors * 2; i++)
        {
            Assert.True(throttle.ShouldLog("Error " + i, i, out _, out _));
        }

        Assert.False(throttle.ShouldLog(Error, 1001, out _, out _));
    }
}
