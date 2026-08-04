using Avalonia.Media;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for SubRip (.srt) and WebVTT (.vtt) subtitle formats
/// </summary>
public partial class SubRipSourceSyntaxHighlighting : ISourceSyntaxHighlighter
{
    // SubRip-specific colors
    private static readonly Color NumberColor = Color.FromRgb(140, 170, 0);
    private static readonly Color TimeColor = Color.FromArgb(128, 80, 160, 210); // half transparent, so it reads as softer
    private static readonly Color TimeSeparatorColor = Color.FromRgb(170, 110, 180);

    // HTML/ASS syntax highlighting colors (the shared, theme-dependent scheme from
    // SubtitleSyntaxTokenizer) - resolved per use so a theme switch is picked up.
    private static Color ElementColor => SubtitleSyntaxTokenizer.ElementColor;
    private static Color AttributeColor => SubtitleSyntaxTokenizer.AttributeColor;
    private static Color CommentColor => SubtitleSyntaxTokenizer.CommentColor;
    private static Color CharsColor => SubtitleSyntaxTokenizer.CharsColor;
    private static Color ValuesColor => SubtitleSyntaxTokenizer.ValuesColor;
    private static Color StyleColor => SubtitleSyntaxTokenizer.StyleColor;

    // SubRip number pattern (e.g., "1", "2", "123")
    [GeneratedRegex(@"^\d+$", RegexOptions.Multiline)]
    private static partial Regex SubRipNumberRegex();

    // SubRip timecode pattern (e.g., "00:00:00,000 --> 00:00:01,670")
    [GeneratedRegex(@"\d{2}:\d{2}:\d{2}[,\.]\d{3}\s*-->\s*\d{2}:\d{2}:\d{2}[,\.]\d{3}")]
    private static partial Regex SubRipTimecodeRegex();

    public void HighlightLine(string lineText, SourceSyntaxLineStyler styler)
    {
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // First, colorize SubRip-specific elements (numbers and timecodes)
        if (ColorizeSubRipFormat(lineText, styler))
        {
            return; // This line is a number or timecode, skip HTML coloring
        }

        // Then colorize HTML/ASS tags in subtitle text
        ColorizeHtmlAndAssTags(lineText, styler);
    }

    private static bool ColorizeSubRipFormat(string lineText, SourceSyntaxLineStyler styler)
    {
        // Colorize SubRip sequence numbers
        var numberMatch = SubRipNumberRegex().Match(lineText);
        if (numberMatch.Success && numberMatch.Value == lineText.Trim())
        {
            styler.Apply(0, lineText.Length, NumberColor, bold: true);
            return true;
        }

        // Colorize SubRip timecodes with special handling for the separator
        var timecodeMatch = SubRipTimecodeRegex().Match(lineText);
        if (timecodeMatch.Success)
        {
            var fullTimecode = timecodeMatch.Value;
            var separatorIndex = fullTimecode.IndexOf("-->", StringComparison.Ordinal);

            if (separatorIndex >= 0)
            {
                // Colorize the start timecode (before "-->")
                styler.Apply(timecodeMatch.Index, separatorIndex, TimeColor, bold: true);

                // Colorize the separator "-->" with a different color
                var separatorStart = timecodeMatch.Index + separatorIndex;
                var separatorEnd = separatorStart + 3; // Length of "-->"

                // Skip any whitespace before the separator
                while (separatorStart > timecodeMatch.Index && char.IsWhiteSpace(lineText[separatorStart - 1]))
                {
                    separatorStart--;
                }

                // Include whitespace after the separator
                while (separatorEnd < timecodeMatch.Index + timecodeMatch.Length && char.IsWhiteSpace(lineText[separatorEnd]))
                {
                    separatorEnd++;
                }

                styler.Apply(separatorStart, separatorEnd - separatorStart, TimeSeparatorColor, bold: true);

                // Colorize the end timecode (after "-->")
                var endTimecodeEnd = timecodeMatch.Index + timecodeMatch.Length;
                if (endTimecodeEnd > separatorEnd)
                {
                    styler.Apply(separatorEnd, endTimecodeEnd - separatorEnd, TimeColor, bold: true);
                }
            }
            else
            {
                // Fallback: colorize the entire match as timecode
                styler.Apply(timecodeMatch.Index, timecodeMatch.Length, TimeColor, bold: true);
            }

            return true;
        }

        return false;
    }

    private static void ColorizeHtmlAndAssTags(string lineText, SourceSyntaxLineStyler styler)
    {
        var inComment = false;
        var inHtmlTag = false;
        var inAttributeVal = false;
        var quoteChar = '\0';

        for (int i = 0; i < lineText.Length; i++)
        {
            var c = lineText[i];
            var c2 = i + 1 < lineText.Length ? lineText[i + 1] : '\0';

            // Handle ASS/SSA tags: {\tag} or {\tagValue}
            if (c == '{' && c2 == '\\')
            {
                var tagEnd = lineText.IndexOf('}', i + 2);
                if (tagEnd != -1)
                {
                    // Color opening brace and backslash
                    styler.Apply(i, 2, CharsColor);

                    // Find where the tag name ends (before any numbers or special chars)
                    var tagNameStart = i + 2;
                    var tagNameEnd = tagNameStart;
                    while (tagNameEnd < tagEnd && char.IsLetter(lineText[tagNameEnd]))
                    {
                        tagNameEnd++;
                    }

                    // Color tag name
                    if (tagNameEnd > tagNameStart)
                    {
                        styler.Apply(tagNameStart, tagNameEnd - tagNameStart, ElementColor);
                    }

                    // Color tag value/parameters
                    if (tagNameEnd < tagEnd)
                    {
                        styler.Apply(tagNameEnd, tagEnd - tagNameEnd, ValuesColor);
                    }

                    // Color closing brace
                    styler.Apply(tagEnd, 1, CharsColor);

                    i = tagEnd;
                    continue;
                }
            }

            if (!inComment && c == '<')
            {
                if (i + 3 < lineText.Length && c2 == '!' && lineText[i + 2] == '-' && lineText[i + 3] == '-')
                {
                    // Comment start: <!--
                    var commentEnd = lineText.IndexOf("-->", i + 4, StringComparison.Ordinal);
                    var commentLength = commentEnd != -1 ? commentEnd + 3 - i : lineText.Length - i;
                    styler.Apply(i, commentLength, CommentColor);
                    i += commentLength - 1;
                    continue;
                }
                else
                {
                    // HTML tag start
                    styler.Apply(i, 1, CharsColor);

                    if (c2 == '/')
                    {
                        // Closing tag
                        styler.Apply(i + 1, 1, CharsColor);
                        i++;
                    }

                    inHtmlTag = true;

                    // Find element name end
                    var elementStart = i + 1;
                    if (elementStart < lineText.Length && lineText[elementStart] == '/')
                    {
                        elementStart++;
                    }

                    var elementEnd = elementStart;
                    while (elementEnd < lineText.Length && !char.IsWhiteSpace(lineText[elementEnd]) &&
                           lineText[elementEnd] != '>' && lineText[elementEnd] != '/')
                    {
                        elementEnd++;
                    }

                    if (elementEnd > elementStart)
                    {
                        styler.Apply(elementStart, elementEnd - elementStart, ElementColor);
                        i = elementEnd - 1;
                    }
                }
            }
            else if (inHtmlTag && c == '>')
            {
                // HTML tag end
                styler.Apply(i, 1, CharsColor);
                inHtmlTag = false;
                inAttributeVal = false;
                quoteChar = '\0';
            }
            else if (inHtmlTag && c == '/' && c2 == '>')
            {
                // Self-closing tag
                styler.Apply(i, 2, CharsColor);
                inHtmlTag = false;
                inAttributeVal = false;
                quoteChar = '\0';
                i++;
            }
            else if (inHtmlTag && !inAttributeVal && char.IsLetter(c))
            {
                // Attribute name
                var attrStart = i;
                while (i < lineText.Length && (char.IsLetterOrDigit(lineText[i]) || lineText[i] == '-' || lineText[i] == '_'))
                {
                    i++;
                }

                styler.Apply(attrStart, i - attrStart, AttributeColor);
                i--;
            }
            else if (inHtmlTag && c == '=')
            {
                // Equals sign
                styler.Apply(i, 1, CharsColor);
            }
            else if (inHtmlTag && (c == '"' || c == '\''))
            {
                if (!inAttributeVal)
                {
                    // Start of attribute value
                    quoteChar = c;
                    inAttributeVal = true;
                    var valueStart = i;
                    var valueEnd = lineText.IndexOf(quoteChar, i + 1);
                    if (valueEnd == -1)
                    {
                        valueEnd = lineText.Length;
                    }
                    else
                    {
                        valueEnd++;
                    }

                    // Color the quotes
                    styler.Apply(valueStart, 1, CharsColor);

                    // Color the value content (check for style attribute)
                    var hasColon = lineText.IndexOf(':', i + 1, valueEnd - i - 2) != -1;
                    var valueColor = hasColon ? StyleColor : ValuesColor;

                    if (valueEnd > valueStart + 1)
                    {
                        styler.Apply(valueStart + 1, valueEnd - 1 - (valueStart + 1), valueColor);

                        // Color closing quote
                        styler.Apply(valueEnd - 1, 1, CharsColor);
                    }

                    i = valueEnd - 1;
                    inAttributeVal = false;
                    quoteChar = '\0';
                }
            }
        }
    }
}
