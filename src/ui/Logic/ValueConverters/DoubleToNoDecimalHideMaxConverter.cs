using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToNoDecimalHideMaxConverter : IValueConverter
{
    public static readonly DoubleToNoDecimalHideMaxConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            if (d == double.MaxValue || d == double.NaN)
            {
                return string.Empty;
            }

            return d.ToString("0", culture); // rounded automatically
        }

        return "0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && double.TryParse(s, NumberStyles.Float, culture, out var result))
        {
            return result;
        }

        return 0.0;
    }
}
