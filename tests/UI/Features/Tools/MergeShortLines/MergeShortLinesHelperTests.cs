using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.MergeShortLines;
using System.Collections.ObjectModel;

namespace UITests.Features.Tools.MergeShortLines;

public class MergeShortLinesHelperTests
{
    private static string Flat(string text) => text.Replace("\r\n", " ").Replace('\n', ' ');

    private static SubtitleLineViewModel MakeSubtitle(string text, double startSec, double endSec) =>
        new()
        {
            Text = text,
            StartTime = TimeSpan.FromSeconds(startSec),
            EndTime = TimeSpan.FromSeconds(endSec),
        };

    private static List<SubtitleLineViewModel> ThreeMergeableLines() => new()
    {
        MakeSubtitle("Line one", 1.0, 2.0),
        MakeSubtitle("Line two", 2.1, 3.0),
        MakeSubtitle("Line three", 3.1, 4.0),
    };

    private static MergeShortLinesResult Merge(List<SubtitleLineViewModel> subtitles, ISet<Guid>? excluded = null) =>
        MergeShortLinesHelper.Merge(subtitles, new List<double>(), singleLineMaxLength: 50, maxNumberOfLines: 2, gapThresholdMs: 500, unbreakLinesShorterThan: 10, excluded);

    private static MergeShortLinesResult MergeWithHighlights(List<SubtitleLineViewModel> subtitles, ISet<Guid>? excluded = null) =>
        MergeShortLinesHelper.MergeWithHighlights(subtitles, new List<double>(), singleLineMaxLength: 50, maxNumberOfLines: 2, gapThresholdMs: 500, unbreakLinesShorterThan: 10, excluded);

    [Fact]
    public void Merge_NoExclusions_MergesChain()
    {
        var subtitles = ThreeMergeableLines();

        var result = Merge(subtitles);

        Assert.Equal(2, result.MergeCount);
        Assert.Single(result.MergedSubtitles);
        Assert.Equal(new[] { subtitles[1].Id, subtitles[2].Id }, result.Fixes.Select(f => f.SourceLineId));
        Assert.All(result.Fixes, f => Assert.True(f.Apply && f.CanToggle));
    }

    [Fact]
    public void Merge_ExcludedLine_HeadsItsOwnGroup()
    {
        var subtitles = ThreeMergeableLines();

        // Untick "line two into line one": line two must not be merged into line one, but it
        // may still absorb line three.
        var result = Merge(subtitles, new HashSet<Guid> { subtitles[1].Id });

        Assert.Equal(1, result.MergeCount);
        Assert.Equal(2, result.MergedSubtitles.Count);
        Assert.Equal("Line one", result.MergedSubtitles[0].Text);
        Assert.Equal("Line two Line three", Flat(result.MergedSubtitles[1].Text));

        // The refused candidate stays in the list, unticked, so it can be ticked again.
        Assert.Equal(2, result.Fixes.Count);
        Assert.Equal(subtitles[1].Id, result.Fixes[0].SourceLineId);
        Assert.False(result.Fixes[0].Apply);
        Assert.Equal(subtitles[2].Id, result.Fixes[1].SourceLineId);
        Assert.True(result.Fixes[1].Apply);
    }

    [Fact]
    public void Merge_ExcludedLastLine_MergesTheRest()
    {
        var subtitles = ThreeMergeableLines();

        var result = Merge(subtitles, new HashSet<Guid> { subtitles[2].Id });

        Assert.Equal(1, result.MergeCount);
        Assert.Equal(2, result.MergedSubtitles.Count);
        Assert.Equal("Line one Line two", Flat(result.MergedSubtitles[0].Text));
        Assert.Equal("Line three", result.MergedSubtitles[1].Text);
    }

    [Fact]
    public void Merge_AllExcluded_ChangesNothing()
    {
        var subtitles = ThreeMergeableLines();

        var result = Merge(subtitles, new HashSet<Guid> { subtitles[1].Id, subtitles[2].Id });

        Assert.Equal(0, result.MergeCount);
        Assert.Equal(3, result.MergedSubtitles.Count);
        Assert.Equal(2, result.Fixes.Count);
        Assert.All(result.Fixes, f => Assert.False(f.Apply));
    }

    [Fact]
    public void MergeWithHighlights_HeadRowHasNoCheckbox()
    {
        var subtitles = ThreeMergeableLines();

        var result = MergeWithHighlights(subtitles);

        Assert.Equal(2, result.MergeCount);
        Assert.Equal(3, result.Fixes.Count);
        Assert.False(result.Fixes[0].CanToggle);
        Assert.Equal(subtitles[0].Id, result.Fixes[0].SourceLineId);
        Assert.True(result.Fixes[1].CanToggle);
        Assert.Equal(subtitles[1].Id, result.Fixes[1].SourceLineId);
        Assert.True(result.Fixes[2].CanToggle);
        Assert.Equal(subtitles[2].Id, result.Fixes[2].SourceLineId);
    }

    [Fact]
    public void MergeWithHighlights_ExcludedLine_ReportedUntickedAndHeadsNextGroup()
    {
        var subtitles = ThreeMergeableLines();

        var result = MergeWithHighlights(subtitles, new HashSet<Guid> { subtitles[1].Id });

        Assert.Equal(1, result.MergeCount);
        Assert.Equal(3, result.MergedSubtitles.Count); // line one alone, lines two and three highlighted
        Assert.Equal("Line one", result.MergedSubtitles[0].Text);
        Assert.Contains("<u>Line two</u>", result.MergedSubtitles[1].Text);

        // Refused row for line two, then the head + member rows of the new group.
        Assert.Equal(3, result.Fixes.Count);
        Assert.Equal(subtitles[1].Id, result.Fixes[0].SourceLineId);
        Assert.False(result.Fixes[0].Apply);
        Assert.True(result.Fixes[0].CanToggle);
        Assert.False(result.Fixes[1].CanToggle);
        Assert.Equal(subtitles[2].Id, result.Fixes[2].SourceLineId);
    }

    [Fact]
    public void ReplaceChangedRows_KeepsUnchangedRowInstances()
    {
        var subtitles = ThreeMergeableLines();
        var first = Merge(subtitles).Fixes;
        var target = new ObservableCollection<MergeShortLinesItem>(first);

        // Untick line three: the row for line two is unchanged and must keep its instance.
        var fresh = Merge(subtitles, new HashSet<Guid> { subtitles[2].Id }).Fixes;
        var inserted = MergeShortLinesViewModel.ReplaceChangedRows(target, fresh);

        Assert.Equal(2, target.Count);
        Assert.Same(first[0], target[0]);
        Assert.Same(fresh[1], target[1]);
        Assert.Equal(new[] { fresh[1] }, inserted);
    }

    [Fact]
    public void MergeShortLinesItem_ApplyDefaultsToTrue()
    {
        var id = Guid.NewGuid();
        var item = new MergeShortLinesItem("Title", 1, "Fix description", new SubtitleLineViewModel(), id);
        Assert.True(item.Apply);
        Assert.True(item.CanToggle);
        Assert.Equal(id, item.SourceLineId);
    }
}
