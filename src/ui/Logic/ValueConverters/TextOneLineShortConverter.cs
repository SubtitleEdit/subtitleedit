using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TextOneLineShortConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
        {
            return string.Empty;
        }

        // Replace line breaks with spaces
        str = str.Replace("\r", " ").Replace("\n", " ").Trim();

        // Allow custom max length via binding parameter
        var maxLength = 25;
        if (parameter is string p && int.TryParse(p, out var len))
        {
            maxLength = len;
        }

        if (str.Length <= maxLength)
        {
            return str;
        }

        // Try to cut at the last space before maxLength
        var lastSpace = str.LastIndexOf(' ', maxLength);
        if (lastSpace > 0)
        {
            return str[..lastSpace] + "...";
        }

        // Fallback: hard cut
        return str[..maxLength] + "...";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}