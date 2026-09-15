using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
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
            Alignment = alignment,
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
        };
    }

    [Fact]
    public void Apply_JustifyAuto_DoesNotChangeText()
    {
        var paragraphs = new List<Paragraph> { new("Short line\nA much longer second line", 0, 1000) };
        var originalText = paragraphs[0].Text;

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "auto", 1920, 1080);

        Assert.Equal(originalText, paragraphs[0].Text);
    }

    [Fact]
    public void Apply_SingleLineParagraph_DoesNotChangeText()
    {
        var paragraphs = new List<Paragraph> { new("Just one line", 0, 1000) };
        var originalText = paragraphs[0].Text;

        // "left" clearly differs from the "top-center" alignment below - a multi-line paragraph
        // would be overridden, but justify has nothing to justify a single line against.
        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.Equal(originalText, paragraphs[0].Text);
    }

    [Fact]
    public void Apply_JustifyAlreadyMatchesAlignment_DoesNotChangeText()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA longer line", 0, 1000) };
        var originalText = paragraphs[0].Text;

        // "5" = middle-center: its own horizontal component is already "center".
        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("5"), "center", 1920, 1080);

        Assert.Equal(originalText, paragraphs[0].Text);
    }

    [Fact]
    public void Apply_EmptyParagraphList_DoesNotThrow()
    {
        var paragraphs = new List<Paragraph>();

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.Empty(paragraphs);
    }

    [Theory]
    [InlineData("7", "left")] // already left - "left" justify has nothing to override
    [InlineData("9", "right")] // already right
    public void Apply_JustifyMatchesAlignmentHorizontal_DoesNotChangeText(string alignment, string justify)
    {
        var paragraphs = new List<Paragraph> { new("Short\nA longer second line", 0, 1000) };
        var originalText = paragraphs[0].Text;

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(alignment), justify, 1920, 1080);

        Assert.Equal(originalText, paragraphs[0].Text);
    }

    [Fact]
    public void Apply_TopCenterAlignmentWithLeftJustify_OverridesToTopLeftAnchor()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.StartsWith("{\\an7\\pos(", paragraphs[0].Text);
        Assert.EndsWith("Short\nA much longer second line", paragraphs[0].Text);
    }

    [Fact]
    public void Apply_BottomRightAlignmentWithCenterJustify_OverridesToBottomCenterAnchor()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("3"), "center", 1920, 1080);

        Assert.StartsWith("{\\an2\\pos(", paragraphs[0].Text);
    }

    [Theory]
    [InlineData("7", 10)] // top: y = MarginVertical
    [InlineData("1", 1070)] // bottom: y = playResY - MarginVertical (1080 - 10)
    public void Apply_KeepsOriginalAlignmentsVerticalAnchor(string alignment, decimal expectedY)
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle(alignment), "right", 1920, 1080);

        var y = ExtractPos(paragraphs[0].Text).y;
        Assert.Equal(expectedY, y);
    }

    [Fact]
    public void Apply_MiddleAlignment_AnchorsVerticallyAtPlayResYOverTwo()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("4"), "right", 1920, 1080);

        var y = ExtractPos(paragraphs[0].Text).y;
        Assert.Equal(540m, y); // 1080 / 2
    }

    [Fact]
    public void Apply_RightJustifyAnchorsFartherRightThanCenterJustify_ForTheSameOriginalAlignment()
    {
        // Both differ from a left-anchored alignment, so both get overridden - comparing their
        // resulting x keeps the assertion independent of exactly which font resolves on the
        // machine running the test, since both measurements come from the same font/size.
        var centerParagraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };
        var rightParagraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        SecondarySubtitleJustifier.Apply(centerParagraphs, MakeStyle("7"), "center", 1920, 1080);
        SecondarySubtitleJustifier.Apply(rightParagraphs, MakeStyle("7"), "right", 1920, 1080);

        var centerX = ExtractPos(centerParagraphs[0].Text).x;
        var rightX = ExtractPos(rightParagraphs[0].Text).x;
        Assert.True(rightX > centerX, $"expected right-justify x ({rightX}) to land right of center-justify x ({centerX})");
    }

    [Fact]
    public void Apply_LeftJustifyFromCenterAlignment_AnchorsLeftOfPlayResCenter()
    {
        var paragraphs = new List<Paragraph> { new("Short\nA much longer second line", 0, 1000) };

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        var x = ExtractPos(paragraphs[0].Text).x;
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

        SecondarySubtitleJustifier.Apply(paragraphs, MakeStyle("8"), "left", 1920, 1080);

        Assert.All(paragraphs, p => Assert.StartsWith("{\\an7\\pos(", p.Text));
    }

    private static (decimal x, decimal y) ExtractPos(string text)
    {
        var match = Regex.Match(text, @"\\pos\(([-\d.]+),([-\d.]+)\)");
        Assert.True(match.Success, $"no \\pos override found in '{text}'");
        return (decimal.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                decimal.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture));
    }
}
