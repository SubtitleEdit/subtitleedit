using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UITests.Logic.Media;

public class SecondarySubtitleJustifierTests
{
    private static SsaStyle MakeStyle(string alignment)
    {
        return new SsaStyle
        {
            FontName = "Arial",
            FontSize = 40,
            Bold = false,
            OutlineWidth = 2,
            Alignment = alignment,
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
        };
    }

    [Fact]
    public void Apply_JustifyAuto_PassesThroughAsACopyWithUnchangedText()
    {
        var source = new Paragraph("Short\nA much longer second line", 0, 1000);
        var paragraphs = new List<Paragraph> { source };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "auto", 1920, 1080);

        Assert.Single(result);
        Assert.NotSame(source, result[0]);
        Assert.Equal(source.Text, result[0].Text);
    }

    [Fact]
    public void Apply_SingleLineParagraph_PassesThroughAsACopyEvenWhenJustifyIsSet()
    {
        var source = new Paragraph("Just one line", 0, 1000);
        var paragraphs = new List<Paragraph> { source };

        // Justify only ever affects how several lines relate to each other.
        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.Single(result);
        Assert.NotSame(source, result[0]);
        Assert.Equal(source.Text, result[0].Text);
    }

    [Fact]
    public void Apply_EmptyParagraphList_ReturnsEmpty()
    {
        var result = SecondarySubtitleJustifier.Apply(new List<Paragraph>(), MakeStyle("8"), "left", 1920, 1080);

        Assert.Empty(result);
    }

    [Fact]
    public void Apply_MultiLineParagraph_SplitsIntoOneEventPerLine()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.Equal(2, result.Count);
        Assert.StartsWith("{\\an7\\pos(", result[0].Text);
        Assert.EndsWith("Short", result[0].Text);
        Assert.StartsWith("{\\an7\\pos(", result[1].Text);
        Assert.EndsWith("A much longer second line", result[1].Text);
    }

    [Fact]
    public void Apply_SplitLines_KeepTheSourceParagraphsTimingAndExtra()
    {
        var source = new Paragraph("Short\nA much longer second line", 1500, 4200) { Extra = "SomeStyle", Layer = 3 };
        var paragraphs = new List<Paragraph> { source };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.All(result, p =>
        {
            Assert.Equal(1500, p.StartTime.TotalMilliseconds);
            Assert.Equal(4200, p.EndTime.TotalMilliseconds);
            Assert.Equal("SomeStyle", p.Extra);
            Assert.Equal(3, p.Layer);
            Assert.NotEqual(source.Id, p.Id); // genuinely separate dialogue events now
        });
    }

    [Fact]
    public void Apply_SplitLines_StepDownByOneLineHeightPerLine()
    {
        var paragraphs = new List<Paragraph> { new("Line one\nLine two\nLine three", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("7"), "left", 1920, 1080);

        Assert.Equal(3, result.Count);
        var y0 = ExtractPos(result[0].Text).y;
        var y1 = ExtractPos(result[1].Text).y;
        var y2 = ExtractPos(result[2].Text).y;
        var step1 = y1 - y0;
        var step2 = y2 - y1;
        Assert.True(step1 > 0, "expected each line to sit below the previous one");
        Assert.True(System.Math.Abs(step1 - step2) < 0.01m, $"expected a uniform line height, got steps {step1} and {step2}");
    }

    [Fact]
    public void Apply_TopAlignment_FirstLineSitsAtMarginVertical()
    {
        var paragraphs = new List<Paragraph> { new("Line one\nLine two", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("7"), "left", 1920, 1080);

        Assert.Equal(10m, ExtractPos(result[0].Text).y);
    }

    [Fact]
    public void Apply_BottomAlignment_LastLineSitsAboveMarginVertical()
    {
        var paragraphs = new List<Paragraph> { new("Line one\nLine two", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("1"), "left", 1920, 1080);

        // The *bottom* edge of the last line's own box sits MarginVertical above the video's
        // bottom edge; SecondarySubtitleJustifier reports the *top* of each line via \pos, so the
        // last line's y must be strictly less than (playResY - MarginVertical).
        var lastLineY = ExtractPos(result[^1].Text).y;
        Assert.True(lastLineY < 1080 - 10, $"expected the last line's top to sit above the bottom margin, got y={lastLineY}");
    }

    [Fact]
    public void Apply_MiddleAlignment_BlockIsCenteredAroundPlayResYOverTwo()
    {
        var paragraphs = new List<Paragraph> { new("Line one\nLine two", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("4"), "left", 1920, 1080);

        var firstY = ExtractPos(result[0].Text).y;
        var lastY = ExtractPos(result[^1].Text).y;
        var blockMiddle = (firstY + lastY) / 2m;
        Assert.True(System.Math.Abs(blockMiddle - 540m) < 50m, $"expected the block roughly centered on 540, got middle={blockMiddle}");
    }

    [Fact]
    public void Apply_RightJustifyAnchorsFartherRightThanLeftJustify_ForTheSameParagraph()
    {
        var leftParagraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };
        var rightParagraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        var left = SecondarySubtitleJustifier.Apply(leftParagraphs, MakeStyle("8"), "left", 1920, 1080);
        var right = SecondarySubtitleJustifier.Apply(rightParagraphs, MakeStyle("8"), "right", 1920, 1080);

        // Compare the SHORT line ("Short"), which differs most visibly between justify modes.
        var leftX = ExtractPos(left[0].Text).x;
        var rightX = ExtractPos(right[0].Text).x;
        Assert.True(rightX > leftX, $"expected right-justify x ({rightX}) to land right of left-justify x ({leftX})");
    }

    [Fact]
    public void Apply_LeftJustify_EveryLineSharesTheSameLeftEdge()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        var x0 = ExtractPos(result[0].Text).x;
        var x1 = ExtractPos(result[1].Text).x;
        Assert.Equal(x0, x1);
    }

    [Fact]
    public void Apply_CenterAlignmentWithLeftJustify_AnchorsLeftOfScreenCenter()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        var x = ExtractPos(result[0].Text).x;
        Assert.True(x < 960m, $"expected a left-justified block anchored left of screen center (960), got {x}"); // 1920 / 2
    }

    [Fact]
    public void Apply_OverridesEveryParagraphInTheList()
    {
        var paragraphs = new List<Paragraph>
        {
            new("Short\nA much longer second line", 0, 1000),
            new("Another\nSecond paragraph, second line", 1000, 2000),
        };

        var result = SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.Equal(4, result.Count); // 2 paragraphs x 2 lines each
        Assert.All(result, p => Assert.StartsWith("{\\an7\\pos(", p.Text));
    }

    private static (decimal x, decimal y) ExtractPos(string text)
    {
        var match = Regex.Match(text, @"\\pos\(([-\d.]+),([-\d.]+)\)");
        Assert.True(match.Success, $"no \\pos override found in '{text}'");
        return (decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }
}
