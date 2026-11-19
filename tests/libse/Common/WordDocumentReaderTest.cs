using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Common;

[Collection("NonParallelTests")]
public class WordDocumentReaderTest
{
    private static string FixturePath()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Files", "unknown108_word_table.doc");
        return File.Exists(path) ? path : Path.Combine("Files", "unknown108_word_table.doc");
    }

    [Fact]
    public void ReadsWordTableAsTabLines()
    {
        var lines = WordDocumentReader.GetLines(FixturePath());

        Assert.NotNull(lines);
        Assert.Contains("1\t00:00:01:00\t2:12\tHello there…", lines);
        Assert.Contains("\t00:00:03:12\t\t– General test line", lines);
        Assert.Contains("\t00:00:05:00\t\tOnly one line (ćčž)", lines);
    }

    [Fact]
    public void UnknownSubtitle108LoadsWordDocument()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var subtitle = Subtitle.Parse(FixturePath());

            Assert.NotNull(subtitle);
            Assert.IsType<UnknownSubtitle108>(subtitle.OriginalFormat);
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("Hello there…" + Environment.NewLine + "– General test line", subtitle.Paragraphs[0].Text);
            Assert.Equal(3480, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal("Only one line (ćčž)", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Fact]
    public void NotWordDocument()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".doc");
        try
        {
            File.WriteAllText(path, "1\t00:00:01:00\tText\r\n\t00:00:02:00\tmore\r\n");
            Assert.False(WordDocumentReader.IsWordDocument(path));
            Assert.Null(WordDocumentReader.GetLines(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void BrokenCompoundFileReturnsNull()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".doc");
        try
        {
            var bytes = new byte[4096];
            new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }.CopyTo(bytes, 0);
            bytes[0x1E] = 9; // 512 byte sectors, everything else garbage/zero
            File.WriteAllBytes(path, bytes);

            Assert.True(WordDocumentReader.IsWordDocument(path));
            Assert.Null(WordDocumentReader.GetLines(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FieldCodesAreDropped()
    {
        var lines = WordDocumentReader.ToLines("Page \u0013 PAGE \u00145\u0015 end\r");

        Assert.Equal(new List<string> { "Page 5 end" }, lines);
    }
}
