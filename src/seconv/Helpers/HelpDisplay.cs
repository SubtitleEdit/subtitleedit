using Spectre.Console;

namespace SeConv.Helpers;

internal static class HelpDisplay
{
    public static void ShowHelp() => ShowHelp(AnsiConsole.Console);

    /// <summary>
    /// Renders the help text to a string at a fixed width. Lets tests assert on the real
    /// rendered output without recording the global console, which xunit runs in parallel.
    /// </summary>
    internal static string Render(int width)
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(writer),
        });
        console.Profile.Width = width;
        ShowHelp(console);
        return writer.ToString();
    }

    private static void ShowHelp(IAnsiConsole console)
    {
        var rule = new Rule("[bold yellow]Subtitle Edit 5.0 - Batch Converter[/]");
        rule.LeftJustified();
        console.Write(rule);
        console.WriteLine();

        console.MarkupLine("[bold cyan]Usage:[/]");
        console.MarkupLine("  [green]seconv[/] [yellow][[pattern]] [[name-of-format-without-spaces]][/] [blue][[optional-parameters]][/]");
        console.WriteLine();

        ShowSection(console, "Pattern", "One or more file name patterns separated by commas\nRelative patterns are relative to --input-folder if specified");
        ShowSection(console, "Values", "Option values accept all of --option:value, --option=value and --option value.\nAn unrecognised option is an error, never a silent no-op.");

        ShowSection(console, "Optional Parameters", null);
        ShowParameter(console, "--adjust-duration:<ms>", "Adjust duration in milliseconds");
        ShowParameter(console, "--assa-style-file:<file name>", "ASSA style file");
        ShowParameter(console, "--change-speed:<percent>", "Change speed by percent (e.g. 125 = 1.25x faster)");
        ShowParameter(console, "--ebu-header-file:<file name>", "EBU header file");
        ShowParameter(console, "--encoding:<encoding name>", "Character encoding (e.g., utf-8, windows-1252, or 'source' to keep the input file's encoding)");
        ShowParameter(console, "--input-encoding-fallback:<name>", "Assumed input encoding when no BOM and not UTF-8 (skips ANSI guess)");
        ShowParameter(console, "--forced-only", "Process forced subtitles only");
        ShowParameter(console, "--fps:<frame rate>", "Frame rate for conversion");
        ShowParameter(console, "--input-folder:<folder name>", "Input folder path");
        ShowParameter(console, "--offset:hh:mm:ss:ms", "Time offset");
        ShowParameter(console, "--output-filename:<file name>", "Output file name (for single file only)");
        ShowParameter(console, "--output-folder:<folder name>", "Output folder path");
        ShowParameter(console, "--overwrite", "Overwrite existing files");
        ShowParameter(console, "--keep-timestamp", "Give output files the source file's modified/created date instead of the conversion time");
        ShowParameter(console, "--pac-codepage:<code page>", "PAC code page");
        ShowParameter(console, "--profile:<profile name>", "Profile name");
        ShowParameter(console, "--renumber:<starting number>", "Renumber subtitles from this number");
        ShowParameter(console, "--resolution:<width>x<height>", "Video resolution (e.g., 1920x1080)");
        ShowParameter(console, "--target-fps:<frame rate>", "Target frame rate");
        ShowParameter(console, "--teletext-only", "Process teletext only");
        ShowParameter(console, "--teletext-only-page:<page number>", "Teletext page number");
        ShowParameter(console, "--track-number:<track list>", "Comma separated track number list");
        ShowParameter(console, "--ocr-engine:<engine>", "OCR engine: tesseract | nocr | binaryocr | ollama | llamacpp | paddle");
        ShowParameter(console, "--ocr-language:<lang>", "Language for OCR (e.g. eng, deu, spa)");
        ShowParameter(console, "--ocr-db:<path>", ".nocr (--ocr-engine=nocr) or .db (--ocr-engine=binaryocr)");
        ShowParameter(console, "--ocr-model:<model>", "llamacpp OCR .gguf file name/path (default: first downloaded OCR model)");
        ShowParameter(console, "--ocr-prompt:<text|file>", "prompt for llamacpp/ollama OCR; {language} = --ocr-language (default: same as the OCR window)");
        ShowParameter(console, "--ocr-url:<url>", "Endpoint of an already-running llama-server for OCR (skips the auto-start)");
        ShowParameter(console, "--time-codes-only", "Image sources (.sup/VobSub/PGS/DVB/XSUB) -> text with time codes only; skips OCR");
        ShowParameter(console, "--no-vobsub-isolate-colors", "Disable VobSub OCR colour isolation (on by default; isolation binarises to black-on-white, dropping outline colours)");
        ShowParameter(console, "--no-pgs-isolate-colors", "Disable PGS/DVB-sub OCR colour isolation (on by default; isolation binarises to black-on-white so the white glyph fill survives the OCR canvas)");
        ShowParameter(console, "--ollama-url:<url>", "Ollama API endpoint (default: http://localhost:11434/api/chat)");
        ShowParameter(console, "--ollama-model:<model>", "Ollama vision model (default: llama3.2-vision)");
        ShowParameter(console, "--translate-to:<lang>", "Auto-translate to this language (code or English name, e.g. de or German)");
        ShowParameter(console, "--translate-from:<lang>", "Auto-translate source language (default: auto-detect per file)");
        ShowParameter(console, "--translate-engine:<engine>", "llamacpp (default; auto-starts a local llama-server) | ollama | lmstudio | libretranslate | nllb-serve | nllb-api");
        ShowParameter(console, "--translate-url:<url>", "Endpoint of an already-running translate server (llamacpp: skips the auto-start)");
        ShowParameter(console, "--translate-model:<model>", "Ollama/LM Studio model name, or llamacpp .gguf file name/path");
        ShowParameter(console, "--translate-prompt:<text|file>", "Prompt for llamacpp/ollama/lmstudio: inline text (\\n = line break) or a text file; {0}=source, {1}=target, {2}=text (completion-format models)");
        ShowParameter(console, "--multiple-replace:<path.xml>", "SE MultipleSearchAndReplaceGroups XML applied per paragraph");
        ShowParameter(console, "--custom-format:<path.xml>", "SE CustomFormatItem XML (use with --format customtext)");
        ShowParameter(console, "--settings:<path.json>", "JSON settings file overriding libse defaults");
        ShowParameter(console, "--quiet", "Suppress per-file progress; only print the final summary");
        ShowParameter(console, "--verbose", "Print extra diagnostic information");
        ShowParameter(console, "--json", "Emit per-file results as JSON (suppresses Spectre output)");

        console.WriteLine();
        ShowSection(console, "Plain Text Output", "Apply when the target format is plaintext.");
        ShowParameter(console, "--plaintext-merge", "Merge all subtitles into one space-separated block (no blank lines)");
        ShowParameter(console, "--plaintext-unbreak", "Unbreak each subtitle, joining its lines into one");
        ShowParameter(console, "--plaintext-no-blank-line", "Do not put a blank line between subtitles (default keeps it)");

        console.WriteLine();
        ShowSection(console, "Image Output", "Apply when the target is an image-based format (e.g. bluraysup, vobsub).");
        ShowParameter(console, "--font-name:<name>", "Font family name (default: Arial)");
        ShowParameter(console, "--font-size:<points>", "Font size in points (default: 50)");
        ShowParameter(console, "--font-color:<colour>", "Text colour as hex (#AARRGGBB/#RRGGBB) or name (default: white)");
        ShowParameter(console, "--font-bold", "Render text bold");
        ShowParameter(console, "--outline-color:<colour>", "Outline colour (default: black)");
        ShowParameter(console, "--outline-width:<px>", "Outline width in pixels (default: 2.5; 0 disables)");
        ShowParameter(console, "--shadow-color:<colour>", "Shadow colour (default: black)");
        ShowParameter(console, "--shadow-width:<px>", "Shadow width in pixels (default: 0 = off)");
        ShowParameter(console, "--background-color:<colour>", "Background box colour, e.g. black or #B4000000. Implies --box-type:one-box unless --box-type is given");
        ShowParameter(console, "--background-corner-radius:<px>", "Corner radius of the background box (default: 0)");
        ShowParameter(console, "--box-type:<type>", "Background box style: none | one-box | box-per-line");
        ShowParameter(console, "--box-padding:<px>", "Box padding in pixels; one value for all sides or left,right,top,bottom (default: 5,5,3,3)");
        ShowParameter(console, "--line-spacing:<percent>", "Extra gap between lines as percent of line height (default: 0)");
        ShowParameter(console, "--alignment:<position>", "Screen position, e.g. bottom-center (default), top-left, middle-right");
        ShowParameter(console, "--content-alignment:<mode>", "Multi-line text justification: left | center (default) | right | from-alignment");
        ShowParameter(console, "--bottom-top-margin:<px>", "Vertical screen-edge margin in pixels (default: 5% of height)");
        ShowParameter(console, "--left-right-margin:<px>", "Horizontal screen-edge margin in pixels (default: 5% of width)");

        console.WriteLine();
        console.MarkupLine("[bold cyan]Operations:[/]");
        console.MarkupLine("  [dim]Operations are applied in a fixed, sensible order regardless of CLI order.[/]");
        console.WriteLine();
        
        ShowParameter(console, "--apply-duration-limits", "Apply duration limits");
        ShowParameter(console, "--apply-min-gap[:<ms>]", "Enforce minimum gap between paragraphs (default: minimumMillisecondsBetweenLines)");
        ShowParameter(console, "--balance-lines", "Balance line lengths");
        ShowParameter(console, "--bridge-gaps:<ms>", "Bridge gaps shorter than N ms (extends previous end time)");
        ShowParameter(console, "--beautify-time-codes", "Beautify time codes");
        ShowParameter(console, "--convert-colors-to-dialog", "Convert colors to dialog");
        ShowParameter(console, "--delete-first:<count>", "Delete first N entries");
        ShowParameter(console, "--delete-last:<count>", "Delete last N entries");
        ShowParameter(console, "--delete-contains:<word>", "Delete entries containing specified word");
        ShowParameter(console, "--fix-common-errors", "Fix common subtitle errors (all rules)");
        ShowParameter(console, "--fix-common-errors-rules:<list>", "FCE rule IDs (csv); supports 'all,-RuleId'. List: seconv list-fce-rules");
        ShowParameter(console, "--fce-language:<code>", "Force language for FCE language-gated rules (e.g. es or Spanish); default: auto-detect");
        ShowParameter(console, "--dictionary-folder:<path>", "Hunspell dictionaries + *_OCRFixReplaceList.xml; enables the 'Fix common OCR errors' FCE pass");
        ShowParameter(console, "--fix-rtl-via-unicode-chars", "Fix RTL via Unicode characters");
        ShowParameter(console, "--merge-same-texts", "Merge entries with same text");
        ShowParameter(console, "--merge-same-time-codes", "Merge entries with same time codes");
        ShowParameter(console, "--merge-short-lines", "Merge short lines");
        ShowParameter(console, "--redo-casing", "Redo text casing");
        ShowParameter(console, "--remove-formatting", "Remove all formatting tags");
        ShowParameter(console, "--remove-formatting-rules:<list>", "Formatting-removal rule IDs (csv); supports 'all,-RuleId'. List: seconv list-rf-rules");
        ShowParameter(console, "--remove-line-breaks", "Remove line breaks");
        ShowParameter(console, "--remove-text-for-hi", "Remove text for hearing impaired");
        ShowParameter(console, "--remove-unicode-control-chars", "Remove Unicode control characters");
        ShowParameter(console, "--reverse-rtl-start-end", "Reverse RTL start/end");
        ShowParameter(console, "--split-long-lines", "Split long lines");

        console.WriteLine();
        ShowSection(console, "Subcommands", null);
        console.MarkupLine("  [dim]Every subcommand below accepts --json for machine-readable output.[/]");
        console.WriteLine();
        ShowParameter(console, "--version", "Print the seconv version and exit");
        ShowParameter(console, "--help-json", "Print the full command-line schema as JSON (options, types, exit codes)");
        ShowParameter(console, "formats", "List all available subtitle formats");
        ShowParameter(console, "list-encodings", "List all supported text encodings (code page + name)");
        ShowParameter(console, "list-pac-codepages", "List PAC code pages (--pac-codepage values)");
        ShowParameter(console, "list-ocr-engines", "List OCR engines + installation status");
        ShowParameter(console, "list-fce-rules", "List FixCommonErrors rule IDs");
        ShowParameter(console, "list-rf-rules", "List remove-formatting rule IDs");
        ShowParameter(console, "dump-settings", "Print a full --settings JSON with libse defaults (redirect to a file)");
        ShowParameter(console, "info <file>", "Print format / encoding / duration / language info");
        ShowParameter(console, "lint <pattern>", "Validate subtitle(s); exit 1 if any issues found");

        console.WriteLine();
        ShowSection(console, "Examples", null);
        ShowExample(console,
            "seconv *.srt sami",
            "Convert all .srt files to SAMI format");
        ShowExample(console,
            "seconv sub1.srt subrip --encoding:windows-1252",
            "Convert with specific encoding");
        ShowExample(console,
            "seconv *.srt subrip --input-encoding-fallback:windows-1250",
            "Bulk convert to UTF-8; assume CP1250 only when input isn't UTF-8");
        ShowExample(console,
            "seconv *.sub subrip --fps:25 --output-folder:C:\\Temp",
            "Convert frame-based to time-based with FPS");
        ShowExample(console,
            "seconv movie.mkv subrip --track-number:3",
            "Extract MKV subtitle track #3 to SRT");
        ShowExample(console,
            "seconv movie.sup subrip --ocr-engine:nocr --ocr-db:Latin.nocr",
            "OCR a Blu-Ray .sup using nOCR");
        ShowExample(console,
            "seconv movie.sup subrip --ocr-engine:llamacpp",
            "OCR a Blu-Ray .sup via llama.cpp (auto-starts a local llama-server)");
        ShowExample(console,
            "seconv movie.sup subrip --time-codes-only",
            "Extract only the time codes from a .sup (no OCR; empty text)");
        ShowExample(console,
            "seconv movie.avi subrip --ocr-engine:tesseract --ocr-language:eng",
            "OCR the XSUB (DivX) subtitles of an .avi");
        ShowExample(console,
            "seconv subs.srt customtext --custom-format:my-template.xml",
            "Render via a custom text format template");
        ShowExample(console,
            "seconv movie.srt subrip --translate-to:de",
            "Translate to German via a local llama.cpp server (started automatically)");
        ShowExample(console,
            "seconv movie.srt subrip --translate-to:da --translate-engine:ollama --translate-model:gemma2",
            "Translate to Danish via a running Ollama instance");
        ShowExample(console,
            "seconv movie.srt subrip --translate-to:de --translate-prompt:my-prompt.txt",
            "Translate with your own prompt ({0}=source language, {1}=target language)");
        ShowExample(console,
            "seconv formats",
            "List all available formats");

        console.WriteLine();
        var footer = new Panel("[dim]For more information, visit: https://github.com/niksedk/subtitleedit[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Grey)
        };
        console.Write(footer);
    }

    private static void ShowSection(IAnsiConsole console, string title, string? description)
    {
        console.MarkupLine($"[bold cyan]{title}:[/]");
        if (!string.IsNullOrEmpty(description))
        {
            console.MarkupLine($"  [dim]{description.Replace("\n", "\n  ")}[/]");
        }
        console.WriteLine();
    }

    private const int ParamColumnWidth = 42;

    private static void ShowParameter(IAnsiConsole console, string param, string description)
    {
        // Pad based on the visible length (param.Length) rather than the markup-escaped
        // length: Spectre.Console renders "[[" / "]]" as single chars, so columns drift
        // if we let the formatter pad against the escaped string.
        // Escape literal square brackets ('[' -> '[[') BEFORE turning '<'/'>' into display
        // brackets, so an option like "--apply-min-gap[:<ms>]" doesn't feed Spectre a stray
        // "[:" it reads as a broken markup tag (which threw mid-render and truncated --help).
        var escapedParam = EscapeParameter(param);
        var escapedDescription = description.EscapeMarkup();
        var pad = Math.Max(1, ParamColumnWidth - param.Length);
        console.MarkupLine($"  [yellow]{escapedParam}[/]{new string(' ', pad)}[dim]{escapedDescription}[/]");
    }

    // Renders literal '[' ']' and the '<value>' convention as visible square brackets.
    // Spectre uses '[[' / ']]' as the escape for a literal '[' / ']', so both the real
    // brackets and the angle-bracket placeholders map onto that same doubled form.
    // Literal-bracket escaping must run first, or the '[[' produced from '<' gets escaped again.
    //
    // Only parameter names go through this. Descriptions are plain prose and get ordinary
    // markup escaping instead: the '>' rewrite turned every arrow in a description into a
    // stray ']' ("-> text with time codes only" rendered as "-] text with time codes only").
    private static string EscapeParameter(string text) =>
        text.Replace("[", "[[").Replace("]", "]]")
            .Replace("<", "[[").Replace(">", "]]");

    private static void ShowExample(IAnsiConsole console, string command, string description)
    {
        var escapedCommand = command.Replace("[", "[[").Replace("]", "]]");
        console.MarkupLine($"  [green]{escapedCommand}[/]");
        console.MarkupLine($"  [dim]→ {description}[/]");
        console.WriteLine();
    }
}
