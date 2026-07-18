using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Splits subtitle text into colored ranges for HTML tags and ASS/SSA override tags, and owns
/// the pastel color scheme plus the color-value parsing shared by every subtitle syntax
/// renderer: the edit box (<see cref="Controls.SyntaxHighlightingTextBox"/> via its presenter)
/// and the subtitle grid (<see cref="ValueConverters.TextWithSubtitleSyntaxHighlightingConverter"/>).
/// </summary>
public static class SubtitleSyntaxTokenizer
{
    // Pastel color scheme for HTML and ASS/SSA syntax highlighting. The pastels are
    // deliberately the same in both themes: tags are de-emphasized markup, and the soft
    // mid-tones read as muted against both a light and a dark background - darker variants
    // just make tags look like normal text next to the near-black light-theme foreground.
    // Only StyleColor needs a theme split: light cyan is illegible on white.
    internal static readonly Color ElementColor = Color.FromRgb(183, 89, 155);    // Soft purple - HTML element tags (e.g., <div>, <span>) / ASS tag names
    internal static readonly Color AttributeColor = Color.FromRgb(86, 156, 214);  // Soft blue - HTML attribute names (e.g., class, id, style)
    internal static readonly Color CommentColor = Color.FromRgb(106, 153, 85);    // Soft green - HTML comments (<!-- -->)
    internal static readonly Color CharsColor = Color.FromRgb(128, 128, 128);     // Gray - delimiters and special chars (<, >, ", ', =, {, }, \)
    internal static readonly Color ValuesColor = Color.FromRgb(206, 145, 120);    // Soft orange/peach - attribute values (e.g., "myclass") / ASS tag values
    private static readonly Color StyleColorDark = Color.FromRgb(156, 220, 254);  // Light cyan - CSS property values in style attribute
    private static readonly Color StyleColorLight = Color.FromRgb(0, 108, 155);   // Teal-blue - same role on a white background

    internal static Color StyleColor => UiTheme.IsDarkThemeEnabled() ? StyleColorDark : StyleColorLight;

    // Named HTML colors mapping
    private static readonly Dictionary<string, Color> NamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        { "aliceblue", Color.FromRgb(240, 248, 255) },
        { "antiquewhite", Color.FromRgb(250, 235, 215) },
        { "aqua", Color.FromRgb(0, 255, 255) },
        { "aquamarine", Color.FromRgb(127, 255, 212) },
        { "azure", Color.FromRgb(240, 255, 255) },
        { "beige", Color.FromRgb(245, 245, 220) },
        { "bisque", Color.FromRgb(255, 228, 196) },
        { "black", Color.FromRgb(0, 0, 0) },
        { "blanchedalmond", Color.FromRgb(255, 235, 205) },
        { "blue", Color.FromRgb(0, 0, 255) },
        { "blueviolet", Color.FromRgb(138, 43, 226) },
        { "brown", Color.FromRgb(165, 42, 42) },
        { "burlywood", Color.FromRgb(222, 184, 135) },
        { "cadetblue", Color.FromRgb(95, 158, 160) },
        { "chartreuse", Color.FromRgb(127, 255, 0) },
        { "chocolate", Color.FromRgb(210, 105, 30) },
        { "coral", Color.FromRgb(255, 127, 80) },
        { "cornflowerblue", Color.FromRgb(100, 149, 237) },
        { "cornsilk", Color.FromRgb(255, 248, 220) },
        { "crimson", Color.FromRgb(220, 20, 60) },
        { "cyan", Color.FromRgb(0, 255, 255) },
        { "darkblue", Color.FromRgb(0, 0, 139) },
        { "darkcyan", Color.FromRgb(0, 139, 139) },
        { "darkgoldenrod", Color.FromRgb(184, 134, 11) },
        { "darkgray", Color.FromRgb(169, 169, 169) },
        { "darkgreen", Color.FromRgb(0, 100, 0) },
        { "darkgrey", Color.FromRgb(169, 169, 169) },
        { "darkkhaki", Color.FromRgb(189, 183, 107) },
        { "darkmagenta", Color.FromRgb(139, 0, 139) },
        { "darkolivegreen", Color.FromRgb(85, 107, 47) },
        { "darkorange", Color.FromRgb(255, 140, 0) },
        { "darkorchid", Color.FromRgb(153, 50, 204) },
        { "darkred", Color.FromRgb(139, 0, 0) },
        { "darksalmon", Color.FromRgb(233, 150, 122) },
        { "darkseagreen", Color.FromRgb(143, 188, 143) },
        { "darkslateblue", Color.FromRgb(72, 61, 139) },
        { "darkslategray", Color.FromRgb(47, 79, 79) },
        { "darkslategrey", Color.FromRgb(47, 79, 79) },
        { "darkturquoise", Color.FromRgb(0, 206, 209) },
        { "darkviolet", Color.FromRgb(148, 0, 211) },
        { "deeppink", Color.FromRgb(255, 20, 147) },
        { "deepskyblue", Color.FromRgb(0, 191, 255) },
        { "dimgray", Color.FromRgb(105, 105, 105) },
        { "dimgrey", Color.FromRgb(105, 105, 105) },
        { "dodgerblue", Color.FromRgb(30, 144, 255) },
        { "firebrick", Color.FromRgb(178, 34, 34) },
        { "floralwhite", Color.FromRgb(255, 250, 240) },
        { "forestgreen", Color.FromRgb(34, 139, 34) },
        { "fuchsia", Color.FromRgb(255, 0, 255) },
        { "gainsboro", Color.FromRgb(220, 220, 220) },
        { "ghostwhite", Color.FromRgb(248, 248, 255) },
        { "gold", Color.FromRgb(255, 215, 0) },
        { "goldenrod", Color.FromRgb(218, 165, 32) },
        { "gray", Color.FromRgb(128, 128, 128) },
        { "green", Color.FromRgb(0, 128, 0) },
        { "greenyellow", Color.FromRgb(173, 255, 47) },
        { "grey", Color.FromRgb(128, 128, 128) },
        { "honeydew", Color.FromRgb(240, 255, 240) },
        { "hotpink", Color.FromRgb(255, 105, 180) },
        { "indianred", Color.FromRgb(205, 92, 92) },
        { "indigo", Color.FromRgb(75, 0, 130) },
        { "ivory", Color.FromRgb(255, 255, 240) },
        { "khaki", Color.FromRgb(240, 230, 140) },
        { "lavender", Color.FromRgb(230, 230, 250) },
        { "lavenderblush", Color.FromRgb(255, 240, 245) },
        { "lawngreen", Color.FromRgb(124, 252, 0) },
        { "lemonchiffon", Color.FromRgb(255, 250, 205) },
        { "lightblue", Color.FromRgb(173, 216, 230) },
        { "lightcoral", Color.FromRgb(240, 128, 128) },
        { "lightcyan", Color.FromRgb(224, 255, 255) },
        { "lightgoldenrodyellow", Color.FromRgb(250, 250, 210) },
        { "lightgray", Color.FromRgb(211, 211, 211) },
        { "lightgreen", Color.FromRgb(144, 238, 144) },
        { "lightgrey", Color.FromRgb(211, 211, 211) },
        { "lightpink", Color.FromRgb(255, 182, 193) },
        { "lightsalmon", Color.FromRgb(255, 160, 122) },
        { "lightseagreen", Color.FromRgb(32, 178, 170) },
        { "lightskyblue", Color.FromRgb(135, 206, 250) },
        { "lightslategray", Color.FromRgb(119, 136, 153) },
        { "lightslategrey", Color.FromRgb(119, 136, 153) },
        { "lightsteelblue", Color.FromRgb(176, 196, 222) },
        { "lightyellow", Color.FromRgb(255, 255, 224) },
        { "lime", Color.FromRgb(0, 255, 0) },
        { "limegreen", Color.FromRgb(50, 205, 50) },
        { "linen", Color.FromRgb(250, 240, 230) },
        { "magenta", Color.FromRgb(255, 0, 255) },
        { "maroon", Color.FromRgb(128, 0, 0) },
        { "mediumaquamarine", Color.FromRgb(102, 205, 170) },
        { "mediumblue", Color.FromRgb(0, 0, 205) },
        { "mediumorchid", Color.FromRgb(186, 85, 211) },
        { "mediumpurple", Color.FromRgb(147, 112, 219) },
        { "mediumseagreen", Color.FromRgb(60, 179, 113) },
        { "mediumslateblue", Color.FromRgb(123, 104, 238) },
        { "mediumspringgreen", Color.FromRgb(0, 250, 154) },
        { "mediumturquoise", Color.FromRgb(72, 209, 204) },
        { "mediumvioletred", Color.FromRgb(199, 21, 133) },
        { "midnightblue", Color.FromRgb(25, 25, 112) },
        { "mintcream", Color.FromRgb(245, 255, 250) },
        { "mistyrose", Color.FromRgb(255, 228, 225) },
        { "moccasin", Color.FromRgb(255, 228, 181) },
        { "navajowhite", Color.FromRgb(255, 222, 173) },
        { "navy", Color.FromRgb(0, 0, 128) },
        { "oldlace", Color.FromRgb(253, 245, 230) },
        { "olive", Color.FromRgb(128, 128, 0) },
        { "olivedrab", Color.FromRgb(107, 142, 35) },
        { "orange", Color.FromRgb(255, 165, 0) },
        { "orangered", Color.FromRgb(255, 69, 0) },
        { "orchid", Color.FromRgb(218, 112, 214) },
        { "palegoldenrod", Color.FromRgb(238, 232, 170) },
        { "palegreen", Color.FromRgb(152, 251, 152) },
        { "paleturquoise", Color.FromRgb(175, 238, 238) },
        { "palevioletred", Color.FromRgb(219, 112, 147) },
        { "papayawhip", Color.FromRgb(255, 239, 213) },
        { "peachpuff", Color.FromRgb(255, 218, 185) },
        { "peru", Color.FromRgb(205, 133, 63) },
        { "pink", Color.FromRgb(255, 192, 203) },
        { "plum", Color.FromRgb(221, 160, 221) },
        { "powderblue", Color.FromRgb(176, 224, 230) },
        { "purple", Color.FromRgb(128, 0, 128) },
        { "rebeccapurple", Color.FromRgb(102, 51, 153) },
        { "red", Color.FromRgb(255, 0, 0) },
        { "rosybrown", Color.FromRgb(188, 143, 143) },
        { "royalblue", Color.FromRgb(65, 105, 225) },
        { "saddlebrown", Color.FromRgb(139, 69, 19) },
        { "salmon", Color.FromRgb(250, 128, 114) },
        { "sandybrown", Color.FromRgb(244, 164, 96) },
        { "seagreen", Color.FromRgb(46, 139, 87) },
        { "seashell", Color.FromRgb(255, 245, 238) },
        { "sienna", Color.FromRgb(160, 82, 45) },
        { "silver", Color.FromRgb(192, 192, 192) },
        { "skyblue", Color.FromRgb(135, 206, 235) },
        { "slateblue", Color.FromRgb(106, 90, 205) },
        { "slategray", Color.FromRgb(112, 128, 144) },
        { "slategrey", Color.FromRgb(112, 128, 144) },
        { "snow", Color.FromRgb(255, 250, 250) },
        { "springgreen", Color.FromRgb(0, 255, 127) },
        { "steelblue", Color.FromRgb(70, 130, 180) },
        { "tan", Color.FromRgb(210, 180, 140) },
        { "teal", Color.FromRgb(0, 128, 128) },
        { "thistle", Color.FromRgb(216, 191, 216) },
        { "tomato", Color.FromRgb(255, 99, 71) },
        { "turquoise", Color.FromRgb(64, 224, 208) },
        { "violet", Color.FromRgb(238, 130, 238) },
        { "wheat", Color.FromRgb(245, 222, 179) },
        { "white", Color.FromRgb(255, 255, 255) },
        { "whitesmoke", Color.FromRgb(245, 245, 245) },
        { "yellow", Color.FromRgb(255, 255, 0) },
        { "yellowgreen", Color.FromRgb(154, 205, 50) }
    };

    internal static Color? TryParseColor(string colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
        {
            return null;
        }

        colorValue = colorValue.Trim();

        // Check named colors
        if (NamedColors.TryGetValue(colorValue, out var namedColor))
        {
            return namedColor;
        }

        // Check hex colors
        if (colorValue.StartsWith('#'))
        {
            var hex = colorValue[1..];
            try
            {
                if (hex.Length == 3)
                {
                    // Short hex format: #RGB -> #RRGGBB
                    var r = Convert.ToByte(new string(hex[0], 2), 16);
                    var g = Convert.ToByte(new string(hex[1], 2), 16);
                    var b = Convert.ToByte(new string(hex[2], 2), 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 6)
                {
                    // Standard hex format: #RRGGBB
                    var r = Convert.ToByte(hex[..2], 16);
                    var g = Convert.ToByte(hex[2..4], 16);
                    var b = Convert.ToByte(hex[4..6], 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 8)
                {
                    // Hex with alpha: #AARRGGBB or #RRGGBBAA
                    var r = Convert.ToByte(hex[2..4], 16);
                    var g = Convert.ToByte(hex[4..6], 16);
                    var b = Convert.ToByte(hex[6..8], 16);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    internal static Color? TryParseAssColor(string colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
        {
            return null;
        }

        colorValue = colorValue.Trim();

        // ASS/SSA color format: &HBBGGRR& or &HAABBGGRR&
        if (colorValue.StartsWith("&H", StringComparison.OrdinalIgnoreCase) && colorValue.EndsWith('&'))
        {
            var hex = colorValue.Substring(2, colorValue.Length - 3);
            try
            {
                if (hex.Length == 6)
                {
                    // Format: &HBBGGRR& (BGR format)
                    var b = Convert.ToByte(hex[..2], 16);
                    var g = Convert.ToByte(hex[2..4], 16);
                    var r = Convert.ToByte(hex[4..6], 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 8)
                {
                    // Format: &HAABBGGRR& (ABGR format)
                    var b = Convert.ToByte(hex[2..4], 16);
                    var g = Convert.ToByte(hex[4..6], 16);
                    var r = Convert.ToByte(hex[6..8], 16);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the end of the ASS/SSA override tag name starting at <paramref name="tagNameStart"/>.
    /// Tag names are letters, optionally preceded by a single digit (\1c-\4c, \1a-\4a); a digit
    /// not followed by a letter belongs to the value (e.g. \b1), not the name.
    /// </summary>
    internal static int GetAssTagNameEnd(string text, int tagNameStart, int tagEnd)
    {
        var tagNameEnd = tagNameStart;
        if (tagNameEnd + 1 < tagEnd && char.IsDigit(text[tagNameEnd]) && char.IsLetter(text[tagNameEnd + 1]))
        {
            tagNameEnd++;
        }

        while (tagNameEnd < tagEnd && char.IsLetter(text[tagNameEnd]))
        {
            tagNameEnd++;
        }

        return tagNameEnd;
    }

    /// <summary>
    /// True for the ASS/SSA color tags whose value is rendered in its actual color
    /// (c, 1c, 2c, 3c, 4c).
    /// </summary>
    internal static bool IsAssColorTag(ReadOnlySpan<char> tagName)
    {
        return tagName.Equals("c", StringComparison.OrdinalIgnoreCase) ||
               tagName.Equals("1c", StringComparison.OrdinalIgnoreCase) ||
               tagName.Equals("2c", StringComparison.OrdinalIgnoreCase) ||
               tagName.Equals("3c", StringComparison.OrdinalIgnoreCase) ||
               tagName.Equals("4c", StringComparison.OrdinalIgnoreCase);
    }

    public readonly record struct ColoredRange(int Start, int Length, Color Color);

    /// <summary>
    /// Returns sorted, non-overlapping colored ranges; text outside the ranges keeps the
    /// default foreground.
    /// </summary>
    public static List<ColoredRange> Tokenize(string text)
    {
        var ranges = new List<ColoredRange>();

        // Snapshot the theme-dependent palette once per pass.
        var elementColor = ElementColor;
        var attributeColor = AttributeColor;
        var commentColor = CommentColor;
        var charsColor = CharsColor;
        var valuesColor = ValuesColor;
        var styleColor = StyleColor;

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
                    Add(i, i + 1, charsColor);

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
                        Add(currentPos, currentPos + 1, charsColor);

                        // Find where this tag ends (next backslash or closing brace)
                        var nextBackslash = text.IndexOf('\\', currentPos + 1);
                        var thisTagEnd = (nextBackslash != -1 && nextBackslash < tagEnd) ? nextBackslash : tagEnd;

                        var tagNameStart = currentPos + 1;
                        var tagNameEnd = GetAssTagNameEnd(text, tagNameStart, thisTagEnd);

                        // Color tag name (e.g., "i", "b", "fn", "fs", "c", "1c", etc.)
                        Add(tagNameStart, tagNameEnd, elementColor);

                        // Color tag value/parameters (e.g., "1", "Arial", "&HFFFFFF&")
                        if (tagNameEnd < thisTagEnd)
                        {
                            var tagName = text.AsSpan(tagNameStart, tagNameEnd - tagNameStart);

                            Color? assColor = null;
                            if (IsAssColorTag(tagName))
                            {
                                assColor = TryParseAssColor(text[tagNameEnd..thisTagEnd]);
                            }

                            Add(tagNameEnd, thisTagEnd, assColor ?? valuesColor);
                        }

                        currentPos = thisTagEnd;
                    }

                    // Color closing brace
                    Add(tagEnd, tagEnd + 1, charsColor);

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
                    Add(i, i + commentLength, commentColor);
                    i += commentLength - 1;
                    continue;
                }
                else
                {
                    // HTML tag start
                    Add(i, i + 1, charsColor);

                    if (c2 == '/')
                    {
                        // Closing tag
                        Add(i + 1, i + 2, charsColor);
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
                        Add(elementStart, elementEnd, elementColor);
                        i = elementEnd - 1;
                    }
                }
            }
            else if (inHtmlTag && c == '>')
            {
                // HTML tag end
                Add(i, i + 1, charsColor);
                inHtmlTag = false;
                lastAttributeIsColor = false;
            }
            else if (inHtmlTag && c == '/' && c2 == '>')
            {
                // Self-closing tag
                Add(i, i + 2, charsColor);
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

                Add(attrStart, i, attributeColor);
                i--;
            }
            else if (inHtmlTag && c == '=')
            {
                // Equals sign
                Add(i, i + 1, charsColor);
            }
            else if (inHtmlTag && (c == '"' || c == '\''))
            {
                // Start of attribute value
                var quoteChar = c;
                var valueStart = i;
                var valueEnd = text.IndexOf(quoteChar, i + 1);
                valueEnd = valueEnd == -1 ? text.Length : valueEnd + 1;

                // Color the opening quote
                Add(valueStart, valueStart + 1, charsColor);

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
                    valueColor = TryParseColor(valueContent.ToString());
                }

                // If not a color attribute or color parsing failed, use default logic
                valueColor ??= valueContent.Contains(':') ? styleColor : valuesColor;

                if (valueEnd > valueStart + 1)
                {
                    Add(contentStart, contentEnd, valueColor.Value);

                    // Color closing quote
                    Add(contentEnd, valueEnd, charsColor);
                }

                i = valueEnd - 1;
            }
        }

        return ranges;
    }
}
