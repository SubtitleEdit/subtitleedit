using System;
using System.Diagnostics;
using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// The wait behind <see cref="UiTickPump"/> (#14909): on Windows a plain 16 ms sleep at the
/// default 15.6 ms timer resolution wakes at a steady 31 ms, which halved the waveform's
/// cursor/scroll tick rate whenever nothing else in the process held a 1 ms resolution.
/// </summary>
public class UiTickPumpWaiterTests
{
    [Fact]
    public void WindowsUsesAHighResolutionWaitOrRaisesTheTimerResolution()
    {
        using var waiter = UiTickPump.TickWaiter.Create();
        if (OperatingSystem.IsWindows())
        {
            Assert.NotEqual("sleep", waiter.Kind);
        }
        else
        {
            Assert.Equal("sleep", waiter.Kind);
        }
    }

    [Fact]
    public void ASixteenMillisecondWaitDoesNotRoundUpToTheNextTimerInterrupt()
    {
        using var waiter = UiTickPump.TickWaiter.Create();
        waiter.Wait(16); // warm-up

        const int rounds = 30;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < rounds; i++)
        {
            waiter.Wait(16);
        }

        var averageMs = sw.Elapsed.TotalMilliseconds / rounds;

        // A sleep quantized to the 15.625 ms interrupt lands at 31.25 ms per wait; a correctly
        // paced wait sits at 16-17 ms. The bound leaves room for a loaded CI runner.
        Assert.True(averageMs < 26, $"average wait {averageMs:0.0} ms - the wait is being rounded up to the timer interrupt ({waiter.Kind})");
    }
}
