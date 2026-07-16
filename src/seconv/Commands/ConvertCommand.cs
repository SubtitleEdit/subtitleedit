using Nikse.SubtitleEdit.Core.Common;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
        [Description("Path to a Subtitle Edit multiple-replace rules file applied per-paragraph before save. Accepts the legacy MultipleSearchAndReplaceGroups XML and the SE5 GUI export (.template JSON or .csv)")]
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
        [Description("OCR engine: tesseract | nocr | binaryocr | ollama | llamacpp | paddle (default: tesseract)")]
        public string? OcrEngine { get; init; }

        [CommandOption("--ocr-language|--ocrlanguage")]
        [Description("Language for OCR (Tesseract: ISO 639-2 like eng/deu; Paddle: en/de; Ollama/llama.cpp: human name like English)")]
        public string? OcrLanguage { get; init; }

        [CommandOption("--ocr-db|--ocrdb")]
        [Description("Path to a .nocr file (--ocr-engine=nocr) or .db file (--ocr-engine=binaryocr)")]
        public string? OcrDb { get; init; }

        [CommandOption("--ocr-model|--ocrmodel")]
        [Description("llama.cpp OCR model: curated .gguf file name or full path (default: first downloaded OCR model)")]
        public string? OcrModel { get; init; }

        [CommandOption("--ocr-url|--ocrurl")]
        [Description("llama.cpp OCR: endpoint of an already-running llama-server; skips the local auto-start")]
        public string? OcrUrl { get; init; }

        [CommandOption("--dictionary-folder|--dictionaryfolder")]
        [Description("Folder with Hunspell dictionaries + *_OCRFixReplaceList.xml; enables the 'Fix common OCR errors' pass of --fix-common-errors")]
        public string? DictionaryFolder { get; init; }

        [CommandOption("--time-codes-only|--timecodesonly")]
        [Description("For image-based sources (.sup, VobSub .sub/.idx, MKV PGS/VobSub, MP4 VobSub, TS DVB-sub): output time codes only with empty text; skips OCR (no OCR engine required)")]
        public bool TimeCodesOnly { get; init; }

        [CommandOption("--no-vobsub-isolate-colors|--novobsubisolatecolors")]
        [Description("Disable VobSub OCR colour isolation (on by default). Isolation binarises each subpicture to black-on-white by keeping the most frequent opaque colour (glyph fill) and dropping the outline/anti-alias colours; pass this to OCR the raw palette instead")]
        public bool NoVobSubIsolateColors { get; init; }

        [CommandOption("--no-pgs-isolate-colors|--nopgsisolatecolors")]
        [Description("Disable PGS/DVB-sub OCR colour isolation (on by default)")]
        public bool NoPgsIsolateColors { get; init; }

        [CommandOption("--ollama-url")]
        [Description("Ollama API endpoint (default: http://localhost:11434/api/chat)")]
        public string? OllamaUrl { get; init; }

        [CommandOption("--ollama-model")]
        [Description("Ollama vision model (default: llama3.2-vision)")]
        public string? OllamaModel { get; init; }

        [CommandOption("--translate-to|--translateto")]
        [Description("Auto-translate to this language (code or English name, e.g. de or German); enables translation")]
        public string? TranslateTo { get; init; }

        [CommandOption("--translate-from|--translatefrom")]
        [Description("Auto-translate source language (default: auto-detect per file)")]
        public string? TranslateFrom { get; init; }

        [CommandOption("--translate-engine|--translateengine")]
        [Description("Translate engine: llamacpp (default) | ollama | lmstudio | libretranslate | nllb-serve | nllb-api")]
        public string? TranslateEngine { get; init; }

        [CommandOption("--translate-url|--translateurl")]
        [Description("Endpoint of an already-running translate server; for llamacpp this skips the local llama-server auto-start")]
        public string? TranslateUrl { get; init; }

        [CommandOption("--translate-model|--translatemodel")]
        [Description("Translate model: ollama/lmstudio model name, or llamacpp .gguf file name/path (default: first downloaded translate model)")]
        public string? TranslateModel { get; init; }

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

        // Image output styling (Blu-Ray sup, VobSub, BDN-XML, ...). Flags override the
        // settings JSON's exportImages section, which overrides the built-in defaults.
        [CommandOption("--font-name|--fontname")]
        [Description("Image output: font family name (default: Arial)")]
        public string? FontName { get; init; }

        [CommandOption("--font-size|--fontsize")]
        [Description("Image output: font size in points (default: 50)")]
        public float? FontSize { get; init; }

        [CommandOption("--font-color|--fontcolor")]
        [Description("Image output: text colour as hex (#AARRGGBB/#RRGGBB) or name (default: white)")]
        public string? FontColor { get; init; }

        [CommandOption("--font-bold|--fontbold")]
        [Description("Image output: render text bold")]
        public bool FontBold { get; init; }

        [CommandOption("--outline-color|--outlinecolor")]
        [Description("Image output: outline colour (default: black)")]
        public string? OutlineColor { get; init; }

        [CommandOption("--outline-width|--outlinewidth")]
        [Description("Image output: outline width in pixels (default: 2.5; 0 disables)")]
        public double? OutlineWidth { get; init; }

        [CommandOption("--shadow-color|--shadowcolor")]
        [Description("Image output: shadow colour (default: black)")]
        public string? ShadowColor { get; init; }

        [CommandOption("--shadow-width|--shadowwidth")]
        [Description("Image output: shadow width in pixels (default: 0 = off)")]
        public double? ShadowWidth { get; init; }

        [CommandOption("--background-color|--backgroundcolor")]
        [Description("Image output: background box colour, e.g. black or #B4000000 (semi-transparent black). Implies --box-type:one-box unless --box-type is given")]
        public string? BackgroundColor { get; init; }

        [CommandOption("--background-corner-radius|--backgroundcornerradius")]
        [Description("Image output: corner radius of the background box (default: 0)")]
        public double? BackgroundCornerRadius { get; init; }

        [CommandOption("--box-type|--boxtype")]
        [Description("Image output: background box style: none | one-box | box-per-line")]
        public string? BoxType { get; init; }

        [CommandOption("--box-padding|--boxpadding")]
        [Description("Image output: box padding in pixels; one value for all sides or left,right,top,bottom (default: 5,5,3,3)")]
        public string? BoxPadding { get; init; }

        [CommandOption("--line-spacing|--linespacing")]
        [Description("Image output: extra gap between lines as percent of line height (default: 0)")]
        public int? LineSpacing { get; init; }

        [CommandOption("--alignment")]
        [Description("Image output: screen position, e.g. bottom-center (default), top-left, middle-right")]
        public string? Alignment { get; init; }

        [CommandOption("--content-alignment|--contentalignment")]
        [Description("Image output: multi-line text justification: left | center (default) | right")]
        public string? ContentAlignment { get; init; }

        [CommandOption("--bottom-top-margin|--bottomtopmargin")]
        [Description("Image output: vertical screen-edge margin in pixels (default: 5% of height)")]
        public int? BottomTopMargin { get; init; }

        [CommandOption("--left-right-margin|--leftrightmargin")]
        [Description("Image output: horizontal screen-edge margin in pixels (default: 5% of width)")]
        public int? LeftRightMargin { get; init; }

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

        // Optional value: bare --apply-min-gap uses minimumMillisecondsBetweenLines from
        // --settings (or the libse default), matching what the UI does (issue #12437).
        [CommandOption("--apply-min-gap|--ApplyMinGap [MS]")]
        [Description("Enforce a minimum gap between paragraphs; omit the value to use minimumMillisecondsBetweenLines")]
        public FlagValue<string>? ApplyMinGap { get; init; }

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

            // Validate --ocr-engine: tesseract | nocr | binaryocr | ollama | llamacpp | paddle
            var supportedEngines = new[] { "tesseract", "nocr", "binaryocr", "binary", "ollama", "llamacpp", "llama.cpp", "llama", "paddle", "paddleocr" };
            if (!string.IsNullOrWhiteSpace(settings.OcrEngine) &&
                !supportedEngines.Contains(settings.OcrEngine, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Error: OCR engine '{settings.OcrEngine.EscapeMarkup()}' is not supported (pass via --ocr-engine). " +
                    "Use one of: tesseract, nocr, binaryocr, ollama, llamacpp, paddle.[/]");
                return 1;
            }

            // --ocr-model/--ocr-url only apply to the llama.cpp OCR engine - fail fast instead
            // of silently ignoring them under the default (tesseract) engine.
            var isLlamaCppOcr = !string.IsNullOrWhiteSpace(settings.OcrEngine) &&
                                settings.OcrEngine.Trim().ToLowerInvariant() is "llamacpp" or "llama.cpp" or "llama";
            if ((!string.IsNullOrWhiteSpace(settings.OcrModel) || !string.IsNullOrWhiteSpace(settings.OcrUrl)) && !isLlamaCppOcr)
            {
                AnsiConsole.MarkupLine("[red]Error: --ocr-model/--ocr-url require --ocr-engine:llamacpp.[/]");
                return 1;
            }

            // Validate the translate options: --translate-to is the trigger, the rest refine it.
            if (string.IsNullOrWhiteSpace(settings.TranslateTo) &&
                (!string.IsNullOrWhiteSpace(settings.TranslateFrom) ||
                 !string.IsNullOrWhiteSpace(settings.TranslateEngine) ||
                 !string.IsNullOrWhiteSpace(settings.TranslateUrl) ||
                 !string.IsNullOrWhiteSpace(settings.TranslateModel)))
            {
                AnsiConsole.MarkupLine("[red]Error: --translate-from/--translate-engine/--translate-url/--translate-model require --translate-to:<language>.[/]");
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(settings.TranslateEngine) &&
                !AutoTranslateRunner.SupportedEngines.Contains(settings.TranslateEngine.Trim(), StringComparer.OrdinalIgnoreCase) &&
                !settings.TranslateEngine.Trim().Equals("llama.cpp", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Error: Translate engine '{settings.TranslateEngine.EscapeMarkup()}' is not supported (pass via --translate-engine). " +
                    $"Use one of: {string.Join(", ", AutoTranslateRunner.SupportedEngines)}.[/]");
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
            var imageStyle = new ImageExportStyle();
            if (!string.IsNullOrWhiteSpace(settings.SettingsPath))
            {
                try
                {
                    var seConvSettings = SeConvSettings.Load(settings.SettingsPath);
                    seConvSettings.ApplyToLibSe(settings.Profile);
                    seConvSettings.ApplyExportImages(imageStyle, settings.Profile);

                    // Unknown keys are ignored by the JSON reader, which used to mean a typo - or a
                    // key from a newer seconv - silently produced default output (issue #12437).
                    var unknownKeys = seConvSettings.GetUnknownKeys().ToList();
                    if (unknownKeys.Count > 0 && !silent)
                    {
                        AnsiConsole.MarkupLineInterpolated(
                            $"[yellow]Warning: ignoring unknown key(s) in --settings file: {string.Join(", ", unknownKeys)}[/]");
                        AnsiConsole.MarkupLine(
                            "[yellow]Check for typos, or update seconv if the key was added in a newer version.[/]");
                    }
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

            // Image styling flags override the settings JSON. Unlike the JSON (which only warns
            // about unknown keys), a bad flag value fails fast so the user notices.
            var imageStyleError = ApplyImageStyleFlags(settings, imageStyle);
            if (imageStyleError != null)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Error: {imageStyleError}[/]");
                return 1;
            }

            // Must run after the --settings JSON is applied: a bare --apply-min-gap takes its
            // value from libse's (possibly overridden) MinimumMillisecondsBetweenLines.
            if (!TryResolveApplyMinGap(settings, silent, out var applyMinGapMs))
            {
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
                ApplyMinGapMs = applyMinGapMs,
                Resolution = resolution,
                ImageStyle = imageStyle,
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
                PgsIsolateColors = !settings.NoPgsIsolateColors,
                OllamaUrl = settings.OllamaUrl,
                OllamaModel = settings.OllamaModel,
                OcrUrl = settings.OcrUrl,
                OcrModel = settings.OcrModel,
                TranslateTo = settings.TranslateTo,
                TranslateFrom = settings.TranslateFrom,
                TranslateEngine = settings.TranslateEngine,
                TranslateUrl = settings.TranslateUrl,
                TranslateModel = settings.TranslateModel,
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

            if (!string.IsNullOrWhiteSpace(settings.TranslateTo))
            {
                var translateEngine = string.IsNullOrWhiteSpace(settings.TranslateEngine) ? "llamacpp" : settings.TranslateEngine.Trim().ToLowerInvariant();
                var translateFrom = string.IsNullOrWhiteSpace(settings.TranslateFrom) ? "auto" : settings.TranslateFrom;
                table.AddRow("Translate", $"{translateFrom} -> {settings.TranslateTo} ({translateEngine})");
            }

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

    /// <summary>
    /// Resolves --apply-min-gap into the gap to enforce, in ms (null = the operation is off).
    /// A bare --apply-min-gap uses libse's MinimumMillisecondsBetweenLines, so it follows the
    /// --settings JSON. Returns false when the value is unusable, after printing the error.
    /// </summary>
    private static bool TryResolveApplyMinGap(Settings settings, bool silent, out int? gapMs)
    {
        gapMs = null;
        if (settings.ApplyMinGap?.IsSet != true)
        {
            return true;
        }

        // FlagValue only carries the raw text, so "no value given" stays distinguishable from ":0".
        var raw = settings.ApplyMinGap.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            gapMs = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            return true;
        }

        if (!int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[red]Error: --apply-min-gap expects a value in milliseconds, got '{raw}'.[/]");
            return false;
        }

        if (ms <= 0)
        {
            // Enforcing a gap of zero is a no-op. Saying so beats silently doing nothing.
            if (!silent)
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[yellow]Warning: --apply-min-gap:{ms} enforces no gap and does nothing. Omit the value to use minimumMillisecondsBetweenLines.[/]");
            }

            return true;
        }

        gapMs = ms;
        return true;
    }

    /// <summary>
    /// Applies the image-styling CLI flags on top of <paramref name="style"/>.
    /// Returns an error message on an unparsable value, null on success.
    /// </summary>
    private static string? ApplyImageStyleFlags(Settings settings, ImageExportStyle style)
    {
        if (!string.IsNullOrWhiteSpace(settings.FontName))
        {
            style.FontName = settings.FontName;
        }

        if (settings.FontSize.HasValue)
        {
            if (settings.FontSize.Value <= 0)
            {
                return $"--font-size must be greater than 0 (got {settings.FontSize.Value}).";
            }
            style.FontSize = settings.FontSize.Value;
        }

        if (!string.IsNullOrWhiteSpace(settings.FontColor))
        {
            if (!ImageExportStyle.TryParseColor(settings.FontColor, out var fontColor))
            {
                return $"Unknown colour '{settings.FontColor}' for --font-color. Use hex (#AARRGGBB/#RRGGBB) or a colour name like white.";
            }
            style.FontColor = fontColor;
        }

        if (settings.FontBold)
        {
            style.IsBold = true;
        }

        if (!string.IsNullOrWhiteSpace(settings.OutlineColor))
        {
            if (!ImageExportStyle.TryParseColor(settings.OutlineColor, out var outlineColor))
            {
                return $"Unknown colour '{settings.OutlineColor}' for --outline-color.";
            }
            style.OutlineColor = outlineColor;
        }

        if (settings.OutlineWidth.HasValue)
        {
            style.OutlineWidth = settings.OutlineWidth.Value;
        }

        if (!string.IsNullOrWhiteSpace(settings.ShadowColor))
        {
            if (!ImageExportStyle.TryParseColor(settings.ShadowColor, out var shadowColor))
            {
                return $"Unknown colour '{settings.ShadowColor}' for --shadow-color.";
            }
            style.ShadowColor = shadowColor;
        }

        if (settings.ShadowWidth.HasValue)
        {
            style.ShadowWidth = settings.ShadowWidth.Value;
        }

        if (!string.IsNullOrWhiteSpace(settings.BackgroundColor))
        {
            if (!ImageExportStyle.TryParseColor(settings.BackgroundColor, out var backgroundColor))
            {
                return $"Unknown colour '{settings.BackgroundColor}' for --background-color.";
            }
            style.BackgroundColor = backgroundColor;
        }

        if (settings.BackgroundCornerRadius.HasValue)
        {
            style.BackgroundCornerRadius = settings.BackgroundCornerRadius.Value;
        }

        if (!string.IsNullOrWhiteSpace(settings.BoxType))
        {
            if (!ImageExportStyle.TryParseBoxType(settings.BoxType, out var boxType))
            {
                return $"Unknown box type '{settings.BoxType}' for --box-type. Use: none, one-box, or box-per-line.";
            }
            style.BoxType = boxType;
        }

        if (!string.IsNullOrWhiteSpace(settings.BoxPadding))
        {
            var parts = settings.BoxPadding.Split(',', StringSplitOptions.TrimEntries);
            var values = new List<int>();
            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var n) || n < 0)
                {
                    values.Clear();
                    break;
                }
                values.Add(n);
            }

            if (values.Count == 1)
            {
                style.BoxPaddingLeft = values[0];
                style.BoxPaddingRight = values[0];
                style.BoxPaddingTop = values[0];
                style.BoxPaddingBottom = values[0];
            }
            else if (values.Count == 4)
            {
                style.BoxPaddingLeft = values[0];
                style.BoxPaddingRight = values[1];
                style.BoxPaddingTop = values[2];
                style.BoxPaddingBottom = values[3];
            }
            else
            {
                return $"Invalid --box-padding '{settings.BoxPadding}'. Use one value for all sides (e.g. 5) or four: left,right,top,bottom (e.g. 5,5,3,3).";
            }
        }

        if (settings.LineSpacing.HasValue)
        {
            style.LineSpacingPercent = settings.LineSpacing.Value;
        }

        if (!string.IsNullOrWhiteSpace(settings.Alignment))
        {
            if (!ImageExportStyle.TryParseAlignment(settings.Alignment, out var alignment))
            {
                return $"Unknown alignment '{settings.Alignment}' for --alignment. Use e.g. bottom-center, top-left, middle-right.";
            }
            style.Alignment = alignment;
        }

        if (!string.IsNullOrWhiteSpace(settings.ContentAlignment))
        {
            if (!ImageExportStyle.TryParseContentAlignment(settings.ContentAlignment, out var contentAlignment))
            {
                return $"Unknown content alignment '{settings.ContentAlignment}' for --content-alignment. Use: left, center, or right.";
            }
            style.ContentAlignment = contentAlignment;
        }

        if (settings.BottomTopMargin.HasValue)
        {
            style.BottomTopMargin = settings.BottomTopMargin.Value;
        }

        if (settings.LeftRightMargin.HasValue)
        {
            style.LeftRightMargin = settings.LeftRightMargin.Value;
        }

        return null;
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
