using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToOneDecimalConverter : IValueConverter
{
    public static readonly DoubleToOneDecimalConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return d.ToString("0.0", culture); // rounded automatically
        }

        return "0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DoubleConverterHelper.ConvertBack(value, culture);
}
