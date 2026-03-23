using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for SubRip (.srt) and WebVTT (.vtt) subtitle formats
/// </summary>
public partial class SubRipSourceSyntaxHighlighting : DocumentColorizingTransformer
{
    // SubRip-specific colors
    private static readonly IBrush NumberBrush =  new SolidColorBrush(Color.FromRgb(140, 170, 0));
    private static readonly IBrush TimeBrush = new SolidColorBrush(Color.FromRgb(80, 160, 210), 0.5);
    private static readonly IBrush TimeSeparatorBrush = new SolidColorBrush(Color.FromRgb(170, 110, 180));
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    // HTML/ASS syntax highlighting colors (matching SubtitleSyntaxHighlighting.cs)
    private static readonly Color ElementColor = Color.FromRgb(183, 89, 155);    // Soft purple - HTML element tags
    private static readonly Color AttributeColor = Color.FromRgb(86, 156, 214);  // Soft blue - HTML attribute names
    private static readonly Color CommentColor = Color.FromRgb(106, 153, 85);    // Soft green - HTML comments
    private static readonly Color CharsColor = Color.FromRgb(128, 128, 128);     // Gray - delimiters and special chars
    private static readonly Color ValuesColor = Color.FromRgb(206, 145, 120);    // Soft orange/peach - attribute values
    private static readonly Color StyleColor = Color.FromRgb(156, 220, 254);     // Light cyan - CSS property values

    // SubRip number pattern (e.g., "1", "2", "123")
    [GeneratedRegex(@"^\d+$", RegexOptions.Multiline)]
    private static partial Regex SubRipNumberRegex();

    // SubRip timecode pattern (e.g., "00:00:00,000 --> 00:00:01,670")
    [GeneratedRegex(@"\d{2}:\d{2}:\d{2}[,\.]\d{3}\s*-->\s*\d{2}:\d{2}:\d{2}[,\.]\d{3}")]
    private static partial Regex SubRipTimecodeRegex();

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // First, colorize SubRip-specific elements (numbers and timecodes)
        if (ColorizeSubRipFormat(line, lineText))
        {
            return; // This line is a number or timecode, skip HTML coloring
        }

        // Then colorize HTML/ASS tags in subtitle text
        ColorizeHtmlAndAssTags(line, lineText);
    }

    private bool ColorizeSubRipFormat(DocumentLine line, string lineText)
    {
        // Colorize SubRip sequence numbers
        var numberMatch = SubRipNumberRegex().Match(lineText);
        if (numberMatch.Success && numberMatch.Value == lineText.Trim())
        {
            ChangeLinePart(
                line.Offset,
                line.Offset + line.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(NumberBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
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
                var startTimecodeLength = separatorIndex;
                ChangeLinePart(
                    line.Offset + timecodeMatch.Index,
                    line.Offset + timecodeMatch.Index + startTimecodeLength,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(TimeBrush);
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    });

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

                ChangeLinePart(
                    line.Offset + separatorStart,
                    line.Offset + separatorEnd,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(TimeSeparatorBrush);
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    });

                // Colorize the end timecode (after "-->")
                var endTimecodeStart = separatorEnd;
                var endTimecodeLength = timecodeMatch.Index + timecodeMatch.Length - endTimecodeStart;

                if (endTimecodeLength > 0)
                {
                    ChangeLinePart(
                        line.Offset + endTimecodeStart,
                        line.Offset + timecodeMatch.Index + timecodeMatch.Length,
                        element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(TimeBrush);
                            element.TextRunProperties.SetTypeface(BoldTypeface);
                        });
                }
            }
            else
            {
                // Fallback: colorize the entire match as timecode
                ChangeLinePart(
                    line.Offset + timecodeMatch.Index,
                    line.Offset + timecodeMatch.Index + timecodeMatch.Length,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(TimeBrush);
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    });
            }
            return true;
        }

        return false;
    }

    private void ColorizeHtmlAndAssTags(DocumentLine line, string lineText)
    {
        var lineStartOffset = line.Offset;
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
                    ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 2, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

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
                        ChangeLinePart(lineStartOffset + tagNameStart, lineStartOffset + tagNameEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ElementColor));
                        });
                    }

                    // Color tag value/parameters
                    if (tagNameEnd < tagEnd)
                    {
                        ChangeLinePart(lineStartOffset + tagNameEnd, lineStartOffset + tagEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ValuesColor));
                        });
                    }

                    // Color closing brace
                    ChangeLinePart(lineStartOffset + tagEnd, lineStartOffset + tagEnd + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

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
                    ChangeLinePart(lineStartOffset + i, lineStartOffset + i + commentLength, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CommentColor));
                    });
                    i += commentLength - 1;
                    continue;
                }
                else
                {
                    // HTML tag start
                    ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

                    if (c2 == '/')
                    {
                        // Closing tag
                        ChangeLinePart(lineStartOffset + i + 1, lineStartOffset + i + 2, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                        });
                        i++;
                    }

                    inHtmlTag = true;

                    // Find element name end
                    var elementStart = i + 1;
                    if (elementStart < lineText.Length && lineText[elementStart] == '/')
                        elementStart++;

                    var elementEnd = elementStart;
                    while (elementEnd < lineText.Length && !char.IsWhiteSpace(lineText[elementEnd]) &&
                           lineText[elementEnd] != '>' && lineText[elementEnd] != '/')
                    {
                        elementEnd++;
                    }

                    if (elementEnd > elementStart)
                    {
                        ChangeLinePart(lineStartOffset + elementStart, lineStartOffset + elementEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ElementColor));
                        });
                        i = elementEnd - 1;
                    }
                }
            }
            else if (inHtmlTag && c == '>')
            {
                // HTML tag end
                ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 1, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                });
                inHtmlTag = false;
                inAttributeVal = false;
                quoteChar = '\0';
            }
            else if (inHtmlTag && c == '/' && c2 == '>')
            {
                // Self-closing tag
                ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 2, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                });
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

                ChangeLinePart(lineStartOffset + attrStart, lineStartOffset + i, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(AttributeColor));
                });
                i--;
            }
            else if (inHtmlTag && c == '=')
            {
                // Equals sign
                ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 1, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                });
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
                        valueEnd = lineText.Length;
                    else
                        valueEnd++;

                    // Color the quotes
                    ChangeLinePart(lineStartOffset + valueStart, lineStartOffset + valueStart + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

                    // Color the value content (check for style attribute)
                    var hasColon = lineText.IndexOf(':', i + 1, valueEnd - i - 2) != -1;
                    var valueColor = hasColon ? StyleColor : ValuesColor;

                    if (valueEnd > valueStart + 1)
                    {
                        ChangeLinePart(lineStartOffset + valueStart + 1, lineStartOffset + valueEnd - 1, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(valueColor));
                        });

                        // Color closing quote
                        ChangeLinePart(lineStartOffset + valueEnd - 1, lineStartOffset + valueEnd, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                        });
                    }

                    i = valueEnd - 1;
                    inAttributeVal = false;
                    quoteChar = '\0';
                }
            }
        }
    }
}
