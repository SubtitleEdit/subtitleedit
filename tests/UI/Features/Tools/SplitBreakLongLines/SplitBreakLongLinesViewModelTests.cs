using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;
using System;
using Xunit;

namespace UITests.Features.Tools.SplitBreakLongLines;

public class SplitBreakLongLinesViewModelTests
{
    private const int MaxLineLength = 36;
    private const int MaxSubtitleLength = 72;

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