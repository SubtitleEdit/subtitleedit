using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using SeConv.Core;

namespace SeConv.Commands;

[Description("List all available subtitle formats")]
internal sealed class FormatsCommand : Command<FormatsCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("[bold cyan]Available Subtitle Formats[/]");
        AnsiConsole.WriteLine();

        var formats = LibSEIntegration.GetAvailableFormats();

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
