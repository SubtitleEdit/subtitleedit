using System.IO.Compression;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Common;

[Collection("NonParallelTests")]
public class WordDocxReaderTest
{
    private static string P(params string[] runs) => "<w:p>" + string.Concat(runs) + "</w:p>";
    private static string R(string inner) => "<w:r>" + inner + "</w:r>";
    private static string T(string text) => "<w:t xml:space=\"preserve\">" + text + "</w:t>";
    private static string Tc(params string[] paragraphs) => "<w:tc><w:tcPr/>" + string.Concat(paragraphs) + "</w:tc>";
    private static string Tr(params string[] cells) => "<w:tr>" + string.Concat(cells) + "</w:tr>";

    private static string WriteDocx(string bodyXml)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".docx");
        using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("word/document.xml");
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                         "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>" +
                         bodyXml +
                         "<w:sectPr/></w:body></w:document>");
        }

        return path;
    }

    [Fact]
    public void ParagraphsTabsBreaksDeletionsAndFields()
    {
        var path = WriteDocx(
            P(R(T("One") + "<w:tab/>" + T("two"))) +
            P(R(T("Line") + "<w:br/>" + T("break"))) +
            P(R(T("Kept")), "<w:del><w:r><w:delText>gone</w:delText></w:r></w:del>", "<w:ins>" + R(T(" added")) + "</w:ins>") +
            P(R(T("Page ")), R("<w:fldChar w:fldCharType=\"begin\"/>"), R("<w:instrText> PAGE </w:instrText>"),
              R("<w:fldChar w:fldCharType=\"separate\"/>"), R(T("5")), R("<w:fldChar w:fldCharType=\"end\"/>")));
        try
        {
            var lines = WordDocumentReader.GetLines(path);

            Assert.Equal(new List<string> { "One\ttwo", "Line", "break", "Kept added", "Page 5" }, lines);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TableRowsBecomeTabLines()
    {
        var path = WriteDocx(
            "<w:tbl><w:tblPr/>" +
            Tr(Tc(P(R(T("1")))), Tc(P(R(T("00:00:01:00"))), P(R(T("00:00:03:12")))), Tc(P(R(T("Hello there…"))), P(R(T("– second line"))))) +
            Tr(Tc(P(R(T("2")))), Tc(P(R(T("00:00:04:00"))), P(R(T("00:00:05:00")))), Tc(P(), P(R(T("Only one line (ćčž)"))))) +
            "</w:tbl>");
        try
        {
            Assert.True(WordDocumentReader.IsWordDocument(path));
            var lines = WordDocumentReader.GetLines(path);
            Assert.Contains("1\t00:00:01:00\tHello there…", lines);
            Assert.Contains("\t00:00:03:12\t– second line", lines);
            Assert.Contains("2\t00:00:04:00\t", lines);

            var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
            try
            {
                Configuration.Settings.General.CurrentFrameRate = 25;
                var subtitle = Subtitle.Parse(path);
                Assert.NotNull(subtitle);
                Assert.IsType<UnknownSubtitle108>(subtitle.OriginalFormat);
                Assert.Equal(2, subtitle.Paragraphs.Count);
                Assert.Equal("Hello there…" + Environment.NewLine + "– second line", subtitle.Paragraphs[0].Text);
                Assert.Equal(3480, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
                Assert.Equal("Only one line (ćčž)", subtitle.Paragraphs[1].Text);
            }
            finally
            {
                Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SubRipPastedIntoDocxLoadsViaUnknownImporter()
    {
        var srt = new[] { "1", "00:00:01,000 --> 00:00:03,000", "Hello from Word.", "", "2", "00:00:04,000 --> 00:00:06,500", "Second", "" };
        var path = WriteDocx(string.Concat(srt.Select(l => P(R(T(l))))));
        try
        {
            var subtitle = new UnknownFormatImporter().AutoGuessImport(new List<string> { "PK" }, path);

            Assert.IsType<SubRip>(subtitle.OriginalFormat);
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("Second", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void NotAZipReturnsNull()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".docx");
        try
        {
            File.WriteAllText(path, new string('x', 500));
            Assert.False(WordDocumentReader.IsWordDocument(path));
            Assert.Null(WordDocumentReader.GetLines(path));

            File.WriteAllText(path, "PK" + new string('x', 500));
            Assert.Null(WordDocumentReader.GetLines(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
