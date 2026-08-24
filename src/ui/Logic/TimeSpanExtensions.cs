using System;

namespace Nikse.SubtitleEdit.Logic;

public static class TimeSpanExtensions
{
    public const double MaxTimeTotalMilliseconds = 359999999; // new TimeCode(99, 59, 59, 999).TotalMilliseconds

    public static bool IsMaxTime(this TimeSpan ts)
    {
        return Math.Abs(ts.TotalMilliseconds - MaxTimeTotalMilliseconds) < 0.01;
    }

    /// <summary>
    /// Rounds a time to a whole millisecond - the only resolution subtitle formats (and every
    /// time code SE displays) can represent.
    /// <para>
    /// Since .NET Core 3.0 <see cref="TimeSpan.FromSeconds(double)"/> and
    /// <see cref="TimeSpan.FromMilliseconds(double)"/> truncate to ticks instead of rounding to
    /// whole milliseconds like .NET Framework did, so e.g. <c>TimeSpan.FromSeconds(0.82)</c> is
    /// 819.9999 ms, not 820 ms. Such a value renders as "0,820" in the duration up/down (which
    /// rounds) and as "0,819" in the grid (which truncates, like the format writers do), and the
    /// end time it produces really is a millisecond early - see issue #14056. Snapping on the way
    /// in keeps the two displays in agreement and the saved file faithful to what was typed.
    /// </para>
    /// </summary>
    public static TimeSpan SnapToWholeMilliseconds(this TimeSpan ts)
    {
        var rest = ts.Ticks % TimeSpan.TicksPerMillisecond;
        if (rest == 0)
        {
            return ts;
        }

        // Away from zero on the half, matching the rounding SE uses elsewhere for time codes.
        // TimeSpan.MaxValue/MinValue have no whole millisecond to round to, so they stay put
        // rather than wrapping around.
        var ticks = ts.Ticks - rest;
        if (rest >= TimeSpan.TicksPerMillisecond / 2)
        {
            if (ticks > long.MaxValue - TimeSpan.TicksPerMillisecond)
            {
                return ts;
            }

            ticks += TimeSpan.TicksPerMillisecond;
        }
        else if (rest <= -TimeSpan.TicksPerMillisecond / 2)
        {
            if (ticks < long.MinValue + TimeSpan.TicksPerMillisecond)
            {
                return ts;
            }

            ticks -= TimeSpan.TicksPerMillisecond;
        }

        return TimeSpan.FromTicks(ticks);
    }

    /// <summary>
    /// <see cref="TimeSpan.FromSeconds(double)"/> snapped to a whole millisecond -
    /// see <see cref="SnapToWholeMilliseconds"/>.
    /// </summary>
    public static TimeSpan FromSecondsWholeMilliseconds(double seconds)
    {
        return FromMillisecondsWholeMilliseconds(seconds * 1000.0);
    }

    /// <summary>
    /// <see cref="TimeSpan.FromMilliseconds(double)"/> snapped to a whole millisecond -
    /// see <see cref="SnapToWholeMilliseconds"/>.
    /// </summary>
    public static TimeSpan FromMillisecondsWholeMilliseconds(double milliseconds)
    {
        if (double.IsNaN(milliseconds))
        {
            return TimeSpan.Zero;
        }

        // Keep the tick multiplication below in range; TimeSpan.FromMilliseconds would throw here.
        const double maxMs = long.MaxValue / (double)TimeSpan.TicksPerMillisecond;
        var ms = Math.Round(milliseconds, MidpointRounding.AwayFromZero);
        if (ms >= maxMs)
        {
            return TimeSpan.MaxValue;
        }

        if (ms <= -maxMs)
        {
            return TimeSpan.MinValue;
        }

        return TimeSpan.FromTicks((long)ms * TimeSpan.TicksPerMillisecond);
    }
}
