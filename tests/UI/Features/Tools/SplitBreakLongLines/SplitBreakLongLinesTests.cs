using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Tools.SplitBreakLongLines;

public class SplitBreakLongLinesTests
{
    private static SubtitleLineViewModel MakeSubtitle(string text, double startSec, double endSec) =>
        new()
        {
            Text = text,
            StartTime = TimeSpan.FromSeconds(startSec),
            EndTime = TimeSpan.FromSeconds(endSec),
        };

    [Fact]
    public void Split_TextWithinLimit_ReturnsOriginalSubtitle()
    {
        var item = MakeSubtitle("This line is short enough.", 1, 3);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 80, singleLineMaxLength: 40);

        Assert.Single(result);
        Assert.Equal("This line is short enough.", result[0].Text);
    }

    [Fact]
    public void Split_LongText_DoesNotInsertLineBreaksIntoSegments()
    {
        // Regression for #10959: split-only should split into multiple subtitles
        // without adding a line break inside each new segment. AutoBreakLine
        // belongs to the opt-in Rebalance long lines step.
        var longText = new string('a', 45) + " " + new string('b', 45);
        var item = MakeSubtitle(longText, 1, 5);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 50, singleLineMaxLength: 40);

        Assert.True(result.Count > 1, "Expected the long text to be split into multiple subtitles.");
        foreach (var line in result)
        {
            Assert.DoesNotContain("\n", line.Text);
            Assert.DoesNotContain("\r", line.Text);
        }
    }

    [Fact]
    public void Split_MaxNumberOfLinesOne_ProducesSingleLineSegments()
    {
        // Regression for #10959: when MaxNumberOfLines is 1
        // (maxCharactersPerSubtitle == singleLineMaxLength), each produced
        // subtitle must be a single short line with no embedded line break.
        const int singleLineMaxLength = 40;
        var longText = string.Join(' ', Enumerable.Repeat("word", 30));
        var item = MakeSubtitle(longText, 1, 10);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: singleLineMaxLength, singleLineMaxLength: singleLineMaxLength);

        Assert.True(result.Count > 1, "Expected the long text to be split into multiple subtitles.");
        foreach (var line in result)
        {
            Assert.DoesNotContain("\n", line.Text);
            Assert.DoesNotContain("\r", line.Text);
        }
    }

    // The "only subtitles with a too-long line" rebalance mode (teletext tester feedback on
    // PR #13862): an intentionally unbalanced subtitle whose lines all fit must be left alone.
    [Theory]
    [InlineData("Four words on top\nline", 37, 2, false)] // unbalanced but compliant
    [InlineData("This single line is exactly forty chars.", 37, 2, true)] // one line too long
    [InlineData("ok\nok\nok", 37, 2, true)] // too many lines
    [InlineData("<i>Italic tags do not count toward the length</i>", 45, 2, false)]
    public void HasLineTooLong_FlagsOnlyNonCompliantSubtitles(string text, int maxLength, int maxLines, bool expected)
    {
        var normalized = text.Replace("\n", "\r\n");
        Assert.Equal(expected, SplitBreakLongLinesViewModel.HasLineTooLong(normalized, maxLength, maxLines));
    }

    // --- SE4 parity (PR #14006 by Triathlon-rally): a subtitle that fits in total can still be
    // --- unusable because one line is too long or it has too many lines.

    [Fact]
    public void Split_TwoLinesOneTooLong_SplitsAtTheExistingLineBreak()
    {
        var item = MakeSubtitle("Und, war ich in der Kantine?\r\nNein, ich konnte Sie dort nicht finden.", 10, 14);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36);

        Assert.Equal(2, result.Count);
        Assert.Equal("Und, war ich in der Kantine?", result[0].Text);
        Assert.Equal("Nein, ich konnte Sie dort nicht finden.", result[1].Text); // 39 > 36
        Assert.Equal(item.StartTime, result[0].StartTime);
        Assert.Equal(item.EndTime, result[^1].EndTime);
    }

    [Fact]
    public void Split_TooManyLines_GroupsLinesIntoEvents()
    {
        var item = MakeSubtitle("One\r\nTwo\r\nThree", 10, 14);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36);

        Assert.Equal(2, result.Count);
        Assert.Equal("One\r\nTwo", result[0].Text.Replace("\n", "\r\n").Replace("\r\r", "\r"));
        Assert.Equal("Three", result[1].Text);
    }

    [Fact]
    public void Split_SingleLineOverLineLimitButWithinTotal_SplitsIntoSingleLineEvents()
    {
        // Split-only never inserts a line break (#10959), so the only way to make this fit is
        // two events that each fit on one line.
        var item = MakeSubtitle("This single line is too long and must become two events.", 10, 14); // 57 chars

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36);

        Assert.Equal(2, result.Count);
        Assert.All(result, line =>
        {
            Assert.DoesNotContain("\n", line.Text);
            Assert.True(line.Text.Length <= 36);
        });
    }

    [Fact]
    public void Split_CompliantUnbalancedTwoLines_RemainsUnchanged()
    {
        var item = MakeSubtitle("This is intentionally longer\r\nshort.", 10, 14);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36);

        Assert.Single(result);
        Assert.Same(item, result[0]);
    }

    [Fact]
    public void Split_ReservesMinimumGapBetweenEventsAndKeepsOuterTimeCodes()
    {
        var item = MakeSubtitle(string.Join(' ', Enumerable.Repeat("word", 40)), 10, 14);
        var options = new SplitBreakLongLinesViewModel.SplitOptions { MinimumGapMs = 120 };

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36, options);

        Assert.True(result.Count >= 2);
        Assert.Equal(item.StartTime, result[0].StartTime);
        Assert.Equal(item.EndTime, result[^1].EndTime);
        for (var i = 1; i < result.Count; i++)
        {
            var gapMs = result[i].StartTime.TotalMilliseconds - result[i - 1].EndTime.TotalMilliseconds;
            Assert.Equal(120, gapMs, 1);
            Assert.True(result[i - 1].Duration.TotalMilliseconds > 0);
        }
    }

    [Fact]
    public void Split_GapIsClampedSoTextKeepsAtLeastHalfTheDuration()
    {
        var item = MakeSubtitle(string.Join(' ', Enumerable.Repeat("word", 40)), 10, 10.2);
        var options = new SplitBreakLongLinesViewModel.SplitOptions { MinimumGapMs = 1000 };

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36, options);

        Assert.True(result.Count >= 2);
        Assert.Equal(item.EndTime, result[^1].EndTime);
        var textMs = result.Sum(p => p.Duration.TotalMilliseconds);
        Assert.True(textMs >= 100 - 1, $"text time {textMs} ms");
        Assert.All(result, p => Assert.True(p.Duration.TotalMilliseconds > 0));
    }

    [Fact]
    public void Split_WithoutGap_EventsAreBackToBack()
    {
        var item = MakeSubtitle(string.Join(' ', Enumerable.Repeat("word", 40)), 10, 14);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36);

        for (var i = 1; i < result.Count; i++)
        {
            Assert.Equal(result[i - 1].EndTime, result[i].StartTime);
        }
    }

    [Theory]
    [InlineData("21", "23")]
    [InlineData("19", "21")]
    [InlineData("20", "22")]
    public void Split_TeletextTwoLinesIntoOneLineEvents_KeepsBottomEdge(string marginV, string expectedOneLineRow)
    {
        var item = MakeSubtitle("Und, war ich in der Kantine?\r\nNein, ich konnte Sie dort wirklich überhaupt nicht finden.", 10, 14);
        item.MarginV = marginV;
        var options = new SplitBreakLongLinesViewModel.SplitOptions { AdjustTeletextRows = true, TeletextDoubleHeight = true };

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36, options);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Single(p.Text.SplitToLines()));
        Assert.All(result, p => Assert.Equal(expectedOneLineRow, p.MarginV)); // bottom edge unchanged
        Assert.NotEqual(expectedOneLineRow, marginV);
    }

    [Theory]
    [InlineData("23", "21")]
    [InlineData("21", "19")]
    [InlineData("22", "20")]
    public void Rebalance_TeletextOneLineIntoTwo_KeepsBottomEdge(string marginV, string expectedTwoLineRow)
    {
        Assert.Equal(int.Parse(expectedTwoLineRow), TeletextRowHelper.GetRowKeepingBottomEdge(marginV, 1, 2, doubleHeight: true));
    }

    [Fact]
    public void Split_NotEbu_LeavesMarginVAlone()
    {
        // MarginV is a pixel margin for ASSA - only an EBU file gets its rows shifted.
        var item = MakeSubtitle("Und, war ich in der Kantine?\r\nNein, ich konnte Sie dort nicht finden.", 10, 14);
        item.MarginV = "21";

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 72, singleLineMaxLength: 36);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal("21", p.MarginV));
    }

    [Fact]
    public void RebalanceFix_UncheckRestoresOriginalText()
    {
        var subtitle = MakeSubtitle("This is the original subtitle text.", 10, 14);
        var fix = new SplitBreakLongLinesItem("Rebalance", 1, "preview", subtitle, "This is the\r\nproposed text.");

        Assert.True(fix.IsSelectable);
        Assert.True(fix.IsSelected);
        Assert.Equal("This is the\r\nproposed text.", subtitle.Text);

        fix.IsSelected = false;
        Assert.Equal("This is the original subtitle text.", subtitle.Text);

        fix.IsSelected = true;
        Assert.Equal("This is the\r\nproposed text.", subtitle.Text);
    }

    [Fact]
    public void RebalanceFix_TeletextRowFollowsTheCheckbox()
    {
        var subtitle = MakeSubtitle("One line on the bottom row.", 10, 14);
        subtitle.MarginV = "23";
        var fix = new SplitBreakLongLinesItem("Rebalance", 1, "preview", subtitle, "One line on\r\nthe bottom row.", "21");

        Assert.Equal("21", subtitle.MarginV);
        fix.IsSelected = false;
        Assert.Equal("23", subtitle.MarginV);
        Assert.Equal("One line on the bottom row.", subtitle.Text);
    }

    [Fact]
    public void SelectAllAndSelectNone_TouchOnlyRebalanceFixes()
    {
        var vm = new SplitBreakLongLinesViewModel();
        var rebalanced = MakeSubtitle("Original.", 10, 14);
        var split = MakeSubtitle("Split item.", 10, 14);
        var rebalanceFix = new SplitBreakLongLinesItem("Rebalance", 1, "preview", rebalanced, "Changed.");
        var splitFix = new SplitBreakLongLinesItem("Split", 2, "preview", split);
        vm.Fixes.Add(rebalanceFix);
        vm.Fixes.Add(splitFix);

        vm.SelectNoneCommand.Execute(null);
        Assert.False(rebalanceFix.IsSelected);
        Assert.True(splitFix.IsSelected);
        Assert.Equal("Original.", rebalanced.Text);

        vm.SelectAllCommand.Execute(null);
        Assert.True(rebalanceFix.IsSelected);
        Assert.Equal("Changed.", rebalanced.Text);

        vm.OnClosingCleanup();
    }
}
