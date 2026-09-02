using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// "Content alignment: From alignment" (issue #14202) - the justification of the lines inside
/// the subtitle follows the "{\anX}" tag that already decides where the subtitle is placed, so
/// a left placed cue gets left justified lines instead of the one justification picked for the
/// whole export.
/// </summary>
public class ExportContentAlignmentFromAlignmentTests
{
    [Theory]
    [InlineData(ExportAlignment.TopLeft, ExportContentAlignment.Left)]
    [InlineData(ExportAlignment.MiddleLeft, ExportContentAlignment.Left)]
    [InlineData(ExportAlignment.BottomLeft, ExportContentAlignment.Left)]
    [InlineData(ExportAlignment.TopRight, ExportContentAlignment.Right)]
    [InlineData(ExportAlignment.MiddleRight, ExportContentAlignment.Right)]
    [InlineData(ExportAlignment.BottomRight, ExportContentAlignment.Right)]
    [InlineData(ExportAlignment.TopCenter, ExportContentAlignment.Center)]
    [InlineData(ExportAlignment.MiddleCenter, ExportContentAlignment.Center)]
    [InlineData(ExportAlignment.BottomCenter, ExportContentAlignment.Center)]
    public void ResolvedContentAlignment_FollowsAlignment(ExportAlignment alignment, ExportContentAlignment expected)
    {
        var param = new ImageParameter
        {
            Alignment = alignment,
            ContentAlignment = ExportContentAlignment.FromAlignment,
        };

        Assert.Equal(expected, param.ResolvedContentAlignment);
    }

    [Theory]
    [InlineData(ExportContentAlignment.Left)]
    [InlineData(ExportContentAlignment.Center)]
    [InlineData(ExportContentAlignment.Right)]
    public void ResolvedContentAlignment_PassesAnExplicitChoiceThrough(ExportContentAlignment contentAlignment)
    {
        // A picked justification still wins over the placement, whatever the "{\anX}" tag said.
        var param = new ImageParameter
        {
            Alignment = ExportAlignment.BottomLeft,
            ContentAlignment = contentAlignment,
        };

        Assert.Equal(contentAlignment, param.ResolvedContentAlignment);
    }

    [Theory]
    [InlineData(ExportAlignment.BottomLeft, ExportContentAlignment.Left)]
    [InlineData(ExportAlignment.BottomRight, ExportContentAlignment.Right)]
    [InlineData(ExportAlignment.BottomCenter, ExportContentAlignment.Center)]
    public void Render_FromAlignment_MatchesTheEquivalentExplicitJustification(
        ExportAlignment alignment, ExportContentAlignment equivalent)
    {
        using var fromAlignment = ImageRenderer.GenerateBitmap(
            MakeParameter(alignment, ExportContentAlignment.FromAlignment));
        using var explicitChoice = ImageRenderer.GenerateBitmap(MakeParameter(alignment, equivalent));

        Assert.True(PixelsEqual(fromAlignment, explicitChoice));
    }

    [Fact]
    public void Render_FromAlignment_DiffersFromTheGlobalJustification()
    {
        // The point of the option: a left placed cue no longer renders centered.
        using var fromAlignment = ImageRenderer.GenerateBitmap(
            MakeParameter(ExportAlignment.BottomLeft, ExportContentAlignment.FromAlignment));
        using var centered = ImageRenderer.GenerateBitmap(
            MakeParameter(ExportAlignment.BottomLeft, ExportContentAlignment.Center));

        Assert.False(PixelsEqual(fromAlignment, centered));
    }

    /// <summary>
    /// Two lines of clearly different width - with one line every justification renders the
    /// same bitmap, so the test would pass without the feature.
    /// </summary>
    private static ImageParameter MakeParameter(ExportAlignment alignment, ExportContentAlignment contentAlignment)
    {
        return new ImageParameter
        {
            Text = "A much longer first line" + Environment.NewLine + "Short",
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ShadowColor = SKColors.Black,
            ShadowWidth = 2,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            Alignment = alignment,
            ContentAlignment = contentAlignment,
        };
    }

    private static bool PixelsEqual(SKBitmap a, SKBitmap b)
    {
        if (a.Width != b.Width || a.Height != b.Height)
        {
            return false;
        }

        for (var y = 0; y < a.Height; y++)
        {
            for (var x = 0; x < a.Width; x++)
            {
                if (a.GetPixel(x, y) != b.GetPixel(x, y))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
