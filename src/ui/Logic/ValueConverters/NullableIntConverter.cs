using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class NullableIntConverter : IValueConverter
{
    public int DefaultValue { get; set; } = 0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue;
        }

        return DefaultValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return (int)decimalValue;
        }

        if (value is double doubleValue)
        {
            return (int)doubleValue;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        return DefaultValue;
    }
}
