using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// The small gap the renderer adds between segments is only for an italic letter followed
/// right away by an upright one. It was added after every italic or bold segment, so a
/// coloured word in an italic or bold line got extra space on both sides.
/// </summary>
public class ImageRendererSegmentGapTests
{
    private const int Tolerance = 7;

    private static int Width(string text, ExportBoxType boxType = ExportBoxType.None, TextEffects? effects = null)
    {
        using var bitmap = ImageRenderer.GenerateBitmap(new ImageParameter
        {
            Text = text,
            FontName = "Arial",
            FontSize = 60,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 3,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            ContentAlignment = ExportContentAlignment.Center,
            BoxType = boxType,
            BackgroundColor = SKColors.Blue,
            TextEffects = effects,
        });
        return bitmap.Width;
    }

    [Theory]
    [InlineData("<i>Hello world again</i>", "<i>Hello <font color=\"#ff4040\">world</font> again</i>")]
    [InlineData("<b>Hello world again</b>", "<b>Hello <font color=\"#ff4040\">world</font> again</b>")]
    [InlineData("<i>Hello world</i>", "<i>Hello</i> world")] // the space already keeps them apart
    [InlineData("<b>Hello</b>world", "<b>Hello</b><font color=\"#ff4040\">world</font>")]
    public void ColourChange_AddsNoSpace(string plain, string coloured)
    {
        // Same text, so about the same width: italic and upright letters and the italic
        // overhang differ by a few pixels, the gap (0.17 em, about 10 px here) by more.
        Assert.InRange(Width(coloured), Width(plain) - Tolerance, Width(plain) + Tolerance);
    }

    [Fact]
    public void ColourChange_AddsNoSpace_WithABoxAndWithEffects()
    {
        const string plain = "<i>Hello world again</i>";
        const string coloured = "<i>Hello <font color=\"#ff4040\">world</font> again</i>";
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.DoubleOutline, 60, SKColors.White, SKColors.Black, SKColors.Black);

        Assert.InRange(Width(coloured, ExportBoxType.BoxPerLine), Width(plain, ExportBoxType.BoxPerLine) - Tolerance, Width(plain, ExportBoxType.BoxPerLine) + Tolerance);
        Assert.InRange(Width(coloured, effects: effects), Width(plain, effects: effects) - Tolerance, Width(plain, effects: effects) + Tolerance);
    }

    [Fact]
    public void ItalicTouchingUpright_KeepsTheGap()
    {
        // "Hel" slants into "lo" without the gap - 0.17 em at 60 px is about 10 px.
        Assert.True(Width("<i>Hel</i>lo") >= Width("<i>Hel</i><i>lo</i>") + 5);
    }
}
