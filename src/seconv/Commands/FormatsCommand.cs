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

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("[bold cyan]Available Subtitle Formats[/]");
        AnsiConsole.WriteLine();

        var formats = GetAvailableFormats();

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[yellow]#[/]");
        table.AddColumn("[green]Format Name[/]");
        table.AddColumn("[cyan]Extension[/]");

        int index = 1;
        foreach (var (name, extension, _) in formats)
        {
            table.AddRow(
                index.ToString(),
                $"[green]{name}[/]",
                $"[cyan]{extension}[/]");
            index++;
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[yellow]Total formats: {formats.Count}[/]");

        return 0;
    }

    private static List<(string Name, string Extension, string Description)> GetAvailableFormats()
    {
        // Get formats from LibSE integration
        var formats = LibSEIntegration.GetAvailableFormats();

        // Group by name to show unique formats (some formats may have same name but different implementations)
        return formats
            .GroupBy(f => f.Name)
            .Select(g => g.First())
            .Select(f => (f.Name, f.Extension, f.Name))
            .ToList();
    }
}
