using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Colors the batch convert status column as a badge: green for converted, red for errors,
/// gray for cancelled. In-progress and idle statuses ("-", "OCR 42%", ...) stay unstyled -
/// bind with ConverterParameter "background" for the badge fill, anything else for the text color.
/// Mid-saturation tones picked to stay readable on both the dark and light theme.
/// </summary>
public class BatchConvertStatusColorConverter : IValueConverter
{
    private static readonly SolidColorBrush SuccessForeground = new(Color.Parse("#3fae74"));
    private static readonly SolidColorBrush SuccessBackground = new(Color.Parse("#3fae74"), 0.16);
    private static readonly SolidColorBrush ErrorForeground = new(Color.Parse("#e05c5c"));
    private static readonly SolidColorBrush ErrorBackground = new(Color.Parse("#e05c5c"), 0.14);
    private static readonly SolidColorBrush CancelledForeground = new(Color.Parse("#8494a4"));
    private static readonly SolidColorBrush CancelledBackground = new(Color.Parse("#8494a4"), 0.16);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value as string;
        var isBackground = parameter as string == "background";

        if (string.IsNullOrEmpty(status) || status == "-")
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (status == Se.Language.General.Converted)
        {
            return isBackground ? SuccessBackground : SuccessForeground;
        }

        if (status == Se.Language.General.Cancelled)
        {
            return isBackground ? CancelledBackground : CancelledForeground;
        }

        // "Error: {0}" - match on the part before the placeholder so translated texts work too.
        var errorPrefix = string.Format(Se.Language.General.ErrorX, string.Empty).Trim();
        if (status == Se.Language.General.NoSubtitlesFound ||
            status == Se.Language.Ocr.OllamaModelLikelyWrong ||
            (errorPrefix.Length > 0 && status.StartsWith(errorPrefix, StringComparison.OrdinalIgnoreCase)) ||
            status.StartsWith("BinaryOcr database not found", StringComparison.Ordinal))
        {
            return isBackground ? ErrorBackground : ErrorForeground;
        }

        // In-progress statuses (OCR percentages etc.) keep the default text color, no badge.
        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
