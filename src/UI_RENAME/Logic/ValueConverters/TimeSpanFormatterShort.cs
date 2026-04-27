using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public static class TimeSpanFormatterShort
{
    public static string ToStringShort(TimeSpan ts)
    {
        // Trim leading zero components for compact display
        if (ts.Hours > 0)
        {
            return string.Format("{0}:{1:D2}:{2:D2},{3:D3}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        if (ts.Minutes > 0)
        {
            return string.Format("{0}:{1:D2},{2:D3}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        return string.Format("{0},{1:D3}", ts.Seconds, ts.Milliseconds);
    }

    public static TimeSpan FromStringShort(string s)
    {
        // Normalize comma to dot for TimeSpan parsing
        s = s.Replace(',', '.');

        // Try longer format first (h:mm:ss.fff)
        if (TimeSpan.TryParseExact(s, @"h\:mm\:ss\.fff", CultureInfo.InvariantCulture, out var ts))
        {
            return ts;
        }

        if (TimeSpan.TryParseExact(s, @"m\:ss\.fff", CultureInfo.InvariantCulture, out ts))
        {
            return ts;
        }

        if (TimeSpan.TryParseExact(s, @"s\.fff", CultureInfo.InvariantCulture, out ts))
        {
            return ts;
        }

        return TimeSpan.Zero;
    }
}
