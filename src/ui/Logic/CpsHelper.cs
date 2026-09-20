using System;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The chars-per-second rules every CPS display shares, so the grid row, the edit box under the
/// text field and the recalculate-duration commands agree on what "over the maximum" means.
/// </summary>
public static class CpsHelper
{
    /// <summary>
    /// CPS as the comparisons see it: rounded to two decimals (the precision the grid column
    /// colors on). A line at 15.004 CPS reads "15.0" everywhere, so it must not be flagged as
    /// exceeding a 15.0 maximum anywhere either (issue #14418).
    /// </summary>
    public static double Round(double cps)
        => Math.Round(cps, 2, MidpointRounding.AwayFromZero);

    public static bool IsAboveMax(double cps, double maxCps)
        => Round(cps) > maxCps;

    /// <summary>
    /// The shortest whole-millisecond duration at which <paramref name="charCount"/> characters
    /// stay at or below <paramref name="cps"/>.
    /// <para>
    /// Rounding the exact duration to the nearest millisecond rounds down half the time, and a
    /// duration one millisecond short of 20 chars / 15 CPS (1333 ms instead of 1334 ms) is 15.004
    /// CPS - over the very limit the duration was computed from (issue #14418). So this rounds
    /// up, verified against the same division the CPS readouts perform rather than with an
    /// epsilon, so binary-fraction noise like 300.00000000000006 ms still yields 300 ms.
    /// </para>
    /// A non-positive <paramref name="cps"/> gives the maximum time code, like the plain
    /// division it replaces did through <see cref="TimeSpanExtensions.FromSecondsWholeMilliseconds"/>.
    /// </summary>
    public static TimeSpan GetDurationForCps(double charCount, double cps)
    {
        if (charCount <= 0)
        {
            return TimeSpan.Zero;
        }

        if (cps <= 0)
        {
            return TimeSpanExtensions.FromMillisecondsWholeMilliseconds(TimeSpanExtensions.MaxTimeTotalMilliseconds);
        }

        var ms = Math.Floor(charCount * 1000.0 / cps);
        if (charCount / (ms / 1000.0) > cps)
        {
            ms += 1;
        }

        return TimeSpanExtensions.FromMillisecondsWholeMilliseconds(ms);
    }
}
