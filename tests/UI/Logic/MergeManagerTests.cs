using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace UITests.Logic;

public class MergeManagerTests
{
    [Fact]
    public void MergeSelectedLines_ShouldMergeOriginalText()
    {
        // Arrange
        var mergeManager = new MergeManager();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Number = 1,
                Text = "Translated one",
                OriginalText = "Original one",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(2),
            },
            new()
            {
                Number = 2,
                Text = "Translated two",
                OriginalText = "Original two",
                StartTime = TimeSpan.FromSeconds(2),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        // Act
        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1]]);

        // Assert
        Assert.Single(subtitles);
        Assert.Equal($"Translated one{Environment.NewLine}Translated two", subtitles[0].Text);
        Assert.Equal($"Original one{Environment.NewLine}Original two", subtitles[0].OriginalText);
    }

    [Fact]
    public void MergeSelectedLinesAsDialog_ShouldMergeOriginalTextAsDialog()
    {
        // Arrange
        Se.Settings.General.DialogStyle = "DashBothLinesWithSpace";
        var mergeManager = new MergeManager();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Number = 1,
                Text = "Hi there",
                OriginalText = "Hej der",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(2),
            },
            new()
            {
                Number = 2,
                Text = "How are you?",
                OriginalText = "Hvordan gar det?",
                StartTime = TimeSpan.FromSeconds(2),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        mergeManager.MergeSelectedLinesAsDialog(subtitles, [subtitles[0], subtitles[1]]);

        Assert.Single(subtitles);
        Assert.Contains("Hej der", subtitles[0].OriginalText);
        Assert.Contains("Hvordan gar det?", subtitles[0].OriginalText);
    }

    [Fact]
    public void MergeSelectedLines_ShouldKeepOriginalTextEmpty_WhenBothOriginalTextsAreEmpty()
    {
        var mergeManager = new MergeManager();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Number = 1,
                Text = "Translated one",
                OriginalText = string.Empty,
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(2),
            },
            new()
            {
                Number = 2,
                Text = "Translated two",
                OriginalText = string.Empty,
                StartTime = TimeSpan.FromSeconds(2),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1]]);

        Assert.Single(subtitles);
        Assert.Equal(string.Empty, subtitles[0].OriginalText);
    }
}




