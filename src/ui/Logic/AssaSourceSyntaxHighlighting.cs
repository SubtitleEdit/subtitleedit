using Avalonia.Media;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for Advanced SubStation Alpha (.ass) subtitle format
/// </summary>
public partial class AssaSourceSyntaxHighlighting : ISourceSyntaxHighlighter
{
    // Color scheme
    private static readonly Color SectionColor = Color.Parse("#569CD6"); // Blue for [Section] headers
    private static readonly Color KeywordColor = Color.Parse("#C586C0"); // Purple for keywords (Dialogue:, Comment:, Style:, Format:)
    private static readonly Color TimeColor = Color.Parse("#4EC9B0"); // Cyan for timecodes
    private static readonly Color PropertyColorDark = Color.Parse("#9CDCFE"); // Light blue for property names
    private static readonly Color PropertyColorLight = Color.Parse("#1565C0"); // Darker blue - light blue is barely readable on white
    private static readonly Color ValueColor = Color.Parse("#CE9178"); // Orange for values
    private static readonly Color CommentColor = Color.Parse("#6A9955"); // Green for comments
    private static readonly Color NumberColor = Color.Parse("#B5CEA8"); // Light green for numbers

    // Resolved per use so a theme switch is picked up
    private static Color PropertyColor => UiTheme.IsDarkThemeEnabled() ? PropertyColorDark : PropertyColorLight;

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

    public void HighlightLine(string lineText, SourceSyntaxLineStyler styler)
    {
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // Check for comment lines first
        if (ColorizeCommentLine(lineText, styler))
        {
            return;
        }

        // Check for section headers
        if (ColorizeSectionHeader(lineText, styler))
        {
            return;
        }

        // Check for keywords (Dialogue, Comment, Style, Format)
        if (ColorizeKeywordLine(lineText, styler))
        {
            return;
        }

        // Check for property lines
        ColorizePropertyLine(lineText, styler);
    }

    private static bool ColorizeCommentLine(string lineText, SourceSyntaxLineStyler styler)
    {
        if (CommentLineRegex().IsMatch(lineText))
        {
            styler.Apply(0, lineText.Length, CommentColor);
            return true;
        }

        return false;
    }

    private static bool ColorizeSectionHeader(string lineText, SourceSyntaxLineStyler styler)
    {
        if (SectionHeaderRegex().IsMatch(lineText))
        {
            styler.Apply(0, lineText.Length, SectionColor, bold: true);
            return true;
        }

        return false;
    }

    private static bool ColorizeKeywordLine(string lineText, SourceSyntaxLineStyler styler)
    {
        var keywordMatch = KeywordRegex().Match(lineText);
        if (!keywordMatch.Success)
        {
            return false;
        }

        // Colorize the keyword
        styler.Apply(keywordMatch.Index, keywordMatch.Length, KeywordColor, bold: true);

        // For Dialogue and Comment lines, colorize timecodes
        if (keywordMatch.Value.StartsWith("Dialogue", StringComparison.OrdinalIgnoreCase) ||
            keywordMatch.Value.StartsWith("Comment", StringComparison.OrdinalIgnoreCase))
        {
            ColorizeTimecodes(lineText, styler);
            ColorizeNumbers(lineText, styler);
        }

        return true;
    }

    private static void ColorizePropertyLine(string lineText, SourceSyntaxLineStyler styler)
    {
        var match = PropertyLineRegex().Match(lineText);
        if (!match.Success)
        {
            return;
        }

        // Colorize property name
        var propertyName = match.Groups[1].Value;
        styler.Apply(0, propertyName.Length, PropertyColor);

        // Colorize the colon
        var colonIndex = lineText.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex >= 0)
        {
            styler.Apply(colonIndex, 1, PropertyColor);
        }

        // Colorize the value
        var valueLength = match.Groups[2].Length;
        if (valueLength > 0)
        {
            styler.Apply(match.Groups[2].Index, valueLength, ValueColor);
        }
    }

    private static void ColorizeTimecodes(string lineText, SourceSyntaxLineStyler styler)
    {
        foreach (Match match in AssaTimecodeRegex().Matches(lineText))
        {
            styler.Apply(match.Index, match.Length, TimeColor, bold: true);
        }
    }

    private static void ColorizeNumbers(string lineText, SourceSyntaxLineStyler styler)
    {
        // Colorize layer numbers at the start of Dialogue/Comment lines
        var parts = lineText.Split(',');
        if (parts.Length == 0)
        {
            return;
        }

        var firstPart = parts[0];
        var colonIndex = firstPart.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex < 0 || colonIndex + 1 >= firstPart.Length)
        {
            return;
        }

        var layerPart = firstPart.Substring(colonIndex + 1).Trim();
        if (!int.TryParse(layerPart, out _))
        {
            return;
        }

        var layerStart = lineText.IndexOf(layerPart, colonIndex, StringComparison.Ordinal);
        if (layerStart >= 0)
        {
            styler.Apply(layerStart, layerPart.Length, NumberColor);
        }
    }
}
