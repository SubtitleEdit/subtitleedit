using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// ASSA "{\fscx..}"/"{\fscy..}" reach the renderer as <see cref="ImageParameter.TagScaleX"/>/
/// <see cref="ImageParameter.TagScaleY"/> and stretch the glyphs - and with "\fscy" the line
/// box - like libass.
/// </summary>
public class ImageRendererScaleTagTests
{
    private static ImageParameter MakeParameter(float scaleX = 100, float scaleY = 100, TextEffects? effects = null)
    {
        return new ImageParameter
        {
            Text = "Hello world",
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ShadowColor = SKColors.Black,
            ShadowWidth = 2,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            ContentAlignment = ExportContentAlignment.Center,
            TagScaleX = scaleX,
            TagScaleY = scaleY,
            TextEffects = effects,
        };
    }

    [Fact]
    public void ScaleX_StretchesTheWidthOnly()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter());
        using var wide = ImageRenderer.GenerateBitmap(MakeParameter(scaleX: 200));

        Assert.InRange(wide.Width / (double)plain.Width, 1.85, 2.1);
        Assert.Equal(plain.Height, wide.Height);
    }

    [Fact]
    public void ScaleY_StretchesTheHeightOnly()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter());
        using var tall = ImageRenderer.GenerateBitmap(MakeParameter(scaleY: 200));

        Assert.InRange(tall.Height / (double)plain.Height, 1.8, 2.1);
        Assert.InRange(tall.Width, plain.Width - 3, plain.Width + 3);
    }

    [Fact]
    public void ScaleX_WorksWithTextEffects()
    {
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.DoubleOutline, 40, SKColors.White, SKColors.Black, SKColors.Black);
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter(effects: effects));
        using var narrow = ImageRenderer.GenerateBitmap(MakeParameter(scaleX: 50, effects: effects));

        Assert.InRange(narrow.Width / (double)plain.Width, 0.45, 0.65);
    }

    [Fact]
    public void ZeroScale_DoesNotThrow()
    {
        using var bitmap = ImageRenderer.GenerateBitmap(MakeParameter(scaleX: 0, scaleY: 0));

        Assert.True(bitmap.Width > 0);
    }
}
