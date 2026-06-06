using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class NullableDoubleConverter : IValueConverter
{
    public double DefaultValue { get; set; } = 0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue;
        }

        if (value is double doubleValue)
        {
            return (decimal)doubleValue;
        }

        if (value is int intValue)
        {
            return (decimal)intValue;
        }

        return (decimal)DefaultValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return (double)decimalValue;
        }

        if (value is double doubleValue)
        {
            return doubleValue;
        }

        if (value is int intValue)
        {
            return (double)intValue;
        }

        return DefaultValue;
    }
}
