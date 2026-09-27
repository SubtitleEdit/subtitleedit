using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// A transcript copied from YouTube can have the time code in markdown bold ("**0:07**") followed by
/// the time code spelled out in the UI language ("7 seconds", here in Burmese with Myanmar digits).
/// </summary>
public class YouTubeTranscriptMarkdownTest
{
    private static readonly List<string> MarkdownBurmese = new List<string>
    {
        "**0:07**",
        "၇ စက္ကန့်",
        "",
        "Assistant Gu.",
        "",
        "**1:00**",
        "",
        "၁ မိနစ်",
        "",
        "Go find her.",
        "",
        "**2:02:14**",
        "၂ နာရီ၊ ၂ မိနစ်၊ ၁၄ စက္ကန့်",
        "Could it be my child?",
    };

    private static Subtitle Load(List<string> lines)
    {
        var subtitle = new Subtitle();
        new YouTubeTranscript().LoadSubtitle(subtitle, lines, "transcript.txt");
        return subtitle;
    }

    [Fact]
    public void MarkdownBoldIsAutoDetected()
    {
        var format = SubtitleFormat.AllSubtitleFormats.First(f => f.IsTextBased && f.IsMine(MarkdownBurmese, "transcript.txt"));
        Assert.IsType<YouTubeTranscript>(format);
    }

    [Fact]
    public void MarkdownBoldTimeCodesAndSpokenTimeCodeLinesAreSkipped()
    {
        var subtitle = Load(MarkdownBurmese);

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(7_000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal("Assistant Gu.", subtitle.Paragraphs[0].Text);
        Assert.Equal(60_000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal("Go find her.", subtitle.Paragraphs[1].Text);
        Assert.Equal(7_334_000, subtitle.Paragraphs[2].StartTime.TotalMilliseconds);
        Assert.Equal("Could it be my child?", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void EnglishSpokenTimeCodeIsSkipped()
    {
        var subtitle = Load(new List<string> { "0:07", "7 seconds", "Hello there.", "1:05", "1 minute, 5 seconds", "Bye." });

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
        Assert.Equal("Bye.", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void TextThatLooksLikeSpokenTimeCodeIsKeptWhenItIsTheOnlyText()
    {
        var subtitle = Load(new List<string> { "0:07", "7 people", "0:09", "Hello there." });

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("7 people", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void PlainTranscriptIsUnchanged()
    {
        var subtitle = Load(new List<string> { "0:01", "First line", "0:04", "Second line", "1:02:03", "Third 5 line" });

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("First line", subtitle.Paragraphs[0].Text);
        Assert.Equal("Second line", subtitle.Paragraphs[1].Text);
        Assert.Equal(3_723_000, subtitle.Paragraphs[2].StartTime.TotalMilliseconds);
        Assert.Equal("Third 5 line", subtitle.Paragraphs[2].Text);
    }
}
