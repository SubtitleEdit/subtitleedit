using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// ASSA "{\fsp..}" and "{\blur..}"/"{\be..}" reach the renderer as
/// <see cref="ImageParameter.TagLetterSpacing"/>/<see cref="ImageParameter.TagBlur"/>. Both only
/// exist in the effects pipeline, so a classic render with one of them has to keep looking
/// classic otherwise.
/// </summary>
public class ImageRendererSpacingBlurTagTests
{
    private static ImageParameter MakeParameter(float? letterSpacing = null, float? blur = null, TextEffects? effects = null)
    {
        return new ImageParameter
        {
            Text = "Hello world",
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 3,
            ShadowColor = SKColors.Black,
            ShadowWidth = 3,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            ContentAlignment = ExportContentAlignment.Center,
            TagLetterSpacing = letterSpacing,
            TagBlur = blur,
            TextEffects = effects,
        };
    }

    private static int CountSemiTransparent(SKBitmap bitmap)
    {
        var count = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var alpha = bitmap.GetPixel(x, y).Alpha;
                if (alpha is > 10 and < 245)
                {
                    count++;
                }
            }
        }

        return count;
    }

    [Fact]
    public void LetterSpacingTag_WidensTheBitmap()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter());
        using var spaced = ImageRenderer.GenerateBitmap(MakeParameter(letterSpacing: 10));

        // 11 characters, 10 gaps of 10 px
        Assert.InRange(spaced.Width - plain.Width, 90, 110);
        Assert.Equal(plain.Height, spaced.Height);
    }

    [Fact]
    public void ZeroLetterSpacingTag_LooksLikeTheClassicRender()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter());
        using var spaced = ImageRenderer.GenerateBitmap(MakeParameter(letterSpacing: 0));

        Assert.InRange(spaced.Width, plain.Width - 2, plain.Width + 2);
        Assert.Equal(plain.Height, spaced.Height);
    }

    [Fact]
    public void BlurTag_SoftensTheEdges()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter());
        using var blurred = ImageRenderer.GenerateBitmap(MakeParameter(blur: 3));

        Assert.True(CountSemiTransparent(blurred) > CountSemiTransparent(plain) * 2,
            $"blurred {CountSemiTransparent(blurred)} vs plain {CountSemiTransparent(plain)}");
    }

    [Fact]
    public void BlurTag_KeepsTheClassicOutlineAndFillColours()
    {
        using var blurred = ImageRenderer.GenerateBitmap(MakeParameter(blur: 1));

        var hasWhite = false;
        var hasBlack = false;
        for (var y = 0; y < blurred.Height; y++)
        {
            for (var x = 0; x < blurred.Width; x++)
            {
                var p = blurred.GetPixel(x, y);
                hasWhite |= p is { Alpha: > 240, Red: > 240, Green: > 240, Blue: > 240 };
                hasBlack |= p is { Alpha: > 240, Red: < 15, Green: < 15, Blue: < 15 };
            }
        }

        Assert.True(hasWhite);
        Assert.True(hasBlack);
    }

    [Fact]
    public void Tags_WinOverTheTextEffects()
    {
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.SoftShadow, 40, SKColors.White, SKColors.Black, SKColors.Black)!;
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter(effects: effects));

        var spacedEffects = TextEffectPresetFactory.Create(TextEffectPreset.SoftShadow, 40, SKColors.White, SKColors.Black, SKColors.Black)!;
        using var spaced = ImageRenderer.GenerateBitmap(MakeParameter(letterSpacing: 10, effects: spacedEffects));

        Assert.InRange(spaced.Width - plain.Width, 90, 110);
    }
}
