using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Text converter for fractional NumericUpDowns that takes both "." and "," as the decimal
/// separator. NumericUpDown parses with NumberStyles.Any in the current culture, where the other
/// separator is the thousands separator - so on a Danish/German system typing "0.5" silently
/// became 5 while the box still showed "0.5" (and "0,5" became 5 on an English one).
/// Values are still shown in the current culture with the control's format string.
/// </summary>
public class NumericUpDownDecimalTextConverter : IValueConverter
{
    private readonly string _formatString;

    public NumericUpDownDecimalTextConverter(string formatString)
    {
        _formatString = formatString;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => ConvertEitherWay(value);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ConvertEitherWay(value);

    // NumericUpDown uses one converter for both directions; go by the value's type.
    private object? ConvertEitherWay(object? value)
    {
        return value switch
        {
            string text => Parse(text),
            decimal number => number.ToString(_formatString, CultureInfo.CurrentCulture),
            double number => ((decimal)number).ToString(_formatString, CultureInfo.CurrentCulture),
            _ => null,
        };
    }

    public static decimal? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var s = text.Trim().Replace(" ", string.Empty).Replace(" ", string.Empty);

        // With both separators present the last one is the decimal separator ("1.234,5" and
        // "1,234.5"); the others group thousands.
        var lastSeparator = Math.Max(s.LastIndexOf('.'), s.LastIndexOf(','));
        if (lastSeparator >= 0)
        {
            var integerPart = s.Substring(0, lastSeparator).Replace(".", string.Empty).Replace(",", string.Empty);
            s = integerPart + "." + s.Substring(lastSeparator + 1);
        }

        return decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }
}
