using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace UITests.Logic;

public class SplitManagerTests
{
    private static SubtitleLineViewModel MakeSubtitle(string text, double startSec, double endSec) =>
        new()
        {
            Text = text,
            StartTime = TimeSpan.FromSeconds(startSec),
            EndTime = TimeSpan.FromSeconds(endSec),
        };

    [Fact]
    public void Split_SingleLineSingleWord_SplitsIntoTwoSubtitles()
    {
        // Arrange
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle("Hello world", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act
        manager.Split(subtitles, subtitle, "en");

        // Assert
        Assert.Equal(2, subtitles.Count);
        Assert.False(string.IsNullOrWhiteSpace(subtitles[0].Text));
        Assert.False(string.IsNullOrWhiteSpace(subtitles[1].Text));
    }

    [Fact]
    public void Split_TwoLineText_SplitsOnLineBreak()
    {
        // Arrange
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"First line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act
        manager.Split(subtitles, subtitle, languageCode: "en");

        // Assert
        Assert.Equal(2, subtitles.Count);
        Assert.Equal("First line", subtitles[0].Text);
        Assert.Equal("Second line", subtitles[1].Text);
    }

    [Fact]
    public void Split_WithTextIndex_SplitsAtIndex()
    {
        // Arrange
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle("Hello world", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act
        manager.Split(subtitles, subtitle, textIndex: 5, languageCode: "en");

        // Assert
        Assert.Equal(2, subtitles.Count);
        Assert.Equal("Hello", subtitles[0].Text);
        Assert.Equal("world", subtitles[1].Text);
    }

    [Fact]
    public void Split_WithVideoPosition_UsesVideoPositionAsTimeSplitPoint()
    {
        // Arrange
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle("Hello world", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act – split at 2.0 seconds, which is inside the subtitle
        manager.Split(subtitles, subtitle, videoPositionSeconds: 2.0, languageCode: "en");

        // Assert
        Assert.Equal(2, subtitles.Count);
        Assert.Equal(2000.0, subtitles[1].StartTime.TotalMilliseconds, 1);
    }

    [Fact]
    public void Split_SubtitleNotInCollection_DoesNotModifyCollection()
    {
        // Arrange
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle("Hello world", 1, 3);
        var other = MakeSubtitle("Other", 4, 5);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { other };

        // Act
        manager.Split(subtitles, subtitle, languageCode: "en");

        // Assert
        Assert.Single(subtitles);
    }

    [Fact]
    public void Split_InsertsNewSubtitleDirectlyAfterOriginal()
    {
        // Arrange
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var first = MakeSubtitle("First subtitle", 0, 2);
        var second = MakeSubtitle("Third subtitle", 5, 7);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { first, second };

        // Act
        manager.Split(subtitles, first, languageCode: "en");

        // Assert
        Assert.Equal(3, subtitles.Count);
        Assert.Equal(second, subtitles[2]);
    }

    [Fact]
    public void Split_TimingsAreConsistent_NewSubtitleStartsAfterOriginalEnd()
    {
        // Arrange
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle("Hello world", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act
        manager.Split(subtitles, subtitle, languageCode: "en");

        // Assert
        Assert.True(subtitles[1].StartTime >= subtitles[0].EndTime);
    }
}
