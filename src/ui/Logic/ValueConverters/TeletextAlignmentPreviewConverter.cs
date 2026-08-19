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
        if (values.Count < 2 || values[0] is not true)
        {
            return TextAlignment.Start;
        }

        if (values[1] is TextAlignment alignment)
        {
            return alignment;
        }

        return TextAlignment.Start;
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
