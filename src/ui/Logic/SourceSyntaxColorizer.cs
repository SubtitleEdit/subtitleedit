using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Draws an <see cref="ISourceSyntaxHighlighter"/> in an AvaloniaEdit editor. The rules live in the
/// highlighter so the editor and the <see cref="Controls.SyntaxHighlightingTextBox"/> color source
/// text identically.
/// </summary>
public sealed class SourceSyntaxColorizer : DocumentColorizingTransformer
{
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);
    private static readonly Dictionary<Color, ImmutableSolidColorBrush> BrushCache = new();

    private readonly ISourceSyntaxHighlighter _highlighter;
    private readonly SourceSyntaxLineStyler _styler = new();
    private readonly List<SourceSyntaxSpan> _spans = new();
    private bool _formatChecked;

    public SourceSyntaxColorizer(ISourceSyntaxHighlighter highlighter)
    {
        _highlighter = highlighter;
    }

    private static ImmutableSolidColorBrush GetBrush(Color color)
    {
        if (!BrushCache.TryGetValue(color, out var brush))
        {
            brush = new ImmutableSolidColorBrush(color);
            BrushCache[color] = brush;
        }

        return brush;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (!_formatChecked)
        {
            _formatChecked = true;
            if (_highlighter is ISourceSyntaxDocumentFormatter formatter)
            {
                var document = CurrentContext.Document;
                if (formatter.TryFormat(document.Text, out var formatted))
                {
                    document.Text = formatted;
                    return; // exit, let the next render handle the formatted document
                }
            }
        }

        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        _styler.Reset(lineText.Length);
        _highlighter.HighlightLine(lineText, _styler);

        _spans.Clear();
        _styler.Flatten(line.Offset, _spans);

        foreach (var span in _spans)
        {
            var brush = GetBrush(span.Color);
            var bold = span.Bold;
            ChangeLinePart(
                span.Start,
                span.Start + span.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(brush);
                    if (bold)
                    {
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    }
                });
        }
    }
}
