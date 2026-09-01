using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class JsonType24Test
{
    private const string Sample =
        "[{\"id\":13077087,\"start_time\":192665,\"end_time\":193887,\"subtitle_language_code\":\"en\"," +
        "\"subtitle_id\":27992065,\"subtitle_content\":\"Hi. What would you like to eat?\"}," +
        "{\"id\":13077088,\"start_time\":194000,\"end_time\":196500,\"subtitle_language_code\":\"en\"," +
        "\"subtitle_id\":27992066,\"subtitle_content\":\"I would like a pizza.\"}]";

    private static List<string> Lines(string s) => new List<string> { s };

    [Fact]
    public void LoadSubtitleReadsMillisecondsAndSubtitleContent()
    {
        var format = new JsonType24();
        var subtitle = new Subtitle();

        Assert.True(format.IsMine(Lines(Sample), null));
        format.LoadSubtitle(subtitle, Lines(Sample), null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hi. What would you like to eat?", subtitle.Paragraphs[0].Text);
        Assert.Equal(192665, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(193887, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("I would like a pizza.", subtitle.Paragraphs[1].Text);
        Assert.Equal(196500, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    /// <summary>JSON Type 8 also uses start_time/end_time, but in seconds and with a "text" tag.</summary>
    [Fact]
    public void JsonType8FilesAreNotClaimed()
    {
        var jsonType8 = "[{\"start_time\":1.5,\"end_time\":3.75,\"text\":\"Hello\"}]";
        Assert.False(new JsonType24().IsMine(Lines(jsonType8), null));
    }

    [Fact]
    public void RoundTripKeepsTextAndTimes()
    {
        var format = new JsonType24();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello \"world\"", 192665, 193887));

        var text = format.ToText(subtitle, "title");
        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, text.SplitToLines(), null);

        Assert.Single(reloaded.Paragraphs);
        Assert.Equal("Hello \"world\"", reloaded.Paragraphs[0].Text);
        Assert.Equal(192665, reloaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(193887, reloaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void AutoDetectPicksJsonType24()
    {
        var lines = Lines(Sample);
        var winner = SubtitleFormat.AllSubtitleFormats.First(f => f.IsTextBased && f.IsMine(lines, "sample.json"));
        Assert.Equal(new JsonType24().Name, winner.Name);
    }
}
