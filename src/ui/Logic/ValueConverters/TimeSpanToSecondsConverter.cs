using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TimeSpanToSecondsConverter : IValueConverter
{
    public static readonly TimeSpanToSecondsConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            var s = timeSpan.TotalSeconds.ToString("0.000");
            return s;
        }

        return "0.000";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            return ts;
        }

        if (value is string stringValue)
        {
            return TimeSpan.Parse(stringValue);
        }
        
        if (value is decimal decimalSeconds)
        {
            return TimeSpan.FromSeconds((double)decimalSeconds);
        }

        if (value is double doubleSeconds)
        {
            return TimeSpan.FromSeconds(doubleSeconds);
        }

        if (value is float floatSeconds)
        {
            return TimeSpan.FromSeconds((double)floatSeconds);
        }

        return TimeSpan.Zero;
    }
}
