using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// YouTube timed text (srv3 / .ytt) is read by the YouTubeTimedText format, but
/// <see cref="TimedTextNoNs"/> is earlier in the format list and used to claim it first:
/// its IsMine accepts any namespace-less XML with a body and a p element. Since real
/// srv3 keeps its text in nested s elements, that produced the worst possible result -
/// the right number of paragraphs with empty text and default time codes, rather than
/// an honest "unknown format".
/// </summary>
public class YouTubeTimedTextShadowingTest
{
    /// <summary>As emitted by yt-dlp --sub-format srv3: text split over several timed s runs.</summary>
    private const string RealWorldSrv3 =
        "<?xml version=\"1.0\" encoding=\"utf-8\" ?><timedtext format=\"3\"><head>" +
        "<ws id=\"0\"/><wp id=\"0\"/><pen id=\"0\" sc=\"#FFFFFF\"/></head><body>" +
        "<p t=\"1500\" d=\"2250\"><s ac=\"212\">Hello</s><s t=\"300\" ac=\"240\"> there,</s><s t=\"700\" ac=\"255\"> world.</s></p>" +
        "<p t=\"4000\" d=\"2500\"><s ac=\"200\">Second caption line.</s></p></body></timedtext>";

    private static List<string> Lines(string s) => new List<string> { s };

    [Fact]
    public void TimedTextNoNsDoesNotClaimYouTubeTimedText()
    {
        Assert.False(new TimedTextNoNs().IsMine(Lines(RealWorldSrv3), "sample.ytt"));
    }

    [Fact]
    public void AutoDetectReadsSrv3TextAndTimes()
    {
        var lines = Lines(RealWorldSrv3);
        var format = SubtitleFormat.AllSubtitleFormats.First(f => f.IsTextBased && f.IsMine(lines, "sample.ytt"));
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, "sample.ytt");

        Assert.Equal("YouTube timed text srv3", format.Name); // the format class is internal to LibSE
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello there, world.", subtitle.Paragraphs[0].Text);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3750, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Second caption line.", subtitle.Paragraphs[1].Text);
        Assert.Equal(6500, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    /// <summary>
    /// The format was called "Unknown 82" before it was named. Settings store formats by
    /// name, so the old one has to keep resolving or an existing user loses it from their
    /// default format and favorites.
    /// </summary>
    [Fact]
    public void TheOldUnknown82NameStillResolves()
    {
        Assert.Equal("YouTube timed text srv3", SubtitleFormat.FromName("Unknown 82", new SubRip()).Name);
        Assert.Equal("YouTube timed text srv3", Utilities.GetSubtitleFormatByFriendlyName("Unknown 82").Name);
    }

    /// <summary>The guard must not cost us plain namespace-less TTML.</summary>
    [Fact]
    public void PlainNamespacelessTimedTextIsStillMine()
    {
        var ttml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><tt><body><div>" +
                   "<p begin=\"00:00:01.500\" end=\"00:00:03.750\">Hello there, world.</p>" +
                   "</div></body></tt>";
        var format = new TimedTextNoNs();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(Lines(ttml), "sample.xml"));
        format.LoadSubtitle(subtitle, Lines(ttml), "sample.xml");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello there, world.", subtitle.Paragraphs[0].Text);
    }
}
