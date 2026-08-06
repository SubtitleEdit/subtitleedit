using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class BooleanAndConverter : IMultiValueConverter
{
    public static readonly BooleanAndConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count == 0)
        {
            return ConverterBoxes.False;
        }

        // Return true only if all values are boolean true - an indexed loop instead of
        // values.All(), which allocates an enumerator per call through the IList interface.
        for (var i = 0; i < values.Count; i++)
        {
            if (values[i] is not true)
            {
                return ConverterBoxes.False;
            }
        }

        return ConverterBoxes.True;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
