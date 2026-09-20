using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class QubeMasterImportTest
{
    [Fact]
    public void NumberedEntriesGoToUnknown64NotQubeMaster()
    {
        // doom9 thread 114440: counter, in point, out point, then the text. QubeMasterPro's
        // reader takes any non-timecode line as text, so it used to win detection and glue the
        // counter in front of every subtitle.
        const string text = "0\r\n00:00:00:00\r\n00:00:00:08\r\nTITLE\r\n" +
                            "\r\n1\r\n10:00:32:07\r\n10:00:38:04\r\nFirst subtitle.\r\n" +
                            "\r\n2\r\n10:00:38:07\r\n10:00:45:19\r\nSecond subtitle.\r\n" +
                            "\r\n3\r\n10:00:46:07\r\n10:00:50:19\r\nThird subtitle.\r\n";
        var lines = text.SplitToLines();

        Assert.False(new QubeMasterImport().IsMine(lines, "subs.txt"));

        foreach (var format in SubtitleFormat.AllSubtitleFormats)
        {
            if (format.IsTextBased && format.IsMine(lines, "subs.txt"))
            {
                Assert.Equal(new UnknownSubtitle64().Name, format.Name);
                var subtitle = new Subtitle();
                format.LoadSubtitle(subtitle, lines, "subs.txt");
                Assert.Equal("TITLE", subtitle.Paragraphs[0].Text);
                Assert.Equal("First subtitle.", subtitle.Paragraphs[1].Text);
                break;
            }
        }
    }

    [Fact]
    public void RealQubeMasterFilesStillLoad()
    {
        var format = new QubeMasterImport();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("SubLine1" + Environment.NewLine + "SubLine2", 3612000, 3615000));
        subtitle.Paragraphs.Add(new Paragraph("Another line", 3620000, 3623000));

        var lines = format.ToText(subtitle, "title").SplitToLines();
        var loaded = new Subtitle();

        Assert.True(format.IsMine(lines, "subs.txt"));
        format.LoadSubtitle(loaded, lines, "subs.txt");

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("SubLine1" + Environment.NewLine + "SubLine2", loaded.Paragraphs[0].Text);
        Assert.Equal("Another line", loaded.Paragraphs[1].Text);
    }
}
