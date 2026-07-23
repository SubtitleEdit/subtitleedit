using Spectre.Console;
using Spectre.Console.Cli;
using SeConv.Commands;
using SeConv.Core;
using SeConv.Helpers;

namespace SeConv;

internal class Program
{
    static int Main(string[] args)
    {
        // Must run before any SkiaSharp.HarfBuzz use (image-based export) so the bundled
        // libHarfBuzzSharp deep-binds its own hb_* symbols and doesn't crash on Linux (#11864).
        Nikse.SubtitleEdit.UiLogic.HarfBuzzNativeFix.Apply();

        // Encoding code pages and the headless EBU UI helper are wired by the
        // module initializer in SeConv.Core.Bootstrap.

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

        // List helpers
        if (args.Length > 0)
        {
            var first = args[0].TrimStart('/').TrimStart('-');
            if (first.Equals("list-encodings", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintEncodings();
                return 0;
            }
            if (first.Equals("list-pac-codepages", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintPacCodepages();
                return 0;
            }
            if (first.Equals("list-ocr-engines", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintOcrEngines();
                return 0;
            }
            if (first.Equals("list-fce-rules", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintFixCommonErrorsRules();
                return 0;
            }
            if (first.Equals("dump-settings", StringComparison.OrdinalIgnoreCase) ||
                first.Equals("default-settings", StringComparison.OrdinalIgnoreCase))
            {
                // Pure JSON to stdout so `seconv dump-settings > my.json` yields a clean file;
                // the usage hint goes to stderr so it never lands in the redirected output.
                Console.Out.WriteLine(SeConvSettings.DumpDefaults());
                Console.Error.WriteLine(
                    "// Above: seconv's settings schema with the current libse defaults. " +
                    "Redirect to a file and pass it via --settings:<path.json>. " +
                    "Note: these are seconv's key names, not the SE GUI's Settings.json.");
                return 0;
            }
            if (first.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                return RunInfoCommand(args.Skip(1).ToArray());
            }
            if (first.Equals("lint", StringComparison.OrdinalIgnoreCase))
            {
                return RunLintCommand(args.Skip(1).ToArray());
            }
        }

        // Capture the raw args so the convert command can recover operation order/repetition
        // (Spectre collapses repeated flags and discards ordering).
        ConvertCommand.RawArgs = args;

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
            AnsiConsole.MarkupLineInterpolated($"[red]Fatal error: {ex.Message}[/]");
            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]  {ex.InnerException.Message}[/]");
            }
            return 1;
        }
    }

    /// <summary>
    /// Converts legacy SE 4.x <c>/parameter:value</c> syntax to modern <c>--parameter:value</c>.
    /// Skips path-like arguments (anything containing a path separator after the leading
    /// slash) so Unix absolute paths and Windows drive paths pass through unchanged.
    /// </summary>
    private static string[] ConvertLegacyArguments(string[] args)
    {
        var converted = new List<string>();

        foreach (var arg in args)
        {
            if (arg.StartsWith('/') && arg != "/?" && arg != "/help" && !LooksLikePath(arg))
            {
                converted.Add("--" + arg.Substring(1));
            }
            else
            {
                converted.Add(arg);
            }
        }

        return converted.ToArray();
    }

    /// <summary>
    /// Heuristic: a leading-slash arg is a Unix-style path if it contains another <c>/</c>
    /// somewhere after position 0 (e.g. <c>/etc/foo</c>), or already contains a backslash
    /// (e.g. <c>/c/Users/...</c> in MSYS) — neither shape matches the legacy <c>/option:value</c>
    /// form, where the value sits after a <c>:</c>.
    /// </summary>
    private static bool LooksLikePath(string arg)
    {
        if (arg.Length < 2)
        {
            return false;
        }
        var rest = arg.AsSpan(1);
        // Legacy options use ':' as the value separator (e.g. /encoding:utf-8) — the option
        // name part is before the first ':'. Only that part shouldn't contain path separators;
        // the value can contain anything (including drive letters and backslashes).
        var colonIdx = rest.IndexOf(':');
        var nameSpan = colonIdx >= 0 ? rest[..colonIdx] : rest;
        return nameSpan.Contains('/') || nameSpan.Contains('\\');
    }

    // Options that take a value as the next argument. Used to know which positional
    // tokens are file patterns vs. option-values when injecting a positional format.
    // Canonical lowercase-hyphenated names; legacy smashed/PascalCase aliases included for
    // backward compatibility with older scripts.
    private static readonly HashSet<string> ValueOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "--format", "-f",
        "--adjust-duration", "--adjustduration",
        "--assa-style-file",
        "--change-speed",
        "--custom-format", "--customformat",
        "--ebu-header-file", "--ebuheaderfile",
        "--encoding",
        "--input-encoding-fallback", "--inputencodingfallback",
        "--fps",
        "--input-folder", "--inputfolder",
        "--multiple-replace", "--multiplereplace",
        "--ocr-engine", "--ocrengine",
        "--ocr-language", "--ocrlanguage",
        "--ocr-db", "--ocrdb",
        "--offset",
        "--output-filename", "--outputfilename",
        "--output-folder", "--outputfolder",
        "--pac-codepage",
        "--profile",
        "--renumber",
        "--resolution",
        "--settings",
        "--target-fps", "--targetfps",
        "--teletext-only-page", "--teletextonlypage",
        "--track-number",
        "--ollama-url",
        "--ollama-model",
        "--translate-to", "--translateto",
        "--translate-from", "--translatefrom",
        "--translate-engine", "--translateengine",
        "--translate-url", "--translateurl",
        "--translate-model", "--translatemodel",
        "--delete-first", "--DeleteFirst",
        "--delete-last", "--DeleteLast",
        "--delete-contains", "--DeleteContains",
        "--fix-common-errors-rules", "--FixCommonErrorsRules",
        "--bridge-gaps", "--BridgeGaps",
        "--apply-min-gap", "--ApplyMinGap",
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

    /// <summary>
    /// Dispatches <c>seconv info &lt;file&gt; [--json]</c>. Returns 0 on success, 1 on
    /// error. The single positional argument must be an existing file — info does no
    /// glob expansion, since the output shape is a single record.
    /// </summary>
    private static int RunInfoCommand(string[] args)
    {
        var json = HasJsonFlag(args);
        var positional = args.Where(a => !LooksLikeOption(a)).ToArray();

        if (positional.Length != 1)
        {
            AnsiConsole.MarkupLine("[red]Error: 'seconv info' requires exactly one file argument.[/]");
            AnsiConsole.MarkupLine("[dim]Usage: seconv info <file> [[--json]][/]");
            return 1;
        }

        try
        {
            var info = SubtitleInfoGatherer.Gather(positional[0]);
            InfoLintReporter.RenderInfo(info, json);
            return 0;
        }
        catch (Exception ex)
        {
            if (json)
            {
                Console.Error.WriteLine($"{{\"error\": {System.Text.Json.JsonSerializer.Serialize(ex.Message)}}}");
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Error: {ex.Message}[/]");
            }
            return 1;
        }
    }

    /// <summary>
    /// Dispatches <c>seconv lint &lt;pattern&gt;... [--json]</c>. Each pattern is glob-
    /// expanded the same way <see cref="Core.SubtitleConverter"/> expands its inputs.
    /// Returns 0 if all matched files are clean, 1 if any issues are found or any file
    /// fails to load.
    /// </summary>
    private static int RunLintCommand(string[] args)
    {
        var json = HasJsonFlag(args);
        var patterns = args.Where(a => !LooksLikeOption(a)).ToArray();

        if (patterns.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error: 'seconv lint' requires at least one file or pattern.[/]");
            AnsiConsole.MarkupLine("[dim]Usage: seconv lint <pattern>... [[--json]][/]");
            return 1;
        }

        var files = ExpandPatterns(patterns);
        if (files.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No files matched.[/]");
            return 1;
        }

        var reports = new List<LintReport>();
        var hasError = false;
        foreach (var file in files)
        {
            try
            {
                reports.Add(SubtitleLinter.Lint(file));
            }
            catch (Exception ex)
            {
                hasError = true;
                reports.Add(new LintReport
                {
                    Path = file,
                    Issues = [new LintIssue
                    {
                        Type = "load-error",
                        ParagraphNumber = 0,
                        Message = ex.Message,
                    }],
                });
            }
        }

        InfoLintReporter.RenderLint(reports, json);
        return hasError || reports.Any(r => r.Issues.Count > 0) ? 1 : 0;
    }

    private static bool HasJsonFlag(string[] args) =>
        args.Any(a =>
            a.Equals("--json", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("-json", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("/json", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Expands shell glob patterns (e.g. <c>*.srt</c>) into a flat list of existing
    /// files. Mirrors <see cref="Core.SubtitleConverter.GetFiles"/> but without the
    /// input-folder option, since lint takes patterns directly.
    /// </summary>
    private static List<string> ExpandPatterns(IEnumerable<string> patterns)
    {
        var files = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pattern in patterns)
        {
            var trimmed = pattern.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            // Bare existing file path — no globbing needed.
            if (File.Exists(trimmed))
            {
                if (seen.Add(trimmed))
                {
                    files.Add(trimmed);
                }
                continue;
            }

            var directory = Path.GetDirectoryName(trimmed);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }
            var filePattern = Path.GetFileName(trimmed);

            if (Directory.Exists(directory))
            {
                foreach (var match in Directory.GetFiles(directory, filePattern))
                {
                    if (seen.Add(match))
                    {
                        files.Add(match);
                    }
                }
            }
        }

        return files;
    }
}
