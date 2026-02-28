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
        ShowParameter("--adjustduration:<ms>", "Adjust duration in milliseconds");
        ShowParameter("--assa-style-file:<file name>", "ASSA style file");
        ShowParameter("--ebuheaderfile:<file name>", "EBU header file");
        ShowParameter("--encoding:<encoding name>", "Character encoding (e.g., utf-8, windows-1252)");
        ShowParameter("--forcedonly", "Process forced subtitles only");
        ShowParameter("--fps:<frame rate>", "Frame rate for conversion");
        ShowParameter("--inputfolder:<folder name>", "Input folder path");
        ShowParameter("--multiplereplace", "Use default replace rules (equivalent to --multiplereplace:.)");
        ShowParameter("--multiplereplace:<file list>", "Comma separated file name list ('.' = default rules)");
        ShowParameter("--ocrengine:<ocr engine>", "OCR engine (tesseract/nOCR)");
        ShowParameter("--offset:hh:mm:ss:ms", "Time offset");
        ShowParameter("--outputfilename:<file name>", "Output file name (for single file only)");
        ShowParameter("--outputfolder:<folder name>", "Output folder path");
        ShowParameter("--overwrite", "Overwrite existing files");
        ShowParameter("--pac-codepage:<code page>", "PAC code page");
        ShowParameter("--profile:<profile name>", "Profile name");
        ShowParameter("--renumber:<starting number>", "Renumber subtitles from this number");
        ShowParameter("--resolution:<width>x<height>", "Video resolution (e.g., 1920x1080)");
        ShowParameter("--targetfps:<frame rate>", "Target frame rate");
        ShowParameter("--teletextonly", "Process teletext only");
        ShowParameter("--teletextonlypage:<page number>", "Teletext page number");
        ShowParameter("--track-number:<track list>", "Comma separated track number list");
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Operations:[/]");
        AnsiConsole.MarkupLine("  [dim]The following operations are applied in command line order[/]");
        AnsiConsole.MarkupLine("  [dim]from left to right, and can be specified multiple times.[/]");
        AnsiConsole.WriteLine();
        
        ShowParameter("--ApplyDurationLimits", "Apply duration limits");
        ShowParameter("--BalanceLines", "Balance line lengths");
        ShowParameter("--BeautifyTimeCodes", "Beautify time codes");
        ShowParameter("--ConvertColorsToDialog", "Convert colors to dialog");
        ShowParameter("--DeleteFirst:<count>", "Delete first N entries");
        ShowParameter("--DeleteLast:<count>", "Delete last N entries");
        ShowParameter("--DeleteContains:<word>", "Delete entries containing specified word");
        ShowParameter("--FixCommonErrors", "Fix common subtitle errors");
        ShowParameter("--FixRtlViaUnicodeChars", "Fix RTL via Unicode characters");
        ShowParameter("--MergeSameTexts", "Merge entries with same text");
        ShowParameter("--MergeSameTimeCodes", "Merge entries with same time codes");
        ShowParameter("--MergeShortLines", "Merge short lines");
        ShowParameter("--RedoCasing", "Redo text casing");
        ShowParameter("--RemoveFormatting", "Remove formatting tags");
        ShowParameter("--RemoveLineBreaks", "Remove line breaks");
        ShowParameter("--RemoveTextForHI", "Remove text for hearing impaired");
        ShowParameter("--RemoveUnicodeControlChars", "Remove Unicode control characters");
        ShowParameter("--ReverseRtlStartEnd", "Reverse RTL start/end");
        ShowParameter("--SplitLongLines", "Split long lines");

        AnsiConsole.WriteLine();
        ShowSection("Examples", null);
        ShowExample(
            "SubtitleEdit *.srt sami",
            "Convert all .srt files to SAMI format");
        ShowExample(
            "SubtitleEdit sub1.srt subrip --encoding:windows-1252",
            "Convert with specific encoding");
        ShowExample(
            "SubtitleEdit *.sub subrip --fps:25 --outputfolder:C:\\Temp",
            "Convert frame-based to time-based with FPS");
        ShowExample(
            "SubtitleEdit formats",
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

    private static void ShowParameter(string param, string description)
    {
        var escapedParam = param.Replace("<", "[[").Replace(">", "]]");
        var escapedDescription = description.Replace("<", "[[").Replace(">", "]]");
        AnsiConsole.MarkupLine($"  [yellow]{escapedParam,-40}[/] [dim]{escapedDescription}[/]");
    }

    private static void ShowExample(string command, string description)
    {
        var escapedCommand = command.Replace("[", "[[").Replace("]", "]]");
        AnsiConsole.MarkupLine($"  [green]{escapedCommand}[/]");
        AnsiConsole.MarkupLine($"  [dim]→ {description}[/]");
        AnsiConsole.WriteLine();
    }
}
