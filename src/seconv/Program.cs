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

        // On Windows the console defaults to the legacy OEM codepage, which mangles any
        // non-ASCII output (file names, previews - and it wrote the ¶ HI separator in
        // dump-settings as raw byte 0x14, #12793). Redirected output stays UTF-8 too.
        // Skipped when stdout is not attached to a console handle it can configure.
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch (Exception)
        {
            // No console (service/pipe edge cases) - keep the default encoding.
        }

        // Encoding code pages and the headless EBU UI helper are wired by the
        // module initializer in SeConv.Core.Bootstrap.

        // Handle legacy /convert syntax and convert to modern syntax
        args = ConvertLegacyArguments(args);

        // Spectre matches option names case-sensitively, but seconv has always accepted the
        // SE 4.x casings (/fixcommonerrors, --RedoCasing, --FIX-COMMON-ERRORS). Rewrite any
        // known option to its canonical spelling so strict parsing keeps accepting them.
        args = CanonicalizeOptionCasing(args);

        // Backward-compat: accept the format as the second positional arg
        // (e.g. `seconv *.srt sami`) when --format / -f is not supplied.
        args = TryInjectPositionalFormat(args);

        var json = HasJsonFlag(args);

        // --help-json before the plain help check: it prints the machine-readable schema and
        // must not be swallowed by a looser "did the user ask for help" match.
        if (args.Any(a => a.TrimStart('/', '-').Equals("help-json", StringComparison.OrdinalIgnoreCase)))
        {
            Console.Out.WriteLine(CliSchema.ToJson());
            return 0;
        }

        // Handle /?, /help, --help and -h. All four render the same text: two different help
        // outputs (the hand-written one and Spectre's generated one) gave callers two
        // different pictures of the same tool.
        if (args.Length == 0 || args.Contains("/?") || args.Contains("/help") ||
            args.Contains("--help") || args.Contains("-h"))
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
            return ((ICommand)formatsCommand).ExecuteAsync(null!, new FormatsCommand.Settings { Json = json }, CancellationToken.None).GetAwaiter().GetResult();
        }

        // List helpers
        if (args.Length > 0)
        {
            var first = args[0].TrimStart('/').TrimStart('-');
            if (first.Equals("list-encodings", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintEncodings(json);
                return 0;
            }
            if (first.Equals("list-pac-codepages", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintPacCodepages(json);
                return 0;
            }
            if (first.Equals("list-ocr-engines", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintOcrEngines(json);
                return 0;
            }
            if (first.Equals("list-fce-rules", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintFixCommonErrorsRules(json);
                return 0;
            }
            if (first.Equals("list-rf-rules", StringComparison.OrdinalIgnoreCase) ||
                first.Equals("list-remove-formatting-rules", StringComparison.OrdinalIgnoreCase))
            {
                ListHelpers.PrintRemoveFormattingRules(json);
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
            if (first.Equals("mcp", StringComparison.OrdinalIgnoreCase))
            {
                // MCP server over stdio: from here on stdout carries JSON-RPC frames only.
                return Mcp.McpServerHost.RunAsync(args.Skip(1).ToArray()).GetAwaiter().GetResult();
            }
        }

        // Capture the raw args so the convert command can recover operation order/repetition
        // (Spectre collapses repeated flags and discards ordering).
        ConvertCommand.RawArgs = args;

        // Set up Spectre.Console CLI with default command
        var app = new CommandApp<ConvertCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("seconv");
            config.SetApplicationVersion(CliSchema.Version);

            config.ValidateExamples();

            // Without strict parsing an unrecognised option is silently dropped: a typo'd or
            // invented flag produced a "conversion completed successfully" run, exit 0, and an
            // output file that quietly lacked the requested operation. Failing loudly is the
            // only way a caller can tell the two apart.
            config.UseStrictParsing();

            // Take over error rendering so parse failures exit 1 like every other failure
            // (Spectre's own handler returns -1/255) and can be reported as JSON.
            config.PropagateExceptions();
        });

        // Under strict parsing Spectre retries a failed parse with a synthetic
        // "__default_command" token inserted into the args; a value option missing its value
        // swallows that token and the run "succeeds" (a bare --output-folder converts into a
        // folder literally named __default_command, exit 0). Catch the missing value first.
        var missingValue = FindMissingOptionValue(args);
        if (missingValue != null)
        {
            return ReportUsageError($"Option '{missingValue}' expects a value but none was provided.", json);
        }

        try
        {
            return app.Run(args);
        }
        catch (CommandAppException ex)
        {
            return ReportUsageError(DescribeParseFailure(ex, args), json);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException != null
                ? $"{ex.Message}: {ex.InnerException.Message}"
                : ex.Message;
            return ReportUsageError($"Fatal error: {message}", json);
        }
    }

    /// <summary>
    /// Prints a usage/parse failure and returns exit code 1. Under <c>--json</c> the error
    /// goes out in the same envelope a failed conversion uses, so a caller parsing stdout
    /// never has to handle a sudden switch to plain text.
    /// </summary>
    private static int ReportUsageError(string message, bool json)
    {
        if (json)
        {
            Console.Out.WriteLine(JsonOut.UsageError(message));
        }
        else
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error: {message}[/]");
        }

        return 1;
    }

    /// <summary>
    /// Turns a Spectre parse exception into a message a caller can act on: names the offending
    /// option, suggests the closest real one, and reports the expected type for a value that
    /// failed to convert (Spectre's own text says "Failed to convert 'x' to Nullable`1").
    /// </summary>
    private static string DescribeParseFailure(CommandAppException ex, string[] args)
    {
        var unknown = FindUnknownOption(args);
        if (unknown != null)
        {
            var suggestion = CliSchema.SuggestClosest(unknown);
            var hint = suggestion != null
                ? $" Did you mean '{suggestion}'?"
                : " Run 'seconv --help' for the option list, or 'seconv --help-json' for a machine-readable one.";
            return $"Unknown option '{unknown}'.{hint}";
        }

        var conversion = System.Text.RegularExpressions.Regex.Match(
            ex.Message, @"Failed to convert '(?<value>.*)' to ");
        if (conversion.Success)
        {
            var value = conversion.Groups["value"].Value;
            var (option, type) = FindOptionCarrying(value, args);
            if (option != null)
            {
                return $"Invalid value '{value}' for {option} (expected {type}).";
            }

            return $"Invalid value '{value}'.";
        }

        // Spectre renders its own multi-line diagnostic (with the caret pointer) into the
        // message; keep only the first line so the JSON envelope stays a single string.
        var firstLine = ex.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return (firstLine ?? ex.Message).Trim();
    }

    /// <summary>
    /// First option-looking token that is not a known option name or alias. Matching against
    /// the schema rather than parsing Spectre's message keeps the reported token exactly as
    /// the caller typed it.
    /// </summary>
    internal static string? FindUnknownOption(string[] args)
    {
        var known = new HashSet<string>(CliSchema.AllTokens, StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (!LooksLikeOption(arg))
            {
                continue;
            }

            // Strip an embedded value so --bogus:1 reports as --bogus.
            var name = arg;
            var cut = name.IndexOf('=');
            var colon = name.IndexOf(':', 2);
            if (colon >= 0 && (cut < 0 || colon < cut))
            {
                cut = colon;
            }
            if (cut > 0)
            {
                name = name[..cut];
            }

            if (!known.Contains(name) && !name.Equals("--help", StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }
        }

        return null;
    }

    /// <summary>
    /// Rewrites every known option token to the canonical spelling the Spectre templates
    /// declare, matching case-insensitively and ignoring interior dashes. Embedded values
    /// (<c>--OUTPUTFOLDER:x</c> / <c>--OUTPUTFOLDER=x</c>) are preserved. Unknown tokens
    /// pass through untouched for the strict parser to reject.
    /// </summary>
    internal static string[] CanonicalizeOptionCasing(string[] args)
    {
        var canonicalByKey = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var option in CliSchema.Options)
        {
            foreach (var token in new[] { option.Name }.Concat(option.Aliases))
            {
                canonicalByKey.TryAdd(NormalizeOptionKey(token), option.Name);
            }
        }

        var result = new string[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            result[i] = arg;
            if (!LooksLikeOption(arg))
            {
                continue;
            }

            // Split an embedded value so only the name part is rewritten.
            var name = arg;
            var cut = name.IndexOf('=');
            var colon = name.IndexOf(':', 2);
            if (colon >= 0 && (cut < 0 || colon < cut))
            {
                cut = colon;
            }
            var value = string.Empty;
            if (cut > 0)
            {
                value = name[cut..];
                name = name[..cut];
            }

            if (canonicalByKey.TryGetValue(NormalizeOptionKey(name), out var canonical))
            {
                result[i] = canonical + value;
            }
        }

        return result;
    }

    private static string NormalizeOptionKey(string token) =>
        token.TrimStart('-').Replace("-", string.Empty).ToLowerInvariant();

    /// <summary>
    /// First value-carrying option whose value is missing: the option is the last token, or
    /// the next token is itself a known option. Options with an optional value (Spectre
    /// FlagValue, e.g. <c>--apply-min-gap</c>) are exempt, as are the embedded-value forms
    /// (<c>--opt:value</c> / <c>--opt=value</c>), which never match a bare option token.
    /// </summary>
    internal static string? FindMissingOptionValue(string[] args)
    {
        var allTokens = new HashSet<string>(CliSchema.AllTokens, StringComparer.OrdinalIgnoreCase);
        var valueTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var option in CliSchema.Options)
        {
            if (option.Type == "flag" || option.ValueOptional)
            {
                continue;
            }

            valueTokens.Add(option.Name);
            foreach (var alias in option.Aliases)
            {
                valueTokens.Add(alias);
            }
        }

        for (var i = 0; i < args.Length; i++)
        {
            if (!valueTokens.Contains(args[i]))
            {
                continue;
            }

            if (i + 1 >= args.Length || allTokens.Contains(args[i + 1]))
            {
                return args[i];
            }

            // The next token is this option's value.
            i++;
        }

        return null;
    }

    /// <summary>
    /// Locates the option whose value is <paramref name="value"/>, in either the
    /// <c>--opt:value</c> / <c>--opt=value</c> or the <c>--opt value</c> form, and returns it
    /// with the type the schema declares for it.
    /// </summary>
    private static (string? Option, string Type) FindOptionCarrying(string value, string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!LooksLikeOption(arg))
            {
                continue;
            }

            string? name = null;
            var separator = arg.IndexOfAny(['=', ':'], 2);
            if (separator > 0 && arg[(separator + 1)..] == value)
            {
                name = arg[..separator];
            }
            else if (i + 1 < args.Length && args[i + 1] == value)
            {
                name = arg;
            }

            if (name == null)
            {
                continue;
            }

            var option = CliSchema.Options.FirstOrDefault(o =>
                o.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                o.Aliases.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase)));

            return (name, option?.Type ?? "value");
        }

        return (null, "value");
    }

    /// <summary>
    /// Converts legacy SE 4.x <c>/parameter:value</c> syntax to modern <c>--parameter:value</c>.
    /// Skips path-like arguments (anything containing a path separator after the leading
    /// slash) so Unix absolute paths and Windows drive paths pass through unchanged.
    /// </summary>
    internal static string[] ConvertLegacyArguments(string[] args)
    {
        var converted = new List<string>();

        foreach (var arg in args)
        {
            // SE 4.x invocations start with a "/convert" verb; modern seconv has no such
            // option, so drop the token entirely — the remaining pattern/format positionals
            // and rewritten /option:value args match the modern grammar.
            if (arg.Equals("/convert", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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
    // Derived from the reflected CLI schema so the set cannot drift from the options the
    // parser actually binds (a hand-maintained copy went stale and broke valid command
    // lines). FlagValue options ("--apply-min-gap [MS]") count as value-taking here: a
    // separate value token following one must not be mistaken for a positional.
    private static readonly HashSet<string> ValueOptions = new(
        CliSchema.Options
            .Where(o => o.Type != "flag")
            .SelectMany(o => new[] { o.Name }.Concat(o.Aliases)),
        StringComparer.OrdinalIgnoreCase);

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

                // Separate value form (--opt value) — skip the next token as the value.
                // An option-looking next token is never a value: it belongs to an option
                // with an optional value (--apply-min-gap --output-folder x), or the value
                // is simply missing, which FindMissingOptionValue reports later.
                if (ValueOptions.Contains(arg) && i + 1 < args.Length && !LooksLikeOption(args[i + 1]))
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
