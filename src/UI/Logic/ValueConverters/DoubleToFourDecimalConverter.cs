using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToFourDecimalConverter : IValueConverter
{
    public static readonly DoubleToFourDecimalConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return d.ToString("0.0000", culture); // rounded automatically
        }

        return "0.0000";
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

        if (value is string s2 && double.TryParse(s2, NumberStyles.Float, culture, out var result))
        {
            return result;
        }

        return 0.0;
    }
}
