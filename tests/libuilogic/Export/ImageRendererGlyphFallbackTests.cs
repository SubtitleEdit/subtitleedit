using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// Characters the chosen font has no glyph for - "♪" in many text fonts - render with a
/// fallback system font instead of as nothing (issue #15499).
/// </summary>
public class ImageRendererGlyphFallbackTests
{
    private const string MusicNote = "♪";

    private static ImageParameter MakeParameter(string text, string fontName, TextEffects? effects = null)
    {
        return new ImageParameter
        {
            Text = text,
            FontName = fontName,
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ShadowColor = SKColors.Black,
            ShadowWidth = 0,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            LineSpacingPercent = 0,
            ContentAlignment = ExportContentAlignment.Center,
            TextEffects = effects,
        };
    }

    private static int CountWhitePixels(SKBitmap bitmap)
    {
        var count = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var p = bitmap.GetPixel(x, y);
                if (p.Alpha > 200 && p.Red > 220 && p.Green > 220 && p.Blue > 220)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// An installed face without a "♪" glyph while some other installed font has one, or null.
    /// </summary>
    private static string? FindFaceWithoutMusicNote()
    {
        using var fallback = SKFontManager.Default.MatchCharacter(0x266A);
        if (fallback == null || !fallback.ContainsGlyph(0x266A))
        {
            return null;
        }

        foreach (var face in FontFaces.GetFontFaces())
        {
            using var typeface = FontFaces.CreateTypeface(face, false, false);
            if (typeface != null && typeface.ContainsGlyph('A') && !typeface.ContainsGlyph(0x266A))
            {
                return face;
            }
        }

        return null;
    }

    [Fact]
    public void MissingGlyph_RendersWithFallbackFont()
    {
        var face = FindFaceWithoutMusicNote();
        if (face == null)
        {
            return; // no font without the glyph, or no font with it, on this machine
        }

        using var noteOnly = ImageRenderer.GenerateBitmap(MakeParameter(MusicNote, face));
        Assert.True(CountWhitePixels(noteOnly) > 20, $"\"♪\" rendered as nothing with {face}");

        using var withNotes = ImageRenderer.GenerateBitmap(MakeParameter($"{MusicNote} Hello {MusicNote}", face));
        using var withoutNotes = ImageRenderer.GenerateBitmap(MakeParameter("Hello", face));
        Assert.True(CountWhitePixels(withNotes) > CountWhitePixels(withoutNotes) + 40);
    }

    [Fact]
    public void MissingGlyph_RendersWithFallbackFont_WithTextEffects()
    {
        var face = FindFaceWithoutMusicNote();
        if (face == null)
        {
            return;
        }

        var effects = new TextEffects { LetterSpacing = 2 };
        using var noteOnly = ImageRenderer.GenerateBitmap(MakeParameter(MusicNote, face, effects));
        Assert.True(CountWhitePixels(noteOnly) > 20, $"\"♪\" rendered as nothing with {face}");
    }
}
