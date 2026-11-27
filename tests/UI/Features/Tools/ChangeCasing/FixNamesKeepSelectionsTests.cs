using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

namespace UITests.Features.Tools.ChangeCasing;

/// <summary>
/// Refresh and other preview rebuilds in Fix names keep the user's unchecked names and preview
/// lines (#17243).
/// </summary>
public class FixNamesKeepSelectionsTests
{
    [Fact]
    public void KeepUserChecks_ExistingNamesKeepState_NewNamesGetDefault()
    {
        var oldNames = new List<FixNameItem>
        {
            new("Patience", false), // user unchecked
            new("US", true),        // user checked a default-unchecked name
        };
        var newNames = new List<FixNameItem>
        {
            new("Patience", true),
            new("US", false),
            new("Bob", true),      // new from extra names
        };

        FixNamesViewModel.KeepUserChecks(newNames, oldNames);

        Assert.False(newNames[0].IsChecked);
        Assert.True(newNames[1].IsChecked);
        Assert.True(newNames[2].IsChecked);
    }

    [Fact]
    public void BuildHits_RemembersUncheckedLines()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Thanks, patience.", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("Hi bob.", 2000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("bob and patience.", 4000, 5000));
        subtitle.Renumber();

        var disabled = new HashSet<int>();
        var hits = FixNamesViewModel.BuildHits(subtitle, ["Patience"], disabled, TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 1, 3 }, hits.Select(h => h.LineIndex));
        Assert.All(hits, h => Assert.True(h.IsEnabled));

        // User unchecks line 1, then adds "Bob" and refreshes.
        hits[0].IsEnabled = false;
        FixNamesViewModel.UpdateDisabledLines(disabled, hits);
        hits = FixNamesViewModel.BuildHits(subtitle, ["Patience", "Bob"], disabled, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { 1, 2, 3 }, hits.Select(h => h.LineIndex));
        Assert.Equal(new[] { false, true, true }, hits.Select(h => h.IsEnabled));
        Assert.Equal("Bob and Patience.", hits[2].After);

        // Line 1 drops out of the preview (Patience unchecked) and comes back: still unchecked.
        FixNamesViewModel.UpdateDisabledLines(disabled, hits);
        hits = FixNamesViewModel.BuildHits(subtitle, ["Bob"], disabled, TestContext.Current.CancellationToken);
        FixNamesViewModel.UpdateDisabledLines(disabled, hits);
        hits = FixNamesViewModel.BuildHits(subtitle, ["Patience", "Bob"], disabled, TestContext.Current.CancellationToken);
        Assert.False(hits.Single(h => h.LineIndex == 1).IsEnabled);

        // Re-checking a line forgets it.
        hits.Single(h => h.LineIndex == 1).IsEnabled = true;
        FixNamesViewModel.UpdateDisabledLines(disabled, hits);
        Assert.Empty(disabled);
    }
}
