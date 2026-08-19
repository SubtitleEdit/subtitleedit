using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class TimedTextBase64ImageTest
{
    /// <summary>
    /// SMPTE-TT bitmap captions as produced in the wild (e.g. VLC's ttml_bitmap.ttml sample):
    /// the timed divs sit inside a plain wrapper div, so index-based image/div pairing loses
    /// the first caption and mispairs the rest - pairing must resolve the
    /// smpte:backgroundImage="#id" fragment reference instead.
    /// </summary>
    [Fact]
    public void LoadSubtitleResolvesFragmentReferencesInsideWrapperDiv()
    {
        var format = new TimedTextBase64Image();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<tt xml:lang=\"\" xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:smpte=\"http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt\">",
            "<head>",
            "  <metadata>",
            "    <smpte:image xml:id=\"img_1\" imagetype=\"PNG\" encoding=\"Base64\">Rmlyc3Q=</smpte:image>",
            "    <smpte:image xml:id=\"img_2\" imagetype=\"PNG\" encoding=\"Base64\">U2Vjb25k</smpte:image>",
            "  </metadata>",
            "</head>",
            "<body>",
            "<div>",
            "<div region=\"speaker\" begin=\"00:00:00.000\" end=\"00:00:05.619\" smpte:backgroundImage=\"#img_1\" ></div>",
            "<div region=\"speaker\" begin=\"00:00:05.619\" end=\"00:00:12.000\" smpte:backgroundImage=\"#img_2\" ></div>",
            "</div>",
            "</body>",
            "</tt>",
        };

        Assert.True(format.IsMine(lines, null));
        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Rmlyc3Q=", subtitle.Paragraphs[0].Text);
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(5619, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("U2Vjb25k", subtitle.Paragraphs[1].Text);
        Assert.Equal(5619, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(12000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void LoadSubtitleResolvesReferencesWithOtherNamespacePrefix()
    {
        var format = new TimedTextBase64Image();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:m=\"http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt\">",
            "<head><metadata>",
            "  <m:image xml:id=\"i1\" imagetype=\"PNG\" encoding=\"Base64\">Rmlyc3Q=</m:image>",
            "</metadata></head>",
            "<body><div>",
            "<div begin=\"00:00:01.000\" end=\"00:00:02.000\" m:backgroundImage=\"#i1\" ></div>",
            "</div></body>",
            "</tt>",
        };

        // the top-level string guard checks for the canonical "smpte:" prefix, so give it one
        lines.Insert(1, "<!-- smpte:backgroundImage smpte:image imagetype= -->");

        format.LoadSubtitle(subtitle, lines, null);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Rmlyc3Q=", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }
}
