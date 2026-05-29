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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
    public void Split_TwoLineDialogText_WithTextIndex_StripsLeadingDashesFromBothParts()
    {
        // Arrange – two-line dialog, split at the text-box cursor position
        // (textIndex points between the two dialog lines).
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        Configuration.Settings.General.DialogStyle = DialogType.DashBothLinesWithSpace;
        var manager = new SplitManager();
        var text = $"- Hello there.{Environment.NewLine}- Hi back.";
        var subtitle = MakeSubtitle(text, 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };
        var cursor = text.IndexOf(Environment.NewLine, StringComparison.Ordinal) + Environment.NewLine.Length;

        // Act – split at video position + text-box cursor position
        manager.Split(subtitles, subtitle, videoPositionSeconds: 2.0, textIndex: cursor, languageCode: "en");

        // Assert – leading dialog dashes are stripped from both parts
        Assert.Equal(2, subtitles.Count);
        Assert.Equal("Hello there.", subtitles[0].Text);
        Assert.Equal("Hi back.", subtitles[1].Text);
    }

    [Fact]
    public void Split_TwoLineNonDialogText_KeepsBothLinesAsIs()
    {
        // Arrange – two plain lines with no leading dashes, so IsDialog returns false.
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
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
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($@"{{\b1}}First line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Contains(@"\b1}", subtitles[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Split_AssaBoldTagClosedInFirstLine_NotPropagatedToSecondLine()
    {
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($@"{{\b1}}First{{\b0}} line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.DoesNotContain(@"\b1}", subtitles[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Split_MultipleMixedTagsOpenInFirstLine_AllClosedAndReopened()
    {
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"<b><i>First line{Environment.NewLine}Second line</i></b>", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Contains("</b>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("</i>", subtitles[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<b>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<i>", subtitles[1].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Split_AutoSplit_LeavesFullMinimumBetweenLinesGap()
    {
        // Bug repro for #11245: with MinimumBetweenLines > 0, the gap between the two
        // halves was previously MinimumBetweenLines / 2. Auto-split (no video position)
        // must leave the full configured gap.
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 100;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"First line{Environment.NewLine}Second line", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Equal(2, subtitles.Count);
        var gapMs = subtitles[1].StartTime.TotalMilliseconds - subtitles[0].EndTime.TotalMilliseconds;
        Assert.Equal(100.0, gapMs, 1);
    }

    [Fact]
    public void Split_AutoSplit_EqualLengthHalves_DivideAtMidpoint()
    {
        // Balanced halves should still split at the literal midpoint, with the gap
        // straddling it (halfGap before, halfGap after).
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 80;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle($"Same len.{Environment.NewLine}Same len.", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        // Midpoint = 2000 ms; with 80 ms gap symmetric around it:
        //   first.End  = 1960 ms
        //   second.Start = 2040 ms
        Assert.Equal(2, subtitles.Count);
        Assert.Equal(1960.0, subtitles[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal(2040.0, subtitles[1].StartTime.TotalMilliseconds, 1);
    }

    [Fact]
    public void Split_AutoSplit_LongerFirstHalf_GetsProportionallyMoreDuration()
    {
        // SE 4 parity (#11245): when one half has materially more text, it should
        // get a proportionally larger share of the original duration, clamped to
        // [0.25, 0.75] so the smaller half never drops below a quarter.
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        var manager = new SplitManager();
        // ~70 chars vs ~10 chars — startFactor before clamp would be ~0.875, clamped
        // to 0.75. Original duration is 4 s, so middle ≈ start + 3.0 s.
        var first = "This is the much longer first half of the subtitle text content.";
        var second = "Tiny end.";
        var subtitle = MakeSubtitle($"{first}{Environment.NewLine}{second}", 1, 5);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Equal(2, subtitles.Count);
        var firstDurationMs = subtitles[0].Duration.TotalMilliseconds;
        var secondDurationMs = subtitles[1].Duration.TotalMilliseconds;
        Assert.True(firstDurationMs > secondDurationMs,
            $"Expected first half > second half; got {firstDurationMs}ms vs {secondDurationMs}ms");
        // Clamped startFactor of 0.75 means first half is ~3 s out of 4 s.
        Assert.InRange(firstDurationMs, 2900, 3100);
    }

    [Fact]
    public void Split_AutoSplit_LongerSecondHalf_GetsProportionallyMoreDuration()
    {
        // Mirror of the previous test: longer text on the second half gets the
        // proportionally larger duration.
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        var manager = new SplitManager();
        var first = "Tiny start.";
        var second = "This is the much longer second half of the subtitle text content.";
        var subtitle = MakeSubtitle($"{first}{Environment.NewLine}{second}", 1, 5);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, languageCode: "en");

        Assert.Equal(2, subtitles.Count);
        var firstDurationMs = subtitles[0].Duration.TotalMilliseconds;
        var secondDurationMs = subtitles[1].Duration.TotalMilliseconds;
        Assert.True(secondDurationMs > firstDurationMs,
            $"Expected second half > first half; got {secondDurationMs}ms vs {firstDurationMs}ms");
        // Clamped startFactor of 0.25 means first half is ~1 s out of 4 s.
        Assert.InRange(firstDurationMs, 900, 1100);
    }

    [Fact]
    public void Split_WithTextIndex_AutoBreaksHalvesThatExceedMaxLineLength()
    {
        // SE 4 parity (#11245 follow-up): cursor-splitting a long single-line
        // subtitle should auto-break either half if it's still over the configured
        // single-line max. Each half here is 39 chars (above MergeLinesShorterThan=33,
        // so AutoBreakLine will actually fold them) and over the configured max of 20.
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        Configuration.Settings.General.SubtitleLineMaximumLength = 20;
        Configuration.Settings.General.MergeLinesShorterThan = 33;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle(
            "AAAAAAAAA BBBBBBBBB CCCCCCCCC DDDDDDDDD EEEEEEEEE FFFFFFFFF GGGGGGGGG HHHHHHHHH",
            1, 5);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        // Cursor at the space between DDDDDDDDD and EEEEEEEEE (index 40).
        manager.Split(subtitles, subtitle, textIndex: 40, languageCode: "en");

        Assert.Equal(2, subtitles.Count);
        Assert.Contains(Environment.NewLine, subtitles[0].Text);
        Assert.Contains(Environment.NewLine, subtitles[1].Text);
        // No individual line on either side exceeds the configured max.
        Assert.All(subtitles[0].Text.Split(Environment.NewLine),
            line => Assert.True(line.Length <= 20, $"'{line}' exceeds 20 chars"));
        Assert.All(subtitles[1].Text.Split(Environment.NewLine),
            line => Assert.True(line.Length <= 20, $"'{line}' exceeds 20 chars"));
    }

    [Fact]
    public void Split_WithTextIndex_ShortHalves_NoAutoBreak()
    {
        // Counter-test: when both halves fit on a single line, the existing line
        // breaks (here: none) are preserved.
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 0;
        Configuration.Settings.General.SubtitleLineMaximumLength = 40;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle("Hello world", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, textIndex: 5, languageCode: "en");

        Assert.Equal(2, subtitles.Count);
        Assert.Equal("Hello", subtitles[0].Text);
        Assert.Equal("world", subtitles[1].Text);
    }

    [Fact]
    public void Split_WithVideoPosition_LeavesFullMinimumBetweenLinesGap()
    {
        // User-chosen video frame becomes the new line's start; the old line ends
        // one full MinGap earlier (mirrors SE 4 SplitBehavior == 0).
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 100;
        var manager = new SplitManager();
        var subtitle = MakeSubtitle("Hello world", 1, 3);
        var subtitles = new ObservableCollection<SubtitleLineViewModel> { subtitle };

        manager.Split(subtitles, subtitle, videoPositionSeconds: 2.0, languageCode: "en");

        Assert.Equal(2, subtitles.Count);
        Assert.Equal(2000.0, subtitles[1].StartTime.TotalMilliseconds, 1);
        Assert.Equal(1900.0, subtitles[0].EndTime.TotalMilliseconds, 1);
        var gapMs = subtitles[1].StartTime.TotalMilliseconds - subtitles[0].EndTime.TotalMilliseconds;
        Assert.Equal(100.0, gapMs, 1);
    }
}
