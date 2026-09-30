using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Subtitle grid cell background: values[0] is the cell's error brush (or transparent),
/// values[1] is the row's IsSelected. The error color is a faint tint by default (alpha 50),
/// which is fine over a plain row but blends into the selection color and reads as gray on
/// the selected row - exactly the row being fixed. On a selected row the tint is made more
/// opaque, so the red stays visible and its disappearing signals the fix.
/// </summary>
public class SelectedRowErrorBrushConverter : IMultiValueConverter
{
    public static readonly SelectedRowErrorBrushConverter Instance = new();

    private const byte SelectedMinimumAlpha = 160;

    // The error color rarely changes, so one cached brush avoids an allocation per cell.
    private static Color _cachedColor;
    private static IBrush? _cachedBrush;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not ISolidColorBrush brush)
        {
            return values.Count > 0 ? values[0] as IBrush : null;
        }

        var color = brush.Color;
        if (values[1] is not true || color.A == 0 || color.A >= SelectedMinimumAlpha)
        {
            return brush;
        }

        if (_cachedBrush == null || _cachedColor != color)
        {
            _cachedColor = color;
            _cachedBrush = new ImmutableSolidColorBrush(Color.FromArgb(SelectedMinimumAlpha, color.R, color.G, color.B));
        }

        return _cachedBrush;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
