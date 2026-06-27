using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;

/// <summary>
/// UI-side (Avalonia) rendering for <see cref="OcrFixLineResult"/>. Kept separate from the data type
/// so the OCR-fix engine can live in libuilogic without an Avalonia dependency (#11744).
/// </summary>
public static class OcrFixLineResultExtensions
{
    public static TextBlock GetFormattedText(this OcrFixLineResult result, IBrush? errorBrush = null, IBrush? normalBrush = null)
    {
        var textBlock = new TextBlock();
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        // LightPink/LightGreen read well on a dark grid but wash out on a light background,
        // so use darker, higher-contrast variants in non-dark themes.
        var isDark = UiTheme.IsDarkThemeEnabled();
        var errorColor = errorBrush ?? (isDark ? Brushes.LightPink : new SolidColorBrush(Color.FromRgb(0xC0, 0x00, 0x00)));
        var normalColor = normalBrush ?? (isDark ? Brushes.LightGreen : new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x00)));

        if (textBlock.Inlines != null)
        {
            foreach (var word in result.Words)
            {
                var displayText = string.IsNullOrEmpty(word.FixedWord) ? word.Word : word.FixedWord;

                if (word.IsSpellCheckedOk == null)
                {
                    var run = new Run(displayText);
                    if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                    {
                        run.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                    }
                    textBlock.Inlines.Add(run);
                }
                else
                {
                    var run = new Run(displayText)
                    {
                        Foreground = (bool)word.IsSpellCheckedOk ? normalColor : errorColor
                    };
                    if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                    {
                        run.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                    }
                    textBlock.Inlines.Add(run);
                }
            }
        }

        return textBlock;
    }
}
