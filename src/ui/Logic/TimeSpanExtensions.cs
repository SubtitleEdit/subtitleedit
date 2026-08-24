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
    /// <see cref="TimeSpan.FromSeconds(double)"/> rounded to a whole millisecond -
    /// see <see cref="FromMillisecondsWholeMilliseconds"/>.
    /// </summary>
    public static TimeSpan FromSecondsWholeMilliseconds(double seconds)
    {
        return FromMillisecondsWholeMilliseconds(seconds * 1000.0);
    }

    /// <summary>
    /// <see cref="TimeSpan.FromMilliseconds(double)"/> rounded to a whole millisecond - the only
    /// resolution subtitle formats (and every time code SE displays) can represent.
    /// <para>
    /// Since .NET Core 3.0 <see cref="TimeSpan.FromSeconds(double)"/> and
    /// <see cref="TimeSpan.FromMilliseconds(double)"/> truncate to ticks instead of rounding to
    /// whole milliseconds like .NET Framework did, so e.g. <c>TimeSpan.FromSeconds(0.82)</c> is
    /// 819.9999 ms, not 820 ms. Such a value renders as "0,820" in the duration up/down (which
    /// rounds) and as "0,819" in the grid (which truncates, like the format writers do), and the
    /// end time it produces really is a millisecond early - see issue #14056. Every place that
    /// turns a fractional-millisecond double (typed input, pixel positions, video positions,
    /// scale factors) into a subtitle time must come through here.
    /// </para>
    /// Out-of-range values are clamped to the time-code domain (±99:59:59,999) rather than
    /// TimeSpan's, so the result is always a whole millisecond a <see cref="Core.Common.TimeCode"/>
    /// can hold. NaN throws, like <see cref="TimeSpan.FromSeconds(double)"/> always has - a NaN
    /// time is a bug at the call site and must not silently become 00:00:00,000.
    /// </summary>
    public static TimeSpan FromMillisecondsWholeMilliseconds(double milliseconds)
    {
        if (double.IsNaN(milliseconds))
        {
            throw new ArgumentException("TimeSpan does not accept floating point Not-a-Number values.", nameof(milliseconds));
        }

        var ms = Math.Round(milliseconds, MidpointRounding.AwayFromZero);
        if (ms > MaxTimeTotalMilliseconds)
        {
            ms = MaxTimeTotalMilliseconds;
        }
        else if (ms < -MaxTimeTotalMilliseconds)
        {
            ms = -MaxTimeTotalMilliseconds;
        }

        return TimeSpan.FromTicks((long)ms * TimeSpan.TicksPerMillisecond);
    }
}
