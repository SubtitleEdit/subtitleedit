using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="TextPresenter"/> that colors HTML tags and ASS/SSA override tags while typing,
/// using the shared subtitle syntax color scheme (via <see cref="SubtitleSyntaxTokenizer"/>).
///
/// <see cref="CreateTextLayout"/> is a replica of the Avalonia 12.1.0 implementation with syntax
/// highlight style overrides merged in; the private members it needs are reached with
/// <see cref="UnsafeAccessorAttribute"/> (the approach is borrowed from Pororoca's
/// SyntaxHighlightingTextBox, MIT licensed, itself based on Carina Studio's AppSuiteBase).
/// Compare with
/// https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/Presenters/TextPresenter.cs
/// when upgrading Avalonia.
/// </summary>
public class SyntaxHighlightingTextPresenter : TextPresenter
{
    private static readonly Dictionary<Color, ImmutableSolidColorBrush> BrushCache = new();

    private static ImmutableSolidColorBrush GetBrush(Color color)
    {
        if (!BrushCache.TryGetValue(color, out var brush))
        {
            if (BrushCache.Count > 512)
            {
                BrushCache.Clear(); // arbitrary user colors ({\c&H...&} etc.) must not grow this unbounded
            }

            brush = new ImmutableSolidColorBrush(color);
            BrushCache[color] = brush;
        }

        return brush;
    }

    // Run properties are cached per token color and rebuilt when font/foreground changes;
    // reusing the instances avoids re-allocating them on every layout pass (each keystroke,
    // selection change etc.).
    private readonly Dictionary<Color, GenericTextRunProperties> _tokenPropertiesCache = new();
    private GenericTextRunProperties? _defaultProperties;
    private Typeface _cachedTypeface;
    private double _cachedFontSize = double.NaN;
    private IBrush? _cachedForeground;
    private FontFeatureCollection? _cachedFontFeatures;
    private bool _cachedIsDarkTheme;

    // Live spell check: the red underline decoration matches SpellCheckUnderlineTransformer's
    // (the AvaloniaEdit editor), so both editors underline identically.
    private static readonly TextDecorationCollection SpellCheckUnderline = new()
    {
        new TextDecoration
        {
            Location = TextDecorationLocation.Underline,
            Stroke = new ImmutableSolidColorBrush(Colors.Red),
            StrokeThickness = 1.5,
            StrokeLineCap = PenLineCap.Round,
            StrokeThicknessUnit = TextDecorationUnit.FontRecommended,
        },
    };

    // Underlined variants of the span properties they replace, keyed by the (cached, reused)
    // originals. Hunspell results are cached per text: they are only recomputed when the text
    // changes or InvalidateSpellCheck is called, not on every selection/caret layout pass.
    private readonly Dictionary<TextRunProperties, GenericTextRunProperties> _underlinedPropertiesCache = new();
    private string? _spellCheckedText;
    private List<SpellCheckWord> _spellCheckedWords = new();

    // Syntax spans are cached per text so selection/caret layout passes (e.g. every pointer
    // move during a mouse-drag selection) don't re-tokenize unchanged text. The cached list is
    // never mutated: the overlay methods below always build new lists.
    private string? _syntaxSpansText;
    private List<ValueSpan<TextRunProperties>>? _syntaxSpans;

    // The selection highlight properties are reused across layout passes while dragging.
    private GenericTextRunProperties? _selectionProperties;
    private IBrush? _cachedSelectionForeground;

    private GenericTextRunProperties GetDefaultProperties(Typeface typeface)
    {
        // The theme is part of the key: the token colors from SubtitleSyntaxTokenizer are
        // theme-dependent, so the spans cached below must not survive a variant switch.
        var isDarkTheme = UiTheme.IsDarkThemeEnabled();
        if (_defaultProperties is null ||
            !typeface.Equals(_cachedTypeface) ||
            FontSize != _cachedFontSize ||
            !ReferenceEquals(Foreground, _cachedForeground) ||
            !ReferenceEquals(FontFeatures, _cachedFontFeatures) ||
            isDarkTheme != _cachedIsDarkTheme)
        {
            _cachedTypeface = typeface;
            _cachedFontSize = FontSize;
            _cachedForeground = Foreground;
            _cachedFontFeatures = FontFeatures;
            _cachedIsDarkTheme = isDarkTheme;
            _defaultProperties = new GenericTextRunProperties(
                typeface,
                FontSize,
                foregroundBrush: Foreground,
                fontFeatures: FontFeatures);
            _tokenPropertiesCache.Clear();
            _underlinedPropertiesCache.Clear();
            _syntaxSpansText = null;
            _syntaxSpans = null;
            _selectionProperties = null;
        }

        return _defaultProperties;
    }

    private GenericTextRunProperties GetTokenProperties(Color color)
    {
        if (!_tokenPropertiesCache.TryGetValue(color, out var properties))
        {
            if (_tokenPropertiesCache.Count > 256)
            {
                _tokenPropertiesCache.Clear();
            }

            properties = new GenericTextRunProperties(
                _cachedTypeface,
                _cachedFontSize,
                foregroundBrush: GetBrush(color),
                fontFeatures: _cachedFontFeatures);
            _tokenPropertiesCache[color] = properties;
        }

        return properties;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_constraint")]
    private static extern ref Size GetConstraint(TextPresenter presenter);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "GetCombinedText")]
    private static extern string? GetCombinedText(TextPresenter presenter, string? text, int caretIndex, string? preeditText);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "CreateTextLayoutInternal")]
    private static extern TextLayout CreateTextLayoutInternal(TextPresenter presenter, Size constraint, string? text, Typeface typeface,
        IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides);

    protected override TextLayout CreateTextLayout()
    {
        var caretIndex = CaretIndex;
        var preeditText = PreeditText;
        var text = GetCombinedText(this, Text, caretIndex, preeditText);
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;
        var start = Math.Min(selectionStart, selectionEnd);
        var length = Math.Max(selectionStart, selectionEnd) - start;
        var isPassword = PasswordChar != default(char) && !RevealPassword;

        var textStyleOverrides = isPassword ? null : BuildSyntaxSpans(text, typeface);

        // Spell check underlines are skipped during IME composition: the combined text has the
        // preedit string spliced in at the caret, so the word positions would not line up.
        if (!isPassword && string.IsNullOrEmpty(preeditText))
        {
            textStyleOverrides = ApplySpellCheckUnderlines(textStyleOverrides, text, typeface);
        }

        if (!string.IsNullOrEmpty(preeditText))
        {
            var preeditHighlight = new ValueSpan<TextRunProperties>(caretIndex, preeditText.Length,
                new GenericTextRunProperties(
                    typeface,
                    FontSize,
                    TextDecorations.Underline,
                    Foreground,
                    fontFeatures: FontFeatures));

            textStyleOverrides = OverlaySpan(textStyleOverrides, preeditHighlight);
        }
        else if (ShowSelectionHighlight && length > 0 && SelectionForegroundBrush != null)
        {
            var selectionHighlight = new ValueSpan<TextRunProperties>(start, length, GetSelectionProperties(typeface));

            textStyleOverrides = OverlaySpan(textStyleOverrides, selectionHighlight);
        }

        ref var constraint = ref GetConstraint(this);

        if (isPassword)
        {
            return CreateTextLayoutInternal(this, constraint, new string(PasswordChar, text?.Length ?? 0), typeface, textStyleOverrides);
        }

        return CreateTextLayoutInternal(this, constraint, text, typeface, textStyleOverrides);
    }

    /// <summary>
    /// Builds sorted, non-overlapping spans covering the whole text: tag tokens get the color
    /// from <see cref="SubtitleSyntaxTokenizer"/> and the text between them gets the default
    /// foreground (gaps must be covered explicitly, otherwise the tag style bleeds into the
    /// surrounding text).
    /// </summary>
    private List<ValueSpan<TextRunProperties>>? BuildSyntaxSpans(string? text, Typeface typeface)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        // GetDefaultProperties drops the cached spans when font/foreground changed, so it must
        // run before the per-text cache check.
        var defaultProperties = GetDefaultProperties(typeface);
        if (text == _syntaxSpansText)
        {
            return _syntaxSpans;
        }

        List<ValueSpan<TextRunProperties>>? spans = null;
        var tokens = SubtitleSyntaxTokenizer.Tokenize(text);
        if (tokens.Count > 0)
        {
            spans = new List<ValueSpan<TextRunProperties>>(tokens.Count * 2 + 1);
            var position = 0;
            foreach (var token in tokens)
            {
                if (token.Start < position)
                {
                    continue; // overlapping token - first one wins
                }

                if (token.Start > position)
                {
                    spans.Add(new ValueSpan<TextRunProperties>(position, token.Start - position, defaultProperties));
                }

                spans.Add(new ValueSpan<TextRunProperties>(token.Start, token.Length, GetTokenProperties(token.Color)));

                position = token.Start + token.Length;
            }

            if (position < text.Length)
            {
                spans.Add(new ValueSpan<TextRunProperties>(position, text.Length - position, defaultProperties));
            }
        }

        _syntaxSpansText = text;
        _syntaxSpans = spans;
        return spans;
    }

    private GenericTextRunProperties GetSelectionProperties(Typeface typeface)
    {
        // Refreshes the caches (and clears _selectionProperties) on font/foreground changes;
        // needed here because the password path skips BuildSyntaxSpans.
        GetDefaultProperties(typeface);

        if (_selectionProperties is null || !ReferenceEquals(SelectionForegroundBrush, _cachedSelectionForeground))
        {
            _cachedSelectionForeground = SelectionForegroundBrush;
            _selectionProperties = new GenericTextRunProperties(
                _cachedTypeface,
                _cachedFontSize,
                foregroundBrush: SelectionForegroundBrush,
                fontFeatures: _cachedFontFeatures);
        }

        return _selectionProperties;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Register with the owning text box so it can invalidate the underlines (enable/disable,
        // "Add to user dictionary", "Ignore all") and hit test words for the context menu.
        if (TemplatedParent is SyntaxHighlightingTextBox box)
        {
            box.SyntaxPresenter = this;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // Unregister so the owner never invalidates a presenter that left the tree (e.g. after
        // a template swap).
        if (TemplatedParent is SyntaxHighlightingTextBox box && ReferenceEquals(box.SyntaxPresenter, this))
        {
            box.SyntaxPresenter = null;
        }
    }

    /// <summary>
    /// Drops the cached hunspell results and rebuilds the text layout - called when the spell
    /// check state changes without the text changing.
    /// </summary>
    public void InvalidateSpellCheck()
    {
        _spellCheckedText = null;
        _spellCheckedWords.Clear();
        InvalidateTextLayout();
    }

    /// <summary>
    /// Overlays the red spell check underline on every misspelled word, preserving the
    /// underlying span's foreground (so a misspelled word inside a colored tag keeps its color).
    /// </summary>
    private List<ValueSpan<TextRunProperties>>? ApplySpellCheckUnderlines(List<ValueSpan<TextRunProperties>>? spans, string? text, Typeface typeface)
    {
        if (string.IsNullOrEmpty(text) ||
            TemplatedParent is not SyntaxHighlightingTextBox { IsSpellCheckEnabled: true, SpellCheckManager: { } spellCheckManager })
        {
            return spans;
        }

        if (_spellCheckedText != text)
        {
            _spellCheckedText = text;
            _spellCheckedWords = SpellCheckUnderlineTransformer.GetMisspelledWords(text, spellCheckManager);
        }

        if (_spellCheckedWords.Count == 0)
        {
            return spans;
        }

        spans ??= new List<ValueSpan<TextRunProperties>> { new(0, text.Length, GetDefaultProperties(typeface)) };

        foreach (var word in _spellCheckedWords)
        {
            spans = OverlayUnderline(spans, word.Index, word.Length);
        }

        return spans;
    }

    /// <summary>
    /// Splits the spans overlapping [start, start+length) and swaps the overlapped segments'
    /// properties for underlined copies. Spans stay sorted and non-overlapping.
    /// </summary>
    private List<ValueSpan<TextRunProperties>> OverlayUnderline(List<ValueSpan<TextRunProperties>> spans, int start, int length)
    {
        var end = start + length;
        var result = new List<ValueSpan<TextRunProperties>>(spans.Count + 2);

        foreach (var span in spans)
        {
            var spanStart = span.Start;
            var spanEnd = span.Start + span.Length;

            if (spanEnd <= start || spanStart >= end)
            {
                result.Add(span);
                continue;
            }

            if (spanStart < start)
            {
                result.Add(new ValueSpan<TextRunProperties>(spanStart, start - spanStart, span.Value));
            }

            var overlapStart = Math.Max(spanStart, start);
            var overlapEnd = Math.Min(spanEnd, end);
            result.Add(new ValueSpan<TextRunProperties>(overlapStart, overlapEnd - overlapStart, GetUnderlinedProperties(span.Value)));

            if (spanEnd > end)
            {
                result.Add(new ValueSpan<TextRunProperties>(end, spanEnd - end, span.Value));
            }
        }

        return result;
    }

    private GenericTextRunProperties GetUnderlinedProperties(TextRunProperties baseProperties)
    {
        if (!_underlinedPropertiesCache.TryGetValue(baseProperties, out var properties))
        {
            if (_underlinedPropertiesCache.Count > 256)
            {
                _underlinedPropertiesCache.Clear();
            }

            properties = new GenericTextRunProperties(
                baseProperties.Typeface,
                baseProperties.FontRenderingEmSize,
                SpellCheckUnderline,
                baseProperties.ForegroundBrush,
                fontFeatures: _cachedFontFeatures);
            _underlinedPropertiesCache[baseProperties] = properties;
        }

        return properties;
    }

    /// <summary>
    /// Places <paramref name="overlay"/> (selection or IME preedit highlight) on top of the
    /// syntax spans, clipping any span it overlaps so the result stays sorted and non-overlapping.
    /// </summary>
    private static List<ValueSpan<TextRunProperties>> OverlaySpan(List<ValueSpan<TextRunProperties>>? spans, ValueSpan<TextRunProperties> overlay)
    {
        if (spans == null)
        {
            return [overlay];
        }

        var overlayStart = overlay.Start;
        var overlayEnd = overlay.Start + overlay.Length;
        var result = new List<ValueSpan<TextRunProperties>>(spans.Count + 2);
        var overlayAdded = false;

        foreach (var span in spans)
        {
            var spanStart = span.Start;
            var spanEnd = span.Start + span.Length;

            if (!overlayAdded && spanStart >= overlayEnd)
            {
                result.Add(overlay);
                overlayAdded = true;
            }

            if (spanEnd <= overlayStart || spanStart >= overlayEnd)
            {
                result.Add(span);
                continue;
            }

            if (spanStart < overlayStart)
            {
                result.Add(new ValueSpan<TextRunProperties>(spanStart, overlayStart - spanStart, span.Value));
            }

            if (!overlayAdded)
            {
                result.Add(overlay);
                overlayAdded = true;
            }

            if (spanEnd > overlayEnd)
            {
                result.Add(new ValueSpan<TextRunProperties>(overlayEnd, spanEnd - overlayEnd, span.Value));
            }
        }

        if (!overlayAdded)
        {
            result.Add(overlay);
        }

        return result;
    }
}
