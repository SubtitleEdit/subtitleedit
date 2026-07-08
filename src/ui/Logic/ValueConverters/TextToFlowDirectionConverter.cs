using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Gives a text presenter the flow direction of its content: text with right to
/// left letters flows right to left even in left to right mode, and text in a
/// left to right script (for example a Turkish original next to an Arabic
/// working language) flows left to right even in right to left mode. Empty
/// cells leave the binding unset so the control keeps its inherited direction.
/// </summary>
public class TextToFlowDirectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            return LanguageAutoDetect.ContainsRightToLeftLetter(text)
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
