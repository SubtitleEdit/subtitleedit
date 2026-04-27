using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class BoolToFontStyleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? FontStyle.Italic : FontStyle.Normal;
        }

        return FontStyle.Normal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontStyle fontStyle)
        {
            return fontStyle == FontStyle.Italic;
        }

        return false;
    }
}
