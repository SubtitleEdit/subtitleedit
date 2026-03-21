using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

internal class DurationToBackgroundConverter : IValueConverter
{
    public static readonly DurationToBackgroundConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            var general = Se.Settings.General;
            if (general.ColorDurationTooShort && ts.TotalMilliseconds < general.SubtitleMinimumDisplayMilliseconds)
            {
                return new SolidColorBrush(general.ErrorColor.FromHexToColor());
            }
            else if (general.ColorDurationTooLong && ts.TotalMilliseconds > general.SubtitleMaximumDisplayMilliseconds)
            {
                return new SolidColorBrush(general.ErrorColor.FromHexToColor());
            }
        }

        // Default background
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
