using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TimeSpanToDisplayFullConverter : IValueConverter
{
    public static readonly TimeSpanToDisplayFullConverter Instance = new();

    // Reused to avoid per-call TimeCode allocations (expected to be used from the UI thread only).
    private readonly TimeCode _formattingTimeCode = new();
    private const string ZeroFrameMode = "00:00:00.00";
    private const string ZeroTime = "00:00:00,000";
    private static readonly char[] SplitChars = ['.', ':', ';', ','];

    // The start and end time of every visible row go through here twice per repaint - see
    // TimeCodeDisplayCache.
    private readonly TimeCodeDisplayCache _cache = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            if (_cache.TryGet(ts.Ticks, out var cached))
            {
                return cached;
            }

            var key = ts.Ticks;
            if (Se.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                ts = ts.Add(TimeSpan.FromMilliseconds(Se.Settings.General.CurrentVideoOffsetInMs));
            }

            _formattingTimeCode.TimeSpan = ts;

            var formatted = Se.Settings.General.UseFrameMode
                ? _formattingTimeCode.ToHHMMSSFF()
                : _formattingTimeCode.ToString();

            _cache.Set(key, formatted);
            return formatted;
        }

        return Se.Settings.General.UseFrameMode ? ZeroFrameMode : ZeroTime;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var span = s.AsSpan();
            var enumerator = span.SplitAny(SplitChars);
            if (enumerator.MoveNext() && int.TryParse(span[enumerator.Current], out var hours) &&
                enumerator.MoveNext() && int.TryParse(span[enumerator.Current], out var minutes) &&
                enumerator.MoveNext() && int.TryParse(span[enumerator.Current], out var seconds) &&
                enumerator.MoveNext() && int.TryParse(span[enumerator.Current], out var millisecondsOrFrames) &&
                !enumerator.MoveNext())
            {
                var milliseconds = Se.Settings.General.UseFrameMode
                    ? SubtitleFormat.FramesToMillisecondsMax999(millisecondsOrFrames)
                    : millisecondsOrFrames;
                var result = new TimeSpan(0, hours, minutes, seconds, milliseconds);

                if (Se.Settings.General.CurrentVideoOffsetInMs != 0)
                {
                    result = result.Add(TimeSpan.FromMilliseconds(-Se.Settings.General.CurrentVideoOffsetInMs));
                }

                return result;
            }
        }

        return TimeSpan.Zero;
    }
}