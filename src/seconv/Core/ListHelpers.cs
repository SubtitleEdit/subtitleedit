using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Spectre.Console;
using System.Text;

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

    public static void PrintOcrEngines()
    {
        AnsiConsole.MarkupLine("[bold cyan]OCR engines (pass via --ocrengine)[/]");
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
            "Pass --ocrdb=<path-to-Latin.nocr> (find under %AppData%\\\\Subtitle Edit\\\\Ocr\\\\)");
        table.AddRow(
            "[green]binaryocr[/]",
            "in-process",
            "Pass --ocrdb=<path-to-Latin.db> (find under %AppData%\\\\Subtitle Edit\\\\Ocr\\\\)");
        table.AddRow(
            "[green]ollama[/]",
            "HTTP",
            "Local Ollama with vision model. --ollama-url, --ollama-model");
        table.AddRow(
            "[green]paddle[/] / [green]paddleocr[/]",
            "subprocess",
            $"`paddleocr` binary on PATH ({paddle}) — `pip install paddleocr`");

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[dim]Pass language via --ocrlanguage (Tesseract: ISO 639-2; Paddle: en/de/fr/...; Ollama: human name).[/]");
    }
}
