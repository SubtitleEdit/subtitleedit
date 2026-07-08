using Avalonia;
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
        if (Se.Settings.Appearance.RightToLeft &&
            value is string text &&
            !string.IsNullOrWhiteSpace(text) &&
            !LanguageAutoDetect.ContainsRightToLeftLetter(text))
        {
            return FlowDirection.LeftToRight;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
