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
            FontName = ArabicFontName.Value,
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

    // Arial when it covers the test text (Windows, macOS), else the first installed font that
    // does - on Linux CI "Arial" has no Arabic, and mixing in fallback-font glyphs makes the ink
    // edges depend on two fonts' side bearings instead of on the shaped line widths these
    // tests are about.
    private static readonly Lazy<string> ArabicFontName = new(() =>
    {
        const string preferred = "Arial";
        var text = ArabicTwoLines.Replace("\n", string.Empty);
        foreach (var face in FontFaces.GetFontFaces().Prepend(preferred))
        {
            using var typeface = FontFaces.CreateTypeface(face, false, false);
            if (typeface != null && typeface.ContainsGlyphs(text))
            {
                return face;
            }
        }

        return preferred;
    });

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

        // A center is the mean of two ink edges, so side-bearing differences between the
        // first/last glyphs of the two lines add up - 2.5 px with the font picked on Linux CI.
        // The bug this guards against was tens of pixels.
        Assert.True(Math.Abs(topCenter - bottomCenter) <= 3, $"centers {topCenter} vs {bottomCenter}");
    }
}
