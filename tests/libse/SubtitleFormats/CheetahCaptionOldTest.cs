using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;

namespace LibSETests.SubtitleFormats;

public class CheetahCaptionOldTest
{
    [Fact]
    public void SaveAndLoadSingleLine()
    {
        var target = new CheetahCaptionOld();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000));

        var ms = new MemoryStream();
        target.Save("test.cap", ms, subtitle, false);

        var reload = new Subtitle();
        target.LoadSubtitle(reload, ms.ToArray());

        Assert.Single(reload.Paragraphs);
        Assert.Equal("Hello world", reload.Paragraphs[0].Text);
    }

    [Fact]
    public void SaveAndLoadTwoLines()
    {
        var target = new CheetahCaptionOld();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Line one\nLine two", 2000, 5000));

        var ms = new MemoryStream();
        target.Save("test.cap", ms, subtitle, false);

        var reload = new Subtitle();
        target.LoadSubtitle(reload, ms.ToArray());

        Assert.Single(reload.Paragraphs);
        Assert.Contains("Line one", reload.Paragraphs[0].Text);
        Assert.Contains("Line two", reload.Paragraphs[0].Text);
    }

    [Fact]
    public void SaveAndLoadTimecodes()
    {
        var target = new CheetahCaptionOld();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Test", 3_661_000, 3_662_000)); // 1:01:01 to 1:01:02

        var ms = new MemoryStream();
        target.Save("test.cap", ms, subtitle, false);

        var reload = new Subtitle();
        target.LoadSubtitle(reload, ms.ToArray());

        Assert.Single(reload.Paragraphs);
        Assert.Equal(1, reload.Paragraphs[0].StartTime.Hours);
        Assert.Equal(1, reload.Paragraphs[0].StartTime.Minutes);
        Assert.Equal(1, reload.Paragraphs[0].StartTime.Seconds);
    }

    [Fact]
    public void SaveAndLoadMultipleParagraphs()
    {
        var target = new CheetahCaptionOld();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("Second", 2000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Third", 4000, 5000));

        var ms = new MemoryStream();
        target.Save("test.cap", ms, subtitle, false);

        var reload = new Subtitle();
        target.LoadSubtitle(reload, ms.ToArray());

        Assert.Equal(3, reload.Paragraphs.Count);
        Assert.Equal("First", reload.Paragraphs[0].Text);
        Assert.Equal("Second", reload.Paragraphs[1].Text);
        Assert.Equal("Third", reload.Paragraphs[2].Text);
    }

    [Fact]
    public void SaveWritesCorrectMagicBytes()
    {
        var target = new CheetahCaptionOld();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Test", 0, 1000));

        var ms = new MemoryStream();
        target.Save("test.cap", ms, subtitle, false);

        var bytes = ms.ToArray();
        Assert.Equal(0xEA, bytes[0]);
        Assert.Equal(0x10, bytes[1]);
    }

    [Fact]
    public void SaveHtmlTagsAreStripped()
    {
        var target = new CheetahCaptionOld();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>Italic text</i>", 0, 1000));

        var ms = new MemoryStream();
        target.Save("test.cap", ms, subtitle, false);

        var reload = new Subtitle();
        target.LoadSubtitle(reload, ms.ToArray());

        Assert.Equal("Italic text", reload.Paragraphs[0].Text);
    }
}
