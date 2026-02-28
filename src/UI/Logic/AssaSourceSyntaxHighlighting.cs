using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for Advanced SubStation Alpha (.ass) subtitle format
/// </summary>
public partial class AssaSourceSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme
    private static readonly IBrush SectionBrush = new SolidColorBrush(Color.Parse("#569CD6")); // Blue for [Section] headers
    private static readonly IBrush KeywordBrush = new SolidColorBrush(Color.Parse("#C586C0")); // Purple for keywords (Dialogue:, Comment:, Style:, Format:)
    private static readonly IBrush TimeBrush = new SolidColorBrush(Color.Parse("#4EC9B0")); // Cyan for timecodes
    private static readonly IBrush PropertyBrush = new SolidColorBrush(Color.Parse("#9CDCFE")); // Light blue for property names
    private static readonly IBrush ValueBrush = new SolidColorBrush(Color.Parse("#CE9178")); // Orange for values
    private static readonly IBrush CommentBrush = new SolidColorBrush(Color.Parse("#6A9955")); // Green for comments
    private static readonly IBrush NumberBrush = new SolidColorBrush(Color.Parse("#B5CEA8")); // Light green for numbers
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    // Section headers like [Script Info], [V4+ Styles], [Events]
    [GeneratedRegex(@"^\s*\[[^\]]+\]\s*$", RegexOptions.Multiline)]
    private static partial Regex SectionHeaderRegex();

    // Keywords: Dialogue, Comment, Style, Format
    [GeneratedRegex(@"^(Dialogue|Comment|Style|Format)\s*:", RegexOptions.IgnoreCase)]
    private static partial Regex KeywordRegex();

    // ASSA timecode pattern (e.g., "0:00:00.92")
    [GeneratedRegex(@"\d{1,2}:\d{2}:\d{2}\.\d{2}")]
    private static partial Regex AssaTimecodeRegex();

    // Property lines like "Title:", "ScriptType:", etc.
    [GeneratedRegex(@"^([A-Za-z][A-Za-z0-9 ]*)\s*:\s*(.*)$")]
    private static partial Regex PropertyLineRegex();

    // Comments (lines starting with ;)
    [GeneratedRegex(@"^\s*;.*$")]
    private static partial Regex CommentLineRegex();

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // Check for comment lines first
        if (ColorizeCommentLine(line, lineText))
        {
            return;
        }

        // Check for section headers
        if (ColorizeSectionHeader(line, lineText))
        {
            return;
        }

        // Check for keywords (Dialogue, Comment, Style, Format)
        if (ColorizeKeywordLine(line, lineText))
        {
            return;
        }

        // Check for property lines
        ColorizePropertyLine(line, lineText);
    }

    private bool ColorizeCommentLine(DocumentLine line, string lineText)
    {
        var match = CommentLineRegex().Match(lineText);
        if (match.Success)
        {
            ChangeLinePart(
                line.Offset,
                line.Offset + line.Length,
                element => element.TextRunProperties.SetForegroundBrush(CommentBrush));
            return true;
        }
        return false;
    }

    private bool ColorizeSectionHeader(DocumentLine line, string lineText)
    {
        var match = SectionHeaderRegex().Match(lineText);
        if (match.Success)
        {
            ChangeLinePart(
                line.Offset,
                line.Offset + line.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(SectionBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
            return true;
        }
        return false;
    }

    private bool ColorizeKeywordLine(DocumentLine line, string lineText)
    {
        var keywordMatch = KeywordRegex().Match(lineText);
        if (!keywordMatch.Success)
        {
            return false;
        }

        // Colorize the keyword
        ChangeLinePart(
            line.Offset + keywordMatch.Index,
            line.Offset + keywordMatch.Index + keywordMatch.Length,
            element =>
            {
                element.TextRunProperties.SetForegroundBrush(KeywordBrush);
                element.TextRunProperties.SetTypeface(BoldTypeface);
            });

        // For Dialogue and Comment lines, colorize timecodes
        if (keywordMatch.Value.StartsWith("Dialogue", StringComparison.OrdinalIgnoreCase) ||
            keywordMatch.Value.StartsWith("Comment", StringComparison.OrdinalIgnoreCase))
        {
            ColorizeTimecodes(line, lineText);
            ColorizeNumbers(line, lineText);
        }

        return true;
    }

    private void ColorizePropertyLine(DocumentLine line, string lineText)
    {
        var match = PropertyLineRegex().Match(lineText);
        if (match.Success)
        {
            // Colorize property name
            var propertyName = match.Groups[1].Value;
            ChangeLinePart(
                line.Offset,
                line.Offset + propertyName.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(PropertyBrush);
                });

            // Colorize the colon
            var colonIndex = lineText.IndexOf(':', StringComparison.Ordinal);
            if (colonIndex >= 0)
            {
                ChangeLinePart(
                    line.Offset + colonIndex,
                    line.Offset + colonIndex + 1,
                    element => element.TextRunProperties.SetForegroundBrush(PropertyBrush));
            }

            // Colorize the value
            var valueStart = match.Groups[2].Index;
            var valueLength = match.Groups[2].Length;
            if (valueLength > 0)
            {
                ChangeLinePart(
                    line.Offset + valueStart,
                    line.Offset + valueStart + valueLength,
                    element => element.TextRunProperties.SetForegroundBrush(ValueBrush));
            }
        }
    }

    private void ColorizeTimecodes(DocumentLine line, string lineText)
    {
        foreach (Match match in AssaTimecodeRegex().Matches(lineText))
        {
            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(TimeBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
        }
    }

    private void ColorizeNumbers(DocumentLine line, string lineText)
    {
        // Colorize layer numbers at the start of Dialogue/Comment lines
        var parts = lineText.Split(',');
        if (parts.Length > 0)
        {
            var firstPart = parts[0];
            var colonIndex = firstPart.IndexOf(':', StringComparison.Ordinal);
            if (colonIndex >= 0 && colonIndex + 1 < firstPart.Length)
            {
                var layerPart = firstPart.Substring(colonIndex + 1).Trim();
                if (int.TryParse(layerPart, out _))
                {
                    var layerStart = lineText.IndexOf(layerPart, colonIndex, StringComparison.Ordinal);
                    if (layerStart >= 0)
                    {
                        ChangeLinePart(
                            line.Offset + layerStart,
                            line.Offset + layerStart + layerPart.Length,
                            element => element.TextRunProperties.SetForegroundBrush(NumberBrush));
                    }
                }
            }
        }
    }
}
