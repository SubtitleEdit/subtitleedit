using Spectre.Console;
using Spectre.Console.Cli;
using SeConv.Commands;
using SeConv.Helpers;

namespace SeConv;

internal class Program
{
    static int Main(string[] args)
    {
        // Handle legacy /convert syntax and convert to modern syntax
        args = ConvertLegacyArguments(args);

        // Handle /? and /help
        if (args.Length == 0 || args.Contains("/?") || args.Contains("/help") || args.Contains("--help"))
        {
            HelpDisplay.ShowHelp();
            return 0;
        }

        // Handle /formats or formats command
        if (args.Length > 0 && (args[0].Equals("/formats", StringComparison.OrdinalIgnoreCase) || 
                                 args[0].Equals("formats", StringComparison.OrdinalIgnoreCase) ||
                                 args[0].Equals("--formats", StringComparison.OrdinalIgnoreCase)))
        {
            var formatsCommand = new FormatsCommand();
            return formatsCommand.Execute(null!, new FormatsCommand.Settings(), CancellationToken.None);
        }

        // Set up Spectre.Console CLI with default command
        var app = new CommandApp<ConvertCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("SubtitleEdit");
            config.SetApplicationVersion("5.0.0");

            config.ValidateExamples();
        });

        try
        {
            return app.Run(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Fatal error: {ex.Message}[/]");
            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLine($"[red]  {ex.InnerException.Message}[/]");
            }
            return 1;
        }
    }

    /// <summary>
    /// Converts legacy /parameter syntax to modern --parameter syntax
    /// </summary>
    private static string[] ConvertLegacyArguments(string[] args)
    {
        var converted = new List<string>();

        foreach (var arg in args)
        {
            if (arg.StartsWith('/') && arg != "/?" && arg != "/help")
            {
                // Convert /parameter to --parameter
                converted.Add("--" + arg.Substring(1));
            }
            else
            {
                converted.Add(arg);
            }
        }

        return converted.ToArray();
    }
}
