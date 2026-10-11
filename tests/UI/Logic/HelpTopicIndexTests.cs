using Nikse.SubtitleEdit.Logic;
using System.Linq;
using Xunit;

namespace UITests.Logic;

public class HelpTopicIndexTests
{
    [Theory]
    [InlineData("Window Areas", "window-areas")]
    [InlineData("1. Menu Bar", "1-menu-bar")]
    [InlineData("6. Audio Visualizer (Waveform / Spectrogram)", "6-audio-visualizer-waveform--spectrogram")]
    [InlineData("Keyboard Controls (in Waveform)", "keyboard-controls-in-waveform")]
    public void MakeAnchor_MatchesGitHubPagesIds(string heading, string expected)
    {
        Assert.Equal(expected, HelpTopicIndex.MakeAnchor(heading));
    }

    [Fact]
    public void Parse_SplitsPageIntoSections()
    {
        const string markdown = """
            # Main Window

            Intro about the [main window](main-window.md).

            <!-- Screenshot -->
            ![Main Window](../screenshots/main-window.png)

            ## Mouse Controls

            Click the waveform.

            ```
            # not a heading
            ```

            ## Mouse Controls

            Duplicate heading.
            """;

        var topics = HelpTopicIndex.Parse("features/main-window", markdown);

        Assert.Equal(["features/main-window", "features/main-window#mouse-controls", "features/main-window#mouse-controls-1"],
            topics.Select(t => t.Id));
        Assert.Equal("Main Window", topics[0].Title);
        Assert.Equal("Main Window › Mouse Controls", topics[1].Title);
        Assert.Contains("main window", topics[0].Body);
        Assert.DoesNotContain("png", topics[0].Body);
        Assert.Contains("not a heading", topics[1].Body);
    }

    [Fact]
    public void Search_RanksHeadingMatchesAboveBodyMatches()
    {
        var topics = HelpTopicIndex.Parse("features/a", "# Spell Check\n\nCheck spelling.\n")
            .Concat(HelpTopicIndex.Parse("features/b", "# Tools\n\n## Other\n\nRun spell check from here.\n"))
            .ToList();

        var hits = HelpTopicIndex.Search(topics, "spell check", 10);

        Assert.Equal(["features/a", "features/b#other"], hits.Select(h => h.Id));
        Assert.Empty(HelpTopicIndex.Search(topics, "nothing-matches", 10));
        Assert.Empty(HelpTopicIndex.Search(topics, "  ", 10));
    }

    [Fact]
    public void EmbeddedDocs_AreIndexed()
    {
        var hits = HelpTopicIndex.Search("waveform", 20);

        Assert.NotEmpty(hits);
        Assert.Contains(hits, h => h.Page == "features/main-window");
    }
}
