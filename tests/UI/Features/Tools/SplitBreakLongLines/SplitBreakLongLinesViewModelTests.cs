using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UITests.Features.Tools.SplitBreakLongLines;

public class SplitBreakLongLinesViewModelTests
{
    private const int MaxLineLength = 36;
    private const int MaxSubtitleLength = 72;

    [Fact]
    public void RenumberSubtitles_AfterSplit_RemovesDuplicateNumbers()
    {
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle("One."),
            MakeSubtitle("First half of former subtitle twelve."),
            MakeSubtitle("Second half of former subtitle twelve."),
            MakeSubtitle("Next subtitle."),
        };

        subtitles[0].Number = 11;
        subtitles[1].Number = 12;
        subtitles[2].Number = 12;
        subtitles[3].Number = 13;

        SplitBreakLongLinesViewModel.RenumberSubtitles(subtitles);

        Assert.Equal(new[] { 1, 2, 3, 4 }, subtitles.Select(p => p.Number).ToArray());
    }

    [Fact]
    public void RebalanceFix_CanBeDeselectedAndRestoresOriginalText()
    {
        var subtitle = MakeSubtitle("This is the original subtitle text.");
        var fix = new SplitBreakLongLinesItem(
            "Rebalance",
            1,
            "preview",
            subtitle,
            isSelectable: true,
            proposedText: "This is the proposed subtitle text.");

        Assert.True(fix.IsSelected);
        Assert.Equal("This is the proposed subtitle text.", subtitle.Text);

        fix.IsSelected = false;
        Assert.Equal("This is the original subtitle text.", subtitle.Text);

        fix.IsSelected = true;
        Assert.Equal("This is the proposed subtitle text.", subtitle.Text);
    }

    [Fact]
    public void SelectAllAndDeselectAll_AffectOnlySelectableRebalanceFixes()
    {
        var vm = new SplitBreakLongLinesViewModel();

        var rebalanceSubtitle1 = MakeSubtitle("Original one.");
        var rebalanceSubtitle2 = MakeSubtitle("Original two.");
        var splitSubtitle = MakeSubtitle("Split item.");

        var rebalance1 = new SplitBreakLongLinesItem(
            "Rebalance",
            1,
            "preview",
            rebalanceSubtitle1,
            isSelectable: true,
            proposedText: "Changed one.");

        var rebalance2 = new SplitBreakLongLinesItem(
            "Rebalance",
            2,
            "preview",
            rebalanceSubtitle2,
            isSelectable: true,
            proposedText: "Changed two.");

        var split = new SplitBreakLongLinesItem(
            "Split",
            3,
            "preview",
            splitSubtitle);

        vm.Fixes.Add(rebalance1);
        vm.Fixes.Add(rebalance2);
        vm.Fixes.Add(split);

        vm.DeselectAllRebalancesCommand.Execute(null);

        Assert.False(rebalance1.IsSelected);
        Assert.False(rebalance2.IsSelected);
        Assert.True(split.IsSelected);
        Assert.Equal("Original one.", rebalanceSubtitle1.Text);
        Assert.Equal("Original two.", rebalanceSubtitle2.Text);

        vm.SelectAllRebalancesCommand.Execute(null);

        Assert.True(rebalance1.IsSelected);
        Assert.True(rebalance2.IsSelected);
        Assert.True(split.IsSelected);
        Assert.Equal("Changed one.", rebalanceSubtitle1.Text);
        Assert.Equal("Changed two.", rebalanceSubtitle2.Text);

        vm.OnClosingCleanup();
    }

    private static SubtitleLineViewModel MakeSubtitle(string text) =>
        new()
        {
            Text = text,
            StartTime = TimeSpan.FromSeconds(10),
            EndTime = TimeSpan.FromSeconds(14),
        };

    [Fact]
    public void Split_CompliantSingleLine_RemainsUnchanged()
    {
        var subtitle = MakeSubtitle("This line is valid.");

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true);

        Assert.Single(result);
        Assert.Equal(subtitle.Text, result[0].Text);
    }

    [Fact]
    public void Split_OverlongSingleLineWithinCapacity_BecomesBalancedTwoLineSubtitle()
    {
        var subtitle = MakeSubtitle(
            "This is an overlong subtitle line that can easily fit into two lines.");

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true);

        Assert.Single(result);
        var textLines = result[0].Text.SplitToLines();
        Assert.Equal(2, textLines.Count);
        Assert.All(textLines, line => Assert.True(line.Length <= MaxLineLength));
        Assert.Equal(subtitle.StartTime, result[0].StartTime);
        Assert.Equal(subtitle.EndTime, result[0].EndTime);
    }

    [Fact]
    public void Split_OverlongSingleLineThatCannotFitInOneEvent_CreatesMultipleEvents()
    {
        var subtitle = MakeSubtitle(
            "This subtitle contains much too much text to fit inside only two lines " +
            "with a maximum length of thirty-six characters and therefore needs " +
            "to become more than one subtitle event.");

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true);

        Assert.True(result.Count >= 2);
        Assert.Equal(subtitle.StartTime, result[0].StartTime);
        Assert.Equal(subtitle.EndTime, result[^1].EndTime);
        foreach (var item in result)
        {
            foreach (var line in item.Text.SplitToLines())
            {
                Assert.True(line.Length <= MaxLineLength);
            }
        }
    }

    [Fact]
    public void Split_TwoLineSubtitleWithOverlongLine_CreatesTwoCompliantEvents()
    {
        var subtitle = MakeSubtitle(
            "Und, war ich in der Kantine?" + Environment.NewLine +
            "Nein, ich konnte Sie dort nicht finden.");

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "de",
            makeCompliant: true);

        Assert.Equal(2, result.Count);
        Assert.Equal(subtitle.StartTime, result[0].StartTime);
        Assert.Equal(subtitle.EndTime, result[^1].EndTime);
        foreach (var item in result)
        {
            foreach (var line in item.Text.SplitToLines())
            {
                Assert.True(line.Length <= MaxLineLength);
            }
        }
    }

    [Fact]
    public void Split_CompliantUnbalancedTwoLineSubtitle_RemainsUnchanged()
    {
        var subtitle = MakeSubtitle(
            "This is intentionally longer" + Environment.NewLine +
            "short.");

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true);

        Assert.Single(result);
        Assert.Equal(subtitle.Text, result[0].Text);
    }

    [Fact]
    public void Split_MultipleEvents_UsesConfiguredMinimumGapAndPreservesOuterTimeCodes()
    {
        var subtitle = MakeSubtitle(
            "This subtitle contains much too much text to fit inside only two lines " +
            "with a maximum length of thirty-six characters and therefore needs " +
            "to become more than one subtitle event.");

        const double minimumGapMs = 120;

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true,
            minimumGapMs);

        Assert.True(result.Count >= 2);
        Assert.Equal(subtitle.StartTime, result[0].StartTime);
        Assert.Equal(subtitle.EndTime, result[^1].EndTime);

        for (var i = 1; i < result.Count; i++)
        {
            var actualGapMs =
                result[i].StartTime.TotalMilliseconds -
                result[i - 1].EndTime.TotalMilliseconds;

            Assert.Equal(minimumGapMs, actualGapMs, 1);
        }
    }

    [Fact]
    public void Split_BottomAnchoredTwoLineSubtitle_FirstNewSingleLineMovesFrom21To23()
    {
        var subtitle = MakeSubtitle(
            "Und, war ich in der Kantine?" + Environment.NewLine +
            "Nein, ich konnte Sie dort wirklich überhaupt nicht finden.");
        subtitle.MarginV = "21";

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "de",
            makeCompliant: true);

        Assert.Equal(2, result.Count);
        Assert.Single(result[0].Text.SplitToLines());
        Assert.Equal("23", result[0].MarginV);

        Assert.Equal(2, result[1].Text.SplitToLines().Count);
        Assert.Equal("21", result[1].MarginV);
    }

    [Fact]
    public void Split_TwoLineSubtitleHigherUp_FirstNewSingleLineMovesFrom19To21()
    {
        var subtitle = MakeSubtitle(
            "Und, war ich in der Kantine?" + Environment.NewLine +
            "Nein, ich konnte Sie dort wirklich überhaupt nicht finden.");
        subtitle.MarginV = "19";

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "de",
            makeCompliant: true);

        Assert.Equal(2, result.Count);
        Assert.Single(result[0].Text.SplitToLines());
        Assert.Equal("21", result[0].MarginV);
        Assert.Equal(2, result[1].Text.SplitToLines().Count);
        Assert.Equal("19", result[1].MarginV);
    }

    [Fact]
    public void Split_TwoLineSubtitleOnEvenRaster_FirstNewSingleLineMovesFrom20To22()
    {
        var subtitle = MakeSubtitle(
            "Und, war ich in der Kantine?" + Environment.NewLine +
            "Nein, ich konnte Sie dort wirklich überhaupt nicht finden.");
        subtitle.MarginV = "20";

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "de",
            makeCompliant: true);

        Assert.Equal(2, result.Count);
        Assert.Single(result[0].Text.SplitToLines());
        Assert.Equal("22", result[0].MarginV);
        Assert.Equal(2, result[1].Text.SplitToLines().Count);
        Assert.Equal("20", result[1].MarginV);
    }

    [Fact]
    public void Split_BottomAnchoredSingleLineBecomingTwoLines_MovesFrom23To21()
    {
        var subtitle = MakeSubtitle(
            "This single line is too long and must wrap into two lines.");
        subtitle.MarginV = "23";

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true);

        Assert.Single(result);
        Assert.Equal(2, result[0].Text.SplitToLines().Count);
        Assert.Equal("21", result[0].MarginV);
    }

    [Fact]
    public void Split_SingleLineHigherUpBecomingTwoLines_MovesFrom21To19()
    {
        var subtitle = MakeSubtitle(
            "This single line is too long and must wrap into two lines.");
        subtitle.MarginV = "21";

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true);

        Assert.Single(result);
        Assert.Equal(2, result[0].Text.SplitToLines().Count);
        Assert.Equal("19", result[0].MarginV);
    }

    [Fact]
    public void Split_SingleLineOnEvenRasterBecomingTwoLines_MovesFrom22To20()
    {
        var subtitle = MakeSubtitle(
            "This single line is too long and must wrap into two lines.");
        subtitle.MarginV = "22";

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength,
            "en",
            makeCompliant: true);

        Assert.Single(result);
        Assert.Equal(2, result[0].Text.SplitToLines().Count);
        Assert.Equal("20", result[0].MarginV);
    }

    [Fact]
    public void Split_LegacyMode_DoesNotAutoBalanceSingleLine()
    {
        var subtitle = MakeSubtitle(
            "This is an overlong subtitle line that can easily fit into two lines.");

        var result = SplitBreakLongLinesViewModel.Split(
            subtitle,
            MaxSubtitleLength,
            MaxLineLength);

        Assert.Single(result);
        Assert.Equal(subtitle.Text, result[0].Text);
    }
}
