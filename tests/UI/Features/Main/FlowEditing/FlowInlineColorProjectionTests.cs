using System.Linq;
using Nikse.SubtitleEdit.Features.Main.FlowEditing;

namespace UITests.Features.Main.FlowEditing;

public class FlowInlineColorProjectionTests
{
    [Fact]
    public void PlainTextRoundTrips()
    {
        var projection = FlowInlineColorProjection.Parse("Hello world");

        Assert.Equal("Hello world", projection.VisibleText);
        Assert.Empty(projection.ColorRuns);
        Assert.Equal("Hello world", projection.Serialize());
    }

    [Fact]
    public void WholeEventColorRoundTripsSemantically()
    {
        var projection = FlowInlineColorProjection.Parse("<font color=\"Yellow\">Hello world</font>");

        Assert.Equal("Hello world", projection.VisibleText);
        AssertRun(projection, 0, 11, "Yellow");
        AssertSemanticEqual(projection, FlowInlineColorProjection.Parse(projection.Serialize()));
    }

    [Fact]
    public void TwoColorsProduceVisibleOffsets()
    {
        var projection = FlowInlineColorProjection.Parse(
            "<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>");

        Assert.Equal("Hello world", projection.VisibleText);
        Assert.Collection(
            projection.ColorRuns,
            run => AssertRun(run, 0, 5, "Yellow"),
            run => AssertRun(run, 6, 5, "Cyan"));
        AssertSemanticEqual(projection, FlowInlineColorProjection.Parse(projection.Serialize()));
    }

    [Fact]
    public void ThreeColoredRegionsAndUncoloredTextArePreserved()
    {
        var projection = FlowInlineColorProjection.Parse(
            "<font color=\"Red\">One</font> plain <font color=\"Green\">two</font> / <font color=\"Blue\">three</font>");

        Assert.Equal("One plain two / three", projection.VisibleText);
        Assert.Equal(new[] { "Red", "Green", "Blue" }, projection.ColorRuns.Select(p => p.Color));
        Assert.Null(ColorAt(projection, 4));
        Assert.Null(ColorAt(projection, 14));
        AssertSemanticEqual(projection, FlowInlineColorProjection.Parse(projection.Serialize()));
    }

    [Fact]
    public void RecolorsOneWordInsideExistingRun()
    {
        var projection = FlowInlineColorProjection
            .Parse("<font color=\"Yellow\">Hello beautiful world</font>")
            .ApplyColor(6, 9, "Cyan");

        Assert.Equal("Yellow", ColorAt(projection, 0));
        Assert.Equal("Cyan", ColorAt(projection, 6));
        Assert.Equal("Yellow", ColorAt(projection, 16));
        AssertSemanticEqual(projection, FlowInlineColorProjection.Parse(projection.Serialize()));
    }

    [Fact]
    public void RecolorsAcrossTwoRunsAndUncoloredBoundary()
    {
        var projection = FlowInlineColorProjection
            .Parse("<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>")
            .ApplyColor(3, 5, "Red");

        Assert.Equal("Yellow", ColorAt(projection, 2));
        Assert.Equal("Red", ColorAt(projection, 3));
        Assert.Equal("Red", ColorAt(projection, 7));
        Assert.Equal("Cyan", ColorAt(projection, 8));
    }

    [Fact]
    public void RemovesColorFromOnlySelectedRange()
    {
        var projection = FlowInlineColorProjection
            .Parse("<font color=\"Yellow\">Hello </font><font color=\"Cyan\">beautiful world</font>")
            .RemoveColor(6, 9);

        Assert.Equal("Yellow", ColorAt(projection, 0));
        Assert.Null(ColorAt(projection, 6));
        Assert.Null(ColorAt(projection, 14));
        Assert.Equal("Cyan", ColorAt(projection, 15));
    }

    [Fact]
    public void InsertsBeforeInsideAndAfterColoredRange()
    {
        var original = FlowInlineColorProjection.Parse("x<font color=\"Yellow\">abc</font>y");

        var before = original.Insert(0, "B");
        var inside = original.Insert(2, "I");
        var after = original.Insert(original.VisibleText.Length, "A");

        Assert.Null(ColorAt(before, 0));
        Assert.Equal("Yellow", ColorAt(inside, 2));
        Assert.Null(ColorAt(after, after.VisibleText.Length - 1));
        Assert.Equal("Bxabcy", before.VisibleText);
        Assert.Equal("xaIbcy", inside.VisibleText);
        Assert.Equal("xabcyA", after.VisibleText);
    }

    [Fact]
    public void DeletesBeforeInsideAndAcrossColorBoundaries()
    {
        var original = FlowInlineColorProjection.Parse(
            "xx<font color=\"Yellow\">abc</font>-<font color=\"Cyan\">def</font>yy");

        var before = original.Delete(0, 1);
        var inside = original.Delete(3, 1);
        var across = original.Delete(4, 4);

        Assert.Equal("xabc-defyy", before.VisibleText);
        Assert.Equal("xxac-defyy", inside.VisibleText);
        Assert.Equal("xxabfyy", across.VisibleText);
        Assert.Equal("Yellow", ColorAt(across, 2));
        Assert.Equal("Cyan", ColorAt(across, 4));
        AssertSemanticEqual(across, FlowInlineColorProjection.Parse(across.Serialize()));
    }

    [Fact]
    public void ExtractsAndRecombinesMultipleColors()
    {
        var original = FlowInlineColorProjection.Parse(
            "Start <font color=\"Yellow\">one</font> + <font color=\"Cyan\">two</font> end");
        var fragment = original.Extract(6, 9);
        var withoutFragment = original.Delete(6, 9);
        var recombined = withoutFragment.Insert(6, fragment);

        Assert.Equal(original.VisibleText, recombined.VisibleText);
        AssertSemanticEqual(original, recombined);
    }

    [Fact]
    public void AlignmentAndFlowVisibleFormattingArePreserved()
    {
        const string canonical = "{\\an8}<font color=\"Yellow\"><i>Hello</i></font> world";
        var projection = FlowInlineColorProjection.Parse(canonical);

        Assert.Equal("<i>Hello</i> world", projection.VisibleText);
        Assert.StartsWith("{\\an8}", projection.Serialize());
        Assert.Contains("<i>Hello</i>", projection.Serialize());
        AssertSemanticEqual(projection, FlowInlineColorProjection.Parse(projection.Serialize()));
    }

    [Fact]
    public void NonColorFontTagRemainsWellNestedAroundColor()
    {
        const string canonical = "<font face=\"Arial\"><font color=\"Yellow\">Hello</font></font>";
        var projection = FlowInlineColorProjection.Parse(canonical);

        Assert.Equal("Hello", projection.VisibleText);
        Assert.Equal(canonical, projection.Serialize());
        AssertSemanticEqual(projection, FlowInlineColorProjection.Parse(projection.Serialize()));
    }

    [Fact]
    public void AdjacentRunsOfTheSameColorAreMerged()
    {
        var projection = FlowInlineColorProjection.Parse(
            "<font color=\"Yellow\">Hello</font><font color=\"Yellow\"> world</font>");

        AssertRun(projection, 0, 11, "Yellow");
    }

    [Fact]
    public void VisibleOffsetsUseUtf16CodeUnits()
    {
        var projection = FlowInlineColorProjection
            .Parse("<font color=\"Yellow\">A😀B</font>")
            .ApplyColor(1, 2, "Cyan");

        Assert.Equal(4, projection.VisibleText.Length);
        Assert.Equal("Yellow", ColorAt(projection, 0));
        Assert.Equal("Cyan", ColorAt(projection, 1));
        Assert.Equal("Cyan", ColorAt(projection, 2));
        Assert.Equal("Yellow", ColorAt(projection, 3));
        AssertSemanticEqual(projection, FlowInlineColorProjection.Parse(projection.Serialize()));
    }

    private static string? ColorAt(FlowInlineColorProjection projection, int offset) => projection.ColorRuns
        .SingleOrDefault(p => p.Start <= offset && p.End > offset)
        ?.Color;

    private static void AssertRun(FlowInlineColorProjection projection, int start, int length, string color) =>
        AssertRun(Assert.Single(projection.ColorRuns), start, length, color);

    private static void AssertRun(FlowInlineColorRun run, int start, int length, string color)
    {
        Assert.Equal(start, run.Start);
        Assert.Equal(length, run.Length);
        Assert.Equal(color, run.Color);
    }

    private static void AssertSemanticEqual(FlowInlineColorProjection expected, FlowInlineColorProjection actual)
    {
        Assert.Equal(expected.VisibleText, actual.VisibleText);
        Assert.Equal(expected.ColorRuns, actual.ColorRuns);
    }
}
