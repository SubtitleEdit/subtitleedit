using Avalonia.Controls;
using SkiaSharp;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public static class TextMeasurer
{
    // Callers (statistics, batch convert) measure thousands of lines with the same font, and
    // SKTypeface.FromFamilyName is a font-manager lookup - cache the font per (family, size,
    // weight). The lock also serializes MeasureText, which SKFont does not allow concurrently.
    private static readonly object CacheLock = new();
    private static readonly Dictionary<(string family, float size, SKFontStyleWeight weight), SKFont> FontCache = new();

    private static (string family, float size)? _defaultFont;

    /// <summary>
    /// Measures with the font a new, unattached <see cref="TextBlock"/> has. Callers created a
    /// TextBlock per measured line just to read those two defaults; they are read once here.
    /// </summary>
    public static SKSize MeasureStringWithDefaultFont(string text)
    {
        var defaultFont = _defaultFont ??= GetDefaultFont();
        return MeasureString(text, defaultFont.family, defaultFont.size);
    }

    private static (string family, float size) GetDefaultFont()
    {
        var textBlock = new TextBlock();
        return (textBlock.FontFamily.Name, (float)textBlock.FontSize);
    }

    public static SKSize MeasureString(string text, string fontFamily, float fontSize, SKFontStyleWeight weight = SKFontStyleWeight.Normal)
    {
        lock (CacheLock)
        {
            var key = (fontFamily, fontSize, weight);
            if (!FontCache.TryGetValue(key, out var font))
            {
                var typeface = SKTypeface.FromFamilyName(fontFamily, weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                font = new SKFont(typeface, fontSize);
                FontCache[key] = font;
            }

            float width = font.MeasureText(text);
            var metrics = font.Metrics;
            float height = metrics.Descent - metrics.Ascent;

            return new SKSize(width, height);
        }
    }
}
