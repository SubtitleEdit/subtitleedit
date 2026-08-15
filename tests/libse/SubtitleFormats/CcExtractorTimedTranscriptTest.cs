using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class CcExtractorTimedTranscriptTest
{
    [Fact]
    public void LoadSubtitleMergesLinesWithSameTimeCodes()
    {
        var format = new CcExtractorTimedTranscript();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "00:00:01,886|00:00:03,820|POP|e guests of Bunny Saunders.",
            "00:00:05,489|00:00:07,357|POP|  Very young, of course, to be",
            "00:00:05,489|00:00:07,357|POP|  directing his first picture.",
        };

        Assert.True(format.IsMine(lines, null));
        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("e guests of Bunny Saunders.", subtitle.Paragraphs[0].Text);
        Assert.Equal(1886, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3820, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Very young, of course, to be" + Environment.NewLine + "directing his first picture.", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void RoundTripKeepsTextAndTimes()
    {
        var format = new CcExtractorTimedTranscript();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello" + Environment.NewLine + "world", 1886, 3820));

        var text = format.ToText(subtitle, "title");
        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, new List<string>(text.SplitToLines()), null);

        Assert.Single(reloaded.Paragraphs);
        Assert.Equal("Hello" + Environment.NewLine + "world", reloaded.Paragraphs[0].Text);
        Assert.Equal(1886, reloaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3820, reloaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void IsMineRejectsSubRip()
    {
        var format = new CcExtractorTimedTranscript();
        var lines = new List<string>
        {
            "1",
            "00:00:01,886 --> 00:00:03,820",
            "Hello world",
        };

        Assert.False(format.IsMine(lines, null));
    }
}
