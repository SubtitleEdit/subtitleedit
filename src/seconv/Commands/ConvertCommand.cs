using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics;
using SeConv.Core;

namespace SeConv.Commands;

internal sealed class ConvertCommand : AsyncCommand<ConvertCommand.Settings>
{
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

        [CommandOption("--adjustduration")]
        [Description("Adjust duration in milliseconds")]
        public int? AdjustDuration { get; init; }

        [CommandOption("--assa-style-file")]
        [Description("ASSA style file name")]
        public string? AssaStyleFile { get; init; }

        [CommandOption("--ebuheaderfile")]
        [Description("EBU header file name")]
        public string? EbuHeaderFile { get; init; }

        [CommandOption("--encoding")]
        [Description("Encoding name")]
        public string? Encoding { get; init; }

        [CommandOption("--forcedonly")]
        [Description("Forced subtitles only")]
        public bool ForcedOnly { get; init; }

        [CommandOption("--fps")]
        [Description("Frame rate")]
        public double? Fps { get; init; }

        [CommandOption("--inputfolder")]
        [Description("Input folder name")]
        public string? InputFolder { get; init; }

        [CommandOption("--multiplereplace")]
        [Description("Path to a Subtitle Edit MultipleSearchAndReplaceGroups XML file applied per-paragraph before save")]
        public string? MultipleReplace { get; init; }

        [CommandOption("--customformat")]
        [Description("Path to a Subtitle Edit custom-format XML file (used with --format customtext)")]
        public string? CustomFormat { get; init; }

        [CommandOption("--ocrengine")]
        [Description("OCR engine: tesseract | nocr | ollama | paddle (default: tesseract)")]
        public string? OcrEngine { get; init; }

        [CommandOption("--ocrlanguage")]
        [Description("Language for OCR (Tesseract: ISO 639-2 like eng/deu; Paddle: en/de; Ollama: human name like English)")]
        public string? OcrLanguage { get; init; }

        [CommandOption("--ocrdb")]
        [Description("Path to a .nocr database file (required when --ocrengine=nocr)")]
        public string? OcrDb { get; init; }

        [CommandOption("--ollama-url")]
        [Description("Ollama API endpoint (default: http://localhost:11434/api/chat)")]
        public string? OllamaUrl { get; init; }

        [CommandOption("--ollama-model")]
        [Description("Ollama vision model (default: llama3.2-vision)")]
        public string? OllamaModel { get; init; }

        [CommandOption("--offset")]
        [Description("Offset time (hh:mm:ss:ms)")]
        public string? Offset { get; init; }

        [CommandOption("--outputfilename")]
        [Description("Output file name (for single file only)")]
        public string? OutputFilename { get; init; }

        [CommandOption("--outputfolder")]
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

        [CommandOption("--targetfps")]
        [Description("Target frame rate")]
        public double? TargetFps { get; init; }

        [CommandOption("--teletextonly")]
        [Description("Teletext only")]
        public bool TeletextOnly { get; init; }

        [CommandOption("--teletextonlypage")]
        [Description("Teletext page number")]
        public int? TeletextOnlyPage { get; init; }

        [CommandOption("--track-number")]
        [Description("Comma separated track number list")]
        public string? TrackNumber { get; init; }

        // Operations
        [CommandOption("--ApplyDurationLimits")]
        [Description("Apply duration limits")]
        public bool ApplyDurationLimits { get; init; }

        [CommandOption("--BalanceLines")]
        [Description("Balance lines")]
        public bool BalanceLines { get; init; }

        [CommandOption("--BeautifyTimeCodes")]
        [Description("Beautify time codes")]
        public bool BeautifyTimeCodes { get; init; }

        [CommandOption("--ConvertColorsToDialog")]
        [Description("Convert colors to dialog")]
        public bool ConvertColorsToDialog { get; init; }

        [CommandOption("--DeleteFirst")]
        [Description("Delete first N entries")]
        public int? DeleteFirst { get; init; }

        [CommandOption("--DeleteLast")]
        [Description("Delete last N entries")]
        public int? DeleteLast { get; init; }

        [CommandOption("--DeleteContains")]
        [Description("Delete entries containing this word")]
        public string? DeleteContains { get; init; }

        [CommandOption("--FixCommonErrors")]
        [Description("Fix common errors")]
        public bool FixCommonErrors { get; init; }

        [CommandOption("--FixRtlViaUnicodeChars")]
        [Description("Fix RTL via Unicode characters")]
        public bool FixRtlViaUnicodeChars { get; init; }

        [CommandOption("--MergeSameTexts")]
        [Description("Merge same texts")]
        public bool MergeSameTexts { get; init; }

        [CommandOption("--MergeSameTimeCodes")]
        [Description("Merge same time codes")]
        public bool MergeSameTimeCodes { get; init; }

        [CommandOption("--MergeShortLines")]
        [Description("Merge short lines")]
        public bool MergeShortLines { get; init; }

        [CommandOption("--RedoCasing")]
        [Description("Redo casing")]
        public bool RedoCasing { get; init; }

        [CommandOption("--RemoveFormatting")]
        [Description("Remove formatting")]
        public bool RemoveFormatting { get; init; }

        [CommandOption("--RemoveLineBreaks")]
        [Description("Remove line breaks")]
        public bool RemoveLineBreaks { get; init; }

        [CommandOption("--RemoveTextForHI")]
        [Description("Remove text for hearing impaired")]
        public bool RemoveTextForHI { get; init; }

        [CommandOption("--RemoveUnicodeControlChars")]
        [Description("Remove Unicode control characters")]
        public bool RemoveUnicodeControlChars { get; init; }

        [CommandOption("--ReverseRtlStartEnd")]
        [Description("Reverse RTL start/end")]
        public bool ReverseRtlStartEnd { get; init; }

        [CommandOption("--SplitLongLines")]
        [Description("Split long lines")]
        public bool SplitLongLines { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!settings.Quiet)
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

            // Validate --ocrengine: tesseract | nocr | ollama | paddle
            var supportedEngines = new[] { "tesseract", "nocr", "ollama", "paddle", "paddleocr" };
            if (!string.IsNullOrWhiteSpace(settings.OcrEngine) &&
                !supportedEngines.Contains(settings.OcrEngine, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Error: OCR engine '{settings.OcrEngine.EscapeMarkup()}' is not supported. " +
                    $"Use one of: {string.Join(", ", supportedEngines)}.[/]");
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
                    AnsiConsole.MarkupLine($"[red]Error loading --settings file: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            }
            else if (!string.IsNullOrWhiteSpace(settings.Profile))
            {
                AnsiConsole.MarkupLine("[red]Error: --profile requires --settings:<path.json>[/]");
                return 1;
            }

            // Build operations list
            var operations = new List<string>();
            if (settings.ApplyDurationLimits) operations.Add("ApplyDurationLimits");
            if (settings.BalanceLines) operations.Add("BalanceLines");
            if (settings.BeautifyTimeCodes) operations.Add("BeautifyTimeCodes");
            if (settings.ConvertColorsToDialog) operations.Add("ConvertColorsToDialog");
            if (settings.FixCommonErrors) operations.Add("FixCommonErrors");
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
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
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
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
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
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
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
                Fps = settings.Fps,
                TargetFps = settings.TargetFps,
                Overwrite = settings.Overwrite,
                Operations = operations,
                DeleteFirst = settings.DeleteFirst,
                DeleteLast = settings.DeleteLast,
                DeleteContains = settings.DeleteContains,
                Offset = offset,
                Renumber = settings.Renumber,
                AdjustDurationMs = settings.AdjustDuration,
                Resolution = resolution,
                AssaStyleFile = settings.AssaStyleFile,
                PacCodePage = pacCodePage,
                EbuHeaderFile = settings.EbuHeaderFile,
                MultipleReplaceFile = settings.MultipleReplace,
                CustomFormatFile = settings.CustomFormat,
                TrackNumbers = ParseTrackNumbers(settings.TrackNumber),
                ForcedOnly = settings.ForcedOnly,
                OcrEngine = string.IsNullOrWhiteSpace(settings.OcrEngine) ? "tesseract" : settings.OcrEngine,
                OcrLanguage = settings.OcrLanguage ?? "eng",
                OcrDb = settings.OcrDb,
                OllamaUrl = settings.OllamaUrl,
                OllamaModel = settings.OllamaModel,
                TeletextOnly = settings.TeletextOnly,
                TeletextOnlyPage = settings.TeletextOnlyPage,
                Quiet = settings.Quiet,
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

            if (operations.Count > 0)
                table.AddRow("Operations", string.Join(", ", operations));

            if (settings.DeleteFirst.HasValue)
                table.AddRow("Delete First", settings.DeleteFirst.Value.ToString());

            if (settings.DeleteLast.HasValue)
                table.AddRow("Delete Last", settings.DeleteLast.Value.ToString());

            if (!string.IsNullOrEmpty(settings.DeleteContains))
                table.AddRow("Delete Contains", settings.DeleteContains);

            if (!settings.Quiet)
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
                        AnsiConsole.MarkupLine($"  [red]•[/] {error.EscapeMarkup()}");
                    }
                }

                return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLine($"[dim]{ex.InnerException.Message}[/]");
            }
            return 1;
        }
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
