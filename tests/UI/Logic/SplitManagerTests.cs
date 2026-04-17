using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
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

    [Fact]
    public void Split_TwoLineDialogText_StripsLeadingDashesFromBothParts()
    {
        // Arrange – two lines where the second starts with a dash and the first ends
        // with a sentence-ending period, so IsDialog returns true.
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        Configuration.Settings.General.DialogStyle = DialogType.DashBothLinesWithSpace;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"- Hello there.{Environment.NewLine}- Hi back.", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act
        manager.Split(subtitles, subtitle, languageCode: "en");

        // Assert – dashes and leading spaces are stripped from both parts
        Assert.Equal(2, subtitles.Count);
        Assert.Equal("Hello there.", subtitles[0].Text);
        Assert.Equal("Hi back.", subtitles[1].Text);
    }

    [Fact]
    public void Split_TwoLineNonDialogText_KeepsBothLinesAsIs()
    {
        // Arrange – two plain lines with no leading dashes, so IsDialog returns false.
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        Configuration.Settings.General.DialogStyle = DialogType.DashBothLinesWithSpace;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"First line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act
        manager.Split(subtitles, subtitle, languageCode: "en");

        // Assert – lines are kept verbatim (only trimmed)
        Assert.Equal(2, subtitles.Count);
        Assert.Equal("First line", subtitles[0].Text);
        Assert.Equal("Second line", subtitles[1].Text);
    }

    // FixTags tests

    [Fact]
    public void Split_BoldTagOpenInFirstPart_IsClosedAndReopenedInSecond()
    {
        // Arrange – the split happens at the word boundary; <b> in text1 must be closed and re-opened in text2
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"<b>Hello world</b>", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Act – split at textIndex so <b> stays in text1
        manager.Split(subtitles, subtitle, textIndex: 9, languageCode: "en");

        // Assert – text1 should be closed with </b> and text2 should start with <b>
        Assert.Contains("</b>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("<b>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Split_ItalicTagOpenInFirstLine_ClosedAndReopenedInSecondLine()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"<i>First line{Environment.NewLine}Second line</i>", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Contains("</i>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("<i>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Split_UnderlineTagOpenInFirstLine_ClosedAndReopenedInSecondLine()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"<u>First line{Environment.NewLine}Second line</u>", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Contains("</u>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("<u>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Split_AlreadyClosedTagInFirstLine_NotDuplicated()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"<b>First</b> line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        // text1 already has matching </b>, so no extra closing tag should be added
        var countClose = subtitles[0].Text.Split(["</b>", "</B>"], StringSplitOptions.None).Length - 1;
        Assert.Equal(1, countClose);
        // text2 should NOT start with <b> since the tag was properly closed in text1
        Assert.DoesNotContain("<b>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Split_FontTagOpenInFirstLine_ClosedAndReopenedInSecondLine()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"<font color=\"red\">First line{Environment.NewLine}Second line</font>", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Contains("</font>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("<font", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Split_NoTagsInText_TextUnchanged()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"Hello world{Environment.NewLine}How are you", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Equal("Hello world", subtitles[0].Text);
        Assert.Equal("How are you", subtitles[1].Text);
    }

    [Fact]
    public void Split_AssaBoldTagActiveInFirstLine_PropagatedToSecondLine()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($@"{{\b1}}First line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Contains(@"\b1}", subtitles[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Split_AssaBoldTagClosedInFirstLine_NotPropagatedToSecondLine()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($@"{{\b1}}First{{\b0}} line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.DoesNotContain(@"\b1}", subtitles[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Split_MultipleMixedTagsOpenInFirstLine_AllClosedAndReopened()
    {
        Se.Settings.General.MinimumMillisecondsBetweenLines = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"<b><i>First line{Environment.NewLine}Second line</i></b>", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Contains("</b>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("</i>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<b>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<i>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
    }
}
