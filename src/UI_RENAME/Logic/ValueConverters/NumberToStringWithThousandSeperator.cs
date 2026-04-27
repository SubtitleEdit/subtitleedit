using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class NumberToStringWithThousandSeparator : IValueConverter
{
    public static readonly NumberToStringWithThousandSeparator Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            return i.ToString("N0", culture); // "1,234" in en-US, "1.234" in da-DK
        }

        if (value is long l)
        {
            return l.ToString("N0", culture);
        }

        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && int.TryParse(s, NumberStyles.Any, culture, out var i))
        {
            return i;
        }

        return value;
    }
}
