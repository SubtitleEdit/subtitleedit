using Avalonia.Data.Converters;
using Avalonia.Media.Immutable;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>A bookmark color index (0-2) to a brush in that color, see <see cref="BookmarkColors"/>.</summary>
public class BookmarkColorToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new ImmutableSolidColorBrush(BookmarkColors.Get(value is int index ? index : 0));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
