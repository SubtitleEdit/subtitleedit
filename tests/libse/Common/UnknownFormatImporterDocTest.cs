using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Common;

public class UnknownFormatImporterDocTest
{
    private static string FixturePath(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Files", name);
        return File.Exists(path) ? path : Path.Combine("Files", name);
    }

    private static Subtitle Import(string name)
    {
        var fileName = FixturePath(name);
        var rawLines = File.ReadAllText(fileName).SplitToLines(); // what the UI passes: the binary as text
        return new UnknownFormatImporter().AutoGuessImport(rawLines, fileName);
    }

    [Fact]
    public void SubRipPastedIntoWordLoadsAsSubRip()
    {
        var subtitle = Import("unknown_importer_srt.doc");

        Assert.IsType<SubRip>(subtitle.OriginalFormat);
        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Hello from Word.", subtitle.Paragraphs[0].Text);
        Assert.Equal("Second line" + Environment.NewLine + "with two rows.", subtitle.Paragraphs[1].Text);
        Assert.Equal(6500, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal("Æble på bordet.", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void LooseTimeCodeListInWordIsGuessed()
    {
        var subtitle = Import("unknown_importer_loose.doc");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal("Hello there", subtitle.Paragraphs[0].Text);
        Assert.Equal(7250, subtitle.Paragraphs[2].StartTime.TotalMilliseconds);
        Assert.Equal("Third cue", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void BrokenWordDocumentGivesEmptySubtitle()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".doc");
        try
        {
            var bytes = new byte[4096];
            new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }.CopyTo(bytes, 0);
            bytes[0x1E] = 9;
            File.WriteAllBytes(path, bytes);

            var subtitle = new UnknownFormatImporter().AutoGuessImport(new List<string> { "x" }, path);

            Assert.Empty(subtitle.Paragraphs);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
