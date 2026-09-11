using SeConv.Commands;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace SeConvTests.Commands;

public class ConvertCommandTest
{
    [Fact]
    public void BuildSummaryTable_SquareBracketsInUserValues_AreNotParsedAsMarkup()
    {
        // Regression for #14692: Spectre parses table cells as markup, so an input file named
        // "input [test].json" threw "Could not find color or style 'test'" before any
        // conversion ran - even under --json/--quiet, where the table is never displayed.
        var settings = new ConvertCommand.Settings
        {
            Pattern = ["input [test].json", "[red]other[/].srt"],
            InputFolder = "Title [1080p Bluray]",
            OutputFolder = "out [done]",
            Encoding = "[bold]utf-8",
            TranslateTo = "[green]da",
            DeleteContains = "[/]",
        };

        var table = ConvertCommand.BuildSummaryTable(settings, ["FixCommonErrors"], "SubRip (*.srt)");

        var console = new TestConsole();
        console.Write(table);
        var output = console.Output;

        Assert.Contains("input [test].json", output);
        Assert.Contains("[red]other[/].srt", output);
        Assert.Contains("Title [1080p Bluray]", output);
        Assert.Contains("out [done]", output);
        Assert.Contains("[bold]utf-8", output);
        Assert.Contains("[green]da", output);
        Assert.Contains("[/]", output);
    }

    private sealed class TestConsole
    {
        private readonly IAnsiConsole _console;
        private readonly StringWriter _writer = new();

        public TestConsole()
        {
            _console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Out = new AnsiConsoleOutput(_writer),
                Interactive = InteractionSupport.No,
            });
            _console.Profile.Width = 200;
        }

        public void Write(IRenderable renderable) => _console.Write(renderable);

        public string Output => _writer.ToString();
    }
}
