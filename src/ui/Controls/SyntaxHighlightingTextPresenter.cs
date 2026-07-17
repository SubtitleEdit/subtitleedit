using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="TextPresenter"/> that colors HTML tags and ASS/SSA override tags while typing,
/// using the same color scheme as the AvaloniaEdit colorizer (via
/// <see cref="SubtitleSyntaxTokenizer"/>).
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

    private GenericTextRunProperties GetDefaultProperties(Typeface typeface)
    {
        if (_defaultProperties is null ||
            !typeface.Equals(_cachedTypeface) ||
            FontSize != _cachedFontSize ||
            !ReferenceEquals(Foreground, _cachedForeground) ||
            !ReferenceEquals(FontFeatures, _cachedFontFeatures))
        {
            _cachedTypeface = typeface;
            _cachedFontSize = FontSize;
            _cachedForeground = Foreground;
            _cachedFontFeatures = FontFeatures;
            _defaultProperties = new GenericTextRunProperties(
                typeface,
                FontSize,
                foregroundBrush: Foreground,
                fontFeatures: FontFeatures);
            _tokenPropertiesCache.Clear();
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
            var selectionHighlight = new ValueSpan<TextRunProperties>(start, length,
                new GenericTextRunProperties(
                    typeface,
                    FontSize,
                    foregroundBrush: SelectionForegroundBrush,
                    fontFeatures: FontFeatures));

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

        var tokens = SubtitleSyntaxTokenizer.Tokenize(text);
        if (tokens.Count == 0)
        {
            return null;
        }

        var defaultProperties = GetDefaultProperties(typeface);

        var spans = new List<ValueSpan<TextRunProperties>>(tokens.Count * 2 + 1);
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

        return spans;
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
