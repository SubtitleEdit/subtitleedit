using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// The ConvertBack shared by the double-to-text converters: accepts any numeric value or a
/// number typed in the current culture, and falls back to 0.
/// </summary>
internal static class DoubleConverterHelper
{
    public static object ConvertBack(object? value, CultureInfo culture)
    {
        if (value is double d)
        {
            return d;
        }

        if (value is float f)
        {
            return (double)f;
        }

        if (value is decimal dec)
        {
            return (double)dec;
        }

        if (value is string s && double.TryParse(s, NumberStyles.Float, culture, out var result))
        {
            return result;
        }

        return 0.0;
    }
}
