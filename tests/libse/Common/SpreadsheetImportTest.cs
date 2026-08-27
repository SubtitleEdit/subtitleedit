using Nikse.SubtitleEdit.Core.Common;
using System.IO.Compression;
using System.Text;

namespace LibSETests.Common;

/// <summary>
/// Spreadsheets are an import path of their own: no SubtitleFormat claims .xlsx/.ods, so opening
/// one only works through UnknownFormatImporter's extension-keyed detour (#14168). These guard
/// that a plain "Start / End / Text" sheet really does come back as subtitle lines.
/// </summary>
public class SpreadsheetImportTest
{
    private static void WithTempFile(string extension, byte[] content, Action<string> assert)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        try
        {
            File.WriteAllBytes(path, content);
            assert(path);
        }
        finally
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // best effort
            }
        }
    }

    private static byte[] MakeXlsx()
    {
        const string sheet = """
                             <?xml version="1.0" encoding="UTF-8"?>
                             <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                               <sheetData>
                                 <row r="1"><c r="A1" t="inlineStr"><is><t>Start</t></is></c><c r="B1" t="inlineStr"><is><t>End</t></is></c><c r="C1" t="inlineStr"><is><t>Text</t></is></c></row>
                                 <row r="2"><c r="A2" t="inlineStr"><is><t>00:00:01.000</t></is></c><c r="B2" t="inlineStr"><is><t>00:00:03.500</t></is></c><c r="C2" t="inlineStr"><is><t>Hello there.</t></is></c></row>
                                 <row r="3"><c r="A3" t="inlineStr"><is><t>00:00:04.000</t></is></c><c r="B3" t="inlineStr"><is><t>00:00:06.000</t></is></c><c r="C3" t="inlineStr"><is><t>General Kenobi!</t></is></c></row>
                               </sheetData>
                             </worksheet>
                             """;

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("xl/worksheets/sheet1.xml");
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(sheet);
        }

        return stream.ToArray();
    }

    private static byte[] MakeOds()
    {
        const string content = """
                               <?xml version="1.0" encoding="UTF-8"?>
                               <office:document-content
                                   xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
                                   xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
                                   xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
                                 <office:body><office:spreadsheet>
                                   <table:table table:name="Sheet1">
                                     <table:table-row>
                                       <table:table-cell><text:p>Start</text:p></table:table-cell>
                                       <table:table-cell><text:p>End</text:p></table:table-cell>
                                       <table:table-cell><text:p>Text</text:p></table:table-cell>
                                     </table:table-row>
                                     <table:table-row>
                                       <table:table-cell><text:p>00:00:01.000</text:p></table:table-cell>
                                       <table:table-cell><text:p>00:00:03.500</text:p></table:table-cell>
                                       <table:table-cell><text:p>Hello there.</text:p></table:table-cell>
                                     </table:table-row>
                                     <table:table-row>
                                       <table:table-cell><text:p>00:00:04.000</text:p></table:table-cell>
                                       <table:table-cell><text:p>00:00:06.000</text:p></table:table-cell>
                                       <table:table-cell><text:p>General Kenobi!</text:p></table:table-cell>
                                     </table:table-row>
                                   </table:table>
                                 </office:spreadsheet></office:body>
                               </office:document-content>
                               """;

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("content.xml");
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(content);
        }

        return stream.ToArray();
    }

    private static void AssertTwoLines(Subtitle subtitle)
    {
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello there.", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal(3500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 0);
        Assert.Equal("General Kenobi!", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void XlsxWithStartEndTextHeaderIsImported()
    {
        WithTempFile(".xlsx", MakeXlsx(), path =>
        {
            var subtitle = new UnknownFormatImporter().AutoGuessImport(new List<string>(), path);
            AssertTwoLines(subtitle);
        });
    }

    [Fact]
    public void OdsWithStartEndTextHeaderIsImported()
    {
        WithTempFile(".ods", MakeOds(), path =>
        {
            var subtitle = new UnknownFormatImporter().AutoGuessImport(new List<string>(), path);
            AssertTwoLines(subtitle);
        });
    }

    [Fact]
    public void CsvWithStartEndTextHeaderIsImported()
    {
        var lines = new List<string>
        {
            "Start,End,Text",
            "00:00:01.000,00:00:03.500,\"Hello there.\"",
            "00:00:04.000,00:00:06.000,\"General Kenobi!\"",
        };

        AssertTwoLines(new UnknownFormatImporterCsv().AutoGuessImport(lines));
    }

    [Fact]
    public void HeaderWithOnlyOneKnownColumnIsNotImported()
    {
        // Fewer than two recognized header names means this is not a subtitle spreadsheet - the
        // file must fall through to the ordinary unknown-format parsing instead.
        var lines = new List<string>
        {
            "Text,Note,Reviewer",
            "\"Hello there.\",ok,jane",
            "\"General Kenobi!\",ok,jane",
        };

        Assert.Empty(new UnknownFormatImporterCsv().AutoGuessImport(lines).Paragraphs);
    }
}
