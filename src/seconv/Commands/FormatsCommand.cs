using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using SeConv.Core;
using SeConv.Helpers;

namespace SeConv.Commands;

[Description("List all available subtitle formats")]
internal sealed class FormatsCommand : Command<FormatsCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--json")]
        [Description("Emit the format list as JSON to stdout")]
        public bool Json { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var formats = LibSEIntegration.GetAvailableFormats();

        if (settings.Json)
        {
            JsonOut.Write(new
            {
                formats = formats.Select(entry => new
                {
                    // 'id' is what --format actually matches on: the name with spaces removed,
                    // compared case-insensitively. The display name is kept separately so a
                    // caller never has to derive one from the other.
                    id = entry.Format.Name.Replace(" ", string.Empty),
                    name = entry.Format.Name,
                    extension = entry.Format.Extension,
                    type = entry.Kind.StartsWith("binary", StringComparison.Ordinal) ? "binary" : "text",
                    inputOnly = entry.Kind.Contains("(input)", StringComparison.Ordinal),
                }),
                total = formats.Count,
                extraIds = new[] { "customtext", "customtextformat", "plaintext" },
                note = "Pass 'id' to --format or as the second positional argument. An inputOnly format can be loaded but not used as the conversion target.",
            });
            return 0;
        }

        AnsiConsole.MarkupLine("[bold cyan]Available Subtitle Formats[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[yellow]#[/]");
        table.AddColumn("[green]Format Name[/]");
        table.AddColumn("[cyan]Extension[/]");
        table.AddColumn("[magenta]Type[/]");

        var index = 1;
        foreach (var entry in formats)
        {
            table.AddRow(
                index.ToString(),
                $"[green]{entry.Format.Name}[/]",
                $"[cyan]{entry.Format.Extension}[/]",
                $"[magenta]{entry.Kind}[/]");
            index++;
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[yellow]Total formats: {formats.Count}[/]");
        AnsiConsole.MarkupLine("[dim]'(input)' types can be loaded but not saved as the conversion target.[/]");

        return 0;
    }
}
