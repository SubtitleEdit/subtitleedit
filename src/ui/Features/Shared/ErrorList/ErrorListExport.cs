using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

/// <summary>
/// Turns the rows of "List errors" into something that can leave the window: the clipboard,
/// a plain text log, an Excel workbook, or a stand-alone html page (#14379 - the list had to
/// be screenshotted to be shared or handed to an AI).
/// <para>
/// Every writer takes the rows exactly as the window shows them, so an active summary-card
/// filter is part of what is exported.
/// </para>
/// </summary>
public static class ErrorListExport
{
    /// <summary>Tab separated with a header row - what the clipboard gets, and what Excel/Sheets paste as columns.</summary>
    public static string ToTabSeparated(IEnumerable<ErrorListItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join("\t", Headers()));
        foreach (var item in items)
        {
            sb.AppendLine(string.Join("\t", Cells(item).Select(CleanForTabSeparated)));
        }

        return sb.ToString();
    }

    /// <summary>
    /// A readable log: a short header, then two lines per error - the time code and the error on
    /// the first, the subtitle text on the second. Meant to be pasted into a mail, an issue, or
    /// a prompt.
    /// </summary>
    public static string ToPlainText(IEnumerable<ErrorListItem> items, string summary, string? subtitleFileName)
    {
        var l = Se.Language.ErrorList;
        var sb = new StringBuilder();
        sb.AppendLine(l.Title);
        if (!string.IsNullOrEmpty(subtitleFileName))
        {
            sb.AppendLine(string.Format(l.ExportFileX, subtitleFileName));
        }

        sb.AppendLine(string.Format(l.ExportGeneratedX, Now()));
        sb.AppendLine(summary);
        sb.AppendLine();

        foreach (var item in items)
        {
            sb.AppendLine($"#{item.Number}  {item.Show} --> {item.Hide}  {item.Category}: {item.Detail}");
            sb.AppendLine("    " + item.Text);
        }

        return sb.ToString();
    }

    /// <summary>The same rows as an .xlsx workbook - see <see cref="XlsxWriter"/> for why not csv.</summary>
    public static byte[] ToXlsx(IEnumerable<ErrorListItem> items)
    {
        var rows = items.Select(item => (IReadOnlyList<object?>)new object?[]
        {
            item.Number,
            item.Category,
            item.Show,
            item.Hide,
            item.Detail,
            item.Text,
        });

        return XlsxWriter.Create(Se.Language.ErrorList.Title, Headers(), rows, new double[] { 6, 20, 14, 14, 26, 80 });
    }

    /// <summary>
    /// A stand-alone page - no scripts, no web fonts, nothing to load - that follows the
    /// reader's light/dark preference and paints the error classes in the same colours as the
    /// window. The summary cards become filter chips backed by <c>:checked</c> radios, so the
    /// page filters without a line of JavaScript.
    /// </summary>
    public static string ToHtml(IEnumerable<ErrorListItem> items, string summary, string? subtitleFileName)
    {
        var l = Se.Language.ErrorList;
        var list = items.ToList();
        var title = string.IsNullOrEmpty(subtitleFileName)
            ? l.Title
            : l.Title + " - " + Path.GetFileName(subtitleFileName);

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
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<main>");

        sb.AppendLine("<header>");
        sb.AppendLine("  <h1>" + Encode(l.Title) + "</h1>");
        sb.AppendLine("  <p class=\"summary\">" + Encode(summary) + "</p>");
        sb.AppendLine("  <p class=\"meta\">");
        if (!string.IsNullOrEmpty(subtitleFileName))
        {
            sb.AppendLine("    <span>" + Encode(string.Format(l.ExportFileX, subtitleFileName)) + "</span>");
        }

        sb.AppendLine("    <span>" + Encode(string.Format(l.ExportGeneratedX, Now())) + "</span>");
        sb.AppendLine("  </p>");
        sb.AppendLine("</header>");

        AppendChips(sb, list, l.All);
        AppendTable(sb, list);

        sb.AppendLine("<footer>" + Encode(l.ExportMadeWithSubtitleEdit) + "</footer>");
        sb.AppendLine("</main>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    /// <summary>
    /// One radio per error class in front of the table: checking it hides the rows of the other
    /// classes through the sibling selectors in <see cref="Css"/>. Classes with no rows are left out.
    /// </summary>
    private static void AppendChips(StringBuilder sb, IReadOnlyList<ErrorListItem> items, string allLabel)
    {
        sb.AppendLine("<input type=\"radio\" name=\"f\" id=\"f-all\" checked>");
        foreach (var type in Enum.GetValues<LineErrorType>())
        {
            if (items.Any(p => p.Type == type))
            {
                sb.AppendLine("<input type=\"radio\" name=\"f\" id=\"f-" + (int)type + "\">");
            }
        }

        sb.AppendLine("<nav class=\"chips\">");
        sb.AppendLine("  <label class=\"chip\" for=\"f-all\"><i style=\"background:" + LineError.AllColor + "\"></i>" +
                      Encode(allLabel) + "<b>" + items.Count + "</b></label>");
        foreach (var type in Enum.GetValues<LineErrorType>())
        {
            var count = items.Count(p => p.Type == type);
            if (count == 0)
            {
                continue;
            }

            sb.AppendLine("  <label class=\"chip\" for=\"f-" + (int)type + "\" title=\"" + Encode(LineError.GetHint(type)) + "\">" +
                          "<i style=\"background:" + LineError.GetColor(type) + "\"></i>" +
                          Encode(LineError.GetLabel(type)) + "<b>" + count + "</b></label>");
        }

        sb.AppendLine("</nav>");
    }

    private static void AppendTable(StringBuilder sb, IReadOnlyList<ErrorListItem> items)
    {
        var l = Se.Language.ErrorList;
        sb.AppendLine("<div class=\"table-wrap\">");
        sb.AppendLine("<table>");
        sb.AppendLine("  <thead><tr>" +
                      "<th class=\"num\">" + Encode(Se.Language.General.NumberSymbol) + "</th>" +
                      "<th>" + Encode(Se.Language.General.Error) + "</th>" +
                      "<th>" + Encode(Se.Language.General.Show) + "</th>" +
                      "<th>" + Encode(Se.Language.General.Hide) + "</th>" +
                      "<th>" + Encode(l.Detail) + "</th>" +
                      "<th>" + Encode(Se.Language.General.Text) + "</th>" +
                      "</tr></thead>");
        sb.AppendLine("  <tbody>");
        foreach (var item in items)
        {
            sb.AppendLine("    <tr class=\"t" + (int)item.Type + "\" style=\"--dot:" + LineError.GetColor(item.Type) + "\">" +
                          "<td class=\"num\">" + item.Number + "</td>" +
                          "<td><span class=\"pill\"><i></i>" + Encode(item.Category) + "</span></td>" +
                          "<td class=\"time\">" + Encode(item.Show) + "</td>" +
                          "<td class=\"time\">" + Encode(item.Hide) + "</td>" +
                          "<td class=\"detail\">" + Encode(item.Detail) + "</td>" +
                          "<td class=\"text\">" + Encode(item.Text) + "</td>" +
                          "</tr>");
        }

        sb.AppendLine("  </tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");
    }

    private static IReadOnlyList<string> Headers()
    {
        return new[]
        {
            Se.Language.General.NumberSymbol,
            Se.Language.General.Error,
            Se.Language.General.Show,
            Se.Language.General.Hide,
            Se.Language.ErrorList.Detail,
            Se.Language.General.Text,
        };
    }

    private static IEnumerable<string> Cells(ErrorListItem item)
    {
        yield return item.Number.ToString(CultureInfo.CurrentCulture);
        yield return item.Category;
        yield return item.Show;
        yield return item.Hide;
        yield return item.Detail;
        yield return item.Text;
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

/* Chip filtering, without script: the checked radio decides which rows stay visible. */
input[name='f'] { display: none; }

#f-all:checked ~ .chips [for='f-all'],
#f-0:checked ~ .chips [for='f-0'],
#f-1:checked ~ .chips [for='f-1'],
#f-2:checked ~ .chips [for='f-2'],
#f-3:checked ~ .chips [for='f-3'],
#f-4:checked ~ .chips [for='f-4'],
#f-5:checked ~ .chips [for='f-5'],
#f-6:checked ~ .chips [for='f-6'],
#f-7:checked ~ .chips [for='f-7'] {
  border-color: var(--accent);
  background: var(--panel-2);
  background: color-mix(in srgb, var(--accent) 16%, var(--panel));
  box-shadow: inset 0 0 0 1px var(--accent);
}

#f-0:checked ~ .table-wrap tbody tr:not(.t0),
#f-1:checked ~ .table-wrap tbody tr:not(.t1),
#f-2:checked ~ .table-wrap tbody tr:not(.t2),
#f-3:checked ~ .table-wrap tbody tr:not(.t3),
#f-4:checked ~ .table-wrap tbody tr:not(.t4),
#f-5:checked ~ .table-wrap tbody tr:not(.t5),
#f-6:checked ~ .table-wrap tbody tr:not(.t6),
#f-7:checked ~ .table-wrap tbody tr:not(.t7) { display: none; }

@media print {
  body { background: #fff; padding: 0; }
  .chips { display: none; }
  .table-wrap { max-height: none; box-shadow: none; border: 0; }
  thead th { position: static; }
}
";
}
