using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;

/// <summary>
/// The licence and consent layout of the TTS voice windows (voice manager, voice packs, cast, the
/// licence and consent gates, the engine download); the rest of their editor look comes from
/// <see cref="EditorChrome"/>.
/// </summary>
internal static class TtsVoicesChrome
{
    /// <summary>
    /// The title block at the top of a dialog: big title, dim intro under it.
    /// </summary>
    internal static StackPanel MakeTitle(string title, string? intro)
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        });
        if (!string.IsNullOrEmpty(intro))
        {
            panel.Children.Add(new TextBlock
            {
                Text = intro,
                FontSize = 12.5,
                Foreground = EditorChrome.DimTextBrush,
                TextWrapping = TextWrapping.Wrap,
            });
        }

        return panel;
    }

    /// <summary>
    /// The bullet points of a licence or consent dialog, in a framed card. Every point is its own
    /// wrapping text block, so none can be cut off.
    /// </summary>
    internal static Border MakeBulletCard(IEnumerable<string> points)
    {
        var stack = new StackPanel { Spacing = 8 };
        foreach (var point in points)
        {
            var row = new Grid
            {
                ColumnSpacing = 8,
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
            };
            row.Add(new TextBlock { Text = "•", Foreground = EditorChrome.AccentBrush, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Top }, 0, 0);
            row.Add(new TextBlock { Text = point, TextWrapping = TextWrapping.Wrap, FontSize = 12.5 }, 0, 1);
            stack.Children.Add(row);
        }

        return new Border
        {
            Child = stack,
            Padding = new Thickness(14, 12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            Background = EditorChrome.FieldBrush,
        };
    }
}
