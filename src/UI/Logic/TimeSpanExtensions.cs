using System;

namespace Nikse.SubtitleEdit.Logic;

public static class TimeSpanExtensions
{
    public const double MaxTimeTotalMilliseconds = 359999999; // new TimeCode(99, 59, 59, 999).TotalMilliseconds

    public static bool IsMaxTime(this TimeSpan ts)
    {
        return Math.Abs(ts.TotalMilliseconds - MaxTimeTotalMilliseconds) < 0.01;
    }
}
