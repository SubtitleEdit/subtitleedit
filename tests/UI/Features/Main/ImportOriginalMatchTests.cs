using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using System.Collections.ObjectModel;

namespace UITests.Features.Main;

/// <summary>
/// Aligning an original subtitle that does not line up 1:1 with the working rows (#13449). The
/// three matching passes get progressively looser, the last one settling for nothing more than
/// overlapping middles - so without the used-line check one original line spanning two working
/// rows was projected onto both, showing the same source text twice while the line that really
/// had no row of its own turned into a reference-only row.
/// </summary>
public class ImportOriginalMatchTests
{
    private static ObservableCollection<SubtitleLineViewModel> Working(params (int Start, int End)[] times)
    {
        var list = new ObservableCollection<SubtitleLineViewModel>();
        var number = 1;
        foreach (var (start, end) in times)
        {
            list.Add(new SubtitleLineViewModel(new Paragraph("working " + number++, start, end), new SubRip()));
        }

        return list;
    }

    private static Subtitle Original(params (int Start, int End, string Text)[] paragraphs)
    {
        var subtitle = new Subtitle();
        foreach (var (start, end, text) in paragraphs)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, start, end));
        }

        return subtitle;
    }

    private static string Projected(ImportOriginalHelper.OriginalMatch match) =>
        string.Join("|", match.Projection.Paragraphs.Select(p => p.Text));

    private static string Unmatched(ImportOriginalHelper.OriginalMatch match) =>
        string.Join("|", match.Unmatched.Select(p => p.Text));

    [Fact]
    public void OneOriginalLineSpanningTwoRows_GoesToOneRowOnly()
    {
        // The translator split one original line in two - the usual reason for a count mismatch.
        var match = ImportOriginalHelper.MatchOriginalLines(
            Working((1000, 2000), (2000, 3000)),
            Original((1000, 3000, "long"), (4000, 5000, "second")));

        Assert.Equal("long|", Projected(match));
        Assert.Equal("second", Unmatched(match));
    }

    [Fact]
    public void IdenticalOriginalLines_AreHandedOutOnePerRow()
    {
        // Two original lines with the same text and timings are still two lines.
        var match = ImportOriginalHelper.MatchOriginalLines(
            Working((1000, 2000), (1000, 2000)),
            Original((1000, 2000, "same"), (1000, 2000, "same")));

        Assert.Equal("same|same", Projected(match));
        Assert.Equal(string.Empty, Unmatched(match));
        Assert.NotSame(match.Projection.Paragraphs[0], match.Projection.Paragraphs[1]);
    }

    [Fact]
    public void FewerOriginalLinesThanRows_LeavesTheExtraRowsEmpty()
    {
        var match = ImportOriginalHelper.MatchOriginalLines(
            Working((1000, 2000), (3000, 4000), (5000, 6000)),
            Original((1000, 2000, "first")));

        Assert.Equal("first||", Projected(match));
        Assert.Equal(string.Empty, Unmatched(match));
    }

    [Fact]
    public void EveryRowMatching_StillProjectsOneForOne()
    {
        var match = ImportOriginalHelper.MatchOriginalLines(
            Working((1000, 2000), (3000, 4000)),
            Original((1000, 2000, "a"), (3000, 4000, "b")));

        Assert.Equal("a|b", Projected(match));
        Assert.Equal(string.Empty, Unmatched(match));
    }

    // Every original line lands in exactly one place: a working row, or the unmatched list.
    [Fact]
    public void NoOriginalLineIsUsedTwiceOrLost()
    {
        var original = Original(
            (1000, 5000, "wide"),
            (1200, 1800, "inside a"),
            (2000, 2500, "inside b"),
            (9000, 9500, "far away"));

        var match = ImportOriginalHelper.MatchOriginalLines(
            Working((1000, 2000), (2000, 3000), (3000, 4000)),
            original);

        var placed = match.Projection.Paragraphs
            .Where(p => !string.IsNullOrEmpty(p.Text))
            .Concat(match.Unmatched)
            .ToList();

        Assert.Equal(original.Paragraphs.Count, placed.Count);
        foreach (var paragraph in original.Paragraphs)
        {
            Assert.Single(placed, p => ReferenceEquals(p, paragraph));
        }
    }
}
