using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics;
using SeConv.Core;

namespace SeConv.Commands;

internal sealed class ConvertCommand : AsyncCommand<ConvertCommand.Settings>
{
    /// <summary>
    /// The original process arguments, captured in Program.Main before Spectre parsing.
    /// Used to recover operation order/repetition, which the bound settings discard.
    /// </summary>
    public static string[] RawArgs { get; set; } = [];

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<pattern>")]
        [Description("One or more file name patterns. Use quotes around paths that contain spaces (e.g. \"my file.srt\"). Pass multiple patterns as separate quoted arguments (e.g. \"file1.srt\" \"file2.srt\") or as a single comma-separated value (e.g. \"*.srt,*.ass\").")]
        public string[] Pattern { get; init; } = [];

        [CommandOption("--format|-f")]
        [Description("Name of format without spaces")]
        public string Format { get; init; } = string.Empty;

        [CommandOption("--quiet|-q")]
        [Description("Suppress per-file progress; only print the final summary")]
        public bool Quiet { get; init; }

        [CommandOption("--verbose|-v")]
        [Description("Print extra diagnostic information")]
        public bool Verbose { get; init; }

        [CommandOption("--json")]
        [Description("Emit per-file results as JSON to stdout (suppresses table/progress output)")]
        public bool Json { get; init; }

        [CommandOption("--adjust-duration|--adjustduration")]
        [Description("Adjust duration in milliseconds")]
        public int? AdjustDuration { get; init; }

        [CommandOption("--change-speed")]
        [Description("Change speed by percent (e.g. 125 = 1.25x faster, 80 = 0.8x slower)")]
        public double? ChangeSpeed { get; init; }

        [CommandOption("--assa-style-file")]
        [Description("ASSA style file name")]
        public string? AssaStyleFile { get; init; }

        [CommandOption("--ebu-header-file|--ebuheaderfile")]
        [Description("EBU header file name")]
        public string? EbuHeaderFile { get; init; }

        [CommandOption("--encoding")]
        [Description("Encoding name (e.g. utf-8, utf-8-no-bom, windows-1252, 1252). Use 'source' to keep the input file's detected encoding.")]
        public string? Encoding { get; init; }

        [CommandOption("--input-encoding-fallback|--inputencodingfallback")]
        [Description("Encoding to assume when input is not UTF-8 / has no BOM (skips ANSI auto-detection). Ignored when --encoding is set.")]
        public string? InputEncodingFallback { get; init; }

        [CommandOption("--forced-only|--forcedonly")]
        [Description("Forced subtitles only")]
        public bool ForcedOnly { get; init; }

        [CommandOption("--fps")]
        [Description("Frame rate")]
        public double? Fps { get; init; }

        [CommandOption("--input-folder|--inputfolder")]
        [Description("Input folder name")]
        public string? InputFolder { get; init; }

        [CommandOption("--multiple-replace|--multiplereplace")]
        [Description("Path to a Subtitle Edit MultipleSearchAndReplaceGroups XML file applied per-paragraph before save")]
        public string? MultipleReplace { get; init; }

        [CommandOption("--custom-format|--customformat")]
        [Description("Path to a Subtitle Edit custom-format XML file (used with --format customtext)")]
        public string? CustomFormat { get; init; }

        [CommandOption("--plaintext-merge|--plaintextmerge")]
        [Description("Plain text output: merge all subtitles into one space-separated block (no blank lines)")]
        public bool PlainTextMerge { get; init; }

        [CommandOption("--plaintext-unbreak|--plaintextunbreak")]
        [Description("Plain text output: unbreak each subtitle, joining its lines into one")]
        public bool PlainTextUnbreak { get; init; }

        [CommandOption("--plaintext-no-blank-line|--plaintextnoblankline")]
        [Description("Plain text output: do not put a blank line between subtitles (default keeps it)")]
        public bool PlainTextNoBlankLine { get; init; }

        [CommandOption("--ocr-engine|--ocrengine")]
        [Description("OCR engine: tesseract | nocr | binaryocr | ollama | paddle (default: tesseract)")]
        public string? OcrEngine { get; init; }

        [CommandOption("--ocr-language|--ocrlanguage")]
        [Description("Language for OCR (Tesseract: ISO 639-2 like eng/deu; Paddle: en/de; Ollama: human name like English)")]
        public string? OcrLanguage { get; init; }

        [CommandOption("--ocr-db|--ocrdb")]
        [Description("Path to a .nocr file (--ocr-engine=nocr) or .db file (--ocr-engine=binaryocr)")]
        public string? OcrDb { get; init; }

        [CommandOption("--dictionary-folder|--dictionaryfolder")]
        [Description("Folder with Hunspell dictionaries + *_OCRFixReplaceList.xml; enables the 'Fix common OCR errors' pass of --fix-common-errors")]
        public string? DictionaryFolder { get; init; }

        [CommandOption("--time-codes-only|--timecodesonly")]
        [Description("For image-based sources (.sup, VobSub .sub/.idx, MKV PGS/VobSub, MP4 VobSub, TS DVB-sub): output time codes only with empty text; skips OCR (no OCR engine required)")]
        public bool TimeCodesOnly { get; init; }

        [CommandOption("--no-vobsub-isolate-colors|--novobsubisolatecolors")]
        [Description("Disable VobSub OCR colour isolation (on by default). Isolation binarises each subpicture to black-on-white by keeping the most frequent opaque colour (glyph fill) and dropping the outline/anti-alias colours; pass this to OCR the raw palette instead")]
        public bool NoVobSubIsolateColors { get; init; }

        [CommandOption("--ollama-url")]
        [Description("Ollama API endpoint (default: http://localhost:11434/api/chat)")]
        public string? OllamaUrl { get; init; }

        [CommandOption("--ollama-model")]
        [Description("Ollama vision model (default: llama3.2-vision)")]
        public string? OllamaModel { get; init; }

        [CommandOption("--offset")]
        [Description("Offset time (hh:mm:ss:ms)")]
        public string? Offset { get; init; }

        [CommandOption("--output-filename|--outputfilename")]
        [Description("Output file name (for single file only)")]
        public string? OutputFilename { get; init; }

        [CommandOption("--output-folder|--outputfolder")]
        [Description("Output folder name")]
        public string? OutputFolder { get; init; }

        [CommandOption("--overwrite")]
        [Description("Overwrite existing files")]
        public bool Overwrite { get; init; }

        [CommandOption("--pac-codepage")]
        [Description("PAC code page")]
        public string? PacCodepage { get; init; }

        [CommandOption("--profile")]
        [Description("Profile name")]
        public string? Profile { get; init; }

        [CommandOption("--settings")]
        [Description("Path to a JSON settings file overriding libse defaults")]
        public string? SettingsPath { get; init; }

        [CommandOption("--renumber")]
        [Description("Renumber starting from this number")]
        public int? Renumber { get; init; }

        [CommandOption("--resolution")]
        [Description("Resolution (widthxheight)")]
        public string? Resolution { get; init; }

        [CommandOption("--target-fps|--targetfps")]
        [Description("Target frame rate")]
        public double? TargetFps { get; init; }

        [CommandOption("--teletext-only|--teletextonly")]
        [Description("Teletext only")]
        public bool TeletextOnly { get; init; }

        [CommandOption("--teletext-only-page|--teletextonlypage")]
        [Description("Teletext page number")]
        public int? TeletextOnlyPage { get; init; }

        [CommandOption("--track-number")]
        [Description("Comma separated track number list")]
        public string? TrackNumber { get; init; }

        // Operations
        // Each option exposes the canonical lowercase-hyphenated form first; the
        // PascalCase alias is kept for backward compatibility with older scripts.
        [CommandOption("--apply-duration-limits|--ApplyDurationLimits")]
        [Description("Apply duration limits")]
        public bool ApplyDurationLimits { get; init; }

        [CommandOption("--apply-min-gap|--ApplyMinGap")]
        [Description("Enforce a minimum gap of N ms between paragraphs")]
        public int? ApplyMinGap { get; init; }

        [CommandOption("--bridge-gaps|--BridgeGaps")]
        [Description("Bridge gaps shorter than N ms by extending the previous end time")]
        public int? BridgeGaps { get; init; }

        [CommandOption("--balance-lines|--BalanceLines")]
        [Description("Balance lines")]
        public bool BalanceLines { get; init; }

        [CommandOption("--beautify-time-codes|--BeautifyTimeCodes")]
        [Description("Beautify time codes")]
        public bool BeautifyTimeCodes { get; init; }

        [CommandOption("--convert-colors-to-dialog|--ConvertColorsToDialog")]
        [Description("Convert colors to dialog")]
        public bool ConvertColorsToDialog { get; init; }

        [CommandOption("--delete-first|--DeleteFirst")]
        [Description("Delete first N entries")]
        public int? DeleteFirst { get; init; }

        [CommandOption("--delete-last|--DeleteLast")]
        [Description("Delete last N entries")]
        public int? DeleteLast { get; init; }

        [CommandOption("--delete-contains|--DeleteContains")]
        [Description("Delete entries containing this word")]
        public string? DeleteContains { get; init; }

        [CommandOption("--fix-common-errors|--FixCommonErrors")]
        [Description("Fix common errors")]
        public bool FixCommonErrors { get; init; }

        [CommandOption("--fix-common-errors-rules|--FixCommonErrorsRules")]
        [Description("Comma-separated FCE rule IDs (or 'all,-RuleId' to subtract). See: seconv list-fce-rules")]
        public string? FixCommonErrorsRules { get; init; }

        [CommandOption("--fix-rtl-via-unicode-chars|--FixRtlViaUnicodeChars")]
        [Description("Fix RTL via Unicode characters")]
        public bool FixRtlViaUnicodeChars { get; init; }

        [CommandOption("--merge-same-texts|--MergeSameTexts")]
        [Description("Merge same texts")]
        public bool MergeSameTexts { get; init; }

        [CommandOption("--merge-same-time-codes|--MergeSameTimeCodes")]
        [Description("Merge same time codes")]
        public bool MergeSameTimeCodes { get; init; }

        [CommandOption("--merge-short-lines|--MergeShortLines")]
        [Description("Merge short lines")]
        public bool MergeShortLines { get; init; }

        [CommandOption("--redo-casing|--RedoCasing")]
        [Description("Redo casing")]
        public bool RedoCasing { get; init; }

        [CommandOption("--remove-formatting|--RemoveFormatting")]
        [Description("Remove formatting")]
        public bool RemoveFormatting { get; init; }

        [CommandOption("--remove-line-breaks|--RemoveLineBreaks")]
        [Description("Remove line breaks")]
        public bool RemoveLineBreaks { get; init; }

        [CommandOption("--remove-text-for-hi|--RemoveTextForHI")]
        [Description("Remove text for hearing impaired")]
        public bool RemoveTextForHI { get; init; }

        [CommandOption("--remove-unicode-control-chars|--RemoveUnicodeControlChars")]
        [Description("Remove Unicode control characters")]
        public bool RemoveUnicodeControlChars { get; init; }

        [CommandOption("--reverse-rtl-start-end|--ReverseRtlStartEnd")]
        [Description("Reverse RTL start/end")]
        public bool ReverseRtlStartEnd { get; init; }

        [CommandOption("--split-long-lines|--SplitLongLines")]
        [Description("Split long lines")]
        public bool SplitLongLines { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            // --json implies the same per-file silence as --quiet, but adds a final
            // JSON document. Header + parameter table are suppressed in both modes.
            var silent = settings.Quiet || settings.Json;

            if (!silent)
            {
                AnsiConsole.MarkupLine("[bold cyan]Subtitle Edit - Batch Converter[/]");
                AnsiConsole.WriteLine();
            }

            // Validate input
            if (settings.Pattern.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]Error: Pattern is required[/]");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(settings.Format))
            {
                AnsiConsole.MarkupLine("[red]Error: Format is required. Use --format <name> or pass it as the second positional argument (e.g. seconv *.srt sami)[/]");
                return 1;
            }

            // Validate --ocr-engine: tesseract | nocr | binaryocr | ollama | paddle
            var supportedEngines = new[] { "tesseract", "nocr", "binaryocr", "binary", "ollama", "paddle", "paddleocr" };
            if (!string.IsNullOrWhiteSpace(settings.OcrEngine) &&
                !supportedEngines.Contains(settings.OcrEngine, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Error: OCR engine '{settings.OcrEngine.EscapeMarkup()}' is not supported (pass via --ocr-engine). " +
                    $"Use one of: {string.Join(", ", supportedEngines)}.[/]");
                return 1;
            }

            // Fail fast on a typo in --encoding so we don't silently substitute UTF-8 and
            // decode the input incorrectly without the user noticing. The literal "source"
            // is a sentinel — resolved per-file from the input's detected encoding.
            if (!string.IsNullOrWhiteSpace(settings.Encoding) &&
                !LibSEIntegration.IsSourceEncodingSentinel(settings.Encoding) &&
                !LibSEIntegration.TryGetEncoding(settings.Encoding, out _))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Error: Unknown encoding '{settings.Encoding.EscapeMarkup()}' for --encoding.[/]");
                AnsiConsole.MarkupLine("[dim]Use 'seconv list-encodings' to see supported encodings, or 'source' to keep the input file's encoding.[/]");
                return 1;
            }

            // Fail fast on a typo in --input-encoding-fallback so we don't silently substitute
            // UTF-8 and decode the input incorrectly without the user noticing.
            if (!string.IsNullOrWhiteSpace(settings.InputEncodingFallback) &&
                !LibSEIntegration.TryGetEncoding(settings.InputEncodingFallback, out _))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Error: Unknown encoding '{settings.InputEncodingFallback.EscapeMarkup()}' for --input-encoding-fallback.[/]");
                AnsiConsole.MarkupLine("[dim]Use 'seconv list-encodings' to see supported encodings.[/]");
                return 1;
            }

            // Load --settings:path.json overrides into libse Configuration before any conversion.
            // --profile <name> selects a named overlay from the same file.
            if (!string.IsNullOrWhiteSpace(settings.SettingsPath))
            {
                try
                {
                    SeConvSettings.Load(settings.SettingsPath).ApplyToLibSe(settings.Profile);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Error loading --settings file: {ex.Message}[/]");
                    return 1;
                }
            }
            else if (!string.IsNullOrWhiteSpace(settings.Profile))
            {
                AnsiConsole.MarkupLine("[red]Error: --profile requires --settings:<path.json>[/]");
                return 1;
            }

            // Resolve FCE rule selection. Passing --FixCommonErrorsRules implicitly
            // enables --FixCommonErrors so users don't have to specify both.
            // We also parse out which rules the user named by hand — that's a separate
            // signal from the resolved set so language-conditional rules can bypass
            // their gate when explicitly requested (see FixCommonErrorsRunner.Run).
            IReadOnlyList<string> fceRules = [];
            IReadOnlyList<string> fceExplicitlyNamed = [];
            var fceRequested = settings.FixCommonErrors || !string.IsNullOrWhiteSpace(settings.FixCommonErrorsRules);
            if (fceRequested)
            {
                try
                {
                    fceRules = FixCommonErrorsRunner.ResolveRuleIds(settings.FixCommonErrorsRules);
                    fceExplicitlyNamed = FixCommonErrorsRunner.ParseExplicitlyNamedRules(settings.FixCommonErrorsRules);
                }
                catch (ArgumentException ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Error: {ex.Message}[/]");
                    return 1;
                }
            }

            // Build operations list. Operations run in the order the user typed them and
            // repeat for each occurrence (SE4 parity) - e.g. "--fix-common-errors" twice
            // runs two FCE passes. Spectre collapses repeated flags, so the order/count is
            // recovered from the raw args. Fall back to a fixed order if raw args are missing.
            List<string> operations;
            if (RawArgs.Length > 0)
            {
                operations = OperationOrderParser.BuildOperations(RawArgs, fceRequested);
            }
            else
            {
                operations = new List<string>();
                if (settings.ApplyDurationLimits) operations.Add("ApplyDurationLimits");
                if (settings.BalanceLines) operations.Add("BalanceLines");
                if (settings.BeautifyTimeCodes) operations.Add("BeautifyTimeCodes");
                if (settings.ConvertColorsToDialog) operations.Add("ConvertColorsToDialog");
                if (fceRequested) operations.Add("FixCommonErrors");
                if (settings.FixRtlViaUnicodeChars) operations.Add("FixRtlViaUnicodeChars");
                if (settings.MergeSameTexts) operations.Add("MergeSameTexts");
                if (settings.MergeSameTimeCodes) operations.Add("MergeSameTimeCodes");
                if (settings.MergeShortLines) operations.Add("MergeShortLines");
                if (settings.RedoCasing) operations.Add("RedoCasing");
                if (settings.RemoveFormatting) operations.Add("RemoveFormatting");
                if (settings.RemoveLineBreaks) operations.Add("RemoveLineBreaks");
                if (settings.RemoveTextForHI) operations.Add("RemoveTextForHI");
                if (settings.RemoveUnicodeControlChars) operations.Add("RemoveUnicodeControlChars");
                if (settings.ReverseRtlStartEnd) operations.Add("ReverseRtlStartEnd");
                if (settings.SplitLongLines) operations.Add("SplitLongLines");
            }

            // Validate --change-speed (must be > 0; 100 means no change)
            if (settings.ChangeSpeed.HasValue && settings.ChangeSpeed.Value <= 0)
            {
                AnsiConsole.MarkupLine($"[red]Error: --change-speed must be greater than 0 (got {settings.ChangeSpeed.Value}).[/]");
                return 1;
            }

            // Parse offset if supplied
            TimeSpan? offset = null;
            if (!string.IsNullOrWhiteSpace(settings.Offset))
            {
                try
                {
                    offset = OffsetParser.Parse(settings.Offset);
                }
                catch (FormatException ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Error: {ex.Message}[/]");
                    return 1;
                }
            }

            // Parse resolution if supplied
            (int Width, int Height)? resolution = null;
            if (!string.IsNullOrWhiteSpace(settings.Resolution))
            {
                try
                {
                    resolution = ResolutionParser.Parse(settings.Resolution);
                }
                catch (FormatException ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Error: {ex.Message}[/]");
                    return 1;
                }
            }

            // Parse PAC code page if supplied
            int? pacCodePage = null;
            if (!string.IsNullOrWhiteSpace(settings.PacCodepage))
            {
                try
                {
                    pacCodePage = PacCodepageParser.Parse(settings.PacCodepage);
                }
                catch (FormatException ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Error: {ex.Message}[/]");
                    return 1;
                }
            }

            // Create conversion options
            var options = new ConversionOptions
            {
                Patterns = settings.Pattern,
                Format = settings.Format,
                InputFolder = settings.InputFolder,
                OutputFolder = settings.OutputFolder,
                OutputFilename = settings.OutputFilename,
                Encoding = settings.Encoding,
                InputEncodingFallback = settings.InputEncodingFallback,
                Fps = settings.Fps,
                TargetFps = settings.TargetFps,
                Overwrite = settings.Overwrite,
                Operations = operations,
                FixCommonErrorsRules = fceRules,
                FixCommonErrorsExplicitlyNamedRules = fceExplicitlyNamed,
                DeleteFirst = settings.DeleteFirst,
                DeleteLast = settings.DeleteLast,
                DeleteContains = settings.DeleteContains,
                Offset = offset,
                Renumber = settings.Renumber,
                AdjustDurationMs = settings.AdjustDuration,
                ChangeSpeedPercent = settings.ChangeSpeed,
                BridgeGapsMaxMs = settings.BridgeGaps,
                ApplyMinGapMs = settings.ApplyMinGap,
                Resolution = resolution,
                AssaStyleFile = settings.AssaStyleFile,
                PacCodePage = pacCodePage,
                EbuHeaderFile = settings.EbuHeaderFile,
                MultipleReplaceFile = settings.MultipleReplace,
                CustomFormatFile = settings.CustomFormat,
                PlainTextMerge = settings.PlainTextMerge,
                PlainTextUnbreak = settings.PlainTextUnbreak,
                PlainTextLineBetweenSubtitles = !settings.PlainTextNoBlankLine,
                TrackNumbers = ParseTrackNumbers(settings.TrackNumber),
                ForcedOnly = settings.ForcedOnly,
                OcrEngine = string.IsNullOrWhiteSpace(settings.OcrEngine) ? "tesseract" : settings.OcrEngine,
                OcrLanguage = settings.OcrLanguage ?? "eng",
                OcrDb = settings.OcrDb,
                DictionaryFolder = settings.DictionaryFolder,
                TimeCodesOnly = settings.TimeCodesOnly,
                VobSubIsolateColors = !settings.NoVobSubIsolateColors,
                OllamaUrl = settings.OllamaUrl,
                OllamaModel = settings.OllamaModel,
                TeletextOnly = settings.TeletextOnly,
                TeletextOnlyPage = settings.TeletextOnlyPage,
                Quiet = silent,
                Verbose = settings.Verbose,
            };

            // Display conversion info
            var normalizedFormat = LibSEIntegration.NormalizeFormatName(settings.Format);
            var extension = LibSEIntegration.GetExtensionForFormat(settings.Format);
            var formatDisplay = $"{normalizedFormat} (*{extension})";

            var table = new Table();
            table.AddColumn("[yellow]Parameter[/]");
            table.AddColumn("[green]Value[/]");
            table.AddRow("Pattern", string.Join(", ", settings.Pattern));
            table.AddRow("Format", formatDisplay);

            if (!string.IsNullOrEmpty(settings.InputFolder))
                table.AddRow("Input Folder", settings.InputFolder);

            if (!string.IsNullOrEmpty(settings.OutputFolder))
                table.AddRow("Output Folder", settings.OutputFolder);

            if (settings.Fps.HasValue)
                table.AddRow("FPS", settings.Fps.Value.ToString());

            if (settings.TargetFps.HasValue)
                table.AddRow("Target FPS", settings.TargetFps.Value.ToString());

            if (!string.IsNullOrEmpty(settings.Encoding))
                table.AddRow("Encoding", settings.Encoding);

            if (string.IsNullOrEmpty(settings.Encoding) && !string.IsNullOrEmpty(settings.InputEncodingFallback))
                table.AddRow("Input encoding fallback", settings.InputEncodingFallback);

            if (operations.Count > 0)
                table.AddRow("Operations", string.Join(", ", operations));

            if (settings.DeleteFirst.HasValue)
                table.AddRow("Delete First", settings.DeleteFirst.Value.ToString());

            if (settings.DeleteLast.HasValue)
                table.AddRow("Delete Last", settings.DeleteLast.Value.ToString());

            if (!string.IsNullOrEmpty(settings.DeleteContains))
                table.AddRow("Delete Contains", settings.DeleteContains);

            if (!silent)
            {
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }

            // Perform conversion
            var stopwatch = Stopwatch.StartNew();
            var converter = new SubtitleConverter();
            var result = await converter.ConvertAsync(options);
            stopwatch.Stop();

            var elapsed = FormatElapsed(stopwatch.Elapsed);

            if (settings.Json)
            {
                EmitJsonResult(result, stopwatch.Elapsed);
                return result.Success ? 0 : 1;
            }

            // Display results
            AnsiConsole.WriteLine();
            if (result.Success)
            {
                AnsiConsole.MarkupLine($"[green]✓[/] Conversion completed successfully!");
                AnsiConsole.MarkupLine($"[dim]Converted {result.SuccessfulFiles} file(s) in {elapsed}[/]");
            }
            else
            {
                if (result.SuccessfulFiles > 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]⚠[/] Conversion completed with errors");
                    AnsiConsole.MarkupLine($"[green]Successful: {result.SuccessfulFiles}[/]");
                    AnsiConsole.MarkupLine($"[red]Failed: {result.FailedFiles}[/]");
                    AnsiConsole.MarkupLine($"[dim]Elapsed: {elapsed}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗[/] Conversion failed");
                }

                if (result.Errors.Count > 0)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[red]Errors:[/]");
                    foreach (var error in result.Errors)
                    {
                        AnsiConsole.MarkupLineInterpolated($"  [red]•[/] {error}");
                    }
                }

                return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (settings.Json)
            {
                Console.Error.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message }));
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Error: {ex.Message}[/]");
                if (ex.InnerException != null)
                {
                    AnsiConsole.MarkupLineInterpolated($"[dim]{ex.InnerException.Message}[/]");
                }
            }
            return 1;
        }
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static void EmitJsonResult(ConversionResult result, TimeSpan elapsed)
    {
        var payload = new
        {
            success = result.Success,
            totalFiles = result.TotalFiles,
            successfulFiles = result.SuccessfulFiles,
            failedFiles = result.FailedFiles,
            elapsedMs = (long)elapsed.TotalMilliseconds,
            files = result.Files.Select(f => new
            {
                input = f.Input,
                output = f.Output,
                success = f.Success,
                error = f.Error,
            }),
            errors = result.Errors,
        };
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static List<int> ParseTrackNumbers(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }
        var nums = new List<int>();
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part.Trim(), out var n))
            {
                nums.Add(n);
            }
        }
        return nums;
    }

    private static string FormatElapsed(TimeSpan e)
    {
        if (e.TotalSeconds < 1)
        {
            return $"{e.TotalMilliseconds:F0} ms";
        }
        if (e.TotalMinutes < 1)
        {
            return $"{e.TotalSeconds:F1} s";
        }
        return $"{(int)e.TotalMinutes}m {e.Seconds}s";
    }
}
