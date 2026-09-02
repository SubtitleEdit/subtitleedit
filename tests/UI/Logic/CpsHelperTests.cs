using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Issue #14418: "Recalculate duration" put a 20-character line at 1333 ms for a 15 CPS target,
/// which is 15.004 CPS, and the edit box then showed "15.0" in red while the grid row for the
/// same line was not flagged. CPS-derived durations round up to the whole millisecond, and every
/// CPS readout compares the same two-decimal value.
/// </summary>
public class CpsHelperTests
{
    [Fact]
    public void GetDurationForCps_TwentyCharsAtFifteenCps_RoundsUpToStayWithinLimit()
    {
        var duration = CpsHelper.GetDurationForCps(20, 15);

        Assert.Equal(1334, duration.TotalMilliseconds);
        Assert.True(20 / duration.TotalSeconds <= 15);
    }

    [Fact]
    public void GetDurationForCps_ExactDivision_DoesNotAddAMillisecond()
    {
        // 3 / 10 * 1000 is 300.00000000000006 in binary - a plain Math.Ceiling would give 301.
        Assert.Equal(300, CpsHelper.GetDurationForCps(3, 10).TotalMilliseconds);
        Assert.Equal(2000, CpsHelper.GetDurationForCps(30, 15).TotalMilliseconds);
    }

    [Fact]
    public void GetDurationForCps_IsTheShortestWholeMillisecondWithinTheLimit()
    {
        foreach (var cps in new[] { 9.0, 12.0, 14.7, 15.0, 17.0, 20.0, 25.0 })
        {
            for (var charCount = 1; charCount <= 200; charCount++)
            {
                var ms = CpsHelper.GetDurationForCps(charCount, cps).TotalMilliseconds;

                Assert.Equal(Math.Round(ms), ms);
                Assert.True(charCount / (ms / 1000.0) <= cps, $"{charCount} chars at {cps} CPS: {ms} ms is over the limit");
                Assert.True(ms == 1 || charCount / ((ms - 1) / 1000.0) > cps, $"{charCount} chars at {cps} CPS: {ms - 1} ms would also fit");
            }
        }
    }

    [Fact]
    public void GetDurationForCps_NoCharacters_IsZero()
    {
        Assert.Equal(TimeSpan.Zero, CpsHelper.GetDurationForCps(0, 15));
    }

    [Fact]
    public void GetDurationForCps_ZeroCps_ClampsToMaxTime()
    {
        Assert.True(CpsHelper.GetDurationForCps(20, 0).IsMaxTime());
    }

    [Fact]
    public void IsAboveMax_ComparesTheDisplayedPrecision()
    {
        Assert.False(CpsHelper.IsAboveMax(15.0037, 15));  // 20 chars / 1333 ms
        Assert.False(CpsHelper.IsAboveMax(15.004, 15));
        Assert.True(CpsHelper.IsAboveMax(15.005, 15));
        Assert.True(CpsHelper.IsAboveMax(15.01, 15));
        Assert.False(CpsHelper.IsAboveMax(15, 15));
    }
}
