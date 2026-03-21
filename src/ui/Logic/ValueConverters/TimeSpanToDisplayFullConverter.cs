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

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            if (Se.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                ts = ts.Add(TimeSpan.FromMilliseconds(Se.Settings.General.CurrentVideoOffsetInMs));
            }

            if (Se.Settings.General.UseFrameMode)
            {
                var resultFrames = new TimeCode(ts).ToHHMMSSFF();
                return resultFrames;
            }

            var result = new TimeCode(ts).ToString();
            return result;
        }

        if (Se.Settings.General.UseFrameMode)
        {
            return "00:00:00.00";
        }

        return "00:00:00,000";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var parts = s.Split('.', ':', ';', ',');
            if (Se.Settings.General.UseFrameMode)
            {
                if (parts.Length == 4 &&
                    int.TryParse(parts[0], out int hours) &&
                    int.TryParse(parts[1], out int minutes) &&
                    int.TryParse(parts[2], out int seconds) &&
                    int.TryParse(parts[3], out int frames))
                {
                    var result = new TimeSpan(0, hours, minutes, seconds, SubtitleFormat.FramesToMillisecondsMax999(frames));

                    if (Se.Settings.General.CurrentVideoOffsetInMs != 0)
                    {
                        result = result.Add(TimeSpan.FromMilliseconds(-Se.Settings.General.CurrentVideoOffsetInMs));
                    }

                    return result;
                }
            }
            else
            {
                if (parts.Length == 4 &&
                    int.TryParse(parts[0], out int hours) &&
                    int.TryParse(parts[1], out int minutes) &&
                    int.TryParse(parts[2], out int seconds) &&
                    int.TryParse(parts[3], out int ms))
                {
                    var result = new TimeSpan(0, hours, minutes, seconds, ms);

                    if (Se.Settings.General.CurrentVideoOffsetInMs != 0)
                    {
                        result = result.Add(TimeSpan.FromMilliseconds(-Se.Settings.General.CurrentVideoOffsetInMs));
                    }

                    return result;
                }
            }
        }

        return TimeSpan.Zero;
    }
}