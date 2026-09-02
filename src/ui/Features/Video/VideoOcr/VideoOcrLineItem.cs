using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

public partial class VideoOcrLineItem : ObservableObject
{
    [ObservableProperty] private int _number;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedText))]
    private string _text = string.Empty;

    /// <summary>The OCR fix engine's per-word result for <see cref="Text"/>, or null when no
    /// fix/spell check ran. Drives the per-word coloring in <see cref="FormattedText"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedText))]
    private OcrFixLineResult? _fixResult;

    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// The text cell's content: per-word colored runs when the fix engine ran (green =
    /// word known, red = unknown, matching the subtitle-bitmap OCR window), otherwise the
    /// plain text. The base foreground is always set explicitly - the table's cell theme
    /// does not flow the theme foreground into templated content, which rendered the text
    /// black on the dark theme.
    /// </summary>
    public TextBlock FormattedText
    {
        get
        {
            var baseBrush = UiTheme.IsDarkThemeEnabled() ? Brushes.WhiteSmoke : Brushes.Black;

            var textBlock = FixResult != null
                ? FixResult.GetFormattedText()
                : new TextBlock { Text = Text };

            textBlock.Foreground = baseBrush;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            return textBlock;
        }
    }
}
