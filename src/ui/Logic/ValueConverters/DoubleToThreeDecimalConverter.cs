using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToThreeDecimalConverter : IValueConverter
{
    public static readonly DoubleToThreeDecimalConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return d.ToString("0.000", culture); // rounded automatically
        }

        return "0.000";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DoubleConverterHelper.ConvertBack(value, culture);
}
