using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;

public class OcrFixLineResult
{
    public int LineIndex { get; set; }
    public List<OcrFixLinePartResult> Words { get; set; } = new();
    public ReplacementUsedItem ReplacementUsed { get; set; } = new();

    public OcrFixLineResult()
    {
    }

    public OcrFixLineResult(int index, string text)
    {
        LineIndex = index;
        Words = new List<OcrFixLinePartResult> { new() { Word = text, IsSpellCheckedOk = null } };
    }

    public string GetText()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var w in Words)
        {
            sb.Append(string.IsNullOrEmpty(w.FixedWord) ? w.Word : w.FixedWord);
        }

        return sb.ToString();
    }

    public TextBlock GetFormattedText(IBrush? errorBrush = null, IBrush? normalBrush = null)
    {
        var textBlock = new TextBlock();
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        var errorColor = errorBrush ?? Brushes.LightPink;
        var normalColor = normalBrush ?? Brushes.LightGreen;

        if (textBlock.Inlines != null)
        {
            foreach (var word in Words)
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