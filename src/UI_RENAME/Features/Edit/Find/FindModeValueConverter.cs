using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

public class FindModeValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is FindService.FindMode mode && mode == (FindService.FindMode)(parameter ?? throw new InvalidOperationException());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
        {
            return parameter;
        }

        return FindService.FindMode.None;
    }
}