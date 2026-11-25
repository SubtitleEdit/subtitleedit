using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using SkiaSharp;

namespace UITests.Controls;

/// <summary>
/// #17189: the Skia waveform renderer picked one fallback font for a whole line from its first
/// character the UI font lacked, so a CJK Extension B ideograph after common Han characters (or
/// any mix of scripts no single font covers) was drawn as boxes. Each run must now use a font that
/// has its characters.
/// </summary>
public class SkiaTextCacheFallbackTests
{
    [Theory]
    [InlineData("…何者是採𤂼yào煉丹 何者是藥苗老嫩…")]
    [InlineData("Hello 何者 𤂼 world")]
    [InlineData("abc ελληνικά 日本語 한국어")]
    public void EveryCharacterIsInItsRunsTypefaceWhenAnyFontHasIt(string text)
    {
        var cache = new SkiaTextCache();
        cache.SetFont("Arial", false);

        var segments = cache.SplitByTypeface(text);
        Assert.Equal(text, string.Concat(segments.Select(s => s.Text)));

        foreach (var (segmentText, typeface) in segments)
        {
            foreach (var rune in segmentText.EnumerateRunes())
            {
                if (System.Text.Rune.IsWhiteSpace(rune))
                {
                    continue;
                }

                using var any = SKFontManager.Default.MatchCharacter(rune.Value);
                if (any == null || !any.ContainsGlyph(rune.Value))
                {
                    continue; // no installed font has it - nothing to fall back to
                }

                Assert.True(typeface.ContainsGlyph(rune.Value),
                    $"U+{rune.Value:X4} in \"{segmentText}\" drawn with {typeface.FamilyName}, which lacks it");
            }
        }
    }

    [Fact]
    public void LatinOnlyLineIsOneRunInTheUiFont()
    {
        var cache = new SkiaTextCache();
        cache.SetFont("Arial", false);

        var segments = cache.SplitByTypeface("Hello, world!");
        Assert.Single(segments);
    }

    [Fact]
    public void MixedLineShapesWithInk()
    {
        var cache = new SkiaTextCache();
        cache.SetFont("Arial", false);

        var shaped = cache.Get("何者是採𤂼yào煉丹", 16f);
        Assert.NotNull(shaped.Blob);
        Assert.True(shaped.Width > 0);
    }
}
