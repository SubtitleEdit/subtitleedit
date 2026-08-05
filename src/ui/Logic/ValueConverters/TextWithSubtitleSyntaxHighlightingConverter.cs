using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TextWithSubtitleSyntaxHighlightingConverter : IValueConverter
{
    private ISpellCheckManager? _spellCheckManager;

    // Pre-compiled <font> attribute patterns (reused across every grid-row render)
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly Regex FontColorRegex = new(@"color\s*=\s*[""']?([^""'\s>]+)[""']?", RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);
    private static readonly Regex FontFaceRegex = new(@"face\s*=\s*[""']?([^""'>]+)[""']?", RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);
    private static readonly Regex FontSizeRegex = new(@"size\s*=\s*[""']?(\d+)[""']?", RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);

    // Reuse brushes instead of creating new ones each time. The scheme colors from
    // SubtitleSyntaxTokenizer are theme-dependent, so the brushes are resolved per render and
    // cached per color (the cache stays tiny: both palettes together are 12 colors).
    private static readonly Dictionary<Color, SolidColorBrush> BrushCache = new();

    private static SolidColorBrush GetBrush(Color color)
    {
        if (!BrushCache.TryGetValue(color, out var brush))
        {
            brush = new SolidColorBrush(color);
            BrushCache[color] = brush;
        }

        return brush;
    }

    private static SolidColorBrush ElementBrush => GetBrush(SubtitleSyntaxTokenizer.ElementColor);
    private static SolidColorBrush AttributeBrush => GetBrush(SubtitleSyntaxTokenizer.AttributeColor);
    private static SolidColorBrush CommentBrush => GetBrush(SubtitleSyntaxTokenizer.CommentColor);
    private static SolidColorBrush CharsBrush => GetBrush(SubtitleSyntaxTokenizer.CharsColor);
    private static SolidColorBrush ValuesBrush => GetBrush(SubtitleSyntaxTokenizer.ValuesColor);
    private static SolidColorBrush StyleBrush => GetBrush(SubtitleSyntaxTokenizer.StyleColor);

    // One shared wavy-red underline for all misspelled words - a brush + decoration +
    // collection used to be allocated per misspelled word on every cell repaint.
    private static readonly TextDecoration MisspelledDecoration = new()
    {
        Location = TextDecorationLocation.Underline,
        Stroke = new ImmutableSolidColorBrush(Colors.Red),
        StrokeThickness = 1.5,
        StrokeLineCap = PenLineCap.Round,
    };

    private static readonly TextDecorationCollection MisspelledDecorations = new() { MisspelledDecoration };

    // Colors parsed out of the subtitle itself (ASSA \c tags, <font color=...>) are arbitrary, so
    // there is nothing worth caching - but an immutable brush is a plain object where
    // SolidColorBrush is an AvaloniaObject that drags a property store along for a fixed color.
    private static ImmutableSolidColorBrush CreateBrush(Color color) => new(color);

    // <font face="..."> and \fnName repeat the same handful of names down a whole file, and
    // constructing a FontFamily parses the name every time.
    private static readonly Dictionary<string, FontFamily> FontFamilyCache = new(StringComparer.Ordinal);
    private const int FontFamilyCacheLimit = 64;

    private static FontFamily GetFontFamily(string name)
    {
        if (FontFamilyCache.TryGetValue(name, out var fontFamily))
        {
            return fontFamily;
        }

        fontFamily = new FontFamily(name);
        if (FontFamilyCache.Count < FontFamilyCacheLimit)
        {
            FontFamilyCache[name] = fontFamily;
        }

        return fontFamily;
    }

    // The characters that end an HTML tag name - a literal array here would be allocated per tag.
    private static readonly char[] TagNameDelimiters = { ' ', '\t', '\r', '\n' };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str || string.IsNullOrEmpty(str))
        {
            return new InlineCollection();
        }

        // Truncate long strings for performance
        if (str.Length > 200)
        {
            str = str.Substring(0, 197).TrimEnd() + "...";
        }

        var formattingType = Se.Settings.Appearance.SubtitleGridFormattingType;
        if (formattingType == (int)SubtitleGridFormattingTypes.ShowFormatting)
        {
            var lines = MakeShowFormatting(str);
            return SpellCheckLines(lines);
        }

        if (formattingType == (int)SubtitleGridFormattingTypes.ShowTags)
        {
            var lines = MakeShowTags(str);
            return SpellCheckLines(lines);
        }

        // No formatting (default) - walk the line breaks directly instead of SplitToLines(),
        // which builds a List plus one string per line only to hand each one to a Run.
        var inlines = new InlineCollection();
        var lineStart = 0;
        var i = 0;
        while (i < str.Length)
        {
            var c = str[i];
            if (c != '\r' && c != '\n' && c != '\u2028')
            {
                i++;
                continue;
            }

            if (i > lineStart)
            {
                inlines.Add(new Run(str.Substring(lineStart, i - lineStart)));
            }

            i += c == '\r' && i + 1 < str.Length && str[i + 1] == '\n' ? 2 : 1;
            lineStart = i;
            AppendLineSeparator(inlines);
        }

        if (lineStart < str.Length)
        {
            inlines.Add(new Run(lineStart == 0 ? str : str.Substring(lineStart)));
        }

        return SpellCheckLines(inlines);
    }

    private InlineCollection SpellCheckLines(InlineCollection lines)
    {
        if (_spellCheckManager == null || !Se.Settings.Appearance.SubtitleGridLiveSpellCheck)
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
                var words = SpellCheckWordLists.Split(runText);

                if (words.Count == 0)
                {
                    newInlines.Add(run);
                    continue;
                }

                // Single spell-check pass: remember each word's verdict so the run-splitting
                // below doesn't run IsSpecialPattern + the Hunspell lookup a second time per
                // word - this converter runs per text cell on every grid repaint.
                bool[]? misspelled = null;
                for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
                {
                    var word = words[wordIndex];
                    if (string.IsNullOrWhiteSpace(word.Text) || word.Length < 2)
                    {
                        continue;
                    }

                    if (!IsSpecialPattern(word, runText) && !_spellCheckManager.IsWordCorrect(word, runText))
                    {
                        misspelled ??= new bool[words.Count];
                        misspelled[wordIndex] = true;
                    }
                }

                // If no misspelled words, keep the run as-is to preserve color inheritance
                if (misspelled == null)
                {
                    newInlines.Add(run);
                    continue;
                }

                // Second pass: break up the run and add underlines to misspelled words
                var lastIndex = 0;
                for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
                {
                    var word = words[wordIndex];
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

                    // Create run for the word
                    var wordRun = CreateRunFromTemplate(run, word.Text);
                    if (misspelled[wordIndex])
                    {
                        // Apply the shared wavy red underline - add to existing decorations
                        // instead of replacing them
                        if (wordRun.TextDecorations == null || wordRun.TextDecorations.Count == 0)
                        {
                            wordRun.TextDecorations = MisspelledDecorations;
                        }
                        else
                        {
                            wordRun.TextDecorations = new TextDecorationCollection(wordRun.TextDecorations)
                            {
                                MisspelledDecoration
                            };
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
        {
            run.Foreground = template.Foreground;
        }

        if (template.IsSet(TextElement.FontStyleProperty))
        {
            run.FontStyle = template.FontStyle;
        }

        if (template.IsSet(TextElement.FontWeightProperty))
        {
            run.FontWeight = template.FontWeight;
        }

        if (template.IsSet(TextElement.FontFamilyProperty))
        {
            run.FontFamily = template.FontFamily;
        }

        if (template.IsSet(TextElement.FontSizeProperty))
        {
            run.FontSize = template.FontSize;
        }

        if (template.IsSet(Inline.TextDecorationsProperty) &&
            template.TextDecorations != null && template.TextDecorations.Count > 0)
        {
            run.TextDecorations = new TextDecorationCollection(template.TextDecorations);
        }

        return run;
    }

    private static bool IsSpecialPattern(SpellCheckWord word, string text)
    {
        // Skip numbers - plain loop, this runs per word per cell repaint (no LINQ enumerator)
        var isNumber = true;
        foreach (var c in word.Text)
        {
            if (!char.IsDigit(c) && c != '.' && c != ',' && c != '-')
            {
                isNumber = false;
                break;
            }
        }

        if (isNumber)
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

    /// <summary>
    /// Collects markup into as few <see cref="Run"/>s as possible. Neighbouring characters that
    /// share a foreground render identically whether they sit in one run or in ten, and a Run is
    /// an AvaloniaObject - the number of runs, not the string work, is what a cell repaint costs.
    /// Text is appended straight out of the source string, so the per-token substrings are gone
    /// as well.
    /// </summary>
    private sealed class InlineBuilder
    {
        private readonly InlineCollection _inlines = new();
        private readonly StringBuilder _pending = new(64);
        private IBrush? _pendingBrush;

        public void Append(char c, IBrush? brush)
        {
            StartSegment(brush);
            _pending.Append(c);
        }

        public void Append(string text, IBrush? brush)
        {
            StartSegment(brush);
            _pending.Append(text);
        }

        public void Append(string source, int start, int length, IBrush? brush)
        {
            if (length <= 0)
            {
                return;
            }

            StartSegment(brush);
            _pending.Append(source, start, length);
        }

        public void AddLineBreak()
        {
            Flush();
            _inlines.Add(new LineBreak());
        }

        public InlineCollection Build()
        {
            Flush();
            return _inlines;
        }

        private void StartSegment(IBrush? brush)
        {
            if (_pending.Length > 0 && !ReferenceEquals(brush, _pendingBrush))
            {
                Flush();
            }

            _pendingBrush = brush;
        }

        private void Flush()
        {
            if (_pending.Length == 0)
            {
                return;
            }

            var run = new Run(_pending.ToString());
            if (_pendingBrush != null)
            {
                run.Foreground = _pendingBrush;
            }

            _inlines.Add(run);
            _pending.Clear();
        }
    }

    private static InlineCollection MakeShowTags(string str)
    {
        var builder = new InlineBuilder();
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
                    builder.Append('{', CharsBrush);

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
                        builder.Append('\\', CharsBrush);

                        // Find where this tag ends (next backslash or closing brace)
                        var nextBackslash = str.IndexOf('\\', currentPos + 1);
                        var thisTagEnd = (nextBackslash != -1 && nextBackslash < tagEnd) ? nextBackslash : tagEnd;

                        var tagNameStart = currentPos + 1;
                        var tagNameEnd = SubtitleSyntaxTokenizer.GetAssTagNameEnd(str, tagNameStart, thisTagEnd);

                        // Add tag name
                        builder.Append(str, tagNameStart, tagNameEnd - tagNameStart, ElementBrush);

                        // Add tag value/parameters (e.g., "1", "Arial", "&HFFFFFF&")
                        if (tagNameEnd < thisTagEnd)
                        {
                            Color? assColor = null;
                            if (SubtitleSyntaxTokenizer.IsAssColorTag(str.AsSpan(tagNameStart, tagNameEnd - tagNameStart)))
                            {
                                // Only the color tags need the value as a string
                                assColor = SubtitleSyntaxTokenizer.TryParseAssColor(str.Substring(tagNameEnd, thisTagEnd - tagNameEnd));
                            }

                            builder.Append(str, tagNameEnd, thisTagEnd - tagNameEnd,
                                assColor.HasValue ? CreateBrush(assColor.Value) : ValuesBrush);
                        }

                        currentPos = thisTagEnd;
                    }

                    // Add closing brace
                    builder.Append('}', CharsBrush);
                    i = tagEnd + 1;
                    continue;
                }
                else
                {
                    // Malformed ASS/SSA tag - treat as regular text
                    builder.Append(c, null);
                    i++;
                    continue;
                }
            }

            // Handle HTML comments
            if (c == '<' && i + 3 < str.Length && c2 == '!' && str[i + 2] == '-' && str[i + 3] == '-')
            {
                var commentEnd = str.IndexOf("-->", i + 4, StringComparison.Ordinal);
                var commentLength = commentEnd != -1 ? commentEnd + 3 - i : str.Length - i;
                builder.Append(str, i, commentLength, CommentBrush);
                i += commentLength;
                continue;
            }

            // Handle HTML tags
            if (c == '<')
            {
                var tagEnd = str.IndexOf('>', i + 1);
                if (tagEnd != -1)
                {
                    ParseHtmlTag(builder, str, i, tagEnd);
                    i = tagEnd + 1;
                    continue;
                }
                else
                {
                    // Malformed HTML tag - treat as regular text
                    builder.Append(c, null);
                    i++;
                    continue;
                }
            }

            // Handle line breaks
            if (AppendLineBreak(builder, c, c2, ref i))
            {
                continue;
            }

            // Regular text - add character by character until we hit special markup
            var textStart = i;
            while (i < str.Length && str[i] != '<' && str[i] != '{' && str[i] != '\r' && str[i] != '\n' && str[i] != '\u2028')
            {
                i++;
            }

            if (i > textStart)
            {
                builder.Append(str, textStart, i - textStart, null);
            }
            else
            {
                // Safety: if we didn't match any condition and haven't advanced, treat as regular character
                builder.Append(str[i], null);
                i++;
            }
        }

        return builder.Build();
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
                    ParseAssaTags(str, i + 1, tagEnd, state); // Content between { and }
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
                    // Tracked as indexes into str: the tag name is only compared, so it never
                    // needs to be cut out and lower-cased, and only <font> needs its content
                    // as a string.
                    var contentStart = i + 1;
                    var contentEnd = tagEnd;
                    TrimRange(str, ref contentStart, ref contentEnd);
                    if (contentEnd <= contentStart)
                    {
                        // Empty tag - treat as regular text
                        var run = CreateFormattedRun(c.ToString(), state);
                        inlines.Add(run);
                        i++;
                        continue;
                    }

                    var isClosingTag = str[contentStart] == '/';
                    if (isClosingTag)
                    {
                        contentStart++;
                        TrimRange(str, ref contentStart, ref contentEnd);
                    }

                    var tagNameEnd = contentEnd > contentStart
                        ? str.IndexOfAny(TagNameDelimiters, contentStart, contentEnd - contentStart)
                        : -1;
                    var tagName = str.AsSpan(
                        contentStart,
                        (tagNameEnd > contentStart ? tagNameEnd : contentEnd) - contentStart);

                    if (isClosingTag)
                    {
                        // Handle closing tags
                        if (tagName.Equals("i", StringComparison.OrdinalIgnoreCase))
                        {
                            state.Italic = false;
                        }
                        else if (tagName.Equals("b", StringComparison.OrdinalIgnoreCase))
                        {
                            state.Bold = false;
                        }
                        else if (tagName.Equals("u", StringComparison.OrdinalIgnoreCase))
                        {
                            state.Underline = false;
                        }
                        else if (tagName.Equals("font", StringComparison.OrdinalIgnoreCase))
                        {
                            state.Color = null;
                            state.FontName = null;
                            state.FontSize = null;
                        }
                    }
                    else
                    {
                        // Handle opening tags
                        if (tagName.Equals("i", StringComparison.OrdinalIgnoreCase))
                        {
                            state.Italic = true;
                        }
                        else if (tagName.Equals("b", StringComparison.OrdinalIgnoreCase))
                        {
                            state.Bold = true;
                        }
                        else if (tagName.Equals("u", StringComparison.OrdinalIgnoreCase))
                        {
                            state.Underline = true;
                        }
                        else if (tagName.Equals("font", StringComparison.OrdinalIgnoreCase))
                        {
                            ParseFontTag(str.Substring(contentStart, contentEnd - contentStart), state);
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
            if (AppendLineBreak(inlines, c, c2, ref i))
            {
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
                if (ch == '<' || ch == '\r' || ch == '\n' || ch == '\u2028')
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

    /// <summary>
    /// Narrows [start, end) to the same range String.Trim() would keep.
    /// </summary>
    private static void TrimRange(string source, ref int start, ref int end)
    {
        while (start < end && char.IsWhiteSpace(source[start]))
        {
            start++;
        }

        while (end > start && char.IsWhiteSpace(source[end - 1]))
        {
            end--;
        }
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
            run.Foreground = CreateBrush(state.Color.Value);
        }

        // Apply font name
        if (!string.IsNullOrEmpty(state.FontName))
        {
            run.FontFamily = GetFontFamily(state.FontName);
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
            // Every Match allocates, and a <font> tag almost never carries all three attributes -
            // a plain substring probe first tells us whether the pattern can match at all.
            // Parse color attribute
            var colorMatch = tagContent.Contains("color", StringComparison.OrdinalIgnoreCase)
                ? FontColorRegex.Match(tagContent)
                : Match.Empty;
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
            var faceMatch = tagContent.Contains("face", StringComparison.OrdinalIgnoreCase)
                ? FontFaceRegex.Match(tagContent)
                : Match.Empty;
            if (faceMatch.Success)
            {
                var fontName = faceMatch.Groups[1].Value.Trim();
                if (fontName.Length > 0 && fontName.Length <= 100)
                {
                    state.FontName = fontName;
                }
            }

            // Parse size attribute
            var sizeMatch = tagContent.Contains("size", StringComparison.OrdinalIgnoreCase)
                ? FontSizeRegex.Match(tagContent)
                : Match.Empty;
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

    /// <summary>
    /// Applies the ASSA override tags in source[start..end) to <paramref name="state"/>. Indexes
    /// plus spans instead of Split + Trim + Substring: this runs for every ASSA tag block of
    /// every visible row on every repaint, and only a font name ends up as a string.
    /// </summary>
    private static void ParseAssaTags(string source, int start, int end, FormattingState state)
    {
        var length = end - start;
        if (length <= 0 || length > 500)
        {
            return; // Skip empty or excessively long tag content
        }

        var content = source.AsSpan(start, length);

        // A block can carry more than one primary color - a static color plus the destination
        // of a \t(...) fade transition ({\c&HFFFFFF&\t(20,1000,0.9,\c&H29F2FF&)}, #10955).
        // Collect them all and pick the most visible one against the grid background at the
        // end of the block instead of letting the textually last tag win.
        Span<Color> colorCandidates = stackalloc Color[MaxColorCandidates];
        var colorCandidateCount = 0;
        var sawColorTag = false;

        // Set by a \t(...) animation in this block. Only then can two primary colors both be
        // "current" (the static one and the transition's destination); without a transition,
        // consecutive color tags are plain overrides and the last one wins.
        var sawTransitionTag = false;

        // Limit number of tags to prevent excessive processing
        const int maxTags = 50;
        var handled = 0;
        var pos = 0;

        while (pos < content.Length && handled < maxTags)
        {
            var next = content.Slice(pos).IndexOf('\\');
            var segment = next < 0 ? content.Slice(pos) : content.Slice(pos, next);
            pos = next < 0 ? content.Length : pos + next + 1;

            if (segment.Length == 0)
            {
                continue; // Split(RemoveEmptyEntries) dropped these before they were counted
            }

            handled++;

            var trimmedTag = segment.Trim();
            if (trimmedTag.Length == 0 || trimmedTag.Length > 100)
            {
                continue; // Skip empty or excessively long individual tags
            }

            var firstChar = trimmedTag[0];
            var tagLen = trimmedTag.Length;

            // Reset: \r - reset all formatting (check first for performance)
            if (firstChar == 'r' && tagLen == 1)
            {
                state.Reset();
                colorCandidateCount = 0;
                sawColorTag = false;
                sawTransitionTag = false;
                continue;
            }

            // Animation: \t(...) - the tags splitter cuts on '\', so the transition's own
            // color arrives as a later segment; just note that a transition is in play.
            if (firstChar == 't' && tagLen > 1 && trimmedTag[1] == '(')
            {
                sawTransitionTag = true;
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
                var value = trimmedTag.Slice(1);
                if (value.Length == 1 && value[0] == '0')
                {
                    state.Bold = false;
                }
                else if (value.Length == 1 && value[0] == '1')
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
                var fontName = trimmedTag.Slice(2).Trim();
                if (fontName.Length > 0 && fontName.Length <= 100)
                {
                    state.FontName = fontName.ToString();
                }
                continue;
            }

            // Font size: \fs20
            if (tagLen > 2 && firstChar == 'f' && trimmedTag[1] == 's')
            {
                var sizeStr = trimmedTag.Slice(2);
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
                    colorCandidateCount = 0;
                    sawColorTag = false;
                }
                else
                {
                    sawColorTag = true;
                    var color = ParseAssaColor(trimmedTag.Slice(1));
                    if (color.HasValue && colorCandidateCount < colorCandidates.Length)
                    {
                        colorCandidates[colorCandidateCount++] = color.Value;
                    }
                }
                continue;
            }

            // Numbered color tags: \1c&HBBGGRR& (primary), \2c, \3c, \4c
            if (tagLen > 2 && char.IsDigit(firstChar) && trimmedTag[1] == 'c')
            {
                // For numbered color tags, we only handle primary (1c) for text color
                if (firstChar == '1')
                {
                    sawColorTag = true;
                    var color = ParseAssaColor(trimmedTag.Slice(2));
                    if (color.HasValue && colorCandidateCount < colorCandidates.Length)
                    {
                        colorCandidates[colorCandidateCount++] = color.Value;
                    }
                }
            }
        }

        if (colorCandidateCount > 1 && sawTransitionTag)
        {
            // Static color plus a \t(...) fade destination: the grid can only show one, so show
            // the one with the best contrast against the grid background - or none when even
            // the best would be near-invisible (#10955).
            state.Color = PickMostVisibleColor(colorCandidates.Slice(0, colorCandidateCount));
        }
        else if (colorCandidateCount > 0)
        {
            // No transition involved, so ASSA's last-wins rule applies: consecutive static
            // color tags resolve to the last one, matching libass and the video preview. The
            // visibility guard deliberately does not apply here - a single explicit color is
            // shown as authored even when it has little contrast.
            state.Color = colorCandidates[colorCandidateCount - 1];
        }
        else if (sawColorTag)
        {
            // A color tag whose value did not parse resets the color, same as before candidate
            // collection was introduced ({\c&HFFFFFF&}a{\c&HZZZZZZ&}b leaves "b" unset).
            state.Color = null;
        }
    }

    private const int MaxColorCandidates = 8;

    // Below this WCAG contrast ratio a color is treated as unusable against the grid
    // background (pure white on white is 1.0; white on the default dark background is ~17).
    private const double MinimumVisibleContrast = 1.3;

    /// <summary>
    /// Test hook: the grid background normally follows the active theme, which headless unit
    /// tests do not set up consistently.
    /// </summary>
    internal static Func<Color>? GridBackgroundOverride { get; set; }

    private static Color GetGridBackgroundColor()
    {
        var backgroundOverride = GridBackgroundOverride;
        if (backgroundOverride != null)
        {
            return backgroundOverride();
        }

        // Not just white for "not dark": SE also ships the Classic (gray) and Pastel
        // (lavender) light themes, and contrast has to be measured against the real
        // background. Also swallows a malformed DarkModeBackgroundColor rather than
        // letting it throw out of IValueConverter.Convert.
        return UiTheme.GetThemeBackgroundColor();
    }

    private static Color? PickMostVisibleColor(ReadOnlySpan<Color> candidates)
    {
        var background = GetGridBackgroundColor();
        var backgroundLuminance = RelativeLuminance(background);

        Color? best = null;
        var bestContrast = 0.0;
        foreach (var candidate in candidates)
        {
            var luminance = RelativeLuminance(CompositeOver(candidate, background));
            var contrast = ContrastRatio(luminance, backgroundLuminance);

            // ">=" so that on equal contrast the later tag wins - for a fade that is the
            // transition's destination color, the most logical of the two to show.
            if (contrast >= bestContrast)
            {
                best = candidate;
                bestContrast = contrast;
            }
        }

        return bestContrast >= MinimumVisibleContrast ? best : null;
    }

    /// <summary>
    /// Composites a (possibly semi-transparent) color over the given background.
    /// </summary>
    private static Color CompositeOver(Color color, Color background)
    {
        if (color.A == byte.MaxValue)
        {
            return color;
        }

        var alpha = color.A / 255.0;
        return Color.FromRgb(
            (byte)Math.Round(color.R * alpha + background.R * (1 - alpha)),
            (byte)Math.Round(color.G * alpha + background.G * (1 - alpha)),
            (byte)Math.Round(color.B * alpha + background.B * (1 - alpha)));
    }

    /// <summary>
    /// WCAG relative luminance (0 = black, 1 = white).
    /// </summary>
    private static double RelativeLuminance(Color color)
    {
        return 0.2126 * Linearize(color.R) + 0.7152 * Linearize(color.G) + 0.0722 * Linearize(color.B);

        static double Linearize(byte value)
        {
            var channel = value / 255.0;
            return channel <= 0.04045 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }
    }

    /// <summary>
    /// WCAG contrast ratio between two relative luminances (1 = identical, 21 = black/white).
    /// </summary>
    private static double ContrastRatio(double luminance1, double luminance2)
    {
        var lighter = Math.Max(luminance1, luminance2);
        var darker = Math.Min(luminance1, luminance2);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static Color? ParseAssaColor(ReadOnlySpan<char> colorStr)
    {
        if (colorStr.Length == 0 || colorStr.Length > 20)
        {
            return null; // Skip empty or excessively long color strings
        }

        // Inside a \t(...) transition the color tag is followed by the transition's closing
        // paren, which lands on the color fragment when the block is split on '\'
        // ({\c&HFFFFFF&\t(20,1000,\c&H29F2FF&)} - #10955); trim it so the color still parses.
        colorStr = colorStr.TrimEnd(')');

        // ASSA colors are in format &HAABBGGRR& or &HBBGGRR& (BGR order, not RGB)
        colorStr = colorStr.Trim("&Hh".AsSpan());

        if (colorStr.Length < 6 || colorStr.Length > 8)
        {
            return null; // Invalid length
        }

        // ASSA format: [AA]BBGGRR - the color bytes are always the last six characters, and a
        // six-character value is the same as one padded with an alpha of 00 (= fully opaque).
        var rgb = colorStr.Slice(colorStr.Length - 6);
        if (!TryParseHexByte(rgb.Slice(0, 2), out var blue) ||
            !TryParseHexByte(rgb.Slice(2, 2), out var green) ||
            !TryParseHexByte(rgb.Slice(4, 2), out var red))
        {
            return null;
        }

        byte alpha = 255;
        if (colorStr.Length >= 8)
        {
            if (!TryParseHexByte(colorStr.Slice(0, 2), out var alphaValue))
            {
                return null;
            }

            alpha = (byte)(255 - alphaValue); // ASSA alpha is inverted (0 = opaque, 255 = transparent)
        }

        return Color.FromArgb(alpha, red, green, blue);
    }

    private static bool TryParseHexByte(ReadOnlySpan<char> hex, out byte value)
    {
        if (byte.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        // Convert.ToByte(s, 16) - what this used to be - also accepts shapes byte.TryParse
        // rejects, e.g. a leading sign ("+1") or an "0x" prefix. Malformed color values are rare
        // enough that reproducing them through the old, allocating path costs nothing in practice.
        try
        {
            value = System.Convert.ToByte(hex.ToString(), 16);
            return true;
        }
        catch
        {
            value = 0;
            return false;
        }
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

    /// <summary>
    /// Renders the tag between <paramref name="start"/> (the '&lt;') and <paramref name="end"/>
    /// (the '&gt;'). Works on indexes into the source string so nothing between them has to be
    /// copied out first.
    /// </summary>
    private static void ParseHtmlTag(InlineBuilder builder, string source, int start, int end)
    {
        // Add opening <
        builder.Append('<', CharsBrush);

        var contentStart = start + 1; // just after <
        var contentEnd = end;         // exclusive, i.e. just before >

        if (contentStart < contentEnd && source[contentStart] == '/')
        {
            builder.Append('/', CharsBrush);
            contentStart++;
        }

        var isSelfClosing = contentEnd > contentStart && source[contentEnd - 1] == '/';
        if (isSelfClosing)
        {
            contentEnd--;
            while (contentEnd > contentStart && char.IsWhiteSpace(source[contentEnd - 1]))
            {
                contentEnd--;
            }
        }

        // Find element name
        var spaceIndex = contentEnd > contentStart
            ? source.IndexOf(' ', contentStart, contentEnd - contentStart)
            : -1;
        var elementNameEnd = spaceIndex > contentStart ? spaceIndex : contentEnd;

        // Add element name
        builder.Append(source, contentStart, elementNameEnd - contentStart, ElementBrush);

        // Parse attributes if any
        if (spaceIndex > contentStart)
        {
            ParseHtmlAttributes(builder, source, spaceIndex, contentEnd);
        }

        // Add self-closing /
        if (isSelfClosing)
        {
            builder.Append(" /", CharsBrush);
        }

        // Add closing >
        builder.Append('>', CharsBrush);
    }

    private static void ParseHtmlAttributes(InlineBuilder builder, string source, int start, int end)
    {
        var i = start;
        while (i < end)
        {
            // Skip whitespace - one segment for the whole stretch instead of one per space
            i = AppendWhitespace(builder, source, i, end);
            if (i >= end)
            {
                break;
            }

            // Read attribute name
            var attrStart = i;
            while (i < end && (char.IsLetterOrDigit(source[i]) || source[i] == '-' || source[i] == '_'))
            {
                i++;
            }

            var attributeNameLength = i - attrStart;
            if (attributeNameLength > 0)
            {
                builder.Append(source, attrStart, attributeNameLength, AttributeBrush);
            }
            else if (i < end)
            {
                // Unexpected character - add it and move on to prevent infinite loop
                builder.Append(source[i], null);
                i++;
            }

            // Skip whitespace
            i = AppendWhitespace(builder, source, i, end);

            // Check for =
            if (i >= end || source[i] != '=')
            {
                continue;
            }

            builder.Append('=', CharsBrush);
            i++;

            // Skip whitespace
            i = AppendWhitespace(builder, source, i, end);

            // Read attribute value (quoted)
            if (i >= end || (source[i] != '"' && source[i] != '\''))
            {
                continue;
            }

            var quote = source[i];
            builder.Append(quote, CharsBrush);
            i++;

            var valueStart = i;
            while (i < end && source[i] != quote)
            {
                i++;
            }

            if (i > valueStart)
            {
                // Check if this is a color attribute and try to parse the actual color
                IBrush valueBrush;
                if (source.AsSpan(attrStart, attributeNameLength).Equals("color", StringComparison.OrdinalIgnoreCase))
                {
                    var parsedColor = SubtitleSyntaxTokenizer.TryParseColor(source.Substring(valueStart, i - valueStart));
                    valueBrush = parsedColor.HasValue
                        ? CreateBrush(parsedColor.Value)
                        : ValuesBrush;
                }
                else
                {
                    var hasColon = source.AsSpan(valueStart, i - valueStart).IndexOf(':') >= 0;
                    valueBrush = hasColon ? StyleBrush : ValuesBrush;
                }

                builder.Append(source, valueStart, i - valueStart, valueBrush);
            }

            if (i < end)
            {
                builder.Append(quote, CharsBrush);
                i++;
            }
        }
    }

    private static int AppendWhitespace(InlineBuilder builder, string source, int i, int end)
    {
        var whitespaceStart = i;
        while (i < end && char.IsWhiteSpace(source[i]))
        {
            i++;
        }

        builder.Append(source, whitespaceStart, i - whitespaceStart, null);
        return i;
    }

    private static bool AppendLineBreak(InlineBuilder builder, char c, char c2, ref int i)
    {
        if (c != '\n' && c != '\r' && c != '\u2028')
        {
            return false;
        }

        var separator = GetLineSeparator();
        if (separator == null)
        {
            builder.AddLineBreak();
        }
        else
        {
            builder.Append(separator, AttributeBrush);
        }

        i += c == '\r' && c2 == '\n' ? 2 : 1;
        return true;
    }

    private static bool AppendLineBreak(InlineCollection inlines, char c, char c2, ref int i)
    {
        if (c != '\n' && c != '\r' && c != '\u2028')
        {
            return false;
        }

        AppendLineSeparator(inlines);
        i += c == '\r' && c2 == '\n' ? 2 : 1;
        return true;
    }

    private static void AppendLineSeparator(InlineCollection inlines)
    {
        var separator = GetLineSeparator();
        if (separator == null)
        {
            inlines.Add(new LineBreak());
            return;
        }

        inlines.Add(new Run(separator) { Foreground = AttributeBrush });
    }

    /// <summary>
    /// The text a line break renders as in single-line mode, or null for a real line break.
    /// The separator is added as literal text so markup-like separators (e.g. "&lt;br /&gt;") are
    /// shown verbatim instead of being parsed away by the formatter. It is rendered in the soft
    /// blue accent (AttributeBrush) - a mid-tone that reads on both light and dark themes - so it
    /// stands out clearly from the actual subtitle text.
    /// </summary>
    private static string? GetLineSeparator()
    {
        var appearance = Se.Settings.Appearance;
        if (!appearance.SubtitleGridTextSingleLine)
        {
            return null;
        }

        var separator = appearance.SubtitleGridTextSingleLineSeparator;
        return string.IsNullOrEmpty(separator) ? " " : separator;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    internal void SetSpellCheck(ISpellCheckManager? spellCheckManager)
    {
        _spellCheckManager = spellCheckManager;
    }
}