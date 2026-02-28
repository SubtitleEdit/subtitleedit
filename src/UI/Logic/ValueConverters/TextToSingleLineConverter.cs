using Avalonia.Data.Converters;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TextToSingleLineConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
        {
            return string.Empty;
        }

        var separator = Se.Settings.Appearance.SubtitleGridTextSingleLineSeparator;
        str = str
            .Replace("\r\n", separator)
            .Replace("\n", separator);

        // Allow custom max length via binding parameter
        var maxLength = 250;
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