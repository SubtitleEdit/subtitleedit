using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;

namespace LibSETests.Core;

public class DialogTypeTest
{
    [Fact]
    public void FixSpacesDashBothLinesWithSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixSpaces($"-How are you?{Environment.NewLine}-I'm fine.");
        Assert.Equal($"- How are you?{Environment.NewLine}- I'm fine.", result);
    }

    [Fact]
    public void FixSpacesDashBothLinesWithSpace2()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixSpaces($"- How are you?{Environment.NewLine}- I'm fine.");
        Assert.Equal($"- How are you?{Environment.NewLine}- I'm fine.", result);
    }

    [Fact]
    public void FixSpacesDashBothLinesWithSpace3()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixSpaces($"{{\\an8}}-How are you?{Environment.NewLine}{{\\an8}}-I'm fine.");
        Assert.Equal($"{{\\an8}}- How are you?{Environment.NewLine}{{\\an8}}- I'm fine.", result);
    }

    [Fact]
    public void FixSpacesDashBothLinesWithSpace4()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixSpaces($"<i>-How are you?</i>{Environment.NewLine}<i>-I'm fine.</i>");
        Assert.Equal($"<i>- How are you?</i>{Environment.NewLine}<i>- I'm fine.</i>", result);
    }



    [Fact]
    public void FixSpacesDashBothLinesWithoutSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
        var result = splitMerge.FixSpaces($"- How are you?{Environment.NewLine}- I'm fine.'");
        Assert.Equal($"-How are you?{Environment.NewLine}-I'm fine.'", result);
    }

    [Fact]
    public void FixSpacesDashBothLinesWithoutSpace2()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
        var result = splitMerge.FixSpaces($"-How are you?{Environment.NewLine}-I'm fine.'");
        Assert.Equal($"-How are you?{Environment.NewLine}-I'm fine.'", result);
    }



    [Fact]
    public void FixSpacesDashSecondLineWithSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithSpace };
        var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}-I'm fine.'");
        Assert.Equal($"How are you?{Environment.NewLine}- I'm fine.'", result);
    }

    [Fact]
    public void FixSpacesDashSecondLineWithSpace2()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithSpace };
        var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}- I'm fine.'");
        Assert.Equal($"How are you?{Environment.NewLine}- I'm fine.'", result);
    }


    [Fact]
    public void FixSpacesDashSecondLineWithoutSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithoutSpace };
        var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}- I'm fine.'");
        Assert.Equal($"How are you?{Environment.NewLine}-I'm fine.'", result);
    }

    [Fact]
    public void FixSpacesDashSecondLineWithoutSpace2()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithoutSpace };
        var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}-I'm fine.'");
        Assert.Equal($"How are you?{Environment.NewLine}-I'm fine.'", result);
    }



    [Fact]
    public void FixDashesDashBothLinesWithSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixDashes($"How are you?{Environment.NewLine}- I'm fine.");
        Assert.Equal($"- How are you?{Environment.NewLine}- I'm fine.", result);
    }


    [Fact]
    public void FixDashesDashBothLinesWithoutSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
        var result = splitMerge.FixDashes($"How are you?{Environment.NewLine}-I'm fine.");
        Assert.Equal($"-How are you?{Environment.NewLine}-I'm fine.", result);
    }

    [Fact]
    public void FixDashesDashSecondLineWithSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithSpace };
        var result = splitMerge.FixDashes($"- How are you?{Environment.NewLine}- I'm fine.");
        Assert.Equal($"How are you?{Environment.NewLine}- I'm fine.", result);
    }

    [Fact]
    public void FixDashesDashSecondLineWithoutSpace1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithoutSpace };
        var result = splitMerge.FixDashes($"-How are you?{Environment.NewLine}-I'm fine.");
        Assert.Equal($"How are you?{Environment.NewLine}-I'm fine.", result);
    }



    [Fact]
    public void FixHyphensAddDash1()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixDashes($"Hi Joe!{Environment.NewLine}- Hi Pete!");
        Assert.Equal($"- Hi Joe!{Environment.NewLine}- Hi Pete!", result);
    }

    [Fact]
    public void FixHyphensWithQuotes()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixDashes($"\"Into The Woods.\"{Environment.NewLine}- \"Sweeney Todd.\"");
        Assert.Equal($"- \"Into The Woods.\"{Environment.NewLine}- \"Sweeney Todd.\"", result);
    }

    [Fact]
    public void FixHyphensFirstLineParentheses()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixSpaces($"-(howling){Environment.NewLine}-I see what's on the other side.");
        Assert.Equal($"- (howling){Environment.NewLine}- I see what's on the other side.", result);
    }

    [Fact]
    public void FixHyphensThreeLinesTwoOne()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixDashesAndSpaces($"-I'm a'{Environment.NewLine}one-in-a-generation artist.{Environment.NewLine}-Oh.");
        Assert.Equal($"- I'm a'{Environment.NewLine}one-in-a-generation artist.{Environment.NewLine}- Oh.", result);
    }

    [Fact]
    public void FixHyphensThreeLinesTwoOneNoSpace()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
        var result = splitMerge.FixDashesAndSpaces($"- I'm a'{Environment.NewLine}one-in-a-generation artist.{Environment.NewLine}- Oh.");
        Assert.Equal($"-I'm a'{Environment.NewLine}one-in-a-generation artist.{Environment.NewLine}-Oh.", result);
    }

    [Fact]
    public void FixHyphensThreeLinesOneTwo()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var result = splitMerge.FixDashesAndSpaces($"-What are you?{Environment.NewLine}-I'm a{Environment.NewLine}one-in-a-generation artist.");
        Assert.Equal($"- What are you?{Environment.NewLine}- I'm a{Environment.NewLine}one-in-a-generation artist.", result);
    }

    [Fact]
    public void FixHyphensThreeLinesOneTwoNoSpace()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
        var result = splitMerge.FixDashesAndSpaces($"- What are you?{Environment.NewLine}- I'm a{Environment.NewLine}one-in-a-generation artist.");
        Assert.Equal($"-What are you?{Environment.NewLine}-I'm a{Environment.NewLine}one-in-a-generation artist.", result);
    }

    [Fact]
    public void FixHyphensThreeLinesOneTwoDashSecondLineWithSpace()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithSpace };
        var result = splitMerge.FixDashesAndSpaces($"-What are you?{Environment.NewLine}-I'm a{Environment.NewLine}one-in-a-generation artist.");
        Assert.Equal($"What are you?{Environment.NewLine}- I'm a{Environment.NewLine}one-in-a-generation artist.", result);
    }

    [Fact]
    public void FixHyphensTwoLinesWithSpaceMissingSecondLine()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var p = new Paragraph($"- You can't talk either.{Environment.NewLine}That's what I said.", 4000, 7000);
        var result = splitMerge.FixDashesAndSpaces(p.Text, p, null);
        Assert.Equal($"- You can't talk either.{Environment.NewLine}- That's what I said.", result);
    }

    [Fact]
    public void FixHyphensTwoLinesWithSpaceMissingSecondLine3()
    {
        var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
        var prev = new Paragraph($"But I really think that -", 0, 3900);
        var p = new Paragraph($"- you can't talk either.{Environment.NewLine}That's what I said.", 4000, 7000);
        var result = splitMerge.FixDashesAndSpaces(p.Text, p, prev);
        Assert.Equal($"- you can't talk either.{Environment.NewLine}That's what I said.", result);
    }
}
