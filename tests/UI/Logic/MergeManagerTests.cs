using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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
        Assert.Equal("Translated one Translated two", subtitles[0].Text);
        Assert.Equal("Original one Original two", subtitles[0].OriginalText);
    }

    // Issue #15441: merging a two-line subtitle with an empty line must not unbreak it.
    [Theory]
    [InlineData(MergeManager.BreakMode.Normal)]
    [InlineData(MergeManager.BreakMode.AutoBreak)]
    public void MergeSelectedLines_WithEmptyLine_ShouldKeepLineBreaks(MergeManager.BreakMode breakMode)
    {
        var mergeManager = new MergeManager();
        var twoLines = "САСОН ГАБАЙ" + Environment.NewLine + "ДОУДИ ШУА";
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Number = 1, Text = twoLines, OriginalText = "Sasson Gabai" + Environment.NewLine + "Doudi Shoua", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) },
            new() { Number = 2, Text = string.Empty, OriginalText = string.Empty, StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(3) },
        };

        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1]], breakMode);

        Assert.Single(subtitles);
        Assert.Equal(twoLines, subtitles[0].Text);
        Assert.Equal("Sasson Gabai" + Environment.NewLine + "Doudi Shoua", subtitles[0].OriginalText);
        Assert.Equal(TimeSpan.FromSeconds(3), subtitles[0].EndTime);
    }

    [Fact]
    public void MergeSelectedLines_EmptyLineFirst_ShouldKeepLineBreaks()
    {
        var mergeManager = new MergeManager();
        var twoLines = "Hi" + Environment.NewLine + "there";
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Number = 1, Text = " ", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) },
            new() { Number = 2, Text = twoLines, StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(3) },
        };

        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1]]);

        Assert.Single(subtitles);
        Assert.Equal(twoLines, subtitles[0].Text);
    }

    [Fact]
    public void MergeSelectedLines_EmptyLineInMiddle_ShouldNotAddEmptyLine()
    {
        var mergeManager = new MergeManager();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Number = 1, Text = "One", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) },
            new() { Number = 2, Text = string.Empty, StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(3) },
            new() { Number = 3, Text = "Two", StartTime = TimeSpan.FromSeconds(3), EndTime = TimeSpan.FromSeconds(4) },
        };

        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1], subtitles[2]], MergeManager.BreakMode.KeepBreaks);

        Assert.Single(subtitles);
        Assert.Equal("One" + Environment.NewLine + "Two", subtitles[0].Text);
    }

    [Fact]
    public void MergeSelectedLines_Subtitle_WithEmptyLine_ShouldKeepLineBreaks()
    {
        var mergeManager = new MergeManager();
        var twoLines = "САСОН ГАБАЙ" + Environment.NewLine + "ДОУДИ ШУА";
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(twoLines, 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph(string.Empty, 2000, 3000));

        var result = mergeManager.MergeSelectedLines(subtitle, [0, 1]);

        Assert.Single(result.Paragraphs);
        Assert.Equal(twoLines, result.Paragraphs[0].Text);
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

    // Issue #13307: the configured dialog style must decide dash placement and spacing,
    // also when the first line has no sentence ending.
    [Theory]
    [InlineData("DashBothLinesWithSpace", "- Hi there", "- How are you?")]
    [InlineData("DashBothLinesWithoutSpace", "-Hi there", "-How are you?")]
    [InlineData("DashSecondLineWithSpace", "Hi there", "- How are you?")]
    [InlineData("DashSecondLineWithoutSpace", "Hi there", "-How are you?")]
    public void MergeSelectedLinesAsDialog_ShouldFollowDialogStyleSetting(string dialogStyle, string expectedLine1, string expectedLine2)
    {
        var originalDialogStyle = Se.Settings.General.DialogStyle;
        try
        {
            Se.Settings.General.DialogStyle = dialogStyle;
            var mergeManager = new MergeManager();
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                new()
                {
                    Number = 1,
                    Text = "Hi there",
                    StartTime = TimeSpan.FromSeconds(1),
                    EndTime = TimeSpan.FromSeconds(2),
                },
                new()
                {
                    Number = 2,
                    Text = "How are you?",
                    StartTime = TimeSpan.FromSeconds(2),
                    EndTime = TimeSpan.FromSeconds(3),
                },
            };

            mergeManager.MergeSelectedLinesAsDialog(subtitles, [subtitles[0], subtitles[1]]);

            Assert.Single(subtitles);
            Assert.Equal(expectedLine1 + Environment.NewLine + expectedLine2, subtitles[0].Text);
        }
        finally
        {
            Se.Settings.General.DialogStyle = originalDialogStyle;
        }
    }

    // Issue #15600: the dash goes inside leading formatting, so it gets the line's color/italic.
    [Theory]
    [InlineData("DashBothLinesWithSpace", "<font color='#ffe200'>Subtitle one.</font>", "<font color='#ffe200'>Subtitle two.</font>", "<font color='#ffe200'>- Subtitle one.</font>", "<font color='#ffe200'>- Subtitle two.</font>")]
    [InlineData("DashSecondLineWithSpace", "<font color='#ffe200'>Subtitle one.</font>", "<font color='#ffe200'>Subtitle two.</font>", "<font color='#ffe200'>Subtitle one.</font>", "<font color='#ffe200'>- Subtitle two.</font>")]
    [InlineData("DashBothLinesWithSpace", "<i>- Subtitle one.</i>", "{\\an8}<i>-Subtitle two.</i>", "<i>- Subtitle one.</i>", "{\\an8}<i>- Subtitle two.</i>")]
    public void MergeSelectedLinesAsDialog_ShouldPutDashInsideStartTags(string dialogStyle, string text1, string text2, string expectedLine1, string expectedLine2)
    {
        var originalDialogStyle = Se.Settings.General.DialogStyle;
        try
        {
            Se.Settings.General.DialogStyle = dialogStyle;
            var mergeManager = new MergeManager();
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                new() { Number = 1, Text = text1, StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) },
                new() { Number = 2, Text = text2, StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(3) },
            };

            mergeManager.MergeSelectedLinesAsDialog(subtitles, [subtitles[0], subtitles[1]]);

            Assert.Equal(expectedLine1 + Environment.NewLine + expectedLine2, subtitles[0].Text);
        }
        finally
        {
            Se.Settings.General.DialogStyle = originalDialogStyle;
        }
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

    private static ObservableCollection<SubtitleLineViewModel> MakeSubtitlesWithOverlapAfterMerge()
    {
        // Line 2 overlaps line 3, so merging 1+2 makes the merged line end after line 3 starts.
        return new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Number = 1,
                Text = "One",
                StartTime = TimeSpan.FromMilliseconds(500),
                EndTime = TimeSpan.FromMilliseconds(1500),
            },
            new()
            {
                Number = 2,
                Text = "Two",
                StartTime = TimeSpan.FromMilliseconds(1600),
                EndTime = TimeSpan.FromMilliseconds(3500),
            },
            new()
            {
                Number = 3,
                Text = "Three",
                StartTime = TimeSpan.FromMilliseconds(3000),
                EndTime = TimeSpan.FromMilliseconds(4000),
            },
        };
    }

    [Fact]
    public void MergeSelectedLines_ShouldTrimEndTimeToNextStart_ByDefault()
    {
        var mergeManager = new MergeManager();
        var subtitles = MakeSubtitlesWithOverlapAfterMerge();

        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1]]);

        Assert.Equal(2, subtitles.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(2999), subtitles[0].EndTime);
    }

    [Fact]
    public void MergeSelectedLines_ShouldKeepEndTime_WhenKeepEndTimeIsSet()
    {
        var mergeManager = new MergeManager();
        var subtitles = MakeSubtitlesWithOverlapAfterMerge();

        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1]], keepEndTime: true);

        Assert.Equal(2, subtitles.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(500), subtitles[0].StartTime);
        Assert.Equal(TimeSpan.FromMilliseconds(3500), subtitles[0].EndTime);
    }

    // The merged line must span every merged line, so the end time is the latest of them - not
    // the last one in selection order. An ASSA sign event that outlives a dialog line merged
    // into it is exactly the case the ASSA-only default targets, and taking "last" silently
    // truncated it.
    [Fact]
    public void MergeSelectedLines_ShouldKeepLatestEndTime_NotLastLineEndTime()
    {
        var mergeManager = new MergeManager();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new()
            {
                Number = 1,
                Text = "Sign that stays up",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(10),
            },
            new()
            {
                Number = 2,
                Text = "Short dialog",
                StartTime = TimeSpan.FromSeconds(2),
                EndTime = TimeSpan.FromSeconds(4),
            },
        };

        mergeManager.MergeSelectedLines(subtitles, [subtitles[0], subtitles[1]], keepEndTime: true);

        Assert.Single(subtitles);
        Assert.Equal(TimeSpan.Zero, subtitles[0].StartTime);
        Assert.Equal(TimeSpan.FromSeconds(10), subtitles[0].EndTime);
    }

    [Theory]
    [InlineData(false, true, true, false)] // setting off => never keep, even for ASSA
    [InlineData(false, false, false, false)]
    [InlineData(true, true, true, true)] // on + only-ASSA => keep for ASSA only
    [InlineData(true, true, false, false)]
    [InlineData(true, false, false, true)] // on + not limited to ASSA => keep for any format
    [InlineData(true, false, true, true)]
    public void ShouldKeepEndTime_FollowsSettingsAndFormat(bool keepEndTime, bool onlyAssa, bool isAssaFormat, bool expected)
    {
        var originalKeepEndTime = Se.Settings.Tools.MergeKeepEndTime;
        var originalOnlyAssa = Se.Settings.Tools.MergeKeepEndTimeOnlyAssa;
        try
        {
            Se.Settings.Tools.MergeKeepEndTime = keepEndTime;
            Se.Settings.Tools.MergeKeepEndTimeOnlyAssa = onlyAssa;
            var format = isAssaFormat ? (SubtitleFormat)new AdvancedSubStationAlpha() : new SubRip();

            Assert.Equal(expected, MergeManager.ShouldKeepEndTime(format));
        }
        finally
        {
            Se.Settings.Tools.MergeKeepEndTime = originalKeepEndTime;
            Se.Settings.Tools.MergeKeepEndTimeOnlyAssa = originalOnlyAssa;
        }
    }

    [Theory]
    [InlineData(true, MergeManager.BreakMode.Normal)]
    [InlineData(false, MergeManager.BreakMode.KeepBreaks)]
    public void DefaultBreakMode_FollowsMergeAutoBreakSetting(bool mergeAutoBreak, MergeManager.BreakMode expected)
    {
        var original = Se.Settings.Tools.MergeAutoBreak;
        try
        {
            Se.Settings.Tools.MergeAutoBreak = mergeAutoBreak;

            Assert.Equal(expected, MergeManager.DefaultBreakMode());
        }
        finally
        {
            Se.Settings.Tools.MergeAutoBreak = original;
        }
    }
}
