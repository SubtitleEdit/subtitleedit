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
            return boolValue ? ConverterBoxes.FontStyleItalic : ConverterBoxes.FontStyleNormal;
        }

        return ConverterBoxes.FontStyleNormal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontStyle fontStyle)
        {
            return ConverterBoxes.Bool(fontStyle == FontStyle.Italic);
        }

        return ConverterBoxes.False;
    }
}
