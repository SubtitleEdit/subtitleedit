using Spectre.Console;

namespace SeConv.Helpers;

internal static class HelpDisplay
{
    public static void ShowHelp()
    {
        var rule = new Rule("[bold yellow]Subtitle Edit 5.0 - Batch Converter[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold cyan]Usage:[/]");
        AnsiConsole.MarkupLine("  [green]SubtitleEdit[/] [yellow][[pattern]] [[name-of-format-without-spaces]][/] [blue][[optional-parameters]][/]");
        AnsiConsole.WriteLine();

        ShowSection("Pattern", "One or more file name patterns separated by commas\nRelative patterns are relative to /inputfolder if specified");

        ShowSection("Optional Parameters", null);
        ShowParameter("--adjust-duration:<ms>", "Adjust duration in milliseconds");
        ShowParameter("--assa-style-file:<file name>", "ASSA style file");
        ShowParameter("--change-speed:<percent>", "Change speed by percent (e.g. 125 = 1.25x faster)");
        ShowParameter("--ebu-header-file:<file name>", "EBU header file");
        ShowParameter("--encoding:<encoding name>", "Character encoding (e.g., utf-8, windows-1252, or 'source' to keep the input file's encoding)");
        ShowParameter("--input-encoding-fallback:<name>", "Assumed input encoding when no BOM and not UTF-8 (skips ANSI guess)");
        ShowParameter("--forced-only", "Process forced subtitles only");
        ShowParameter("--fps:<frame rate>", "Frame rate for conversion");
        ShowParameter("--input-folder:<folder name>", "Input folder path");
        ShowParameter("--offset:hh:mm:ss:ms", "Time offset");
        ShowParameter("--output-filename:<file name>", "Output file name (for single file only)");
        ShowParameter("--output-folder:<folder name>", "Output folder path");
        ShowParameter("--overwrite", "Overwrite existing files");
        ShowParameter("--pac-codepage:<code page>", "PAC code page");
        ShowParameter("--profile:<profile name>", "Profile name");
        ShowParameter("--renumber:<starting number>", "Renumber subtitles from this number");
        ShowParameter("--resolution:<width>x<height>", "Video resolution (e.g., 1920x1080)");
        ShowParameter("--target-fps:<frame rate>", "Target frame rate");
        ShowParameter("--teletext-only", "Process teletext only");
        ShowParameter("--teletext-only-page:<page number>", "Teletext page number");
        ShowParameter("--track-number:<track list>", "Comma separated track number list");
        ShowParameter("--ocr-engine:<engine>", "OCR engine: tesseract | nocr | binaryocr | ollama | llamacpp | paddle");
        ShowParameter("--ocr-language:<lang>", "Language for OCR (e.g. eng, deu, spa)");
        ShowParameter("--ocr-db:<path>", ".nocr (--ocr-engine=nocr) or .db (--ocr-engine=binaryocr)");
        ShowParameter("--ocr-model:<model>", "llamacpp OCR .gguf file name/path (default: first downloaded OCR model)");
        ShowParameter("--ocr-url:<url>", "Endpoint of an already-running llama-server for OCR (skips the auto-start)");
        ShowParameter("--time-codes-only", "Image sources (.sup/VobSub/PGS/DVB) -> text with time codes only; skips OCR");
        ShowParameter("--no-vobsub-isolate-colors", "Disable VobSub OCR colour isolation (on by default; isolation binarises to black-on-white, dropping outline colours)");
        ShowParameter("--no-pgs-isolate-colors", "Disable PGS/DVB-sub OCR colour isolation (on by default; isolation binarises to black-on-white so the white glyph fill survives the OCR canvas)");
        ShowParameter("--ollama-url:<url>", "Ollama API endpoint (default: http://localhost:11434/api/chat)");
        ShowParameter("--ollama-model:<model>", "Ollama vision model (default: llama3.2-vision)");
        ShowParameter("--translate-to:<lang>", "Auto-translate to this language (code or English name, e.g. de or German)");
        ShowParameter("--translate-from:<lang>", "Auto-translate source language (default: auto-detect per file)");
        ShowParameter("--translate-engine:<engine>", "llamacpp (default; auto-starts a local llama-server) | ollama | lmstudio | libretranslate | nllb-serve | nllb-api");
        ShowParameter("--translate-url:<url>", "Endpoint of an already-running translate server (llamacpp: skips the auto-start)");
        ShowParameter("--translate-model:<model>", "Ollama/LM Studio model name, or llamacpp .gguf file name/path");
        ShowParameter("--multiple-replace:<path.xml>", "SE MultipleSearchAndReplaceGroups XML applied per paragraph");
        ShowParameter("--custom-format:<path.xml>", "SE CustomFormatItem XML (use with --format customtext)");
        ShowParameter("--settings:<path.json>", "JSON settings file overriding libse defaults");
        ShowParameter("--quiet", "Suppress per-file progress; only print the final summary");
        ShowParameter("--verbose", "Print extra diagnostic information");
        ShowParameter("--json", "Emit per-file results as JSON (suppresses Spectre output)");
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Operations:[/]");
        AnsiConsole.MarkupLine("  [dim]Operations are applied in a fixed, sensible order regardless of CLI order.[/]");
        AnsiConsole.WriteLine();
        
        ShowParameter("--apply-duration-limits", "Apply duration limits");
        ShowParameter("--apply-min-gap[:<ms>]", "Enforce minimum gap between paragraphs (default: minimumMillisecondsBetweenLines)");
        ShowParameter("--balance-lines", "Balance line lengths");
        ShowParameter("--bridge-gaps:<ms>", "Bridge gaps shorter than N ms (extends previous end time)");
        ShowParameter("--beautify-time-codes", "Beautify time codes");
        ShowParameter("--convert-colors-to-dialog", "Convert colors to dialog");
        ShowParameter("--delete-first:<count>", "Delete first N entries");
        ShowParameter("--delete-last:<count>", "Delete last N entries");
        ShowParameter("--delete-contains:<word>", "Delete entries containing specified word");
        ShowParameter("--fix-common-errors", "Fix common subtitle errors (all rules)");
        ShowParameter("--fix-common-errors-rules:<list>", "FCE rule IDs (csv); supports 'all,-RuleId'. List: seconv list-fce-rules");
        ShowParameter("--dictionary-folder:<path>", "Hunspell dictionaries + *_OCRFixReplaceList.xml; enables the 'Fix common OCR errors' FCE pass");
        ShowParameter("--fix-rtl-via-unicode-chars", "Fix RTL via Unicode characters");
        ShowParameter("--merge-same-texts", "Merge entries with same text");
        ShowParameter("--merge-same-time-codes", "Merge entries with same time codes");
        ShowParameter("--merge-short-lines", "Merge short lines");
        ShowParameter("--redo-casing", "Redo text casing");
        ShowParameter("--remove-formatting", "Remove formatting tags");
        ShowParameter("--remove-line-breaks", "Remove line breaks");
        ShowParameter("--remove-text-for-hi", "Remove text for hearing impaired");
        ShowParameter("--remove-unicode-control-chars", "Remove Unicode control characters");
        ShowParameter("--reverse-rtl-start-end", "Reverse RTL start/end");
        ShowParameter("--split-long-lines", "Split long lines");

        AnsiConsole.WriteLine();
        ShowSection("Subcommands", null);
        ShowParameter("--version", "Print the seconv version and exit");
        ShowParameter("formats", "List all available subtitle formats");
        ShowParameter("list-encodings", "List all supported text encodings (code page + name)");
        ShowParameter("list-pac-codepages", "List PAC code pages (--pac-codepage values)");
        ShowParameter("list-ocr-engines", "List OCR engines + installation status");
        ShowParameter("list-fce-rules", "List FixCommonErrors rule IDs");
        ShowParameter("dump-settings", "Print a full --settings JSON with libse defaults (redirect to a file)");
        ShowParameter("info <file>", "Print format / encoding / duration / language info");
        ShowParameter("lint <pattern>", "Validate subtitle(s); exit 1 if any issues found");

        AnsiConsole.WriteLine();
        ShowSection("Examples", null);
        ShowExample(
            "seconv *.srt sami",
            "Convert all .srt files to SAMI format");
        ShowExample(
            "seconv sub1.srt subrip --encoding:windows-1252",
            "Convert with specific encoding");
        ShowExample(
            "seconv *.srt subrip --input-encoding-fallback:windows-1250",
            "Bulk convert to UTF-8; assume CP1250 only when input isn't UTF-8");
        ShowExample(
            "seconv *.sub subrip --fps:25 --output-folder:C:\\Temp",
            "Convert frame-based to time-based with FPS");
        ShowExample(
            "seconv movie.mkv subrip --track-number:3",
            "Extract MKV subtitle track #3 to SRT");
        ShowExample(
            "seconv movie.sup subrip --ocr-engine:nocr --ocr-db:Latin.nocr",
            "OCR a Blu-Ray .sup using nOCR");
        ShowExample(
            "seconv movie.sup subrip --ocr-engine:llamacpp",
            "OCR a Blu-Ray .sup via llama.cpp (auto-starts a local llama-server)");
        ShowExample(
            "seconv movie.sup subrip --time-codes-only",
            "Extract only the time codes from a .sup (no OCR; empty text)");
        ShowExample(
            "seconv subs.srt customtext --custom-format:my-template.xml",
            "Render via a custom text format template");
        ShowExample(
            "seconv movie.srt subrip --translate-to:de",
            "Translate to German via a local llama.cpp server (started automatically)");
        ShowExample(
            "seconv movie.srt subrip --translate-to:da --translate-engine:ollama --translate-model:gemma2",
            "Translate to Danish via a running Ollama instance");
        ShowExample(
            "seconv formats",
            "List all available formats");

        AnsiConsole.WriteLine();
        var footer = new Panel("[dim]For more information, visit: https://github.com/niksedk/subtitleedit[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Grey)
        };
        AnsiConsole.Write(footer);
    }

    private static void ShowSection(string title, string? description)
    {
        AnsiConsole.MarkupLine($"[bold cyan]{title}:[/]");
        if (!string.IsNullOrEmpty(description))
        {
            AnsiConsole.MarkupLine($"  [dim]{description.Replace("\n", "\n  ")}[/]");
        }
        AnsiConsole.WriteLine();
    }

    private const int ParamColumnWidth = 42;

    private static void ShowParameter(string param, string description)
    {
        // Pad based on the visible length (param.Length) rather than the markup-escaped
        // length: Spectre.Console renders "[[" / "]]" as single chars, so columns drift
        // if we let the formatter pad against the escaped string.
        // Escape literal square brackets ('[' -> '[[') BEFORE turning '<'/'>' into display
        // brackets, so an option like "--apply-min-gap[:<ms>]" doesn't feed Spectre a stray
        // "[:" it reads as a broken markup tag (which threw mid-render and truncated --help).
        var escapedParam = EscapeForMarkup(param);
        var escapedDescription = EscapeForMarkup(description);
        var pad = Math.Max(1, ParamColumnWidth - param.Length);
        AnsiConsole.MarkupLine($"  [yellow]{escapedParam}[/]{new string(' ', pad)}[dim]{escapedDescription}[/]");
    }

    // Renders literal '[' ']' and the '<value>' convention as visible square brackets.
    // Spectre uses '[[' / ']]' as the escape for a literal '[' / ']', so both the real
    // brackets and the angle-bracket placeholders map onto that same doubled form.
    // Literal-bracket escaping must run first, or the '[[' produced from '<' gets escaped again.
    private static string EscapeForMarkup(string text) =>
        text.Replace("[", "[[").Replace("]", "]]")
            .Replace("<", "[[").Replace(">", "]]");

    private static void ShowExample(string command, string description)
    {
        var escapedCommand = command.Replace("[", "[[").Replace("]", "]]");
        AnsiConsole.MarkupLine($"  [green]{escapedCommand}[/]");
        AnsiConsole.MarkupLine($"  [dim]→ {description}[/]");
        AnsiConsole.WriteLine();
    }
}
