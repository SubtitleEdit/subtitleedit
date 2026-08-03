using Avalonia.Data.Converters;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;
internal class FileSizeConverter : IValueConverter
{

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Se.Language.General.ZeroBytes;
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
            return Se.Language.General.InvalidSize;
        }

        if (bytes < 0)
        {
            return Se.Language.General.InvalidSize;
        }

        if (bytes == 0)
        {
            return Se.Language.General.ZeroBytes;
        }

        int magnitude = 0;
        var sizeSuffixes = new[] { Se.Language.General.Bytes, "KB", "MB", "GB", "TB", "PB" };
        double adjustedSize = bytes;

        // Keep dividing by 1024 until we get a manageable number
        while (adjustedSize >= 1024 && magnitude < sizeSuffixes.Length - 1)
        {
            magnitude++;
            adjustedSize /= 1024;
        }

        // Format with appropriate decimal places
        string format = adjustedSize >= 100 ? "0" : (adjustedSize >= 10 ? "0.0" : "0.##");

        var result = $"{adjustedSize.ToString(format, culture)} {sizeSuffixes[magnitude]}";
        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return 0;
    }
}
