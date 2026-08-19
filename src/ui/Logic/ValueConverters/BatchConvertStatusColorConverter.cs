using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Colors the batch convert status column as a badge: green for converted, red for errors,
/// gray for cancelled. In-progress and idle statuses ("-", "OCR 42%", ...) get the plain theme
/// text color and no badge fill -
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
            return DefaultBrush(isBackground);
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
        var errorPrefix = GetErrorPrefix(Se.Language.General.ErrorX);
        if (status == Se.Language.General.NoSubtitlesFound ||
            status == Se.Language.Ocr.OllamaModelLikelyWrong ||
            status == Se.Language.Ocr.LlamaCppNotDownloaded ||
            status == Se.Language.Ocr.LlamaCppReturnedNoText ||
            status == Se.Language.Ocr.CrispEmbedNotDownloaded ||
            status == Se.Language.Ocr.CrispEmbedReturnedNoText ||
            (errorPrefix.Length > 0 && status.StartsWith(errorPrefix, StringComparison.OrdinalIgnoreCase)) ||
            status.StartsWith("BinaryOcr database not found", StringComparison.Ordinal))
        {
            return isBackground ? ErrorBackground : ErrorForeground;
        }

        // In-progress statuses (OCR percentages etc.) keep the default text color, no badge.
        return DefaultBrush(isBackground);
    }

    // An explicit theme text brush, not UnsetValue: a binding that yields UnsetValue falls
    // back to the property default (black for TextBlock.Foreground), not the inherited
    // foreground, so in-progress statuses rendered black on the dark theme.
    private static object DefaultBrush(bool isBackground)
    {
        return isBackground ? AvaloniaProperty.UnsetValue : UiUtil.GetTextColor();
    }

    // Formatting + trimming "Error: {0}" per status cell showed up as the converter's only
    // allocation; the language string changes at most once, when a translation is loaded.
    private static string? _errorXFormat;
    private static string _errorPrefix = string.Empty;

    private static string GetErrorPrefix(string errorXFormat)
    {
        if (!ReferenceEquals(_errorXFormat, errorXFormat))
        {
            _errorXFormat = errorXFormat;
            _errorPrefix = string.Format(errorXFormat, string.Empty).Trim();
        }

        return _errorPrefix;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
