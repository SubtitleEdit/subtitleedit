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

        // Backward-compat: accept the format as the second positional arg
        // (e.g. `seconv *.srt sami`) when --format / -f is not supplied.
        args = TryInjectPositionalFormat(args);

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
            return ((ICommand)formatsCommand).ExecuteAsync(null!, new FormatsCommand.Settings(), CancellationToken.None).GetAwaiter().GetResult();
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

    // Options that take a value as the next argument. Used to know which positional
    // tokens are file patterns vs. option-values when injecting a positional format.
    private static readonly HashSet<string> ValueOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "--format", "-f",
        "--adjustduration",
        "--assa-style-file",
        "--ebuheaderfile",
        "--encoding",
        "--fps",
        "--inputfolder",
        "--multiplereplace",
        "--ocrengine",
        "--offset",
        "--outputfilename",
        "--outputfolder",
        "--pac-codepage",
        "--profile",
        "--renumber",
        "--resolution",
        "--targetfps",
        "--teletextonlypage",
        "--track-number",
        "--DeleteFirst",
        "--DeleteLast",
        "--DeleteContains",
        "--settings",
    };

    private static bool HasFormatOption(string[] args)
    {
        foreach (var arg in args)
        {
            if (arg.Equals("--format", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-f", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("--format=", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("-f=", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("--format:", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("-f:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static bool LooksLikeOption(string arg) =>
        arg.Length >= 2 && arg[0] == '-' &&
        // not a negative number like -42 or -1.5
        !(arg.Length > 1 && (char.IsDigit(arg[1]) || (arg[1] == '.' && arg.Length > 2 && char.IsDigit(arg[2]))));

    /// <summary>
    /// Old SE syntax was <c>seconv &lt;pattern&gt; &lt;format&gt; [options]</c>. If the user
    /// did not pass <c>--format</c>/<c>-f</c> but supplied two or more positional arguments,
    /// treat the last positional as the format and inject it as <c>--format &lt;value&gt;</c>.
    /// </summary>
    private static string[] TryInjectPositionalFormat(string[] args)
    {
        if (args.Length < 2 || HasFormatOption(args))
        {
            return args;
        }

        var positionalIndices = new List<int>();
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (LooksLikeOption(arg))
            {
                // Embedded value form (--opt=value or --opt:value) — single token
                if (arg.Contains('=') || arg.IndexOf(':', 2) > 0)
                {
                    continue;
                }

                // Separate value form (--opt value) — skip the next token as the value
                if (ValueOptions.Contains(arg) && i + 1 < args.Length)
                {
                    i++;
                }
                continue;
            }

            positionalIndices.Add(i);
        }

        if (positionalIndices.Count < 2)
        {
            return args;
        }

        var formatIndex = positionalIndices[^1];
        var formatValue = args[formatIndex];

        var newArgs = new List<string>(args.Length + 1);
        for (var i = 0; i < args.Length; i++)
        {
            if (i == formatIndex)
            {
                continue;
            }
            newArgs.Add(args[i]);
        }
        newArgs.Add("--format");
        newArgs.Add(formatValue);

        return newArgs.ToArray();
    }
}
