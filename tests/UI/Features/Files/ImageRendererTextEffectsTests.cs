using System;
using System.Linq;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using Xunit;

namespace UITests.Features.Files;

/// <summary>
/// The advanced text effects path in ImageRenderer: when ImageParameter.TextEffects is set,
/// rendering goes through the layered pipeline (gradient fills, multiple outlines, blurred
/// shadows, glow, extrude, bevel) instead of the classic fill/outline/shadow path.
/// </summary>
public class ImageRendererTextEffectsTests
{
    private static ImageParameter MakeParameter(TextEffectPreset preset)
    {
        return new ImageParameter
        {
            Text = "Hello <i>world</i>",
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ShadowColor = SKColors.Black,
            ShadowWidth = 2,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            LineSpacingPercent = 0,
            ContentAlignment = ExportContentAlignment.Center,
            TextEffects = TextEffectPresetFactory.Create(preset, 40, SKColors.White, SKColors.Black, SKColors.Black),
        };
    }

    private static bool HasVisiblePixels(SKBitmap bitmap)
    {
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [Fact]
    public void NonePreset_YieldsNullEffects_SoClassicPathIsUsed()
    {
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.None, 40, SKColors.White, SKColors.Black, SKColors.Black);
        Assert.Null(effects);
    }

    [Theory]
    [InlineData(TextEffectPreset.SoftShadow)]
    [InlineData(TextEffectPreset.GradientGold)]
    [InlineData(TextEffectPreset.DoubleOutline)]
    [InlineData(TextEffectPreset.NeonGlow)]
    [InlineData(TextEffectPreset.Extrude3D)]
    [InlineData(TextEffectPreset.Chrome)]
    [InlineData(TextEffectPreset.Fire)]
    [InlineData(TextEffectPreset.Comic)]
    [InlineData(TextEffectPreset.Retro80s)]
    [InlineData(TextEffectPreset.Anaglyph3D)]
    [InlineData(TextEffectPreset.Ice)]
    [InlineData(TextEffectPreset.Emboss)]
    [InlineData(TextEffectPreset.Hollow)]
    public void GenerateBitmap_WithPreset_DrawsVisiblePixels(TextEffectPreset preset)
    {
        var ip = MakeParameter(preset);

        using var bitmap = ImageRenderer.GenerateBitmap(ip);

        Assert.True(bitmap.Width > 2, $"bitmap width was {bitmap.Width}");
        Assert.True(bitmap.Height > 2, $"bitmap height was {bitmap.Height}");
        Assert.True(HasVisiblePixels(bitmap), "expected at least one non-transparent pixel");
    }

    [Fact]
    public void GenerateBitmap_WithPreset_DiffersFromClassicRendering()
    {
        var classic = MakeParameter(TextEffectPreset.None);
        var styled = MakeParameter(TextEffectPreset.GradientGold);

        using var classicBitmap = ImageRenderer.GenerateBitmap(classic);
        using var styledBitmap = ImageRenderer.GenerateBitmap(styled);

        using var classicImage = SKImage.FromBitmap(classicBitmap);
        using var styledImage = SKImage.FromBitmap(styledBitmap);
        using var classicPng = classicImage.Encode(SKEncodedImageFormat.Png, 100);
        using var styledPng = styledImage.Encode(SKEncodedImageFormat.Png, 100);

        Assert.False(classicPng.ToArray().SequenceEqual(styledPng.ToArray()));
    }

    [Fact]
    public void GenerateBitmap_EmptyText_StillReturnsTinyTransparentBitmap()
    {
        var ip = MakeParameter(TextEffectPreset.NeonGlow);
        ip.Text = "   ";

        using var bitmap = ImageRenderer.GenerateBitmap(ip);

        Assert.True(bitmap.Width <= 2 && bitmap.Height <= 2, "whitespace-only text should not produce a full-size bitmap");
    }

    [Fact]
    public void LetterSpacing_WidensTheRenderedText()
    {
        var plain = MakeParameter(TextEffectPreset.SoftShadow);
        var spaced = MakeParameter(TextEffectPreset.SoftShadow);
        spaced.TextEffects = TextEffectPresetFactory.Create(TextEffectPreset.SoftShadow, 40,
            SKColors.White, SKColors.Black, SKColors.Black,
            new TextEffectAdjustments { LetterSpacing = 20 });

        using var plainBitmap = ImageRenderer.GenerateBitmap(plain);
        using var spacedBitmap = ImageRenderer.GenerateBitmap(spaced);

        Assert.True(spacedBitmap.Width > plainBitmap.Width + 50,
            $"spaced width {spacedBitmap.Width} should clearly exceed plain width {plainBitmap.Width}");
    }

    [Fact]
    public void ArcBend_MakesTheRenderedTextTaller()
    {
        var straight = MakeParameter(TextEffectPreset.SoftShadow);
        straight.Text = "A longer single line of subtitle text";
        var curved = MakeParameter(TextEffectPreset.SoftShadow);
        curved.Text = straight.Text;
        curved.TextEffects = TextEffectPresetFactory.Create(TextEffectPreset.SoftShadow, 40,
            SKColors.White, SKColors.Black, SKColors.Black,
            new TextEffectAdjustments { ArcBendPercent = 60 });

        using var straightBitmap = ImageRenderer.GenerateBitmap(straight);
        using var curvedBitmap = ImageRenderer.GenerateBitmap(curved);

        Assert.True(curvedBitmap.Height > straightBitmap.Height + 20,
            $"curved height {curvedBitmap.Height} should clearly exceed straight height {straightBitmap.Height}");
    }

    [Fact]
    public void Strength_ScalesEffectSizes()
    {
        var normal = TextEffectPresetFactory.Create(TextEffectPreset.DoubleOutline, 40,
            SKColors.White, SKColors.Black, SKColors.Black,
            new TextEffectAdjustments { StrengthPercent = 100 })!;
        var strong = TextEffectPresetFactory.Create(TextEffectPreset.DoubleOutline, 40,
            SKColors.White, SKColors.Black, SKColors.Black,
            new TextEffectAdjustments { StrengthPercent = 200 })!;

        Assert.Equal(normal.Strokes[0].Width * 2, strong.Strokes[0].Width, 3);
        Assert.Equal(normal.Shadows[0].Blur * 2, strong.Shadows[0].Blur, 3);
    }

    [Fact]
    public void SafetyMargin_CoversBlurredShadowAndGlow()
    {
        var effects = new TextEffects
        {
            Shadows = { new TextEffectShadow { Dx = 4, Dy = 6, Blur = 10 } },
            Glow = new TextEffectGlow { Radius = 20 },
        };

        var margin = effects.GetSafetyMargin();

        Assert.True(margin >= 20 * 3, $"margin {margin} does not cover the glow radius");
    }
}
