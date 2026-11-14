using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToOneDecimalHideMaxConverter : IValueConverter
{
    public static readonly DoubleToOneDecimalHideMaxConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            if (d == double.MaxValue || double.IsNaN(d))
            {
                return string.Empty;
            }

            return d.ToString("0.0", culture); // rounded automatically
        }

        return "0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DoubleConverterHelper.ConvertBack(value, culture);
}
