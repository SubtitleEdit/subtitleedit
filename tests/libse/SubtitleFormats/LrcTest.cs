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
}