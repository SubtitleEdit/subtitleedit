using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

internal class ColorToBrushConverter : IValueConverter
{
    // The colors are arbitrary (color pickers drag through the whole space), so there is nothing
    // to cache - but an immutable brush is a plain object, while SolidColorBrush is an
    // AvaloniaObject that drags a property store along for a value that never changes.
    private static readonly ImmutableSolidColorBrush WhiteBrush = new(Colors.White);

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Color color)
        {
            return new ImmutableSolidColorBrush(color);
        }

        return WhiteBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
