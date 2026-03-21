using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public static class TimeSpanFormatter
{
    public static string ToStringHms(TimeSpan ts) =>
        string.Format("{0:D2}:{1:D2}:{2:D2},{3:D3}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);

    public static TimeSpan FromStringHms(string s)
    {
        if (TimeSpan.TryParseExact(s.Replace(',', '.'), @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return TimeSpan.Zero;
    }
}
