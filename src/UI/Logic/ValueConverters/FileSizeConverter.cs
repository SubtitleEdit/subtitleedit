using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;
internal class FileSizeConverter : IValueConverter
{
    private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB" };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return "0 bytes";
        }

        long bytes;

        // Handle different numeric types
        if (value is long longValue)
        {
            bytes = longValue;
        }
        else if (value is int intValue)
        {
            bytes = intValue;
        }
        else if (!long.TryParse(value.ToString(), out bytes))
        {
            return "Invalid size";
        }

        if (bytes < 0)
        {
            return "Invalid size";
        }

        if (bytes == 0)
        {
            return "0 bytes";
        }

        int magnitude = 0;
        double adjustedSize = bytes;

        // Keep dividing by 1024 until we get a manageable number
        while (adjustedSize >= 1024 && magnitude < SizeSuffixes.Length - 1)
        {
            magnitude++;
            adjustedSize /= 1024;
        }

        // Format with appropriate decimal places
        string format = adjustedSize >= 100 ? "0" : (adjustedSize >= 10 ? "0.0" : "0.##");

        var result = $"{adjustedSize.ToString(format, culture)} {SizeSuffixes[magnitude]}";
        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return 0;
    }
}
