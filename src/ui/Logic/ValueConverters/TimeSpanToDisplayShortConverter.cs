using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TimeSpanToDisplayShortConverter : IValueConverter
{
    public static readonly TimeSpanToDisplayShortConverter Instance = new();

    // Reused to avoid per-call TimeCode allocations (expected to be used from the UI thread only).
    private readonly TimeCode _formattingTimeCode = new();
    private const string ZeroFrameMode = "00.00";
    private const string ZeroTime = "00,000";

    // The duration of every visible row goes through here twice per repaint - see
    // TimeCodeDisplayCache.
    private readonly TimeCodeDisplayCache _cache = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var useFrameMode = Se.Settings.General.UseFrameMode;
        if (value is TimeSpan ts)
        {
            if (_cache.TryGet(ts.Ticks, out var cached))
            {
                return cached;
            }

            _formattingTimeCode.TimeSpan = ts;
            var formatted = useFrameMode
                ? _formattingTimeCode.ToShortStringHHMMSSFF()
                : _formattingTimeCode.ToShortString();

            _cache.Set(ts.Ticks, formatted);
            return formatted;
        }

        return useFrameMode ? ZeroFrameMode : ZeroTime;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var arr = s.Split(',', ':', '.', ';');

            if (Se.Settings.General.UseFrameMode)
            {
                if (arr.Length == 2 &&
                    int.TryParse(arr[0], out var twoSec) &&
                    int.TryParse(arr[1], out var twoframes))
                {
                    return new TimeSpan(0, 0, 0, twoSec,  SubtitleFormat.FramesToMillisecondsMax999(twoframes), 0);
                }

                if (arr.Length == 3 &&
                    int.TryParse(arr[0], out var threeMin) &&
                    int.TryParse(arr[1], out var threeSec) &&
                    int.TryParse(arr[2], out var threeFrames))
                {
                    return new TimeSpan(0, 0, threeMin, threeSec, SubtitleFormat.FramesToMillisecondsMax999(threeFrames), 0);
                }

                if (arr.Length == 4 &&
                    int.TryParse(arr[0], out var fourHour) &&
                    int.TryParse(arr[1], out var fourMin) &&
                    int.TryParse(arr[2], out var fourSec) &&
                    int.TryParse(arr[3], out var fourFrames))
                {
                    return new TimeSpan(0, fourHour, fourMin, fourSec, SubtitleFormat.FramesToMillisecondsMax999(fourFrames), 0);
                }                
            }
            else
            {
                if (arr.Length == 2 &&
                    int.TryParse(arr[0], out var twoSec) &&
                    int.TryParse(arr[1], out var twoMs))
                {
                    return new TimeSpan(0, 0, 0, twoSec, twoMs, 0);
                }

                if (arr.Length == 3 &&
                    int.TryParse(arr[0], out var threeMin) &&
                    int.TryParse(arr[1], out var threeSec) &&
                    int.TryParse(arr[2], out var threeMs))
                {
                    return new TimeSpan(0, 0, threeMin, threeSec, threeMs, 0);
                }

                if (arr.Length == 4 &&
                    int.TryParse(arr[0], out var fourHour) &&
                    int.TryParse(arr[1], out var fourMin) &&
                    int.TryParse(arr[2], out var fourSec) &&
                    int.TryParse(arr[3], out var fourMs))
                {
                    return new TimeSpan(0, fourHour, fourMin, fourSec, fourMs, 0);
                }
            }

        }

        return TimeSpan.Zero;
    }
}