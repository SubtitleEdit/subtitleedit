using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;
using System.Linq;

namespace UITests.Logic.Media;

public class SecondarySubtitleJustifierTests
{
    private static SsaStyle MakeStyle(string alignment = "8")
    {
        return new SsaStyle
        {
            Name = "Secondary",
            FontName = "Arial",
            FontSize = 20,
            Bold = false,
            Alignment = alignment,
            OutlineWidth = 2,
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
        };
    }

    [Fact]
    public void Apply_Auto_ReturnsUnchangedCopies()
    {
        var paragraphs = new[] { new Paragraph("line one\nline two", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "auto", 1920, 1080);

        Assert.Single(result);
        Assert.NotSame(paragraphs[0], result[0]);
        Assert.Equal("line one\nline two", result[0].Text);
    }

    [Fact]
    public void Apply_EmptyJustifyCode_ReturnsUnchangedCopies()
    {
        var paragraphs = new[] { new Paragraph("line one\nline two", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), string.Empty, 1920, 1080);

        Assert.Single(result);
        Assert.Equal("line one\nline two", result[0].Text);
    }

    [Fact]
    public void Apply_SingleLineParagraph_ReturnsUnchangedCopyEvenWhenJustified()
    {
        var paragraphs = new[] { new Paragraph("only one line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Single(result);
        Assert.Equal("only one line", result[0].Text);
    }

    [Fact]
    public void Apply_LeftJustify_SplitsIntoOnePositionedEventPerLine()
    {
        var paragraphs = new[] { new Paragraph("short\na much longer line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Contains("\\an7\\pos(", p.Text));
        Assert.EndsWith("short", result[0].Text);
        Assert.EndsWith("a much longer line", result[1].Text);
    }

    [Fact]
    public void Apply_LeftJustify_AllLinesShareTheSameLeftX()
    {
        var paragraphs = new[] { new Paragraph("short\na much longer line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        var xs = result.Select(ExtractPosX).ToList();
        Assert.Equal(xs[0], xs[1], 2);
    }

    [Fact]
    public void Apply_RightJustify_ShorterLineIsFurtherRightThanLongerLine()
    {
        var paragraphs = new[] { new Paragraph("short\na much longer line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "right", 1920, 1080);

        var shortX = ExtractPosX(result[0]);
        var longX = ExtractPosX(result[1]);
        Assert.True(shortX > longX);
    }

    [Fact]
    public void Apply_CenterJustify_ShorterLineIsIndentedRelativeToLongerLine()
    {
        var paragraphs = new[] { new Paragraph("short\na much longer line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "center", 1920, 1080);

        var shortX = ExtractPosX(result[0]);
        var longX = ExtractPosX(result[1]);
        Assert.True(shortX > longX);
    }

    [Fact]
    public void Apply_TopAlignment_FirstLineSitsAtMarginVertical()
    {
        var paragraphs = new[] { new Paragraph("first\nsecond", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("7"), "left", 1920, 1080);

        Assert.Equal(10, ExtractPosY(result[0]), 1);
    }

    [Fact]
    public void Apply_BottomAlignment_LastLineSitsAboveMarginVertical()
    {
        var paragraphs = new[] { new Paragraph("first\nsecond", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("2"), "left", 1920, 1080);

        var lastY = ExtractPosY(result[1]);
        Assert.True(lastY < 1080 - 10);
    }

    [Fact]
    public void Apply_StepsDownByOneLineHeightPerLine()
    {
        var paragraphs = new[] { new Paragraph("first\nsecond\nthird", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("7"), "left", 1920, 1080);

        var y0 = ExtractPosY(result[0]);
        var y1 = ExtractPosY(result[1]);
        var y2 = ExtractPosY(result[2]);
        var step1 = y1 - y0;
        var step2 = y2 - y1;
        Assert.True(System.Math.Abs(step1 - step2) < 0.01m, $"expected equal steps, got {step1} and {step2}");
    }

    [Fact]
    public void Apply_StepsDownByTheFontSize_LikeLibass()
    {
        // libass makes the line height equal the style's font size; Skia's ascent + descent is
        // larger, which spread justified lines further apart than unjustified ones.
        var paragraphs = new[] { new Paragraph("first\nsecond", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("7"), "left", 1920, 1080);

        Assert.Equal(20m, ExtractPosY(result[1]) - ExtractPosY(result[0]), 2);
    }

    [Fact]
    public void Apply_MultipleParagraphs_EachSplitIndependently()
    {
        var paragraphs = new[]
        {
            new Paragraph("a1\na2", 0, 1000),
            new Paragraph("just one line", 1000, 2000),
            new Paragraph("b1\nb2\nb3", 2000, 3000),
        };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void Apply_DoesNotMutateSourceParagraphs()
    {
        var source = new Paragraph("one\ntwo", 0, 1000);
        var paragraphs = new[] { source };

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Equal("one\ntwo", source.Text);
    }

    [Fact]
    public void Apply_CalledTwiceOnSameSourceParagraphs_ProducesIdenticalResults()
    {
        var paragraphs = new[] { new Paragraph("one\ntwo", 0, 1000) };

        var first = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);
        var second = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Equal(first[0].Text, second[0].Text);
        Assert.Equal(first[1].Text, second[1].Text);
    }

    [Fact]
    public void Apply_PreservesParagraphTimingAndExtra()
    {
        var paragraphs = new[] { new Paragraph("one\ntwo", 1234, 5678) { Extra = "Secondary" } };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.All(result, p =>
        {
            Assert.Equal(1234, p.StartTime.TotalMilliseconds);
            Assert.Equal(5678, p.EndTime.TotalMilliseconds);
            Assert.Equal("Secondary", p.Extra);
        });
    }

    [Fact]
    public void Apply_StripsExistingSsaTagsBeforeMeasuring_ButKeepsThemInOutput()
    {
        var paragraphs = new[] { new Paragraph("{\\i1}short{\\i0}\na much longer line here", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Contains("{\\i1}short{\\i0}", result[0].Text);
    }

    [Fact]
    public void Apply_ItalicSpanningLines_IsRepeatedOnTheLaterLines()
    {
        var paragraphs = new[] { new Paragraph("{\\i1}first\nsecond{\\i0}", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Matches(@"^\{\\an7\\pos\([^)]*\)\}\{\\i1\}first$", result[0].Text);
        Assert.Matches(@"^\{\\an7\\pos\([^)]*\)\\i1\}second\{\\i0\}$", result[1].Text);
    }

    [Fact]
    public void Apply_HtmlItalicSpanningLines_IsRepeatedOnTheLaterLines()
    {
        var paragraphs = new[] { new Paragraph("<i>first\nsecond</i>", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.EndsWith("{\\i1}first", result[0].Text);
        Assert.Matches(@"^\{\\an7\\pos\([^)]*\)\\i1\}second\{\\i0\}$", result[1].Text);
    }

    [Fact]
    public void Apply_ItalicClosedOnItsOwnLine_IsNotCarriedButStaysInOrder()
    {
        // "\i1\i0" on the next line still ends up upright: the later tag wins, as in one event.
        var paragraphs = new[] { new Paragraph("{\\i1}first{\\i0}\nsecond\nthird", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Matches(@"^\{\\an7\\pos\([^)]*\)\\i1\\i0\}second$", result[1].Text);
        Assert.Matches(@"^\{\\an7\\pos\([^)]*\)\\i1\\i0\}third$", result[2].Text);
    }

    [Fact]
    public void Apply_EventWideAndAnimationTags_AreNotCarried()
    {
        var paragraphs = new[] { new Paragraph("{\\an8\\pos(1,2)\\fad(100,100)\\t(0,500,\\fs30)\\k20\\c&H0000FF&\\alpha&H80&}first\nsecond", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "left", 1920, 1080);

        Assert.Matches(@"^\{\\an7\\pos\([^)]*\)\\c&H0000FF&\\alpha&H80&\}second$", result[1].Text);
    }

    [Fact]
    public void Apply_LargerPlayResX_ScalesRightJustifiedPositionAccordingly()
    {
        var paragraphs = new[] { new Paragraph("only one line here", 0, 1000) };

        var narrow = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(), "auto", 1920, 1080);
        // "auto" leaves the line unsplit, so use a two-line paragraph for a real right-justify comparison.
        var twoLine = new[] { new Paragraph("only one line here\nx", 0, 1000) };
        var small = SecondarySubtitleJustifier.Apply(twoLine, MakeStyle(), "right", 640, 360);
        var large = SecondarySubtitleJustifier.Apply(twoLine, MakeStyle(), "right", 1920, 1080);

        Assert.True(ExtractPosX(large[0]) > ExtractPosX(small[0]));
        Assert.Single(narrow);
    }

    private static decimal ExtractPosX(Paragraph p) => ExtractPos(p).x;

    private static decimal ExtractPosY(Paragraph p) => ExtractPos(p).y;

    private static (decimal x, decimal y) ExtractPos(Paragraph p)
    {
        var start = p.Text.IndexOf("\\pos(", System.StringComparison.Ordinal) + "\\pos(".Length;
        var end = p.Text.IndexOf(')', start);
        var inner = p.Text.Substring(start, end - start);
        var parts = inner.Split(',');
        return (decimal.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                decimal.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
    }
}
