using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TextWithSubtitleSyntaxHighlightingConverter : IValueConverter
{
    private ISpellCheckManager? _spellCheckManager;

    // Pastel color scheme for HTML and ASS/SSA syntax highlighting
    private static readonly Color ElementColor = Color.FromRgb(183, 89, 155);    // Soft purple - HTML element tags / ASS tag names
    private static readonly Color AttributeColor = Color.FromRgb(86, 156, 214);  // Soft blue - HTML attribute names
    private static readonly Color CommentColor = Color.FromRgb(106, 153, 85);    // Soft green - HTML comments
    private static readonly Color CharsColor = Color.FromRgb(128, 128, 128);     // Gray - delimiters and special chars
    private static readonly Color ValuesColor = Color.FromRgb(206, 145, 120);    // Soft orange/peach - attribute values / ASS tag values
    private static readonly Color StyleColor = Color.FromRgb(156, 220, 254);     // Light cyan - CSS property values

    // Reuse brushes instead of creating new ones each time
    private static readonly SolidColorBrush ElementBrush = new(ElementColor);
    private static readonly SolidColorBrush AttributeBrush = new(AttributeColor);
    private static readonly SolidColorBrush CommentBrush = new(CommentColor);
    private static readonly SolidColorBrush CharsBrush = new(CharsColor);
    private static readonly SolidColorBrush ValuesBrush = new(ValuesColor);
    private static readonly SolidColorBrush StyleBrush = new(StyleColor);

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
                    var r = System.Convert.ToByte(new string(hex[0], 2), 16);
                    var g = System.Convert.ToByte(new string(hex[1], 2), 16);
                    var b = System.Convert.ToByte(new string(hex[2], 2), 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 6)
                {
                    // Standard hex format: #RRGGBB
                    var r = System.Convert.ToByte(hex[..2], 16);
                    var g = System.Convert.ToByte(hex[2..4], 16);
                    var b = System.Convert.ToByte(hex[4..6], 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 8)
                {
                    // Hex with alpha: #AARRGGBB or #RRGGBBAA
                    var r = System.Convert.ToByte(hex[2..4], 16);
                    var g = System.Convert.ToByte(hex[4..6], 16);
                    var b = System.Convert.ToByte(hex[6..8], 16);
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
                    var b = System.Convert.ToByte(hex[..2], 16);
                    var g = System.Convert.ToByte(hex[2..4], 16);
                    var r = System.Convert.ToByte(hex[4..6], 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 8)
                {
                    // Format: &HAABBGGRR& (ABGR format)
                    var b = System.Convert.ToByte(hex[2..4], 16);
                    var g = System.Convert.ToByte(hex[4..6], 16);
                    var r = System.Convert.ToByte(hex[6..8], 16);
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

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str || string.IsNullOrEmpty(str))
        {
            return new InlineCollection();
        }

        if (Se.Settings.Appearance.SubtitleGridTextSingleLine)
        {
            var separator = Se.Settings.Appearance.SubtitleGridTextSingleLineSeparator;
            if (string.IsNullOrEmpty(separator))
            {
                separator = " ";
            }

            str = str
                .Replace("\r\n", separator)
                .Replace("\n", separator);
        }

        // Truncate long strings for performance
        if (str.Length > 200)
        {
            str = str.Substring(0, 197).TrimEnd() + "...";
        }

        if (Se.Settings.Appearance.SubtitleGridFormattingType == (int)SubtitleGridFormattingTypes.ShowFormatting)
        {
            var lines = MakeShowFormatting(str);
            return SpellCheckLines(lines);
        }

        if (Se.Settings.Appearance.SubtitleGridFormattingType == (int)SubtitleGridFormattingTypes.ShowTags)
        {
            var lines = MakeShowTags(str);
            return SpellCheckLines(lines);
        }

        // No formatting (default)
        var inlines = new InlineCollection();
        inlines.Add(new Run(str));
        return SpellCheckLines(inlines);
    }

    private InlineCollection SpellCheckLines(InlineCollection lines)
    {
        if (_spellCheckManager == null || !Se.Settings.Appearance.SubtitleTextBoxLiveSpellCheck)
        {
            return lines;
        }

        try
        {
            var newInlines = new InlineCollection();

            foreach (var inline in lines)
            {
                if (inline is not Run run || string.IsNullOrWhiteSpace(run.Text))
                {
                    newInlines.Add(inline);
                    continue;
                }

                var runText = run.Text;
                var words = SpellCheckWordLists2.Split(runText);

                if (words.Count == 0)
                {
                    newInlines.Add(run);
                    continue;
                }

                // First pass: check if there are any misspelled words in this run
                var hasMisspelledWords = false;
                foreach (var word in words)
                {
                    if (string.IsNullOrWhiteSpace(word.Text) || word.Length < 2)
                    {
                        continue;
                    }

                    if (!IsSpecialPattern(word, runText) && !_spellCheckManager.IsWordCorrect(word.Text))
                    {
                        hasMisspelledWords = true;
                        break;
                    }
                }

                // If no misspelled words, keep the run as-is to preserve color inheritance
                if (!hasMisspelledWords)
                {
                    newInlines.Add(run);
                    continue;
                }

                // Second pass: break up the run and add underlines to misspelled words
                var lastIndex = 0;
                foreach (var word in words)
                {
                    if (string.IsNullOrWhiteSpace(word.Text) || word.Length < 2)
                    {
                        continue;
                    }

                    // Add any text before this word
                    if (word.Index > lastIndex)
                    {
                        var beforeText = runText.Substring(lastIndex, word.Index - lastIndex);
                        var beforeRun = CreateRunFromTemplate(run, beforeText);
                        newInlines.Add(beforeRun);
                    }

                    // Check if word is misspelled and not a special pattern
                    var isMisspelled = !IsSpecialPattern(word, runText) && !_spellCheckManager.IsWordCorrect(word.Text);

                    // Create run for the word
                    var wordRun = CreateRunFromTemplate(run, word.Text);
                    if (isMisspelled)
                    {
                        // Apply wavy red underline for misspelled words
                        var wavyUnderline = new TextDecoration
                        {
                            Location = TextDecorationLocation.Underline,
                            Stroke = new SolidColorBrush(Colors.Red),
                            StrokeThickness = 1.5,
                            StrokeLineCap = PenLineCap.Round
                        };

                        // Add to existing decorations instead of replacing them
                        if (wordRun.TextDecorations == null || wordRun.TextDecorations.Count == 0)
                        {
                            wordRun.TextDecorations = new TextDecorationCollection { wavyUnderline };
                        }
                        else
                        {
                            var decorations = new List<TextDecoration>(wordRun.TextDecorations)
                            {
                                wavyUnderline
                            };
                            wordRun.TextDecorations = new TextDecorationCollection(decorations);
                        }
                    }

                    newInlines.Add(wordRun);
                    lastIndex = word.Index + word.Length;
                }

                // Add any remaining text after the last word
                if (lastIndex < runText.Length)
                {
                    var remainingText = runText.Substring(lastIndex);
                    var remainingRun = CreateRunFromTemplate(run, remainingText);
                    newInlines.Add(remainingRun);
                }
            }

            return newInlines;
        }
        catch (Exception ex)
        {
            Se.LogError(ex);
            return lines; // Return original lines if spell check fails
        }
    }

    private static Run CreateRunFromTemplate(Run template, string text)
    {
        var run = new Run(text);

        // Use IsSet() to only copy properties that were explicitly set locally,
        // not inherited/resolved values - this preserves the inheritance chain
        if (template.IsSet(TextElement.ForegroundProperty))
            run.Foreground = template.Foreground;

        if (template.IsSet(TextElement.FontStyleProperty))
            run.FontStyle = template.FontStyle;

        if (template.IsSet(TextElement.FontWeightProperty))
            run.FontWeight = template.FontWeight;

        if (template.IsSet(TextElement.FontFamilyProperty))
            run.FontFamily = template.FontFamily;

        if (template.IsSet(TextElement.FontSizeProperty))
            run.FontSize = template.FontSize;

        if (template.IsSet(Inline.TextDecorationsProperty) &&
            template.TextDecorations != null && template.TextDecorations.Count > 0)
        {
            run.TextDecorations = new TextDecorationCollection(template.TextDecorations);
        }

        return run;
    }

    private static bool IsSpecialPattern(SpellCheckWord word, string text)
    {
        // Skip numbers
        if (word.Text.All(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-'))
        {
            return true;
        }

        // Skip URLs
        if (word.Text.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
            word.Text.Contains("https://", StringComparison.OrdinalIgnoreCase) ||
            word.Text.Contains("www.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip email-like patterns
        if (word.Text.Contains('@'))
        {
            return true;
        }

        // Skip hashtags
        if (word.Text.StartsWith('#'))
        {
            return true;
        }

        // Skip words inside ASS/SSA tags
        if (IsBetweenAssaTags(word, text))
        {
            return true;
        }

        // Skip words inside HTML tags
        if (IsInsideHtmlTag(word, text))
        {
            return true;
        }

        return false;
    }

    private static bool IsBetweenAssaTags(SpellCheckWord word, string text)
    {
        if (word == null || string.IsNullOrEmpty(text))
        {
            return false;
        }

        // Find the last occurrence of an opening brace before the word starts
        var openBrace = text.LastIndexOf('{', word.Index);

        // Find the first occurrence of a closing brace after the word starts
        var closeBrace = text.IndexOf('}', word.Index);

        // If both exist, check if there is another closing brace between
        // the opening brace and our word.
        if (openBrace != -1 && closeBrace != -1 && openBrace < closeBrace)
        {
            // Check if there's a '}' between the '{' and the word.
            var closingBeforeWord = text.IndexOf('}', openBrace, word.Index - openBrace);
            return closingBeforeWord == -1;
        }

        return false;
    }

    private static bool IsInsideHtmlTag(SpellCheckWord word, string text)
    {
        if (word == null || string.IsNullOrEmpty(text))
        {
            return false;
        }

        // Find the last opening bracket before the word
        var openBracket = text.LastIndexOf('<', word.Index);

        // Find the next closing bracket after the word starts
        var closeBracket = text.IndexOf('>', word.Index);

        // If both exist in the correct order
        if (openBracket != -1 && closeBracket != -1 && openBracket < closeBracket)
        {
            // Ensure there isn't a '>' between the opening '<' and the word
            var closingBeforeWord = text.IndexOf('>', openBracket, word.Index - openBracket);
            return closingBeforeWord == -1;
        }

        return false;
    }

    private static InlineCollection MakeShowTags(string str)
    {
        var inlines = new InlineCollection();
        var i = 0;
        while (i < str.Length)
        {
            var c = str[i];
            var c2 = i + 1 < str.Length ? str[i + 1] : '\0';

            // Handle ASS/SSA tags: {\tag} or {\tagValue} or {\tag1\tag2\tag3}
            if (c == '{' && c2 == '\\')
            {
                var tagEnd = str.IndexOf('}', i + 2);
                if (tagEnd != -1)
                {
                    // Add opening brace
                    inlines.Add(new Run("{") { Foreground = CharsBrush });

                    // Process all tags within the braces (separated by backslashes)
                    var currentPos = i + 1;
                    while (currentPos < tagEnd)
                    {
                        if (str[currentPos] != '\\')
                        {
                            currentPos++;
                            continue;
                        }

                        // Add the backslash
                        inlines.Add(new Run("\\") { Foreground = CharsBrush });

                        // Find where this tag ends (next backslash or closing brace)
                        var nextBackslash = str.IndexOf('\\', currentPos + 1);
                        var thisTagEnd = (nextBackslash != -1 && nextBackslash < tagEnd) ? nextBackslash : tagEnd;

                        // Find where the tag name ends (before any numbers or special chars)
                        var tagNameStart = currentPos + 1;
                        var tagNameEnd = tagNameStart;
                        while (tagNameEnd < thisTagEnd && char.IsLetter(str[tagNameEnd]))
                        {
                            tagNameEnd++;
                        }

                        // Add tag name
                        if (tagNameEnd > tagNameStart)
                        {
                            inlines.Add(new Run(str.Substring(tagNameStart, tagNameEnd - tagNameStart))
                            {
                                Foreground = ElementBrush
                            });
                        }

                        // Add tag value/parameters (e.g., "1", "Arial", "&HFFFFFF&")
                        if (tagNameEnd < thisTagEnd)
                        {
                            var tagName = str.Substring(tagNameStart, tagNameEnd - tagNameStart);
                            var tagValue = str.Substring(tagNameEnd, thisTagEnd - tagNameEnd);
                            
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
                                    inlines.Add(new Run(tagValue)
                                    {
                                        Foreground = new SolidColorBrush(assColor.Value)
                                    });
                                }
                                else
                                {
                                    inlines.Add(new Run(tagValue)
                                    {
                                        Foreground = ValuesBrush
                                    });
                                }
                            }
                            else
                            {
                                inlines.Add(new Run(tagValue)
                                {
                                    Foreground = ValuesBrush
                                });
                            }
                        }

                        currentPos = thisTagEnd;
                    }

                    // Add closing brace
                    inlines.Add(new Run("}") { Foreground = CharsBrush });
                    i = tagEnd + 1;
                    continue;
                }
                else
                {
                    // Malformed ASS/SSA tag - treat as regular text
                    inlines.Add(new Run(c.ToString()));
                    i++;
                    continue;
                }
            }

            // Handle HTML comments
            if (c == '<' && i + 3 < str.Length && c2 == '!' && str[i + 2] == '-' && str[i + 3] == '-')
            {
                var commentEnd = str.IndexOf("-->", i + 4, StringComparison.Ordinal);
                var commentLength = commentEnd != -1 ? commentEnd + 3 - i : str.Length - i;
                inlines.Add(new Run(str.Substring(i, commentLength))
                {
                    Foreground = CommentBrush
                });
                i += commentLength;
                continue;
            }

            // Handle HTML tags
            if (c == '<')
            {
                var tagEnd = str.IndexOf('>', i + 1);
                if (tagEnd != -1)
                {
                    var tagContent = str.Substring(i, tagEnd - i + 1);
                    ParseHtmlTag(inlines, tagContent);
                    i = tagEnd + 1;
                    continue;
                }
                else
                {
                    // Malformed HTML tag - treat as regular text
                    inlines.Add(new Run(c.ToString()));
                    i++;
                    continue;
                }
            }

            // Handle line breaks
            if (c == '\n' || (c == '\r' && c2 == '\n'))
            {
                inlines.Add(new Run(Environment.NewLine));
                i += c == '\r' ? 2 : 1;
                continue;
            }

            // Regular text - add character by character until we hit special markup
            var textStart = i;
            while (i < str.Length && str[i] != '<' && str[i] != '{' && str[i] != '\r' && str[i] != '\n')
            {
                i++;
            }

            if (i > textStart)
            {
                inlines.Add(new Run(str.Substring(textStart, i - textStart)));
            }
            else
            {
                // Safety: if we didn't match any condition and haven't advanced, treat as regular character
                inlines.Add(new Run(str[i].ToString()));
                i++;
            }
        }

        return inlines;
    }

    private static InlineCollection MakeShowFormatting(string str)
    {
        // Track current formatting state
        var state = new FormattingState();
        var inlines = new InlineCollection();
        
        // Limit iterations to prevent infinite loops (should never exceed string length)
        var maxIterations = str.Length * 2; // Safety margin
        var iterations = 0;
        
        int i = 0;
        while (i < str.Length && iterations < maxIterations)
        {
            iterations++;
            var c = str[i];
            var c2 = i + 1 < str.Length ? str[i + 1] : '\0';

            // Handle ASS/SSA tags: {\tag} or {\tag1\tag2}
            if (c == '{' && c2 == '\\')
            {
                var tagEnd = str.IndexOf('}', i + 2);
                if (tagEnd != -1 && tagEnd - i < 500) // Limit tag length to prevent malicious input
                {
                    var tagContent = str.Substring(i + 1, tagEnd - i - 1); // Content between { and }
                    ParseAssaTags(tagContent, state);
                    i = tagEnd + 1;
                    continue;
                }
                // Malformed or overly long tag - treat as regular text
                var run = CreateFormattedRun(c.ToString(), state);
                inlines.Add(run);
                i++;
                continue;
            }

            // Handle HTML comments - skip them
            if (c == '<' && i + 3 < str.Length && c2 == '!' && str[i + 2] == '-' && str[i + 3] == '-')
            {
                var commentEnd = str.IndexOf("-->", i + 4, StringComparison.Ordinal);
                if (commentEnd != -1 && commentEnd - i < 1000) // Limit comment length
                {
                    i = commentEnd + 3;
                    continue;
                }
                // Malformed or overly long comment - skip to end
                i = str.Length;
                continue;
            }

            // Handle HTML tags
            if (c == '<')
            {
                var tagEnd = str.IndexOf('>', i + 1);
                if (tagEnd != -1 && tagEnd - i < 500) // Limit tag length
                {
                    var tagContent = str.Substring(i + 1, tagEnd - i - 1).Trim();
                    if (string.IsNullOrEmpty(tagContent))
                    {
                        // Empty tag - treat as regular text
                        var run = CreateFormattedRun(c.ToString(), state);
                        inlines.Add(run);
                        i++;
                        continue;
                    }

                    var isClosingTag = tagContent.StartsWith('/');
                    if (isClosingTag)
                    {
                        tagContent = tagContent.Substring(1).Trim();
                    }

                    var tagNameEnd = tagContent.IndexOfAny(new[] { ' ', '\t', '\r', '\n' });
                    var tagName = (tagNameEnd > 0 ? tagContent.Substring(0, tagNameEnd) : tagContent).ToLowerInvariant();

                    if (isClosingTag)
                    {
                        // Handle closing tags
                        switch (tagName)
                        {
                            case "i":
                                state.Italic = false;
                                break;
                            case "b":
                                state.Bold = false;
                                break;
                            case "u":
                                state.Underline = false;
                                break;
                            case "font":
                                state.Color = null;
                                state.FontName = null;
                                state.FontSize = null;
                                break;
                        }
                    }
                    else
                    {
                        // Handle opening tags
                        switch (tagName)
                        {
                            case "i":
                                state.Italic = true;
                                break;
                            case "b":
                                state.Bold = true;
                                break;
                            case "u":
                                state.Underline = true;
                                break;
                            case "font":
                                ParseFontTag(tagContent, state);
                                break;
                        }
                    }
                    i = tagEnd + 1;
                    continue;
                }
                // Malformed or overly long tag - treat as regular text
                var textRun = CreateFormattedRun(c.ToString(), state);
                inlines.Add(textRun);
                i++;
                continue;
            }

            // Handle line breaks
            if (c == '\n' || (c == '\r' && c2 == '\n'))
            {
                inlines.Add(new Run(Environment.NewLine));
                i += c == '\r' ? 2 : 1;
                continue;
            }

            // Regular text - collect characters until we hit special markup
            var textStart = i;
            var textLength = 0;
            
            // Scan ahead efficiently
            while (i < str.Length)
            {
                var ch = str[i];
                
                // Check for special characters that require immediate handling
                if (ch == '<' || ch == '\r' || ch == '\n')
                {
                    break;
                }
                
                // Check for ASS tag start
                if (ch == '{' && i + 1 < str.Length && str[i + 1] == '\\')
                {
                    break;
                }
                
                // Skip standalone '{' that are not ASS tags
                if (ch == '{' && (i + 1 >= str.Length || str[i + 1] != '\\'))
                {
                    i++;
                    textLength++;
                    continue;
                }
                
                i++;
                textLength++;
            }

            if (textLength > 0)
            {
                var text = str.Substring(textStart, textLength);
                var run = CreateFormattedRun(text, state);
                inlines.Add(run);
            }
        }

        return inlines;
    }

    private static Run CreateFormattedRun(string text, FormattingState state)
    {
        var run = new Run(text);

        // Apply italic
        if (state.Italic)
        {
            run.FontStyle = FontStyle.Italic;
        }

        // Apply bold
        if (state.Bold)
        {
            run.FontWeight = FontWeight.Bold;
        }

        // Apply underline
        if (state.Underline)
        {
            run.TextDecorations = TextDecorations.Underline;
        }

        // Apply color
        if (state.Color.HasValue)
        {
            run.Foreground = new SolidColorBrush(state.Color.Value);
        }

        // Apply font name
        if (!string.IsNullOrEmpty(state.FontName))
        {
            run.FontFamily = new FontFamily(state.FontName);
        }

        // Apply font size
        if (state.FontSize.HasValue && state.FontSize.Value > 0)
        {
            // Scale font size relative to default (assuming default is around 12-14pt)
            // We'll cap it to reasonable display sizes
            var fontSize = Math.Min(Math.Max(state.FontSize.Value * 0.8, 8), 24);
            run.FontSize = fontSize;
        }

        return run;
    }

    private static void ParseFontTag(string tagContent, FormattingState state)
    {
        if (string.IsNullOrEmpty(tagContent) || tagContent.Length > 500)
        {
            return; // Skip empty or excessively long tag content
        }

        try
        {
            // Use regex with timeout to prevent hanging
            var timeout = TimeSpan.FromMilliseconds(100);

            // Parse color attribute
            var colorMatch = Regex.Match(tagContent, @"color\s*=\s*[""']?([^""'\s>]+)[""']?", RegexOptions.IgnoreCase, timeout);
            if (colorMatch.Success)
            {
                var colorValue = colorMatch.Groups[1].Value;
                if (colorValue.Length <= 100)
                {
                    try
                    {
                        var skColor = HtmlUtil.GetColorFromString(colorValue);
                        state.Color = Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
                    }
                    catch
                    {
                        // Ignore invalid colors
                    }
                }
            }

            // Parse face (font name) attribute
            var faceMatch = Regex.Match(tagContent, @"face\s*=\s*[""']?([^""'>]+)[""']?", RegexOptions.IgnoreCase, timeout);
            if (faceMatch.Success)
            {
                var fontName = faceMatch.Groups[1].Value.Trim();
                if (fontName.Length > 0 && fontName.Length <= 100)
                {
                    state.FontName = fontName;
                }
            }

            // Parse size attribute
            var sizeMatch = Regex.Match(tagContent, @"size\s*=\s*[""']?(\d+)[""']?", RegexOptions.IgnoreCase, timeout);
            if (sizeMatch.Success && double.TryParse(sizeMatch.Groups[1].Value, out var size))
            {
                state.FontSize = size;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Regex timed out - skip parsing this tag
        }
        catch
        {
            // Ignore any other parsing errors
        }
    }

    private static void ParseAssaTags(string tagContent, FormattingState state)
    {
        if (string.IsNullOrEmpty(tagContent) || tagContent.Length > 500)
        {
            return; // Skip empty or excessively long tag content
        }

        // Split by backslash to get individual tags
        var tags = tagContent.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        
        // Limit number of tags to prevent excessive processing
        var maxTags = Math.Min(tags.Length, 50);

        for (var i = 0; i < maxTags; i++)
        {
            var tag = tags[i];
            var trimmedTag = tag.Trim();
            if (string.IsNullOrEmpty(trimmedTag) || trimmedTag.Length > 100)
            {
                continue; // Skip empty or excessively long individual tags
            }

            var firstChar = trimmedTag[0];
            var tagLen = trimmedTag.Length;

            // Reset: \r - reset all formatting (check first for performance)
            if (firstChar == 'r' && tagLen == 1)
            {
                state.Reset();
                continue;
            }

            // Italic: \i1 or \i0
            if (firstChar == 'i' && tagLen >= 2 && char.IsDigit(trimmedTag[1]))
            {
                state.Italic = trimmedTag[1] == '1';
                continue;
            }

            // Bold: \b1 or \b0 (can also be \b700 for weight)
            if (firstChar == 'b' && tagLen >= 2 && char.IsDigit(trimmedTag[1]))
            {
                var value = trimmedTag.Substring(1);
                if (value == "0")
                {
                    state.Bold = false;
                }
                else if (value == "1")
                {
                    state.Bold = true;
                }
                else if (value.Length <= 4 && int.TryParse(value, out var weight))
                {
                    state.Bold = weight >= 700;
                }
                continue;
            }

            // Underline: \u1 or \u0
            if (firstChar == 'u' && tagLen >= 2 && char.IsDigit(trimmedTag[1]))
            {
                state.Underline = trimmedTag[1] == '1';
                continue;
            }

            // Font name: \fnFontName
            if (tagLen > 2 && firstChar == 'f' && trimmedTag[1] == 'n')
            {
                var fontName = trimmedTag.Substring(2).Trim();
                if (fontName.Length > 0 && fontName.Length <= 100)
                {
                    state.FontName = fontName;
                }
                continue;
            }

            // Font size: \fs20
            if (tagLen > 2 && firstChar == 'f' && trimmedTag[1] == 's')
            {
                var sizeStr = trimmedTag.Substring(2);
                if (sizeStr.Length <= 4 && double.TryParse(sizeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
                {
                    state.FontSize = size;
                }
                continue;
            }

            // Primary color: \c&HBBGGRR& or \1c&HBBGGRR& or \c (reset color)
            if (firstChar == 'c' && (tagLen == 1 || !char.IsDigit(trimmedTag[1])))
            {
                if (tagLen == 1)
                {
                    // \c without value resets the color
                    state.Color = null;
                }
                else
                {
                    var colorStr = trimmedTag.Substring(1);
                    state.Color = ParseAssaColor(colorStr);
                }
                continue;
            }

            // Numbered color tags: \1c&HBBGGRR& (primary), \2c, \3c, \4c
            if (tagLen > 2 && char.IsDigit(firstChar) && trimmedTag[1] == 'c')
            {
                // For numbered color tags, we only handle primary (1c) for text color
                if (firstChar == '1')
                {
                    var colorStr = trimmedTag.Substring(2);
                    state.Color = ParseAssaColor(colorStr);
                }
            }
        }
    }

    private static Color? ParseAssaColor(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr) || colorStr.Length > 20)
        {
            return null; // Skip empty or excessively long color strings
        }

        try
        {
            // ASSA colors are in format &HAABBGGRR& or &HBBGGRR& (BGR order, not RGB)
            colorStr = colorStr.Trim('&', 'H', 'h');

            if (string.IsNullOrEmpty(colorStr) || colorStr.Length < 6 || colorStr.Length > 8)
            {
                return null; // Invalid length
            }

            // Pad to 8 characters if needed (add alpha)
            if (colorStr.Length == 6)
            {
                colorStr = "00" + colorStr; // Add alpha = 0 (fully opaque in ASSA)
            }

            // ASSA format: AABBGGRR
            var blue = System.Convert.ToByte(colorStr.Substring(colorStr.Length - 6, 2), 16);
            var green = System.Convert.ToByte(colorStr.Substring(colorStr.Length - 4, 2), 16);
            var red = System.Convert.ToByte(colorStr.Substring(colorStr.Length - 2, 2), 16);
            byte alpha = 255;
            
            if (colorStr.Length >= 8)
            {
                var alphaValue = System.Convert.ToByte(colorStr.Substring(0, 2), 16);
                alpha = (byte)(255 - alphaValue); // ASSA alpha is inverted (0 = opaque, 255 = transparent)
            }
            
            return Color.FromArgb(alpha, red, green, blue);
        }
        catch
        {
            // Ignore parsing errors
        }

        return null;
    }

    private class FormattingState
    {
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public bool Underline { get; set; }
        public Color? Color { get; set; }
        public string? FontName { get; set; }
        public double? FontSize { get; set; }

        public void Reset()
        {
            Italic = false;
            Bold = false;
            Underline = false;
            Color = null;
            FontName = null;
            FontSize = null;
        }
    }

    private static void ParseHtmlTag(InlineCollection inlines, string tagContent)
    {
        // Add opening <
        inlines.Add(new Run("<") { Foreground = CharsBrush });

        var content = tagContent.Substring(1, tagContent.Length - 2); // Remove < and >
        var isClosingTag = content.StartsWith('/');
        if (isClosingTag)
        {
            inlines.Add(new Run("/") { Foreground = CharsBrush });
            content = content.Substring(1);
        }

        var isSelfClosing = content.EndsWith('/');
        if (isSelfClosing)
        {
            content = content.Substring(0, content.Length - 1).TrimEnd();
        }

        // Find element name
        var spaceIndex = content.IndexOf(' ');
        var elementName = spaceIndex > 0 ? content.Substring(0, spaceIndex) : content;

        // Add element name
        inlines.Add(new Run(elementName) { Foreground = ElementBrush });

        // Parse attributes if any
        if (spaceIndex > 0)
        {
            var attributesPart = content.Substring(spaceIndex);
            ParseHtmlAttributes(inlines, attributesPart);
        }

        // Add self-closing /
        if (isSelfClosing)
        {
            inlines.Add(new Run(" /") { Foreground = CharsBrush });
        }

        // Add closing >
        inlines.Add(new Run(">") { Foreground = CharsBrush });
    }

    private static void ParseHtmlAttributes(InlineCollection inlines, string attributesPart)
    {
        int i = 0;
        while (i < attributesPart.Length)
        {
            // Skip whitespace
            while (i < attributesPart.Length && char.IsWhiteSpace(attributesPart[i]))
            {
                inlines.Add(new Run(attributesPart[i].ToString()));
                i++;
            }

            if (i >= attributesPart.Length)
                break;

            // Read attribute name
            var attrStart = i;
            while (i < attributesPart.Length && (char.IsLetterOrDigit(attributesPart[i]) || attributesPart[i] == '-' || attributesPart[i] == '_'))
            {
                i++;
            }

            var attributeName = string.Empty;
            if (i > attrStart)
            {
                attributeName = attributesPart.Substring(attrStart, i - attrStart);
                inlines.Add(new Run(attributeName)
                {
                    Foreground = AttributeBrush
                });
            }
            else if (i < attributesPart.Length)
            {
                // Unexpected character - add it and move on to prevent infinite loop
                inlines.Add(new Run(attributesPart[i].ToString()));
                i++;
            }

            // Skip whitespace
            while (i < attributesPart.Length && char.IsWhiteSpace(attributesPart[i]))
            {
                inlines.Add(new Run(attributesPart[i].ToString()));
                i++;
            }

            // Check for =
            if (i < attributesPart.Length && attributesPart[i] == '=')
            {
                inlines.Add(new Run("=") { Foreground = CharsBrush });
                i++;

                // Skip whitespace
                while (i < attributesPart.Length && char.IsWhiteSpace(attributesPart[i]))
                {
                    inlines.Add(new Run(attributesPart[i].ToString()));
                    i++;
                }

                // Read attribute value (quoted)
                if (i < attributesPart.Length && (attributesPart[i] == '"' || attributesPart[i] == '\''))
                {
                    var quote = attributesPart[i];
                    inlines.Add(new Run(quote.ToString()) { Foreground = CharsBrush });
                    i++;

                    var valueStart = i;
                    while (i < attributesPart.Length && attributesPart[i] != quote)
                    {
                        i++;
                    }

                    if (i > valueStart)
                    {
                        var value = attributesPart.Substring(valueStart, i - valueStart);

                        // Check if this is a color attribute and try to parse the actual color
                        IBrush valueBrush;
                        if (attributeName.Equals("color", StringComparison.OrdinalIgnoreCase))
                        {
                            var parsedColor = TryParseColor(value);
                            valueBrush = parsedColor.HasValue
                                ? new SolidColorBrush(parsedColor.Value)
                                : ValuesBrush;
                        }
                        else
                        {
                            var hasColon = value.Contains(':');
                            valueBrush = hasColon ? StyleBrush : ValuesBrush;
                        }

                        inlines.Add(new Run(value)
                        {
                            Foreground = valueBrush
                        });
                    }

                    if (i < attributesPart.Length)
                    {
                        inlines.Add(new Run(quote.ToString()) { Foreground = CharsBrush });
                        i++;
                    }
                }
            }
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    internal void EnableSpellCheck(ISpellCheckManager spellCheckManager)
    {
        _spellCheckManager = spellCheckManager;
    }
}