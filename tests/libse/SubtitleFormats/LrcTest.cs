using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class LrcTest
{
    [Fact]
    public void TimeCodeRoundingLrc()
    {
        const string input = @"1
00:00:59,999 --> 00:01:02,080
I wasn't in love with him.

2
00:01:02,600 --> 00:01:03,850
I know everyone thought IRT was.";
        var srt = new SubRip();
        var lrc = new Lrc();
        var subtitle = new Subtitle();
        srt.LoadSubtitle(subtitle, new List<string>(input.SplitToLines()), null);
        var text = subtitle.ToText(lrc);

        Assert.Contains("[01:00.00]I wasn't in love with him.", text);
    }

    [Fact]
    public void LoadText()
    {
        const string input = @"﻿[00:23.02]Lyric 01
[00:25.61]Lyric 02
[00:30.24]
[00:34.07]Lyric 03
[00:40.02]
[00:43.75]Lyric 04
[00:49.45]";
        var lrc = new Lrc();
        var subtitle = new Subtitle();
        lrc.LoadSubtitle(subtitle, new List<string>(input.SplitToLines()), null);
        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal("Lyric 01", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void HeaderFromFileIsWrittenUnchanged()
    {
        const string input = @"[ti:Title1]
[ar:Artist1]
[al:]
[re:Other Editor]

[00:14.95]Lyric 01
[00:19.73]Lyric 02
[00:24.58]";
        foreach (var format in new SubtitleFormat[] { new Lrc(), new LrcNoEndTime(), new Lrc3DigitsMs() })
        {
            var lines = format is Lrc3DigitsMs ? input.Replace(".95]", ".950]").Replace(".73]", ".730]").Replace(".58]", ".580]") : input;
            var subtitle = new Subtitle();
            format.LoadSubtitle(subtitle, new List<string>(lines.SplitToLines()), null);
            Assert.Equal(2, subtitle.Paragraphs.Count);
            var text = subtitle.ToText(format);

            Assert.StartsWith("[ti:Title1]" + Environment.NewLine +
                              "[ar:Artist1]" + Environment.NewLine +
                              "[al:]" + Environment.NewLine +
                              "[re:Other Editor]" + Environment.NewLine +
                              Environment.NewLine + "[00:14.", text);
            Assert.DoesNotContain("[ve:", text);
        }
    }

    [Fact]
    public void DefaultHeaderWhenNoLrcTags()
    {
        const string input = @"1
00:00:01,000 --> 00:00:02,000
Hello";
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, new List<string>(input.SplitToLines()), null);
        subtitle.Header = "[Script Info]" + Environment.NewLine + "Title: x";
        var text = new Lrc().ToText(subtitle, "My song");

        Assert.StartsWith("[ti:My song]" + Environment.NewLine + "[re: Subtitle Edit]" + Environment.NewLine + "[ve: ", text);
        Assert.DoesNotContain("Script Info", text);
    }
}
