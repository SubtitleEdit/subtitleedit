using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

[Collection("NonParallelTests")]
public class UnknownSubtitle108Test
{
    private static Subtitle Load(string text)
    {
        var lines = text.SplitToLines();
        var format = new UnknownSubtitle108();
        Assert.True(format.IsMine(lines, "test.txt"));
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, "test.txt");
        return subtitle;
    }

    [Fact]
    public void LoadsPlainTabExport()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var subtitle = Load(
                "1\t00:02:10:01\tFirst line one,\r\n" +
                "\t00:02:15:03\tfirst line two\r\n" +
                "\t\t\r\n" +
                "2\t00:02:23:14\t\r\n" +
                "\t00:02:26:03\tSingle line\r\n" +
                "\t\t\r\n" +
                "3\t00:03:23:03\tThird one\r\n" +
                "\t00:03:27:22\tand two\r\n");

            Assert.Equal(3, subtitle.Paragraphs.Count);
            Assert.Equal("First line one," + Environment.NewLine + "first line two", subtitle.Paragraphs[0].Text);
            Assert.Equal(new TimeCode(0, 2, 10, 40).TotalMilliseconds, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(new TimeCode(0, 2, 15, 120).TotalMilliseconds, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal("Single line", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Fact]
    public void LoadsWordTableWithDurationColumnAndQuirks()
    {
        var subtitle = Load(
            "No.\tTimecode\tEr.\tDur.\tSubtitle text\n" +
            "\n" +
            "1\t00:02:10:01\t5:02\tFirst line one,\n" +
            "\t00:02:15:03\t\tfirst line two\n" +
            "\n" +
            "2\t00:05:11:23\t1:24\n" +
            "\t00:05:13:22\t\tSingle line\n" +
            "\n" +
            "3 \t00:15:36:04       W         4:15\tWord… marker\n" +
            "\t00:15:40:19\t\tsecond\n" +
            "\n" +
            "4\t01:03:02.13\t2:11\t\n" +
            "\t01:03:04:24\t\tDot frames\n" +
            "\n" +
            "5\t00:00:00:00        W\t\t\n" +
            "\t00:00:00:00\n");

        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal("First line one," + Environment.NewLine + "first line two", subtitle.Paragraphs[0].Text);
        Assert.Equal("Single line", subtitle.Paragraphs[1].Text);
        Assert.Equal("Word… marker" + Environment.NewLine + "second", subtitle.Paragraphs[2].Text);
        Assert.Equal("Dot frames", subtitle.Paragraphs[3].Text);
        Assert.Equal(3, subtitle.Paragraphs[3].StartTime.Minutes);
        Assert.Equal(2, subtitle.Paragraphs[3].StartTime.Seconds);
    }

    [Fact]
    public void RoundTrip()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("One line", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Line one" + Environment.NewLine + "Line two", 4000, 6000));
        var text = new UnknownSubtitle108().ToText(subtitle, "test");

        var loaded = Load(text);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("One line", loaded.Paragraphs[0].Text);
        Assert.Equal("Line one" + Environment.NewLine + "Line two", loaded.Paragraphs[1].Text);
        Assert.InRange(loaded.Paragraphs[1].StartTime.TotalMilliseconds, 3950, 4050);
    }
}
