using System.Linq;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.FlowEditing;
using Nikse.SubtitleEdit.Logic;

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

    [Fact]
    public void OrdinaryInsertionInsideColoredWordKeepsItsColor()
    {
        var projection = FlowInlineColorProjection
            .Parse("<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>")
            .ApplyVisibleEdit("HelXlo world");

        Assert.Equal("HelXlo world", projection.VisibleText);
        Assert.Equal("Yellow", ColorAt(projection, 3));
        Assert.Equal("Cyan", ColorAt(projection, 7));
    }

    [Fact]
    public void FlowEditingItemSynchronizesOrdinaryEditWithoutFlatteningColors()
    {
        var source = new SubtitleLineViewModel
        {
            Text = "{\\an8}<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>",
        };
        using var item = new FlowEditingItem(source);

        item.Text = "HelXlo world";

        var projection = FlowInlineColorProjection.Parse(source.Text);
        Assert.Equal("HelXlo world", projection.VisibleText);
        Assert.Equal("Yellow", ColorAt(projection, 3));
        Assert.Equal("Cyan", ColorAt(projection, 7));
        Assert.StartsWith("{\\an8}", projection.Serialize());
    }

    [Fact]
    public void OrdinaryDeletionInsideColoredRunKeepsRemainingRun()
    {
        var projection = FlowInlineColorProjection
            .Parse("<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>")
            .ApplyVisibleEdit("Hlo world");

        Assert.Equal("Hlo world", projection.VisibleText);
        Assert.Equal("Yellow", ColorAt(projection, 0));
        Assert.Equal("Yellow", ColorAt(projection, 2));
        Assert.Equal("Cyan", ColorAt(projection, 4));
    }

    [Fact]
    public void SplitFragmentsCarryTheirOwnColors()
    {
        var projection = FlowInlineColorProjection.Parse(
            "{\\an8}<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>");

        var before = projection.Extract(0, 5).TransferColorsTo("{\\an8}Hello");
        var after = projection.Extract(6, 5).TransferColorsTo("{\\an8}world");

        Assert.Equal("Yellow", ColorAt(before, 0));
        Assert.Equal("Cyan", ColorAt(after, 0));
        Assert.StartsWith("{\\an8}", before.Serialize());
        Assert.StartsWith("{\\an8}", after.Serialize());
    }

    [Fact]
    public void ReturnSplitKeepsColorsAroundExistingSplitManagerResult()
    {
        var source = new SubtitleLineViewModel
        {
            Text = "{\\an8}<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>",
            StartTime = System.TimeSpan.FromSeconds(1),
            EndTime = System.TimeSpan.FromSeconds(4),
        };
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { source };
        var projection = FlowInlineColorProjection.Parse(source.Text);
        var before = projection.Extract(0, 5);
        var after = projection.Extract(6, 5);

        new SplitManager().Split(subtitles, source, projection.GetCanonicalOffset(6), string.Empty);
        source.Text = before.TransferColorsTo(source.Text).Serialize();
        subtitles[1].Text = after.TransferColorsTo(subtitles[1].Text).Serialize();

        var firstResult = FlowInlineColorProjection.Parse(source.Text);
        var secondResult = FlowInlineColorProjection.Parse(subtitles[1].Text);
        Assert.Equal("Hello", firstResult.VisibleText);
        Assert.Equal("world", secondResult.VisibleText);
        Assert.Equal("Yellow", ColorAt(firstResult, 0));
        Assert.Equal("Cyan", ColorAt(secondResult, 0));
    }

    [Fact]
    public void MergeKeepsDifferentWordColorsAndUncoloredSeparator()
    {
        var previous = FlowInlineColorProjection.Parse("{\\an8}<font color=\"Yellow\">Hello</font>");
        var current = FlowInlineColorProjection.Parse("<font color=\"Cyan\">world</font>");
        var merged = previous
            .Insert(previous.VisibleText.Length, " ", inheritColor: false)
            .Insert(previous.VisibleText.Length + 1, current)
            .TransferColorsTo("{\\an8}Hello world");

        Assert.Equal("Hello world", merged.VisibleText);
        Assert.Equal("Yellow", ColorAt(merged, 0));
        Assert.Null(ColorAt(merged, 5));
        Assert.Equal("Cyan", ColorAt(merged, 6));
        Assert.StartsWith("{\\an8}", merged.Serialize());
    }

    [Fact]
    public void WordMovementKeepsTwoColorsAndUncoloredRegion()
    {
        var source = FlowInlineColorProjection.Parse(
            "<font color=\"Yellow\">one</font> <font color=\"Cyan\">two</font> three");
        var moved = source.Extract(0, 7).ReflowVisibleText("one\ntwo");
        var remaining = source.Extract(8, 5);

        Assert.Equal("one\ntwo", moved.VisibleText);
        Assert.Equal("Yellow", ColorAt(moved, 0));
        Assert.Equal("Cyan", ColorAt(moved, 4));
        Assert.Equal("three", remaining.VisibleText);
        Assert.Empty(remaining.ColorRuns);
    }

    [Fact]
    public void ReflowChangesOnlyWhitespaceAndPreservesVisibleCharactersAndColors()
    {
        var projection = FlowInlineColorProjection
            .Parse("<font color=\"Yellow\">Hello</font> <font color=\"Cyan\">world</font>")
            .ReflowVisibleText("Hello\nworld");

        Assert.Equal("Helloworld", string.Concat(projection.VisibleText.Where(p => !char.IsWhiteSpace(p))));
        Assert.Equal("Yellow", ColorAt(projection, 0));
        Assert.Null(ColorAt(projection, 5));
        Assert.Equal("Cyan", ColorAt(projection, 6));
    }

    [Theory]
    [InlineData("controls, empty-document", 9, 10)]
    [InlineData("controls,\nempty-document", 9, 10)]
    [InlineData("empty-document", 0, 0)]
    public void BackspaceCaretFollowsLogicalVisibleBoundary(
        string visibleText,
        int charactersBeforeBoundary,
        int expectedCaretIndex)
    {
        var actual = FlowEditingView.GetCaretIndexForLogicalBoundary(
            visibleText,
            charactersBeforeBoundary);

        Assert.Equal(expectedCaretIndex, actual);
    }

    [Fact]
    public void BackspaceCaretDoesNotCountInlineColorTags()
    {
        var projection = FlowInlineColorProjection.Parse(
            "<font color=\"Yellow\">controls,</font> <font color=\"Cyan\">empty-document</font>");

        var caretIndex = FlowEditingView.GetCaretIndexForLogicalBoundary(
            projection.VisibleText,
            9);

        Assert.Equal(10, caretIndex);
        Assert.Equal('e', projection.VisibleText[caretIndex]);
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
