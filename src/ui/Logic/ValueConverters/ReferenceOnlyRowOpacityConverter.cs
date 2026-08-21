using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Dims the display-only rows that show a read-only reference line with no counterpart in the
/// working subtitle, so they read as "not one of your lines" (issue #13449).
/// </summary>
public class ReferenceOnlyRowOpacityConverter : IValueConverter
{
    private static readonly object DimmedDouble = 0.55d;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? DimmedDouble : ConverterBoxes.OneDouble;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
