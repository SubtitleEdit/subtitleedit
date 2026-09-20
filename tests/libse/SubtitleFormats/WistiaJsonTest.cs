using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Linq;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class WistiaJsonTest
{
    private const string Sample = "{\"captions\":[{\"id\":\"z0ezew3p4xlhrix1\",\"bcp47LanguageTag\":\"en\",\"familyName\":\"English\"," +
                                  "\"hasCaptions\":true,\"mediaHashedId\":\"v5r9kwfqn0\",\"wistiaLanguageCode\":\"eng\"," +
                                  "\"hash\":{\"lines\":[" +
                                  "{\"start\":0.17,\"end\":0.62,\"text\":[\"Hello there,\",\"and welcome.\"]}," +
                                  "{\"start\":5.03,\"end\":9.92,\"text\":[\"A \\\"quoted\\\" line.\"]}," +
                                  "{\"start\":10.5,\"end\":12,\"text\":[\"Last one.\"]}" +
                                  "]}}]}";

    [Fact]
    public void LoadSubtitleReadsSecondsAndTextArray()
    {
        var format = new WistiaJson();
        var lines = Sample.SplitToLines();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(lines, "captions.json"));
        format.LoadSubtitle(subtitle, lines, "captions.json");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Hello there," + Environment.NewLine + "and welcome.", subtitle.Paragraphs[0].Text);
        Assert.Equal(170, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(620, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("A \"quoted\" line.", subtitle.Paragraphs[1].Text);
        Assert.Equal(5030, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(12000, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void OnlyTheFirstCaptionTrackIsRead()
    {
        const string twoTracks = "{\"captions\":[" +
                                 "{\"bcp47LanguageTag\":\"en\",\"hash\":{\"lines\":[{\"start\":1,\"end\":2,\"text\":[\"English\"]}]}}," +
                                 "{\"bcp47LanguageTag\":\"de\",\"hash\":{\"lines\":[{\"start\":1,\"end\":2,\"text\":[\"Deutsch\"]}]}}]}";
        var subtitle = new Subtitle();

        new WistiaJson().LoadSubtitle(subtitle, twoTracks.SplitToLines(), "captions.json");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("English", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void RoundTrip()
    {
        var format = new WistiaJson();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Line one" + Environment.NewLine + "line two", 1500, 3250));
        subtitle.Paragraphs.Add(new Paragraph("He said \"hi\".", 4000, 5000));

        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, format.ToText(subtitle, "title").SplitToLines(), "captions.json");

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Line one" + Environment.NewLine + "line two", loaded.Paragraphs[0].Text);
        Assert.Equal(1500, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3250, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("He said \"hi\".", loaded.Paragraphs[1].Text);
    }

    [Fact]
    public void OtherJsonIsNotClaimed()
    {
        var format = new WistiaJson();

        Assert.False(format.IsMine("{\"lines\":[{\"start\":1,\"end\":2,\"text\":\"no captions here\"}]}".SplitToLines(), "x.json"));
        Assert.False(format.IsMine(new JsonType23().ToText(new Subtitle(new[] { new Paragraph("Hi", 0, 1000) }.ToList()), "t").SplitToLines(), "x.json"));
    }
}
