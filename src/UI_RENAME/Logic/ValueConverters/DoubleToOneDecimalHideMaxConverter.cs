using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToOneDecimalHideMaxConverter : IValueConverter
{
    public static readonly DoubleToOneDecimalConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            if (d == double.MaxValue || d == double.NaN)
            {
                return string.Empty;
            }

            return d.ToString("0.0", culture); // rounded automatically
        }

        return "0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return d;
        }

        if (value is float f)
        {
            return (double)f;
        }

        if (value is decimal dec)
        {
            return (double)dec;
        }

        if (value is string s && double.TryParse(s, NumberStyles.Float, culture, out var result))
        {
            return result;
        }

        return 0.0;
    }
}
