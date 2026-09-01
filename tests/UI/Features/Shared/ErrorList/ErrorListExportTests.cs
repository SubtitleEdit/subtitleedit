using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// "List errors" could only leave the window as a screenshot (#14379). It now exports to the
/// clipboard, a text log, an .xlsx workbook and a stand-alone html page - these check that what
/// comes out is complete, escaped, and (for the workbook) a file Excel will open.
/// </summary>
public class ErrorListExportTests
{
    private static SubtitleLineViewModel MakeLine(int number, string text)
    {
        return new SubtitleLineViewModel(new Paragraph(text, number * 2000, number * 2000 + 1500), null!)
        {
            Number = number,
        };
    }

    private static List<ErrorListItem> MakeItems()
    {
        return new List<ErrorListItem>
        {
            new(MakeLine(1, "First line"), new LineError(LineErrorType.DurationTooShort, "200 < 1000")),
            new(MakeLine(2, "Second line"), new LineError(LineErrorType.CharactersPerSecond, "30 > 25")),
            new(MakeLine(3, "Third line"), new LineError(LineErrorType.CharactersPerSecond, "42 > 25")),
        };
    }

    [AvaloniaFact]
    public void TabSeparated_HasAHeaderAndSixColumnsPerError()
    {
        var text = ErrorListExport.ToTabSeparated(MakeItems());

        var lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, lines.Length); // header + three errors
        Assert.StartsWith(Se.Language.General.NumberSymbol + "\t", lines[0]);
        Assert.All(lines, line => Assert.Equal(6, line.Split('\t').Length));
        Assert.Contains("Second line", lines[2]);
    }

    [AvaloniaFact]
    public void TabSeparated_KeepsATabInTheTextFromStartingANewColumn()
    {
        var items = new List<ErrorListItem>
        {
            new(MakeLine(1, "Tab\there"), new LineError(LineErrorType.LineTooLong, "50 > 43")),
        };

        var lines = ErrorListExport.ToTabSeparated(items).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(6, lines[1].Split('\t').Length);
        Assert.Contains("Tab here", lines[1]);
    }

    [AvaloniaFact]
    public void PlainText_HasTheFileNameTheSummaryAndEveryError()
    {
        var text = ErrorListExport.ToPlainText(MakeItems(), "3 error(s) in 3 of 10 line(s)", "/tmp/my movie.srt");

        Assert.Contains("/tmp/my movie.srt", text);
        Assert.Contains("3 error(s) in 3 of 10 line(s)", text);
        Assert.Contains("#2", text);
        Assert.Contains("30 > 25", text);
        Assert.Contains("Third line", text);
    }

    [AvaloniaFact]
    public void Html_HasOneRowPerErrorAndAChipPerErrorClassInUse()
    {
        var html = ErrorListExport.ToHtml(MakeItems(), "summary", "movie.srt");

        Assert.Equal(3, CountOf(html, "<tr class=\"t"));

        // All + the two classes that have rows - not the six that have none.
        Assert.Equal(3, CountOf(html, "class=\"chip\""));
        Assert.Contains(Se.Language.ErrorList.CharactersPerSecond, html);
        Assert.DoesNotContain(Se.Language.ErrorList.Overlap, html);
    }

    [AvaloniaFact]
    public void Html_FollowsTheReadersTheme()
    {
        var html = ErrorListExport.ToHtml(MakeItems(), "summary", null);

        Assert.Contains("prefers-color-scheme: dark", html);
        Assert.Contains("color-scheme: light dark", html);

        // Every colour is a token on :root, so nothing is defined in the dark block alone.
        var root = html.Substring(html.IndexOf(":root {", StringComparison.Ordinal));
        var dark = root.Substring(root.IndexOf("@media (prefers-color-scheme: dark)", StringComparison.Ordinal));
        foreach (var token in new[] { "--bg:", "--panel:", "--ink:", "--line:", "--accent:" })
        {
            Assert.Equal(2, CountOf(root, token)); // once light, once dark
            Assert.Contains(token, dark);
        }
    }

    [AvaloniaFact]
    public void Html_IsSelfContainedWithNothingToLoad()
    {
        var html = ErrorListExport.ToHtml(MakeItems(), "summary", null);

        Assert.DoesNotContain("<script", html);
        Assert.DoesNotContain("http://", html);
        Assert.DoesNotContain("https://", html);
    }

    [AvaloniaFact]
    public void Html_EscapesSubtitleText()
    {
        var items = new List<ErrorListItem>
        {
            new(MakeLine(1, "<i>Tom & \"Jerry\"</i>"), new LineError(LineErrorType.LineTooLong, "50 > 43")),
        };

        var html = ErrorListExport.ToHtml(items, "summary", "<script>.srt");

        Assert.Contains("&lt;i&gt;Tom &amp; &quot;Jerry&quot;&lt;/i&gt;", html);
        Assert.DoesNotContain("<i>Tom", html);
        Assert.DoesNotContain("<script>", html);
    }

    [AvaloniaFact]
    public void Xlsx_IsAWorkbookWithTheRowsAndNumbersAsNumbers()
    {
        var bytes = ErrorListExport.ToXlsx(MakeItems());

        using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        foreach (var part in new[] { "[Content_Types].xml", "_rels/.rels", "xl/workbook.xml", "xl/_rels/workbook.xml.rels", "xl/styles.xml", "xl/worksheets/sheet1.xml" })
        {
            Assert.NotNull(zip.GetEntry(part));
        }

        var sheet = XDocument.Parse(ReadEntry(zip, "xl/worksheets/sheet1.xml"));
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var rows = sheet.Descendants(ns + "row").ToList();
        Assert.Equal(4, rows.Count); // header + three errors
        Assert.Equal(6, rows[0].Elements(ns + "c").Count());

        // The line number is a number cell, so Excel sorts it as one.
        var firstCell = rows[1].Elements(ns + "c").First();
        Assert.Null(firstCell.Attribute("t"));
        Assert.Equal("1", firstCell.Element(ns + "v")?.Value);
        Assert.Contains("Second line", ReadEntry(zip, "xl/worksheets/sheet1.xml"));

        // Every part must be parseable xml - a single broken part makes Excel call the file corrupt.
        foreach (var entry in zip.Entries)
        {
            XDocument.Parse(ReadEntry(zip, entry.FullName));
        }
    }

    [AvaloniaFact]
    public void Xlsx_SurvivesTextXmlCannotCarry()
    {
        var items = new List<ErrorListItem>
        {
            new(MakeLine(1, "Bell\u0007 & <angle> \"quotes\""), new LineError(LineErrorType.LineTooLong, "50 > 43")),
        };

        var bytes = ErrorListExport.ToXlsx(items);

        using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        var xml = ReadEntry(zip, "xl/worksheets/sheet1.xml");
        // Ordinal: the default comparison treats a control character as ignorable and "finds" it anywhere.
        Assert.DoesNotContain("\u0007", xml, StringComparison.Ordinal);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var cells = XDocument.Parse(xml).Descendants(ns + "row").Last().Elements(ns + "c").ToList();
        Assert.Equal("Bell & <angle> \"quotes\"", cells.Last().Descendants(ns + "t").Single().Value);
    }

    private static string ReadEntry(ZipArchive zip, string name)
    {
        using var stream = zip.GetEntry(name)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static int CountOf(string text, string value)
    {
        var count = 0;
        var index = text.IndexOf(value, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return count;
    }
}
