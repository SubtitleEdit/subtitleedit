using SkiaSharp;

namespace Nikse.SubtitleEdit.Logic;

public static class TextMeasurer
{
    public static SKSize MeasureString(string text, string fontFamily, float fontSize, SKFontStyleWeight weight = SKFontStyleWeight.Normal)
    {
        using var typeface = SKTypeface.FromFamilyName(fontFamily, weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var font = new SKFont(typeface, fontSize);

        float width = font.MeasureText(text);
        var metrics = font.Metrics;
        float height = metrics.Descent - metrics.Ascent;

        return new SKSize(width, height);
    }
}
