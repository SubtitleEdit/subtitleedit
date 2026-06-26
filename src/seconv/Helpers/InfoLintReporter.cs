using SeConv.Core;
using Spectre.Console;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SeConv.Helpers;

/// <summary>
/// Renders <see cref="SubtitleInfo"/> and <see cref="LintReport"/> as either
/// human-readable text (Spectre tables) or as JSON. JSON is camelCase + indented
/// so it is both machine-parseable and readable on the terminal.
/// </summary>
internal static class InfoLintReporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        // Default encoder escapes <, >, &, ' for HTML safety. We're writing to stdout,
        // not HTML — keep angle brackets readable in mismatched-tag messages.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static void RenderInfo(SubtitleInfo info, bool json)
    {
        if (json)
        {
            Console.WriteLine(JsonSerializer.Serialize(info, JsonOptions));
            return;
        }

        AnsiConsole.MarkupLineInterpolated($"[bold cyan]{info.Path}[/]");
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Field[/]");
        table.AddColumn("[green]Value[/]");

        table.AddRow("Format", info.Format);
        table.AddRow("Extension", info.Extension);
        table.AddRow("Encoding", info.Encoding);
        table.AddRow("File size", FormatBytes(info.FileSizeBytes));
        table.AddRow("Paragraphs", info.ParagraphCount.ToString());
        if (info.FirstStartMs.HasValue) table.AddRow("First start", FormatMs(info.FirstStartMs.Value));
        if (info.LastEndMs.HasValue) table.AddRow("Last end", FormatMs(info.LastEndMs.Value));
        if (info.DurationMs.HasValue) table.AddRow("Duration", FormatMs(info.DurationMs.Value));
        if (!string.IsNullOrEmpty(info.Language)) table.AddRow("Language", info.Language);

        AnsiConsole.Write(table);
    }

    public static void RenderLint(IReadOnlyList<LintReport> reports, bool json)
    {
        var totalIssues = reports.Sum(r => r.Issues.Count);

        if (json)
        {
            var payload = new
            {
                totalFiles = reports.Count,
                cleanFiles = reports.Count(r => r.IsClean),
                totalIssues,
                files = reports.Select(r => new
                {
                    path = r.Path,
                    issueCount = r.Issues.Count,
                    issues = r.Issues,
                }),
            };
            Console.WriteLine(JsonSerializer.Serialize(payload, JsonOptions));
            return;
        }

        foreach (var report in reports)
        {
            if (report.IsClean)
            {
                AnsiConsole.MarkupLineInterpolated($"[green]✓[/] {report.Path} [dim](clean)[/]");
                continue;
            }

            AnsiConsole.MarkupLineInterpolated($"[yellow]⚠[/] {report.Path} [dim]({report.Issues.Count} issues)[/]");
            var table = new Table().Border(TableBorder.Minimal);
            table.AddColumn("[yellow]#[/]");
            table.AddColumn("[cyan]Type[/]");
            table.AddColumn("[green]Message[/]");
            foreach (var issue in report.Issues)
            {
                table.AddRow(
                    issue.ParagraphNumber.ToString(),
                    $"[cyan]{issue.Type}[/]",
                    issue.Message.EscapeMarkup());
            }
            AnsiConsole.Write(table);
        }

        AnsiConsole.WriteLine();
        if (totalIssues == 0)
        {
            AnsiConsole.MarkupLine($"[green]All {reports.Count} file(s) clean.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]{totalIssues} issue(s) across {reports.Count} file(s).[/]");
        }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    private static string FormatMs(long ms)
    {
        var ts = TimeSpan.FromMilliseconds(ms);
        return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3} ({ms} ms)";
    }
}
