using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// A styled range of source text: a foreground color plus an optional bold flag.
/// Spans are sorted and never overlap; text not covered by a span keeps the default foreground.
/// </summary>
public readonly record struct SourceSyntaxSpan(int Start, int Length, Color Color, bool Bold);

/// <summary>
/// The syntax rules of one source format, expressed per line and independent of the control that
/// renders them: <see cref="Controls.SyntaxTextEditorControl.SyntaxTextView"/> draws them in the
/// source editor (source view, batch convert ASSA, format preview) and
/// <see cref="Controls.SyntaxHighlightingTextPresenter"/> draws them in a
/// <see cref="Controls.SyntaxHighlightingTextBox"/> (media info).
/// </summary>
public interface ISourceSyntaxHighlighter
{
    /// <param name="lineText">One line, without its newline characters.</param>
    /// <param name="styler">Collects the styles; offsets are relative to the line start.</param>
    void HighlightLine(string lineText, SourceSyntaxLineStyler styler);
}

/// <summary>
/// Implemented by highlighters that also reflow the whole document before it is shown (XML that
/// arrives on a single line).
/// </summary>
public interface ISourceSyntaxDocumentFormatter
{
    bool TryFormat(string text, out string formatted);
}

/// <summary>
/// Collects the styles of one line and flattens them into sorted, non-overlapping spans.
///
/// Styles are applied per character and per property: a later <see cref="Apply"/> replaces the
/// color of the characters it covers, and bold, once set, stays set (no rule ever clears it).
/// </summary>
public sealed class SourceSyntaxLineStyler
{
    private Color[] _colors = Array.Empty<Color>();
    private bool[] _hasColor = Array.Empty<bool>();
    private bool[] _bold = Array.Empty<bool>();
    private int _length;

    /// <summary>
    /// Prepares the styler for a line of <paramref name="length"/> characters. The buffers are
    /// reused across lines, so one styler instance serves a whole document.
    /// </summary>
    public void Reset(int length)
    {
        if (_colors.Length < length)
        {
            var capacity = Math.Max(length, 256);
            _colors = new Color[capacity];
            _hasColor = new bool[capacity];
            _bold = new bool[capacity];
        }
        else
        {
            Array.Clear(_hasColor, 0, _length);
            Array.Clear(_bold, 0, _length);
        }

        _length = length;
    }

    /// <summary>
    /// Colors [start, start+length) - out of range parts are clipped away. Bold is only ever
    /// turned on: pass false to recolor a range without touching the weight set by an earlier rule.
    /// </summary>
    public void Apply(int start, int length, Color color, bool bold = false)
    {
        var end = Math.Min(start + length, _length);
        for (var i = Math.Max(start, 0); i < end; i++)
        {
            _colors[i] = color;
            _hasColor[i] = true;
            if (bold)
            {
                _bold[i] = true;
            }
        }
    }

    /// <summary>
    /// Appends the collected styles as spans, run-length encoded and shifted by
    /// <paramref name="offset"/> (the line's offset in the document).
    /// </summary>
    public void Flatten(int offset, List<SourceSyntaxSpan> target)
    {
        var i = 0;
        while (i < _length)
        {
            if (!_hasColor[i])
            {
                i++;
                continue;
            }

            var runStart = i;
            var color = _colors[i];
            var bold = _bold[i];
            i++;
            while (i < _length && _hasColor[i] && _colors[i] == color && _bold[i] == bold)
            {
                i++;
            }

            target.Add(new SourceSyntaxSpan(offset + runStart, i - runStart, color, bold));
        }
    }
}

/// <summary>
/// Runs an <see cref="ISourceSyntaxHighlighter"/> over a whole text and returns the spans with
/// document offsets - the form <see cref="Controls.SyntaxHighlightingTextPresenter"/> needs, which
/// lays out the entire text in one go instead of line by line.
/// </summary>
public static class SourceSyntaxTokenizer
{
    public static List<SourceSyntaxSpan> Tokenize(string text, ISourceSyntaxHighlighter highlighter)
    {
        var spans = new List<SourceSyntaxSpan>();
        if (string.IsNullOrEmpty(text))
        {
            return spans;
        }

        var styler = new SourceSyntaxLineStyler();
        var lineStart = 0;
        while (lineStart <= text.Length)
        {
            var lineEnd = lineStart;
            while (lineEnd < text.Length && text[lineEnd] != '\n' && text[lineEnd] != '\r')
            {
                lineEnd++;
            }

            if (lineEnd > lineStart)
            {
                var lineText = text.Substring(lineStart, lineEnd - lineStart);
                styler.Reset(lineText.Length);
                highlighter.HighlightLine(lineText, styler);
                styler.Flatten(lineStart, spans);
            }

            if (lineEnd >= text.Length)
            {
                break;
            }

            // Skip the line break: \r\n counts as one.
            lineStart = text[lineEnd] == '\r' && lineEnd + 1 < text.Length && text[lineEnd + 1] == '\n'
                ? lineEnd + 2
                : lineEnd + 1;
        }

        return spans;
    }
}
