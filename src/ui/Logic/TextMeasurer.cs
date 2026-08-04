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
