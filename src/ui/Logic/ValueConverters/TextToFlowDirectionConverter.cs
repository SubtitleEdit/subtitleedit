using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// In right to left mode, gives a text presenter a left to right flow direction
/// when its content is in a left to right script (for example a Turkish original
/// subtitle next to an Arabic working language). In every other case the binding
/// is left unset so the control keeps its inherited direction.
/// </summary>
public class TextToFlowDirectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var rightToLeftMode = Se.Settings.Appearance.RightToLeft;
        if (rightToLeftMode && value is string text && !string.IsNullOrWhiteSpace(text))
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
