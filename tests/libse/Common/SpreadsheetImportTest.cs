using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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

    [Fact]
    public void PipeSeparatedLinesAreSplit()
    {
        // The import window's separator detection can pick '|', so CsvUtil must be able to split
        // on it - it used to handle only comma/semicolon/tab and returned the whole row as one
        // field, which left every column but the first empty.
        var rows = CsvUtil.CsvSplitLines(new List<string> { "Start|End|Text" }, '|');

        Assert.Single(rows);
        Assert.Equal(new[] { "Start", "End", "Text" }, rows[0]);
    }

    [Fact]
    public void FramesAreDetectedWhenSomeCellsAreBlank()
    {
        // A trailing newline (or one empty cell) used to veto frame detection for the whole
        // column, and HH:MM:SS:FF was then re-read as HH:MM:SS.ms - 12 ms instead of 12 frames.
        var lines = new List<string>
        {
            "Start,End,Text",
            "00:00:01:00,00:00:03:12,\"Hello there.\"",
            "00:00:04:00,00:00:06:00,\"General Kenobi!\"",
            string.Empty,
        };

        var subtitle = new UnknownFormatImporterCsv().AutoGuessImport(lines);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal(3000 + SubtitleFormat.FramesToMilliseconds(12), subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 0);
    }

    [Fact]
    public void TheCallersLineListIsNotModified()
    {
        // AutoGuessImport used to drop a leading blank line from the list it was given, and the
        // caller (UnknownFormatImporter) keeps using that same list for the other parsers.
        var lines = new List<string>
        {
            string.Empty,
            "Start,End,Text",
            "00:00:01.000,00:00:03.500,\"Hello there.\"",
        };
        var copy = new List<string>(lines);

        new UnknownFormatImporterCsv().AutoGuessImport(lines);

        Assert.Equal(copy, lines);
    }
    [Fact]
    public void OdsMergedCellsDoNotShiftTheColumnsAfterThem()
    {
        // A merged cell is one table-cell plus a covered-table-cell placeholder per column it
        // swallows. Skipping the placeholders moved every later column one to the left, so the
        // text column of such a row was read as the end time.
        const string content = """
                               <?xml version="1.0" encoding="UTF-8"?>
                               <office:document-content
                                   xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
                                   xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
                                   xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
                                 <office:body><office:spreadsheet>
                                   <table:table table:name="Sheet1">
                                     <table:table-row>
                                       <table:table-cell table:number-columns-spanned="2" table:number-rows-spanned="1"><text:p>merged</text:p></table:table-cell>
                                       <table:covered-table-cell/>
                                       <table:table-cell><text:p>after</text:p></table:table-cell>
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

        WithTempFile(".ods", stream.ToArray(), path =>
        {
            var lines = new UnknownFormatImporterOds().ReadLinesFromFile(path);
            Assert.Equal(new[] { "merged\t\tafter" }, lines);
        });
    }

    private static byte[] MakeXlsxWithSheets(string? workbookXml, string? relsXml, params (string Name, string Sheet)[] sheets)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            void Add(string name, string text)
            {
                var entry = archive.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                writer.Write(text);
            }

            if (workbookXml != null)
            {
                Add("xl/workbook.xml", workbookXml);
            }

            if (relsXml != null)
            {
                Add("xl/_rels/workbook.xml.rels", relsXml);
            }

            foreach (var (name, sheet) in sheets)
            {
                Add(name, sheet);
            }
        }

        return stream.ToArray();
    }

    private static string MakeSheet(string firstCell)
    {
        return $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData>
                    <row r="1"><c r="A1" t="inlineStr"><is><t>{firstCell}</t></is></c></row>
                  </sheetData>
                </worksheet>
                """;
    }

    [Fact]
    public void XlsxWithTenSheetsReadsTheFirstOneNotSheetTen()
    {
        // Ordering the entries as text puts "sheet10.xml" before "sheet2.xml", so a workbook with
        // ten or more sheets was imported from the wrong one.
        var bytes = MakeXlsxWithSheets(null, null,
            ("xl/worksheets/sheet10.xml", MakeSheet("sheet ten")),
            ("xl/worksheets/sheet1.xml", MakeSheet("sheet one")),
            ("xl/worksheets/sheet2.xml", MakeSheet("sheet two")));

        WithTempFile(".xlsx", bytes, path =>
        {
            var lines = new UnknownFormatImporterXlsx().ReadLinesFromFile(path);
            Assert.Equal(new[] { "sheet one" }, lines);
        });
    }

    [Fact]
    public void XlsxReadsTheWorkbooksFirstSheetNotTheFirstFile()
    {
        // The sheet the user sees first is the first <sheet> in workbook.xml, which does not have
        // to be sheet1.xml (moving a sheet in Excel does not rename the parts).
        const string workbook = """
                                <?xml version="1.0" encoding="UTF-8"?>
                                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                  <sheets>
                                    <sheet name="Subtitles" sheetId="2" r:id="rId2"/>
                                    <sheet name="Notes" sheetId="1" r:id="rId1"/>
                                  </sheets>
                                </workbook>
                                """;
        const string rels = """
                            <?xml version="1.0" encoding="UTF-8"?>
                            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                              <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
                            </Relationships>
                            """;

        var bytes = MakeXlsxWithSheets(workbook, rels,
            ("xl/worksheets/sheet1.xml", MakeSheet("notes")),
            ("xl/worksheets/sheet2.xml", MakeSheet("subtitles")));

        WithTempFile(".xlsx", bytes, path =>
        {
            var lines = new UnknownFormatImporterXlsx().ReadLinesFromFile(path);
            Assert.Equal(new[] { "subtitles" }, lines);
        });
    }
    [Fact]
    public void AnUnterminatedQuoteInTheHeaderDoesNotThrow()
    {
        // The open quote makes the header swallow the rest of the file, so there are no data rows
        // left - and the "start time without milliseconds" repair then divided by that count. The
        // file-open path has no catch around this.
        var lines = new List<string>
        {
            "Start,End,\"Text",
            "00:00:01.000,00:00:03.500,Hello there.",
        };

        Assert.Empty(new UnknownFormatImporterCsv().AutoGuessImport(lines).Paragraphs);
    }
}
