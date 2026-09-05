using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>One filter chip in the html export - mirrors a <see cref="SummaryCard"/> in the window.</summary>
public sealed class ReportExportChip
{
    /// <summary>Stable id shared with the rows that belong to it; letters, digits and '-' only.</summary>
    public required string Id { get; init; }
    public required string Label { get; init; }
    public string Hint { get; init; } = string.Empty;

    /// <summary>"#RRGGBB" - the dot colour the window paints for this class.</summary>
    public required string Color { get; init; }
    public int Count { get; init; }
}

/// <summary>One row of a report: a line number, a class, the two time codes, a detail and the text.</summary>
public sealed class ReportExportRow
{
    public int Number { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Show { get; init; } = string.Empty;
    public string Hide { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;

    /// <summary>The <see cref="ReportExportChip.Id"/> of the class the row belongs to.</summary>
    public string ChipId { get; init; } = string.Empty;

    /// <summary>"#RRGGBB" for the pill in the html export.</summary>
    public string Color { get; init; } = string.Empty;
}

/// <summary>Everything a report window has to hand over to be exported - see <see cref="ReportExport"/>.</summary>
public sealed class ReportExportData
{
    public required string Title { get; init; }
    public string Summary { get; init; } = string.Empty;

    /// <summary>The subtitle or video the report is about; shown in the header when set.</summary>
    public string? FileName { get; init; }

    /// <summary>Header of the class column - "Error" in List errors, "Issue" in the transcription report.</summary>
    public required string CategoryHeader { get; init; }
    public required string DetailHeader { get; init; }
    public required string AllLabel { get; init; }
    public required string AllColor { get; init; }

    /// <summary>Filter chips in display order; chips with a count of zero are left out of the html.</summary>
    public IReadOnlyList<ReportExportChip> Chips { get; init; } = Array.Empty<ReportExportChip>();
    public IReadOnlyList<ReportExportRow> Rows { get; init; } = Array.Empty<ReportExportRow>();
}

/// <summary>
/// Turns the rows of a report window - "List errors", the transcription quality report - into
/// something that can leave the window: the clipboard, a plain text log, an Excel workbook, or
/// a stand-alone html page (#14379 - the list had to be screenshotted to be shared or handed
/// to an AI; discussion #12929 asked for the same in the transcription report).
/// <para>
/// Every writer takes the rows exactly as the window shows them, so an active summary-card
/// filter is part of what is exported.
/// </para>
/// </summary>
public static class ReportExport
{
    /// <summary>Tab separated with a header row - what the clipboard gets, and what Excel/Sheets paste as columns.</summary>
    public static string ToTabSeparated(ReportExportData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join("\t", Headers(data)));
        foreach (var row in data.Rows)
        {
            sb.AppendLine(string.Join("\t", Cells(row).Select(CleanForTabSeparated)));
        }

        return sb.ToString();
    }

    /// <summary>
    /// A readable log: a short header, then two lines per row - the time code and the class on
    /// the first, the subtitle text on the second. Meant to be pasted into a mail, an issue, or
    /// a prompt.
    /// </summary>
    public static string ToPlainText(ReportExportData data)
    {
        var l = Se.Language.ErrorList;
        var sb = new StringBuilder();
        sb.AppendLine(data.Title);
        if (!string.IsNullOrEmpty(data.FileName))
        {
            sb.AppendLine(string.Format(l.ExportFileX, data.FileName));
        }

        sb.AppendLine(string.Format(l.ExportGeneratedX, Now()));
        sb.AppendLine(data.Summary);
        sb.AppendLine();

        foreach (var row in data.Rows)
        {
            var detail = string.IsNullOrEmpty(row.Detail) ? string.Empty : ": " + row.Detail;
            sb.AppendLine($"#{row.Number}  {row.Show} --> {row.Hide}  {row.Category}{detail}");
            sb.AppendLine("    " + row.Text);
        }

        return sb.ToString();
    }

    /// <summary>The same rows as an .xlsx workbook - see <see cref="XlsxWriter"/> for why not csv.</summary>
    public static byte[] ToXlsx(ReportExportData data)
    {
        var rows = data.Rows.Select(row => (IReadOnlyList<object?>)new object?[]
        {
            row.Number,
            row.Category,
            row.Show,
            row.Hide,
            row.Detail,
            row.Text,
        });

        return XlsxWriter.Create(data.Title, Headers(data), rows, new double[] { 6, 20, 14, 14, 26, 80 });
    }

    /// <summary>
    /// A stand-alone page - no scripts, no web fonts, nothing to load - that follows the
    /// reader's light/dark preference and paints the classes in the same colours as the
    /// window. The summary cards become filter chips backed by <c>:checked</c> radios, so the
    /// page filters without a line of JavaScript.
    /// </summary>
    public static string ToHtml(ReportExportData data)
    {
        var l = Se.Language.ErrorList;
        var chips = data.Chips.Where(p => p.Count > 0).ToList();
        var title = string.IsNullOrEmpty(data.FileName)
            ? data.Title
            : data.Title + " - " + Path.GetFileName(data.FileName);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.AppendLine("<meta name=\"color-scheme\" content=\"light dark\">");
        sb.AppendLine("<title>" + Encode(title) + "</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(Css);
        sb.AppendLine(MakeFilterCss(chips));
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<main>");

        sb.AppendLine("<header>");
        sb.AppendLine("  <h1>" + Encode(data.Title) + "</h1>");
        sb.AppendLine("  <p class=\"summary\">" + Encode(data.Summary) + "</p>");
        sb.AppendLine("  <p class=\"meta\">");
        if (!string.IsNullOrEmpty(data.FileName))
        {
            sb.AppendLine("    <span>" + Encode(string.Format(l.ExportFileX, data.FileName)) + "</span>");
        }

        sb.AppendLine("    <span>" + Encode(string.Format(l.ExportGeneratedX, Now())) + "</span>");
        sb.AppendLine("  </p>");
        sb.AppendLine("</header>");

        AppendChips(sb, data, chips);
        AppendTable(sb, data);

        sb.AppendLine("<footer>" + Encode(l.ExportMadeWithSubtitleEdit) + "</footer>");
        sb.AppendLine("</main>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    /// <summary>
    /// One radio per class in front of the table: checking it hides the rows of the other
    /// classes through the sibling selectors from <see cref="MakeFilterCss"/>.
    /// </summary>
    private static void AppendChips(StringBuilder sb, ReportExportData data, IReadOnlyList<ReportExportChip> chips)
    {
        sb.AppendLine("<input type=\"radio\" name=\"f\" id=\"f-all\" checked>");
        foreach (var chip in chips)
        {
            sb.AppendLine("<input type=\"radio\" name=\"f\" id=\"f-" + chip.Id + "\">");
        }

        sb.AppendLine("<nav class=\"chips\">");
        sb.AppendLine("  <label class=\"chip\" for=\"f-all\"><i style=\"background:" + data.AllColor + "\"></i>" +
                      Encode(data.AllLabel) + "<b>" + data.Rows.Count + "</b></label>");
        foreach (var chip in chips)
        {
            sb.AppendLine("  <label class=\"chip\" for=\"f-" + chip.Id + "\" title=\"" + Encode(chip.Hint) + "\">" +
                          "<i style=\"background:" + chip.Color + "\"></i>" +
                          Encode(chip.Label) + "<b>" + chip.Count + "</b></label>");
        }

        sb.AppendLine("</nav>");
    }

    private static void AppendTable(StringBuilder sb, ReportExportData data)
    {
        sb.AppendLine("<div class=\"table-wrap\">");
        sb.AppendLine("<table>");
        sb.AppendLine("  <thead><tr>" +
                      "<th class=\"num\">" + Encode(Se.Language.General.NumberSymbol) + "</th>" +
                      "<th>" + Encode(data.CategoryHeader) + "</th>" +
                      "<th>" + Encode(Se.Language.General.Show) + "</th>" +
                      "<th>" + Encode(Se.Language.General.Hide) + "</th>" +
                      "<th>" + Encode(data.DetailHeader) + "</th>" +
                      "<th>" + Encode(Se.Language.General.Text) + "</th>" +
                      "</tr></thead>");
        sb.AppendLine("  <tbody>");
        foreach (var row in data.Rows)
        {
            sb.AppendLine("    <tr class=\"t-" + row.ChipId + "\" style=\"--dot:" + row.Color + "\">" +
                          "<td class=\"num\">" + row.Number + "</td>" +
                          "<td><span class=\"pill\"><i></i>" + Encode(row.Category) + "</span></td>" +
                          "<td class=\"time\">" + Encode(row.Show) + "</td>" +
                          "<td class=\"time\">" + Encode(row.Hide) + "</td>" +
                          "<td class=\"detail\">" + Encode(row.Detail) + "</td>" +
                          "<td class=\"text\">" + Encode(row.Text) + "</td>" +
                          "</tr>");
        }

        sb.AppendLine("  </tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");
    }

    /// <summary>The chip-highlight and row-hiding selectors, one pair per chip that made it into the page.</summary>
    private static string MakeFilterCss(IReadOnlyList<ReportExportChip> chips)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/* Chip filtering, without script: the checked radio decides which rows stay visible. */");
        sb.AppendLine("input[name='f'] { display: none; }");

        var active = new List<string> { "#f-all:checked ~ .chips [for='f-all']" };
        active.AddRange(chips.Select(c => "#f-" + c.Id + ":checked ~ .chips [for='f-" + c.Id + "']"));
        sb.AppendLine(string.Join(",\n", active) + " {");
        sb.AppendLine("  border-color: var(--accent);");
        sb.AppendLine("  background: var(--panel-2);");
        sb.AppendLine("  background: color-mix(in srgb, var(--accent) 16%, var(--panel));");
        sb.AppendLine("  box-shadow: inset 0 0 0 1px var(--accent);");
        sb.AppendLine("}");

        if (chips.Count > 0)
        {
            var hide = chips.Select(c => "#f-" + c.Id + ":checked ~ .table-wrap tbody tr:not(.t-" + c.Id + ")");
            sb.AppendLine(string.Join(",\n", hide) + " { display: none; }");
        }

        return sb.ToString();
    }

    private static IReadOnlyList<string> Headers(ReportExportData data)
    {
        return new[]
        {
            Se.Language.General.NumberSymbol,
            data.CategoryHeader,
            Se.Language.General.Show,
            Se.Language.General.Hide,
            data.DetailHeader,
            Se.Language.General.Text,
        };
    }

    private static IEnumerable<string> Cells(ReportExportRow row)
    {
        yield return row.Number.ToString(CultureInfo.CurrentCulture);
        yield return row.Category;
        yield return row.Show;
        yield return row.Hide;
        yield return row.Detail;
        yield return row.Text;
    }

    /// <summary>A tab or a newline inside a cell would start a new column/row where it is pasted.</summary>
    private static string CleanForTabSeparated(string s)
    {
        return s.Replace('\t', ' ').Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
    }

    private static string Now()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);
    }

    private static string Encode(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    /// <summary>
    /// Light by default, dark through <c>prefers-color-scheme</c>: every colour is a token on
    /// <c>:root</c> and only the tokens are redefined, so nothing can end up defined in the dark
    /// block alone.
    /// </summary>
    private const string Css = @"
:root {
  color-scheme: light dark;
  --bg: #f6f7f9;
  --panel: #ffffff;
  --panel-2: #fbfbfc;
  --ink: #16191d;
  --ink-dim: #5b6470;
  --line: #e3e6ea;
  --line-soft: #eef0f3;
  --accent: #5d8aa8;
  --shadow: 0 1px 2px rgba(16, 24, 40, .06), 0 8px 24px rgba(16, 24, 40, .06);
  --radius: 12px;
}

@media (prefers-color-scheme: dark) {
  :root {
    --bg: #14171b;
    --panel: #1b1f24;
    --panel-2: #20252b;
    --ink: #e8ebef;
    --ink-dim: #9aa4b1;
    --line: #2b3138;
    --line-soft: #23282f;
    --accent: #7fb0cd;
    --shadow: 0 1px 2px rgba(0, 0, 0, .4), 0 8px 24px rgba(0, 0, 0, .35);
  }
}

* { box-sizing: border-box; }

body {
  margin: 0;
  padding: 32px 20px 56px;
  background: var(--bg);
  color: var(--ink);
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Noto Sans', Ubuntu, Cantarell, sans-serif;
  font-size: 14px;
  line-height: 1.5;
  -webkit-text-size-adjust: 100%;
}

main { max-width: 1180px; margin: 0 auto; }

h1 { margin: 0; font-size: 26px; font-weight: 650; letter-spacing: -.01em; }

.summary { margin: 6px 0 0; font-size: 15px; color: var(--ink-dim); }

.meta { margin: 10px 0 0; display: flex; flex-wrap: wrap; gap: 6px 18px; font-size: 12.5px; color: var(--ink-dim); }
.meta span { overflow-wrap: anywhere; }

.chips { display: flex; flex-wrap: wrap; gap: 8px; margin: 22px 0 14px; }

.chip {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 7px 12px;
  border: 1px solid var(--line);
  border-radius: 999px;
  background: var(--panel);
  color: var(--ink);
  font-size: 13px;
  cursor: pointer;
  user-select: none;
  transition: border-color .12s ease, transform .12s ease;
}
.chip:hover { border-color: var(--accent); transform: translateY(-1px); }
.chip i { width: 9px; height: 9px; border-radius: 50%; flex: none; }
.chip b { font-weight: 600; color: var(--ink-dim); font-variant-numeric: tabular-nums; }

.table-wrap {
  background: var(--panel);
  border: 1px solid var(--line);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: auto;
  max-height: 74vh;
}

table { border-collapse: collapse; width: 100%; font-size: 13.5px; }

thead th {
  position: sticky;
  top: 0;
  z-index: 1;
  text-align: left;
  font-weight: 600;
  font-size: 12px;
  letter-spacing: .04em;
  text-transform: uppercase;
  color: var(--ink-dim);
  background: var(--panel-2);
  border-bottom: 1px solid var(--line);
  padding: 11px 14px;
  white-space: nowrap;
}

tbody td { padding: 10px 14px; border-bottom: 1px solid var(--line-soft); vertical-align: top; }
tbody tr:last-child td { border-bottom: 0; }
tbody tr:hover td { background: var(--panel-2); }

td.num { width: 1%; text-align: right; color: var(--ink-dim); font-variant-numeric: tabular-nums; white-space: nowrap; }
td.time, td.detail { white-space: nowrap; font-variant-numeric: tabular-nums; }
td.time { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; font-size: 12.5px; }
td.detail { color: var(--ink-dim); }
td.text { min-width: 22em; overflow-wrap: anywhere; }

.pill {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 3px 10px 3px 8px;
  border-radius: 999px;
  border: 1px solid var(--line);
  background: transparent; /* fallback where color-mix is unknown */
  background: color-mix(in srgb, var(--dot) 12%, transparent);
  white-space: nowrap;
}
.pill i { width: 8px; height: 8px; border-radius: 50%; background: var(--dot); flex: none; }

footer { margin-top: 18px; font-size: 12px; color: var(--ink-dim); text-align: right; }

@media print {
  body { background: #fff; padding: 0; }
  .chips { display: none; }
  .table-wrap { max-height: none; box-shadow: none; border: 0; }
  thead th { position: static; }
}
";
}
