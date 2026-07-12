using Avalonia.Data.Converters;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToDisplayShortConverter : IValueConverter
{
    public static readonly DoubleToDisplayShortConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double ms)
        {
            if (ms == double.MaxValue || double.IsNaN(ms))
            {
                // Sentinel for "no value" (e.g. the gap after the last line) - show nothing
                // instead of a clamped "0,000".
                return string.Empty;
            }

            if (Se.Settings.General.UseFrameMode)
            {
                return new TimeCode(ms).ToShortStringHHMMSSFF();
            }

            return new TimeCode(ms).ToShortString();
        }

        if (Se.Settings.General.UseFrameMode)
        {
            return "00.00";
        }

        return "00,000";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
