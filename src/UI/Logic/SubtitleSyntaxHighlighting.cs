using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public partial class SubtitleSyntaxHighlighting : DocumentColorizingTransformer
{
    // Pastel color scheme for HTML and ASS/SSA syntax highlighting
    private static readonly Color ElementColor = Color.FromRgb(183, 89, 155);    // Soft purple - HTML element tags (e.g., <div>, <span>) / ASS tag names
    private static readonly Color AttributeColor = Color.FromRgb(86, 156, 214);  // Soft blue - HTML attribute names (e.g., class, id, style)
    private static readonly Color CommentColor = Color.FromRgb(106, 153, 85);    // Soft green - HTML comments (<!-- -->)
    private static readonly Color CharsColor = Color.FromRgb(128, 128, 128);     // Gray - delimiters and special chars (<, >, ", ', =, {, }, \)
    private static readonly Color ValuesColor = Color.FromRgb(206, 145, 120);    // Soft orange/peach - attribute values (e.g., "myclass") / ASS tag values
    private static readonly Color StyleColor = Color.FromRgb(156, 220, 254);     // Light cyan - CSS property values in style attribute

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

    private static Color? TryParseColor(string colorValue)
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

    private static Color? TryParseAssColor(string colorValue)
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

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineStartOffset = line.Offset;
        var text = CurrentContext.Document.GetText(line);

        var inComment = false;
        var inHtmlTag = false;
        var inAttributeVal = false;
        var quoteChar = '\0';
        var lastAttributeName = string.Empty;

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
                    ChangeLinePart(lineStartOffset + i, lineStartOffset + i + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

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
                        ChangeLinePart(lineStartOffset + currentPos, lineStartOffset + currentPos + 1, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                        });

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
                        if (tagNameEnd > tagNameStart)
                        {
                            ChangeLinePart(lineStartOffset + tagNameStart, lineStartOffset + tagNameEnd, element =>
                            {
                                element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ElementColor));
                            });
                        }

                        // Color tag value/parameters (e.g., "1", "Arial", "&HFFFFFF&")
                        if (tagNameEnd < thisTagEnd)
                        {
                            var tagName = text[tagNameStart..tagNameEnd];
                            var tagValue = text[tagNameEnd..thisTagEnd];
                            
                            // Check if this is a color tag (c, 1c, 2c, 3c, 4c)
                            if (tagName.Equals("c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("1c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("2c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("3c", StringComparison.OrdinalIgnoreCase) ||
                                tagName.Equals("4c", StringComparison.OrdinalIgnoreCase))
                            {
                                var assColor = TryParseAssColor(tagValue);
                                if (assColor.HasValue)
                                {
                                    ChangeLinePart(lineStartOffset + tagNameEnd, lineStartOffset + thisTagEnd, element =>
                                    {
                                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(assColor.Value));
                                    });
                                }
                                else
                                {
                                    ChangeLinePart(lineStartOffset + tagNameEnd, lineStartOffset + thisTagEnd, element =>
                                    {
                                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ValuesColor));
                                    });
                                }
                            }
                            else
                            {
                                ChangeLinePart(lineStartOffset + tagNameEnd, lineStartOffset + thisTagEnd, element =>
                                {
                                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(ValuesColor));
                                });
                            }
                        }

                        currentPos = thisTagEnd;
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
                if (i + 3 < text.Length && c2 == '!' && text[i + 2] == '-' && text[i + 3] == '-')
                {
                    // Comment start: <!--
                    var commentEnd = text.IndexOf("-->", i + 4, StringComparison.Ordinal);
                    var commentLength = commentEnd != -1 ? commentEnd + 3 - i : text.Length - i;
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
                    lastAttributeName = string.Empty;

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
                lastAttributeName = string.Empty;
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
                lastAttributeName = string.Empty;
                i++;
            }
            else if (inHtmlTag && !inAttributeVal && char.IsLetter(c))
            {
                // Attribute name
                var attrStart = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '-' || text[i] == '_'))
                {
                    i++;
                }

                lastAttributeName = text[attrStart..i];

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
                    var valueEnd = text.IndexOf(quoteChar, i + 1);
                    if (valueEnd == -1)
                    {
                        valueEnd = text.Length;
                    }
                    else
                    {
                        valueEnd++;
                    }

                    // Color the quotes
                    ChangeLinePart(lineStartOffset + valueStart, lineStartOffset + valueStart + 1, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(CharsColor));
                    });

                    // Get the actual value content
                    var valueContent = valueEnd > valueStart + 1 ? text[(valueStart + 1)..(valueEnd - 1)] : string.Empty;

                    // Determine the color to use for the value
                    Color? valueColor = null;

                    // Check if this is a color attribute
                    if (lastAttributeName.Equals("color", StringComparison.OrdinalIgnoreCase))
                    {
                        valueColor = TryParseColor(valueContent);
                    }

                    // If not a color attribute or color parsing failed, use default logic
                    if (valueColor == null)
                    {
                        var hasColon = text.IndexOf(':', i + 1, valueEnd - i - 2) != -1;
                        valueColor = hasColon ? StyleColor : ValuesColor;
                    }

                    if (valueEnd > valueStart + 1)
                    {
                        var colorToUse = valueColor.Value;
                        ChangeLinePart(lineStartOffset + valueStart + 1, lineStartOffset + valueEnd - 1, element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(colorToUse));
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
