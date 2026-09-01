using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Writes a single-sheet .xlsx (Office Open XML) workbook - no dependency, just a zip with a
/// handful of xml parts. Strings are written inline, so there is no shared-strings table to
/// keep in sync, and numbers are written as numbers so Excel sorts and sums them.
/// <para>
/// Preferred over csv for "export to Excel": a csv is split on the list separator of the
/// machine that opens it, so a comma-separated file loses its columns on a Danish or German
/// Excel, and a text/quote guess mangles time codes and non-ASCII text.
/// </para>
/// </summary>
public static class XlsxWriter
{
    private const string MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    /// <summary>Header style (bold, white on blue) - cellXfs index 1 in <see cref="StylesXml"/>.</summary>
    private const int HeaderStyle = 1;

    /// <summary>Body style (top aligned) - cellXfs index 2.</summary>
    private const int BodyStyle = 2;

    /// <summary>
    /// Builds the workbook in memory. Cells are strings unless the value is a number
    /// (int/long/double/decimal), and null becomes an empty cell.
    /// </summary>
    /// <param name="sheetName">Sheet tab name; invalid characters are replaced.</param>
    /// <param name="headers">The first row, written bold and frozen.</param>
    /// <param name="rows">One list of cell values per row; must be at most <paramref name="headers"/> long.</param>
    /// <param name="columnWidths">Optional width (in characters) per column.</param>
    public static byte[] Create(
        string sheetName,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<object?>> rows,
        IReadOnlyList<double>? columnWidths = null)
    {
        var sheet = MakeSheetXml(headers, rows, columnWidths);

        using (var ms = new MemoryStream())
        {
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                AddEntry(zip, "[Content_Types].xml", ContentTypesXml);
                AddEntry(zip, "_rels/.rels", RootRelsXml);
                AddEntry(zip, "xl/workbook.xml", MakeWorkbookXml(sheetName));
                AddEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRelsXml);
                AddEntry(zip, "xl/styles.xml", StylesXml);
                AddEntry(zip, "xl/worksheets/sheet1.xml", sheet);
            }

            return ms.ToArray();
        }
    }

    /// <summary>Builds the workbook and writes it to <paramref name="fileName"/>.</summary>
    public static void Save(
        string fileName,
        string sheetName,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<object?>> rows,
        IReadOnlyList<double>? columnWidths = null)
    {
        File.WriteAllBytes(fileName, Create(sheetName, headers, rows, columnWidths));
    }

    private static void AddEntry(ZipArchive zip, string name, string content)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using (var stream = entry.Open())
        {
            // No BOM: Excel reads the parts as the xml declaration says.
            var bytes = new UTF8Encoding(false).GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    private static string MakeSheetXml(
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<object?>> rows,
        IReadOnlyList<double>? columnWidths)
    {
        var body = new StringBuilder();
        var rowNumber = 1;

        body.Append("<row r=\"1\">");
        for (var i = 0; i < headers.Count; i++)
        {
            AppendInlineString(body, ColumnName(i) + "1", HeaderStyle, headers[i]);
        }

        body.Append("</row>");

        foreach (var row in rows)
        {
            rowNumber++;
            body.Append("<row r=\"").Append(rowNumber).Append("\">");
            for (var i = 0; i < row.Count && i < headers.Count; i++)
            {
                var reference = ColumnName(i) + rowNumber.ToString(CultureInfo.InvariantCulture);
                var value = row[i];
                if (value == null)
                {
                    continue;
                }

                if (TryGetNumber(value, out var number))
                {
                    body.Append("<c r=\"").Append(reference).Append("\" s=\"").Append(BodyStyle).Append("\"><v>")
                        .Append(number.ToString("R", CultureInfo.InvariantCulture))
                        .Append("</v></c>");
                }
                else
                {
                    AppendInlineString(body, reference, BodyStyle, value.ToString() ?? string.Empty);
                }
            }

            body.Append("</row>");
        }

        var lastColumn = ColumnName(Math.Max(headers.Count - 1, 0));
        var dimension = "A1:" + lastColumn + rowNumber.ToString(CultureInfo.InvariantCulture);

        var cols = new StringBuilder();
        if (columnWidths != null && columnWidths.Count > 0)
        {
            cols.Append("<cols>");
            for (var i = 0; i < columnWidths.Count; i++)
            {
                cols.Append("<col min=\"").Append(i + 1).Append("\" max=\"").Append(i + 1)
                    .Append("\" width=\"").Append(columnWidths[i].ToString("0.##", CultureInfo.InvariantCulture))
                    .Append("\" customWidth=\"1\"/>");
            }

            cols.Append("</cols>");
        }

        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
               "<worksheet xmlns=\"" + MainNs + "\">" +
               "<dimension ref=\"" + dimension + "\"/>" +
               "<sheetViews><sheetView workbookViewId=\"0\">" +
               "<pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/>" +
               "</sheetView></sheetViews>" +
               "<sheetFormatPr defaultRowHeight=\"15\"/>" +
               cols +
               "<sheetData>" + body + "</sheetData>" +
               "<autoFilter ref=\"" + dimension + "\"/>" +
               "</worksheet>";
    }

    private static void AppendInlineString(StringBuilder sb, string reference, int style, string text)
    {
        sb.Append("<c r=\"").Append(reference).Append("\" s=\"").Append(style).Append("\" t=\"inlineStr\"><is><t")
            .Append(NeedsSpacePreserve(text) ? " xml:space=\"preserve\"" : string.Empty)
            .Append('>').Append(EncodeXml(text)).Append("</t></is></c>");
    }

    private static bool NeedsSpacePreserve(string text)
    {
        return text.Length > 0 && (char.IsWhiteSpace(text[0]) || char.IsWhiteSpace(text[text.Length - 1]));
    }

    private static bool TryGetNumber(object value, out double number)
    {
        switch (value)
        {
            case int i:
                number = i;
                return true;
            case long l:
                number = l;
                return true;
            case double d:
                number = d;
                return !double.IsNaN(d) && !double.IsInfinity(d);
            case float f:
                number = f;
                return !float.IsNaN(f) && !float.IsInfinity(f);
            case decimal m:
                number = (double)m;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    /// <summary>0 -&gt; "A", 25 -&gt; "Z", 26 -&gt; "AA".</summary>
    internal static string ColumnName(int zeroBasedIndex)
    {
        var name = string.Empty;
        var index = zeroBasedIndex;
        while (true)
        {
            name = (char)('A' + index % 26) + name;
            index = index / 26 - 1;
            if (index < 0)
            {
                return name;
            }
        }
    }

    /// <summary>
    /// Xml-escapes and drops the control characters xml 1.0 cannot carry - Excel reports a
    /// corrupt file rather than skipping them, and subtitle text can hold anything.
    /// </summary>
    internal static string EncodeXml(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '&':
                    sb.Append("&amp;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                case '>':
                    sb.Append("&gt;");
                    break;
                case '"':
                    sb.Append("&quot;");
                    break;
                case '\'':
                    sb.Append("&apos;");
                    break;
                default:
                    if (ch == '\t' || ch == '\n' || ch == '\r' || ch >= ' ' && ch != '\uFFFE' && ch != '\uFFFF')
                    {
                        sb.Append(ch);
                    }

                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>Excel rejects these characters in a sheet name, and caps it at 31 characters.</summary>
    private static string MakeSheetName(string sheetName)
    {
        var sb = new StringBuilder();
        foreach (var ch in sheetName)
        {
            sb.Append("[]:*?/\\".IndexOf(ch) >= 0 ? '-' : ch);
        }

        var name = sb.ToString().Trim('\'');
        if (name.Length > 31)
        {
            name = name.Substring(0, 31);
        }

        return string.IsNullOrWhiteSpace(name) ? "Sheet1" : name;
    }

    private static string MakeWorkbookXml(string sheetName)
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
               "<workbook xmlns=\"" + MainNs + "\" xmlns:r=\"" + RelNs + "\">" +
               "<sheets><sheet name=\"" + EncodeXml(MakeSheetName(sheetName)) + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
               "</workbook>";
    }

    private const string ContentTypesXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
        "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
        "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
        "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
        "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
        "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
        "</Types>";

    private const string RootRelsXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
        "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
        "</Relationships>";

    private const string WorkbookRelsXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
        "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
        "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
        "</Relationships>";

    private const string StylesXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<styleSheet xmlns=\"" + MainNs + "\">" +
        "<fonts count=\"2\">" +
        "<font><sz val=\"11\"/><color theme=\"1\"/><name val=\"Calibri\"/><family val=\"2\"/></font>" +
        "<font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/><family val=\"2\"/></font>" +
        "</fonts>" +
        "<fills count=\"3\">" +
        "<fill><patternFill patternType=\"none\"/></fill>" +
        "<fill><patternFill patternType=\"gray125\"/></fill>" +
        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF5D8AA8\"/><bgColor indexed=\"64\"/></patternFill></fill>" +
        "</fills>" +
        "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
        "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
        "<cellXfs count=\"3\">" +
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
        "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\"/></xf>" +
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyAlignment=\"1\"><alignment vertical=\"top\"/></xf>" +
        "</cellXfs>" +
        "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>" +
        "</styleSheet>";
}
