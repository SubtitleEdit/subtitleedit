using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Spectre.Console;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace SeConv.Core;

/// <summary>
/// CLI list helpers: <c>seconv list-encodings</c>, <c>seconv list-pac-codepages</c>,
/// <c>seconv list-ocr-engines</c>. Each prints a formatted reference table to stdout
/// and returns 0.
/// </summary>
internal static class ListHelpers
{
    public static void PrintEncodings()
    {
        AnsiConsole.MarkupLine("[bold cyan]Supported encodings (pass via --encoding)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Code page[/]");
        table.AddColumn("[green]Name[/]");
        table.AddColumn("[cyan]Description[/]");

        var encodings = Encoding.GetEncodings()
            .OrderBy(e => e.CodePage)
            .ToList();
        foreach (var info in encodings)
        {
            table.AddRow(
                info.CodePage.ToString(),
                $"[green]{info.Name.EscapeMarkup()}[/]",
                $"[cyan]{info.DisplayName.EscapeMarkup()}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Pass either the code page number or the name. Special values: utf-8, utf-8-no-bom.[/]");
    }

    public static void PrintPacCodepages()
    {
        AnsiConsole.MarkupLine("[bold cyan]PAC code pages (pass via --pac-codepage)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]ID[/]");
        table.AddColumn("[green]Name[/]");
        table.AddColumn("[cyan]Aliases[/]");

        (int id, string name, string aliases)[] rows =
        [
            (Pac.CodePageLatin, "Latin", "0"),
            (Pac.CodePageGreek, "Greek", "1"),
            (Pac.CodePageLatinCzech, "LatinCzech", "Czech, 2"),
            (Pac.CodePageArabic, "Arabic", "3"),
            (Pac.CodePageHebrew, "Hebrew", "4"),
            (Pac.CodePageThai, "Thai", "5"),
            (Pac.CodePageCyrillic, "Cyrillic", "6"),
            (Pac.CodePageChineseTraditional, "ChineseTraditional", "7"),
            (Pac.CodePageChineseSimplified, "ChineseSimplified", "8"),
            (Pac.CodePageKorean, "Korean", "9"),
            (Pac.CodePageJapanese, "Japanese", "10"),
            (Pac.CodePageLatinTurkish, "LatinTurkish", "Turkish, 11"),
            (Pac.CodePageLatinPortuguese, "LatinPortuguese", "Portuguese, 12"),
        ];

        foreach (var (id, name, aliases) in rows)
        {
            table.AddRow(id.ToString(), $"[green]{name}[/]", $"[cyan]{aliases}[/]");
        }

        AnsiConsole.Write(table);
    }

    public static void PrintFixCommonErrorsRules()
    {
        AnsiConsole.MarkupLine("[bold cyan]FixCommonErrors rule IDs (pass via --FixCommonErrorsRules)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Rule ID[/]");
        table.AddColumn("[yellow]GUI equivalent[/]");
        table.AddColumn("[yellow]Language gate[/]");

        var gates = FixCommonErrorsRunner.LanguageGates;
        var guiLabels = FixCommonErrorsRunner.GuiLabels;
        foreach (var id in FixCommonErrorsRunner.AvailableRuleIds)
        {
            // GUI equivalent: the checkbox label the desktop Fix Common Errors window shows for
            // this rule, so users who prototyped in the GUI can find the matching CLI ID (#11037).
            var gui = guiLabels.TryGetValue(id, out var label) ? $"[cyan]{label.EscapeMarkup()}[/]" : "[dim]—[/]";

            // Language-gated rules only run when the subtitle's language (auto-detected, or
            // forced via --fce-language) matches. Mark them so the gate is discoverable from
            // the CLI without reading the source.
            var gate = gates.TryGetValue(id, out var lang) ? $"[magenta]{lang} only[/]" : "[dim]—[/]";
            table.AddRow($"[green]{id}[/]", gui, gate);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine(
            "\n[dim]The GUI equivalent is the checkbox label in the desktop Fix Common Errors window.[/]\n" +
            "[dim]Language-gated rules run only when the subtitle auto-detects to that language.[/]\n" +
            "[dim]Force it with[/] [green]--fce-language:<code>[/][dim], or bypass the gate by naming the rule explicitly.[/]");
        AnsiConsole.MarkupLine(
            "\n[dim]Examples:[/]\n" +
            "  [dim]--FixCommonErrors[/]                                       [dim]# all rules (gates active)[/]\n" +
            "  [dim]--FixCommonErrorsRules:FixCommas,FixEllipsesStart[/]       [dim]# only these two[/]\n" +
            "  [dim]--FixCommonErrorsRules:all,-FixDanishLetterI[/]            [dim]# all except one[/]\n" +
            "  [dim]--FixCommonErrors --fce-language:es[/]                     [dim]# force Spanish gate[/]\n" +
            "  [dim]--FixCommonErrorsRules:FixSpanishInvertedQuestionAndExclamationMarks[/]  [dim]# bypass gate[/]");
    }

    public static void PrintOcrEngines()
    {
        AnsiConsole.MarkupLine("[bold cyan]OCR engines (pass via --ocr-engine)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Name[/]");
        table.AddColumn("[green]Type[/]");
        table.AddColumn("[cyan]Requirements[/]");

        var tesseract = TesseractOcrEngine.Detect() is not null ? "installed ✓" : "not on PATH";
        var paddle = PaddleOcrEngine.Detect() is not null ? "installed ✓" : "not on PATH";

        table.AddRow(
            "[green]tesseract[/] (default)",
            "subprocess",
            $"`tesseract` binary on PATH ({tesseract})");
        table.AddRow(
            "[green]nocr[/]",
            "in-process",
            "Pass --ocr-db=<path-to-Latin.nocr> (find under %AppData%\\\\Subtitle Edit\\\\OCR\\\\)");
        table.AddRow(
            "[green]binaryocr[/]",
            "in-process",
            "Pass --ocr-db=<path-to-Latin.db> (find under %AppData%\\\\Subtitle Edit\\\\OCR\\\\)");
        table.AddRow(
            "[green]ollama[/]",
            "HTTP",
            "Local Ollama with vision model. --ollama-url, --ollama-model");

        var llamaCppServer = LlamaCppLocal.TryEnsureServerBinary() ? "installed ✓" : "not found";
        var llamaCppModels = LlamaCppServerManager.OcrModels.Count(LlamaCppServerManager.IsModelInstalled);
        table.AddRow(
            "[green]llamacpp[/]",
            "HTTP",
            $"llama-server ({llamaCppServer}) + OCR model ({llamaCppModels} installed) — download via SE's OCR window, or --ocr-url. --ocr-model, --ocr-url");
        table.AddRow(
            "[green]paddle[/] / [green]paddleocr[/]",
            "subprocess",
            $"`paddleocr` binary on PATH ({paddle}) — `pip install paddleocr`");

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[dim]Pass language via --ocr-language (Tesseract: ISO 639-2; Paddle: en/de/fr/...; Ollama/llama.cpp: human name).[/]");
    }
}
