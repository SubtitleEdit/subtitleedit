using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Splits subtitle text into colored ranges for HTML tags and ASS/SSA override tags.
/// The scanning logic and color scheme are a port of <see cref="SubtitleSyntaxHighlighting"/>
/// (the AvaloniaEdit colorizer), so the SyntaxHighlightingTextBox looks the same as the
/// AvaloniaEdit based editor. Keep the two in sync when changing either.
/// </summary>
public static class SubtitleSyntaxTokenizer
{
    public readonly record struct ColoredRange(int Start, int Length, Color Color);

    /// <summary>
    /// Returns sorted, non-overlapping colored ranges; text outside the ranges keeps the
    /// default foreground.
    /// </summary>
    public static List<ColoredRange> Tokenize(string text)
    {
        var ranges = new List<ColoredRange>();

        void Add(int start, int end, Color color)
        {
            if (end > start)
            {
                ranges.Add(new ColoredRange(start, end - start, color));
            }
        }

        var inHtmlTag = false;
        var lastAttributeIsColor = false;

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var c2 = i + 1 < text.Length ? text[i + 1] : '\0';

            // Handle ASS/SSA tags: {\tag} or {\tagValue} or {\tag1\tag2\tag3}
            if (c == '{' && c2 == '\\')
            {
                var tagEnd = text.IndexOf('}', i + 2);
                if (tagEnd != -1)
                {
                    // Color opening brace
                    Add(i, i + 1, SubtitleSyntaxHighlighting.CharsColor);

                    // Process all tags within the braces (separated by backslashes)
                    var currentPos = i + 1;
                    while (currentPos < tagEnd)
                    {
                        if (text[currentPos] != '\\')
                        {
                            currentPos++;
                            continue;
                        }

                        // Color the backslash
                        Add(currentPos, currentPos + 1, SubtitleSyntaxHighlighting.CharsColor);

                        // Find where this tag ends (next backslash or closing brace)
                        var nextBackslash = text.IndexOf('\\', currentPos + 1);
                        var thisTagEnd = (nextBackslash != -1 && nextBackslash < tagEnd) ? nextBackslash : tagEnd;

                        // Find where the tag name ends (before any numbers or special chars)
                        var tagNameStart = currentPos + 1;
                        var tagNameEnd = tagNameStart;
                        while (tagNameEnd < thisTagEnd && char.IsLetter(text[tagNameEnd]))
                        {
                            tagNameEnd++;
                        }

                        // Color tag name (e.g., "i", "b", "fn", "fs", "c", "1c", etc.)
                        Add(tagNameStart, tagNameEnd, SubtitleSyntaxHighlighting.ElementColor);

                        // Color tag value/parameters (e.g., "1", "Arial", "&HFFFFFF&")
                        if (tagNameEnd < thisTagEnd)
                        {
                            var tagName = text.AsSpan(tagNameStart, tagNameEnd - tagNameStart);

                            // Check if this is a color tag (c, 1c, 2c, 3c, 4c)
                            Color? assColor = null;
                            if (tagName.Equals("c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("1c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("2c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("3c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("4c", StringComparison.OrdinalIgnoreCase))
                            {
                                assColor = SubtitleSyntaxHighlighting.TryParseAssColor(text[tagNameEnd..thisTagEnd]);
                            }

                            Add(tagNameEnd, thisTagEnd, assColor ?? SubtitleSyntaxHighlighting.ValuesColor);
                        }

                        currentPos = thisTagEnd;
                    }

                    // Color closing brace
                    Add(tagEnd, tagEnd + 1, SubtitleSyntaxHighlighting.CharsColor);

                    i = tagEnd;
                    continue;
                }
            }

            if (c == '<')
            {
                if (i + 3 < text.Length && c2 == '!' && text[i + 2] == '-' && text[i + 3] == '-')
                {
                    // Comment start: <!--
                    var commentEnd = text.IndexOf("-->", i + 4, StringComparison.Ordinal);
                    var commentLength = commentEnd != -1 ? commentEnd + 3 - i : text.Length - i;
                    Add(i, i + commentLength, SubtitleSyntaxHighlighting.CommentColor);
                    i += commentLength - 1;
                    continue;
                }
                else
                {
                    // HTML tag start
                    Add(i, i + 1, SubtitleSyntaxHighlighting.CharsColor);

                    if (c2 == '/')
                    {
                        // Closing tag
                        Add(i + 1, i + 2, SubtitleSyntaxHighlighting.CharsColor);
                        i++;
                    }

                    inHtmlTag = true;
                    lastAttributeIsColor = false;

                    // Find element name end
                    var elementStart = i + 1;
                    if (elementStart >= text.Length)
                    {
                        continue;
                    }

                    if (text[elementStart] == '/')
                    {
                        elementStart++;
                    }

                    var elementEnd = elementStart;
                    while (elementEnd < text.Length && !char.IsWhiteSpace(text[elementEnd]) &&
                           text[elementEnd] != '>' && text[elementEnd] != '/')
                    {
                        elementEnd++;
                    }

                    if (elementEnd > elementStart)
                    {
                        Add(elementStart, elementEnd, SubtitleSyntaxHighlighting.ElementColor);
                        i = elementEnd - 1;
                    }
                }
            }
            else if (inHtmlTag && c == '>')
            {
                // HTML tag end
                Add(i, i + 1, SubtitleSyntaxHighlighting.CharsColor);
                inHtmlTag = false;
                lastAttributeIsColor = false;
            }
            else if (inHtmlTag && c == '/' && c2 == '>')
            {
                // Self-closing tag
                Add(i, i + 2, SubtitleSyntaxHighlighting.CharsColor);
                inHtmlTag = false;
                lastAttributeIsColor = false;
                i++;
            }
            else if (inHtmlTag && char.IsLetter(c))
            {
                // Attribute name
                var attrStart = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '-' || text[i] == '_'))
                {
                    i++;
                }

                lastAttributeIsColor = text.AsSpan(attrStart, i - attrStart).Equals("color", StringComparison.OrdinalIgnoreCase);

                Add(attrStart, i, SubtitleSyntaxHighlighting.AttributeColor);
                i--;
            }
            else if (inHtmlTag && c == '=')
            {
                // Equals sign
                Add(i, i + 1, SubtitleSyntaxHighlighting.CharsColor);
            }
            else if (inHtmlTag && (c == '"' || c == '\''))
            {
                // Start of attribute value
                var quoteChar = c;
                var valueStart = i;
                var valueEnd = text.IndexOf(quoteChar, i + 1);
                valueEnd = valueEnd == -1 ? text.Length : valueEnd + 1;

                // Color the opening quote
                Add(valueStart, valueStart + 1, SubtitleSyntaxHighlighting.CharsColor);

                // The value content: between the quotes; when the closing quote is missing the
                // last character stays out, matching the original AvaloniaEdit colorizer.
                var contentStart = valueStart + 1;
                var contentEnd = Math.Max(contentStart, valueEnd - 1);
                var valueContent = text.AsSpan(contentStart, contentEnd - contentStart);

                // Determine the color to use for the value
                Color? valueColor = null;

                // Check if this is a color attribute
                if (lastAttributeIsColor && !valueContent.IsEmpty)
                {
                    valueColor = SubtitleSyntaxHighlighting.TryParseColor(valueContent.ToString());
                }

                // If not a color attribute or color parsing failed, use default logic
                valueColor ??= valueContent.Contains(':') ? SubtitleSyntaxHighlighting.StyleColor : SubtitleSyntaxHighlighting.ValuesColor;

                if (valueEnd > valueStart + 1)
                {
                    Add(contentStart, contentEnd, valueColor.Value);

                    // Color closing quote
                    Add(contentEnd, valueEnd, SubtitleSyntaxHighlighting.CharsColor);
                }

                i = valueEnd - 1;
            }
        }

        return ranges;
    }
}
