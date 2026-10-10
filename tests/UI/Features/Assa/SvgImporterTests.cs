using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// SVG import for ASSA draw: shapes, transforms, colors -> layers, strokes as outlines, holes.
/// </summary>
public class SvgImporterTests
{
    private static List<DrawShape> Import(string body, string viewBox = "0 0 1920 1080", int firstLayer = 0) =>
        SvgImporter.Import($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"{viewBox}\">{body}</svg>", 1920, 1080, firstLayer).Shapes;

    private static float SignedArea(DrawShape shape)
    {
        var polygon = shape.Flatten();
        var area = 0f;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            area += polygon[j].X * polygon[i].Y - polygon[i].X * polygon[j].Y;
        }

        return area / 2f;
    }

    [Fact]
    public void Rect_FullFrameViewBox_KeepsCoordinates()
    {
        var shapes = Import("<rect x=\"10\" y=\"20\" width=\"100\" height=\"50\" fill=\"#ff0000\"/>");

        var shape = Assert.Single(shapes);
        Assert.Equal("m 10 20 l 110 20 110 70 10 70", shape.ToAssa());
        Assert.Equal(Colors.Red, shape.ForeColor);
        Assert.Equal(0, shape.Layer);
    }

    [Fact]
    public void OtherAspectRatio_IsFittedToHalfTheFrameAndCentered()
    {
        var shape = Assert.Single(Import("<rect width=\"100\" height=\"100\"/>", "0 0 100 100"));

        var (left, top, right, bottom) = shape.GetBounds();
        Assert.Equal(690, left, 1);
        Assert.Equal(270, top, 1);
        Assert.Equal(1230, right, 1);
        Assert.Equal(810, bottom, 1);
    }

    [Fact]
    public void ArcPath_BecomesBezierCurves()
    {
        var shape = Assert.Single(Import("<path d=\"M 100 200 A 100 100 0 1 1 300 200 A 100 100 0 1 1 100 200 Z\"/>"));

        Assert.DoesNotContain(" l ", shape.ToAssa());
        Assert.Contains(" b ", shape.ToAssa());
        Assert.True(shape.HitTest(200, 200, 0));
        Assert.False(shape.HitTest(110, 110, 0));
    }

    [Fact]
    public void EvenOddHole_IsWoundOppositeToOutline()
    {
        var shapes = Import("<path fill-rule=\"evenodd\" d=\"M0 0 H100 V100 H0 Z M25 25 H75 V75 H25 Z\"/>");

        Assert.Equal(2, shapes.Count);
        Assert.True(SignedArea(shapes[0]) * SignedArea(shapes[1]) < 0);
    }

    [Fact]
    public void GroupTransforms_AreBakedIntoPoints()
    {
        var shape = Assert.Single(Import("<g transform=\"translate(100 50)\"><rect transform=\"scale(2)\" width=\"10\" height=\"10\"/></g>"));

        Assert.Equal("m 100 50 l 120 50 120 70 100 70", shape.ToAssa());
    }

    [Fact]
    public void Colors_FromClassStyleAndAttributes_StartNewLayers()
    {
        var shapes = Import(
            "<style>.a{fill:#0f0}</style>" +
            "<rect class=\"a\" width=\"10\" height=\"10\"/>" +
            "<rect x=\"20\" class=\"a\" width=\"10\" height=\"10\"/>" +
            "<rect x=\"40\" style=\"fill:rgb(0, 0, 255)\" width=\"10\" height=\"10\"/>" +
            "<rect x=\"60\" fill=\"none\" width=\"10\" height=\"10\"/>", firstLayer: 3);

        Assert.Equal(3, shapes.Count);
        Assert.Equal(Color.FromRgb(0, 255, 0), shapes[0].ForeColor);
        Assert.Equal(3, shapes[0].Layer);
        Assert.Equal(3, shapes[1].Layer);
        Assert.Equal(Colors.Blue, shapes[2].ForeColor);
        Assert.Equal(4, shapes[2].Layer);
    }

    [Fact]
    public void Stroke_BecomesFilledOutlineOnOwnLayer()
    {
        var shapes = Import("<line x1=\"0\" y1=\"100\" x2=\"200\" y2=\"100\" stroke=\"white\" stroke-width=\"20\"/>");

        var shape = Assert.Single(shapes);
        Assert.Equal(Colors.White, shape.ForeColor);
        var (left, top, right, bottom) = shape.GetBounds();
        Assert.Equal(0, left, 1);
        Assert.Equal(90, top, 1);
        Assert.Equal(200, right, 1);
        Assert.Equal(110, bottom, 1);
    }

    [Fact]
    public void Gradient_UsesFirstStopColor_AndOpacityBecomesAlpha()
    {
        var shape = Assert.Single(Import(
            "<defs><linearGradient id=\"g\"><stop offset=\"0\" stop-color=\"#ff8000\"/><stop offset=\"1\" stop-color=\"#000\"/></linearGradient></defs>" +
            "<rect width=\"10\" height=\"10\" fill=\"url(#g)\" opacity=\"0.5\"/>"));

        Assert.Equal(Color.FromArgb(128, 255, 128, 0), shape.ForeColor);
    }

    [Theory]
    [InlineData("#ff000080", 128, 255, 0, 0)]
    [InlineData("#00ff0040", 64, 0, 255, 0)]
    [InlineData("#f008", 136, 255, 0, 0)]
    [InlineData("#0000ffff", 255, 0, 0, 255)]
    public void HexColorWithAlpha_IsReadAsRgba(string fill, byte a, byte r, byte g, byte b)
    {
        var shape = Assert.Single(Import($"<rect width=\"10\" height=\"10\" fill=\"{fill}\"/>"));

        Assert.Equal(Color.FromArgb(a, r, g, b), shape.ForeColor);
    }

    [Theory]
    [InlineData("grey", "gray")]
    [InlineData("lightgrey", "lightgray")]
    [InlineData("darkgrey", "darkgray")]
    [InlineData("slategrey", "slategray")]
    [InlineData("dimgrey", "dimgray")]
    [InlineData("lightslategrey", "lightslategray")]
    [InlineData("DarkSlateGrey", "darkslategray")]
    public void GreySpelling_IsAcceptedLikeGray(string grey, string gray)
    {
        var shape = Assert.Single(Import($"<rect width=\"10\" height=\"10\" fill=\"{grey}\"/>"));

        Assert.Equal(Color.Parse(gray), shape.ForeColor);
    }

    [Theory]
    [InlineData("opacity=\"50%\"")]
    [InlineData("fill-opacity=\"50%\"")]
    [InlineData("style=\"fill-opacity: 50%\"")]
    public void PercentOpacity_IsFraction(string attribute)
    {
        var shape = Assert.Single(Import($"<rect width=\"10\" height=\"10\" fill=\"#ff0000\" {attribute}/>"));

        Assert.Equal(Color.FromArgb(128, 255, 0, 0), shape.ForeColor);
    }

    [Fact]
    public void PercentStopOpacity_IsFraction()
    {
        var shape = Assert.Single(Import(
            "<defs><linearGradient id=\"g\"><stop offset=\"0\" stop-color=\"#ff0000\" stop-opacity=\"25%\"/></linearGradient></defs>" +
            "<rect width=\"10\" height=\"10\" fill=\"url(#g)\"/>"));

        Assert.Equal(Color.FromArgb(64, 255, 0, 0), shape.ForeColor);
    }

    [Fact]
    public void PercentStrokeOpacity_IsFraction()
    {
        var shapes = Import("<rect x=\"10\" y=\"10\" width=\"100\" height=\"100\" fill=\"none\" stroke=\"#0000ff\" stroke-width=\"4\" stroke-opacity=\"50%\"/>");

        Assert.NotEmpty(shapes);
        Assert.All(shapes, s => Assert.Equal(Color.FromArgb(128, 0, 0, 255), s.ForeColor));
    }

    [Fact]
    public void DefsHiddenAndUse_OnlyVisibleContentIsImported()
    {
        var shapes = Import(
            "<defs><rect id=\"r\" width=\"10\" height=\"10\" fill=\"red\"/></defs>" +
            "<rect width=\"10\" height=\"10\" style=\"display:none\"/>" +
            "<use href=\"#r\" x=\"100\" y=\"200\"/>");

        var shape = Assert.Single(shapes);
        Assert.Equal("m 100 200 l 110 200 110 210 100 210", shape.ToAssa());
    }

    [Fact]
    public void IllustratorDoctypeWithEntities_IsRead()
    {
        const string svg = "<?xml version=\"1.0\"?>\n<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\" [\n<!ENTITY ns_svg \"http://www.w3.org/2000/svg\">\n]>\n" +
                           "<svg xmlns=\"&ns_svg;\" width=\"1920px\" height=\"1080px\"><polygon points=\"0,0 100,0 100,100\" fill=\"#123456\"/></svg>";

        var shape = Assert.Single(SvgImporter.Import(svg, 1920, 1080, 0).Shapes);

        Assert.Equal(Color.FromRgb(0x12, 0x34, 0x56), shape.ForeColor);
    }

    [Fact]
    public void ViewModel_ImportsAboveExistingLayers_AndSelectsImport()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var line = new SubtitleLineViewModel(new Paragraph("{\\p1}m 0 0 l 10 0 10 10{\\p0}", 0, 2000) { Extra = "Default", Layer = 2 }, new AdvancedSubStationAlpha());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, [line], 1920, 1080);

        Assert.True(vm.ImportSvgText("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 1920 1080\"><circle cx=\"50\" cy=\"50\" r=\"40\"/></svg>"));

        Assert.Equal(2, vm.Shapes.Count);
        Assert.Equal(3, vm.Shapes[1].Layer);
        Assert.Equal([vm.Shapes[1]], vm.SelectedShapes);
        Assert.False(vm.ImportSvgText("<svg xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"10\" height=\"10\" fill=\"none\"/></svg>"));
    }
}
