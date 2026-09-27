using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class CheetahCaptionAscTest
{
    private const string Sample =
        "*DropFrame\r\n" +
        "*WIDTH 32\r\n" +
        "\r\n" +
        "** Caption Number 1\r\n" +
        "*PopOn\r\n" +
        "*T 01:00:14:13\r\n" +
        "*E 01:00:16:13\r\n" +
        "*BottomUp\r\n" +
        "*Cf16\r\n" +
        "[SCREAM]\\E\r\n" +
        "\r\n" +
        "** Caption Number 2\r\n" +
        "*PopOn\r\n" +
        "*T 01:00:28:11\r\n" +
        "*BottomUp\r\n" +
        "*Lf07\r\n" +
        "IN PURSUIT\r\n" +
        "OF A RUNAWAY VEHICLE.\\E\r\n" +
        "\r\n" +
        "** Caption Number 3\r\n" +
        "*PopOn\r\n" +
        "*T 01:00:31:13\r\n" +
        "*E 01:00:33:12\r\n" +
        "*BottomUp\r\n" +
        "*Cf16\r\n" +
        "AGH!\\E\r\n";

    private static List<string> Lines(string s) => s.SplitToLines();

    [Fact]
    public void LoadReadsCaptionsAndStripsEndMarker()
    {
        var format = new CheetahCaptionAsc();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(Lines(Sample), "a.asc"));
        format.LoadSubtitle(subtitle, Lines(Sample), "a.asc");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("[SCREAM]", subtitle.Paragraphs[0].Text);
        Assert.Equal(new TimeCode(1, 0, 14, SubtitleFormat.FramesToMillisecondsMax999(13)).TotalMilliseconds, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(new TimeCode(1, 0, 16, SubtitleFormat.FramesToMillisecondsMax999(13)).TotalMilliseconds, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("IN PURSUIT" + System.Environment.NewLine + "OF A RUNAWAY VEHICLE.", subtitle.Paragraphs[1].Text);
        Assert.Equal("AGH!", subtitle.Paragraphs[2].Text);
    }

    /// <summary>A pop-on caption without "*E" stays up until the next caption replaces it.</summary>
    [Fact]
    public void MissingEndTimeRunsUntilNextCaption()
    {
        var subtitle = new Subtitle();
        new CheetahCaptionAsc().LoadSubtitle(subtitle, Lines(Sample), "a.asc");

        var expectedEnd = subtitle.Paragraphs[2].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
        Assert.Equal(expectedEnd, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void RoundTripKeepsTextAndTimes()
    {
        var format = new CheetahCaptionAsc();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello" + System.Environment.NewLine + "<i>world</i>", 3_600_000, 3_602_000));
        subtitle.Paragraphs.Add(new Paragraph("Bye", 3_603_000, 3_604_000));

        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, Lines(format.ToText(subtitle, "title")), "a.asc");

        Assert.Equal(2, reloaded.Paragraphs.Count);
        Assert.Equal("Hello" + System.Environment.NewLine + "world", reloaded.Paragraphs[0].Text);
        Assert.Equal(3_600_000, reloaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3_602_000, reloaded.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Bye", reloaded.Paragraphs[1].Text);
    }

    /// <summary>Unknown 30 is another ".asc" format - it must not be claimed.</summary>
    [Fact]
    public void OtherAscFormatIsNotClaimed()
    {
        var unknown30 = "@ headers\r\n\r\n1.\r\n00:00:04:12\r\n00:00:06:05\r\nHello.\r\n";
        Assert.False(new CheetahCaptionAsc().IsMine(Lines(unknown30), "a.asc"));
    }
}
