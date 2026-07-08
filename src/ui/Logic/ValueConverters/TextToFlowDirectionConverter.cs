using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Gives a text presenter the flow direction of its content in either mode: cells
/// with right to left letters flow right to left (also as the original next to a
/// left to right working language), cells in a left to right script flow left to
/// right (also as the original next to a right to left working language). Empty
/// cells follow the active mode.
/// </summary>
public class TextToFlowDirectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var rightToLeftMode = Se.Settings.Appearance.RightToLeft;
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            return LanguageAutoDetect.ContainsRightToLeftLetter(text)
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }

        // Always produce an explicit direction: a binding that yields UnsetValue
        // does not fall back to the inherited direction but to the property
        // default (left to right), which left right to left cells misaligned in
        // right to left mode.
        return rightToLeftMode ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
