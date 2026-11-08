using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// "&lt;u&gt;" and "&lt;s&gt;" (from ASSA "{\u1}"/"{\s1}" or HTML input) draw a bar under or
/// through the text, with the letters' outline - they used to be dropped.
/// </summary>
public class ImageRendererUnderlineStrikeoutTests
{
    private static SKBitmap Render(string text, TextEffects? effects = null)
    {
        return ImageRenderer.GenerateBitmap(new ImageParameter
        {
            Text = text,
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
            TextEffects = effects,
        });
    }

    // Rows where white fill runs across most of the drawn width - only a bar does that. Gaps up
    // to an outline wide are bridged: where two segments meet, the next one's outline is drawn
    // over the end of the previous one's bar, as it is over touching letters.
    private static List<int> BarRows(SKBitmap bitmap)
    {
        const int maxGap = 6;
        var rows = new List<int>();
        for (var y = 0; y < bitmap.Height; y++)
        {
            var longest = 0;
            var runStart = -1;
            var lastWhite = -1;
            for (var x = 0; x < bitmap.Width; x++)
            {
                var p = bitmap.GetPixel(x, y);
                if (p is not { Alpha: > 200, Red: > 200, Green: > 200, Blue: > 200 })
                {
                    continue;
                }

                if (runStart < 0 || x - lastWhite > maxGap)
                {
                    runStart = x;
                }

                lastWhite = x;
                longest = Math.Max(longest, x - runStart + 1);
            }

            if (longest > bitmap.Width * 0.8)
            {
                rows.Add(y);
            }
        }

        return rows;
    }

    [Fact]
    public void PlainText_HasNoBar()
    {
        using var bitmap = Render("Hello world");
        Assert.Empty(BarRows(bitmap));
    }

    [Fact]
    public void Underline_DrawsABarUnderTheText()
    {
        using var bitmap = Render("<u>Hello world</u>");
        var rows = BarRows(bitmap);

        Assert.NotEmpty(rows);
        Assert.True(rows.Min() > bitmap.Height * 0.6, $"bar at {rows.Min()} of {bitmap.Height}");
    }

    [Fact]
    public void Strikeout_DrawsABarThroughTheText()
    {
        using var bitmap = Render("<s>Hello world</s>");
        var rows = BarRows(bitmap);

        Assert.NotEmpty(rows);
        Assert.InRange(rows.Average(), bitmap.Height * 0.25, bitmap.Height * 0.65);
    }

    [Fact]
    public void Strikeout_SurvivesTheItalicClosingInside()
    {
        // "{\s1\i1}Hel{\i0}lo" - the bar runs under the whole word, not just the italic part.
        using var bitmap = Render("<i><s>Hello</i> world");
        Assert.NotEmpty(BarRows(bitmap));
    }

    [Fact]
    public void Underline_WorksWithTextEffects()
    {
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.DoubleOutline, 40, SKColors.White, SKColors.Black, SKColors.Black);
        using var bitmap = Render("<u>Hello world</u>", effects);

        Assert.NotEmpty(BarRows(bitmap));
    }

    [Fact]
    public void Underline_WorksWithLetterSpacing()
    {
        // Letter spacing goes through the per-glyph geometry path.
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.DoubleOutline, 40, SKColors.White, SKColors.Black, SKColors.Black,
            new TextEffectAdjustments { LetterSpacing = 6 });
        using var bitmap = Render("<u>Hello world</u>", effects);

        Assert.NotEmpty(BarRows(bitmap));
    }
}
