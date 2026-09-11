using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// Right/center justified Arabic lines share the same edge (issue #14696): line widths come
/// from the shaped glyphs, not the unshaped string, and "Right" is the visual right edge
/// whether or not the right-to-left flag is on.
/// </summary>
public class ImageRendererRightToLeftAlignmentTests
{
    private static ImageParameter MakeParameter(string text, ExportContentAlignment alignment, bool isRightToLeft)
    {
        return new ImageParameter
        {
            Text = text,
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 0,
            ShadowColor = SKColors.Black,
            ShadowWidth = 0,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            LineSpacingPercent = 0,
            ContentAlignment = alignment,
            IsRightToLeft = isRightToLeft,
        };
    }

    /// <summary>Rightmost/leftmost column with an opaque pixel in each half of the bitmap.</summary>
    private static (int TopLeft, int TopRight, int BottomLeft, int BottomRight) GetInkEdges(SKBitmap bitmap)
    {
        var half = bitmap.Height / 2;
        int Edge(int y0, int y1, bool right)
        {
            var edge = right ? -1 : int.MaxValue;
            for (var y = y0; y < y1; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).Alpha > 128)
                    {
                        edge = right ? Math.Max(edge, x) : Math.Min(edge, x);
                    }
                }
            }
            return edge;
        }

        return (Edge(0, half, false), Edge(0, half, true), Edge(half, bitmap.Height, false), Edge(half, bitmap.Height, true));
    }

    // Two Arabic lines of very different width: unshaped measuring overestimated the long
    // line by ~40%, so the short line ended tens of pixels away from the long line's edge.
    private const string ArabicTwoLines = "- حسناً، استمرّ\n- (شفرة دافينشي)";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RightAlignedArabicLinesShareTheRightEdge(bool isRightToLeft)
    {
        using var bitmap = ImageRenderer.GenerateBitmap(MakeParameter(ArabicTwoLines, ExportContentAlignment.Right, isRightToLeft));

        var edges = GetInkEdges(bitmap);

        Assert.True(Math.Abs(edges.TopRight - edges.BottomRight) <= 2, $"right edges {edges.TopRight} vs {edges.BottomRight}");
        Assert.True(edges.TopLeft != edges.BottomLeft, "lines are expected to differ in width");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LeftAlignedArabicLinesShareTheLeftEdge(bool isRightToLeft)
    {
        using var bitmap = ImageRenderer.GenerateBitmap(MakeParameter(ArabicTwoLines, ExportContentAlignment.Left, isRightToLeft));

        var edges = GetInkEdges(bitmap);

        Assert.True(Math.Abs(edges.TopLeft - edges.BottomLeft) <= 2, $"left edges {edges.TopLeft} vs {edges.BottomLeft}");
    }

    [Fact]
    public void CenteredArabicLinesShareTheCenter()
    {
        using var bitmap = ImageRenderer.GenerateBitmap(MakeParameter(ArabicTwoLines, ExportContentAlignment.Center, true));

        var edges = GetInkEdges(bitmap);
        var topCenter = (edges.TopLeft + edges.TopRight) / 2.0;
        var bottomCenter = (edges.BottomLeft + edges.BottomRight) / 2.0;

        Assert.True(Math.Abs(topCenter - bottomCenter) <= 2, $"centers {topCenter} vs {bottomCenter}");
    }
}
