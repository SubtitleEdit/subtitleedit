using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

internal class DurationToBackgroundConverter : IValueConverter
{
    public static readonly DurationToBackgroundConverter Instance = new();

    private static readonly ImmutableSolidColorBrush TransparentBrush = new(Colors.Transparent);

    // The error color is a hex string in the settings, so the whole cost of this converter used
    // to be parsing it and building an AvaloniaObject brush per cell. Cache the brush and
    // re-derive it only when the setting's value actually changes.
    private static string? _cachedErrorColorHex;
    private static ImmutableSolidColorBrush? _cachedErrorBrush;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            var general = Se.Settings.General;
            var totalMilliseconds = ts.TotalMilliseconds;
            if ((general.ColorDurationTooShort && totalMilliseconds < general.SubtitleMinimumDisplayMilliseconds) ||
                (general.ColorDurationTooLong && totalMilliseconds > general.SubtitleMaximumDisplayMilliseconds))
            {
                return GetErrorBrush(general.ErrorColor);
            }
        }

        // Default background
        return TransparentBrush;
    }

    private static ImmutableSolidColorBrush GetErrorBrush(string errorColorHex)
    {
        // "is null" rather than "== null": ImmutableSolidColorBrush overloads == with
        // non-nullable operands, which a null check would go through.
        if (_cachedErrorBrush is null || !string.Equals(_cachedErrorColorHex, errorColorHex, StringComparison.Ordinal))
        {
            _cachedErrorColorHex = errorColorHex;
            _cachedErrorBrush = new ImmutableSolidColorBrush(errorColorHex.FromHexToColor());
        }

        return _cachedErrorBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
