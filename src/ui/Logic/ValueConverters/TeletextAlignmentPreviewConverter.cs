using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TeletextAlignmentPreviewConverter : IMultiValueConverter
{
    public static readonly TeletextAlignmentPreviewConverter Instance = new();

    public object Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        // When the teletext preview is not driving the alignment, fall back to whatever the
        // caller passes as the parameter - the grid's own "center text" setting (#14316) -
        // rather than assuming left.
        var fallback = parameter is TextAlignment defaultAlignment ? defaultAlignment : TextAlignment.Start;

        if (values.Count < 2 || values[0] is not true)
        {
            return fallback;
        }

        if (values[1] is TextAlignment alignment)
        {
            return alignment;
        }

        return fallback;
    }

    public object[] ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
