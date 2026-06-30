using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class BooleanToBrushConverter : IValueConverter
{
    public IBrush TrueBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));  // green
    public IBrush FalseBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)); // gray

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? TrueBrush : FalseBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
