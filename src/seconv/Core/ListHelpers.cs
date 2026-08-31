using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Spectre.Console;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using SeConv.Helpers;

namespace SeConv.Core;

/// <summary>
/// CLI list helpers: <c>seconv list-encodings</c>, <c>seconv list-pac-codepages</c>,
/// <c>seconv list-ocr-engines</c>. Each prints a formatted reference table to stdout
/// and returns 0, or a machine-readable document when <c>--json</c> is passed.
///
/// These are the discovery commands a script or agent runs first, so every one of them
/// accepts <c>--json</c>: the tables are hundreds of box-drawing lines that are expensive
/// to parse and easy to misread. In JSON, the <c>id</c> field is always the exact token
/// the corresponding option accepts.
/// </summary>
internal static class ListHelpers
{
    public static void PrintEncodings(bool json = false)
    {
        var encodingList = Encoding.GetEncodings().OrderBy(e => e.CodePage).ToList();

        if (json)
        {
            JsonOut.Write(new
            {
                encodings = encodingList.Select(e => new
                {
                    id = e.Name,
                    codePage = e.CodePage,
                    name = e.Name,
                    displayName = e.DisplayName,
                }),
                total = encodingList.Count,
                special = new[] { "utf-8", "utf-8-no-bom", "source" },
                note = "Pass either the code page number or the name to --encoding. 'source' keeps the input file's detected encoding.",
            });
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]Supported encodings (pass via --encoding)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Code page[/]");
        table.AddColumn("[green]Name[/]");
        table.AddColumn("[cyan]Description[/]");

        foreach (var info in encodingList)
        {
            table.AddRow(
                info.CodePage.ToString(),
                $"[green]{info.Name.EscapeMarkup()}[/]",
                $"[cyan]{info.DisplayName.EscapeMarkup()}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Pass either the code page number or the name. Special values: utf-8, utf-8-no-bom.[/]");
    }

    public static void PrintPacCodepages(bool json = false)
    {
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

        if (json)
        {
            JsonOut.Write(new
            {
                codePages = rows.Select(r => new
                {
                    id = r.name,
                    pacId = r.id,
                    name = r.name,
                    aliases = r.aliases.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                }),
                total = rows.Length,
                note = "Pass the name or any alias to --pac-codepage.",
            });
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]PAC code pages (pass via --pac-codepage)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]ID[/]");
        table.AddColumn("[green]Name[/]");
        table.AddColumn("[cyan]Aliases[/]");

        foreach (var (id, name, aliases) in rows)
        {
            table.AddRow(id.ToString(), $"[green]{name}[/]", $"[cyan]{aliases}[/]");
        }

        AnsiConsole.Write(table);
    }

    public static void PrintFixCommonErrorsRules(bool json = false)
    {
        var gates = FixCommonErrorsRunner.LanguageGates;
        var guiLabels = FixCommonErrorsRunner.GuiLabels;

        if (json)
        {
            JsonOut.Write(new
            {
                rules = FixCommonErrorsRunner.AvailableRuleIds.Select(id => new
                {
                    id,
                    guiLabel = guiLabels.TryGetValue(id, out var label) ? label : null,
                    languageGate = gates.TryGetValue(id, out var lang) ? lang : null,
                }),
                total = FixCommonErrorsRunner.AvailableRuleIds.Count,
                syntax = new
                {
                    all = "--fix-common-errors",
                    subset = "--fix-common-errors-rules:FixCommas,FixEllipsesStart",
                    allExcept = "--fix-common-errors-rules:all,-FixDanishLetterI",
                    forceLanguage = "--fce-language:es",
                },
                note = "A language-gated rule runs only when the subtitle's language matches (auto-detected, or forced with --fce-language). Naming a gated rule selects it but does not bypass the gate.",
            });
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]FixCommonErrors rule IDs (pass via --fix-common-errors-rules)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Rule ID[/]");
        table.AddColumn("[yellow]GUI equivalent[/]");
        table.AddColumn("[yellow]Language gate[/]");

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
            "[dim]Language-gated rules run only when the language matches — auto-detected, or forced with[/] " +
            "[green]--fce-language:<code>[/][dim]. Naming a gated rule selects it but does not bypass the gate.[/]");
        AnsiConsole.MarkupLine(
            "\n[dim]Examples:[/]\n" +
            "  [dim]--fix-common-errors[/]                                          [dim]# all rules (gates active)[/]\n" +
            "  [dim]--fix-common-errors-rules:FixCommas,FixEllipsesStart[/]         [dim]# only these two[/]\n" +
            "  [dim]--fix-common-errors-rules:all,-FixDanishLetterI[/]              [dim]# all except one[/]\n" +
            "  [dim]--fix-common-errors --fce-language:es[/]                        [dim]# force Spanish gate[/]\n" +
            "  [dim]--fix-common-errors-rules:FixSpanishInvertedQuestionAndExclamationMarks --fce-language:es[/]  [dim]# force a named gated rule[/]");
    }

    public static void PrintRemoveFormattingRules(bool json = false)
    {
        if (json)
        {
            JsonOut.Write(new
            {
                rules = RemoveFormattingRunner.AvailableRuleIds.Select(id => new
                {
                    id,
                    guiLabel = RemoveFormattingRunner.GuiLabels.TryGetValue(id, out var label) ? label : null,
                }),
                total = RemoveFormattingRunner.AvailableRuleIds.Count,
                syntax = new
                {
                    everyTag = "--remove-formatting",
                    subset = "--remove-formatting-rules:RemoveItalic,RemoveBold",
                    allExcept = "--remove-formatting-rules:all,-RemoveColor",
                },
                note = "Bare --remove-formatting strips every tag wholesale, which is broader than --remove-formatting-rules:all (the union of the rules above). Tags no rule covers, e.g. positioning like {\\pos(..)}, only go away with the bare flag.",
            });
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]Remove-formatting rule IDs (pass via --remove-formatting-rules)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Rule ID[/]");
        table.AddColumn("[yellow]GUI equivalent[/]");

        foreach (var id in RemoveFormattingRunner.AvailableRuleIds)
        {
            // GUI equivalent: the checkbox label in the desktop batch convert "Remove
            // formatting" function, so users who prototyped in the GUI can find the
            // matching CLI ID (#13518).
            var gui = RemoveFormattingRunner.GuiLabels.TryGetValue(id, out var label)
                ? $"[cyan]{label.EscapeMarkup()}[/]"
                : "[dim]—[/]";
            table.AddRow($"[green]{id}[/]", gui);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine(
            "\n[dim]The GUI equivalent is the checkbox label in the desktop batch convert 'Remove formatting' function.[/]\n" +
            "[dim]Note: bare[/] [green]--remove-formatting[/] [dim]strips every tag wholesale (GUI: 'Remove all formatting'),[/]\n" +
            "[dim]which is broader than[/] [green]--remove-formatting-rules:all[/] [dim]- the union of the rules above.[/]\n" +
            "[dim]Tags no rule covers, e.g. positioning like {\\pos(..)}, only go away with the bare flag.[/]");
        AnsiConsole.MarkupLine(
            "\n[dim]Examples:[/]\n" +
            "  [dim]--remove-formatting[/]                                  [dim]# remove every tag[/]\n" +
            "  [dim]--remove-formatting-rules:RemoveItalic,RemoveBold[/]    [dim]# only these two[/]\n" +
            "  [dim]--remove-formatting-rules:all,-RemoveColor[/]           [dim]# every named rule except colors[/]");
    }

    public static void PrintOcrEngines(bool json = false)
    {
        var tesseractInstalled = TesseractOcrEngine.Detect() is not null;
        var paddleInstalled = PaddleOcrEngine.Detect() is not null;
        var llamaCppInstalled = LlamaCppLocal.TryEnsureServerBinary();
        var llamaCppModelCount = LlamaCppServerManager.GetAllOcrModels().Count(LlamaCppServerManager.IsModelInstalled);

        if (json)
        {
            // 'ready' is the actionable field: whether this engine can run right now without
            // further setup. nocr/binaryocr are in-process but need a database file, so they
            // are only ready once --ocr-db points at one — which this command cannot know.
            JsonOut.Write(new
            {
                engines = new object[]
                {
                    new
                    {
                        id = "tesseract", aliases = Array.Empty<string>(), type = "subprocess",
                        isDefault = true, ready = tesseractInstalled,
                        requires = "`tesseract` binary on PATH",
                        options = new[] { "--ocr-language" },
                    },
                    new
                    {
                        id = "nocr", aliases = Array.Empty<string>(), type = "in-process",
                        isDefault = false, ready = (bool?)null,
                        requires = "--ocr-db pointing at a .nocr database (e.g. Latin.nocr, under Subtitle Edit's OCR folder)",
                        options = new[] { "--ocr-db" },
                    },
                    new
                    {
                        id = "binaryocr", aliases = Array.Empty<string>(), type = "in-process",
                        isDefault = false, ready = (bool?)null,
                        requires = "--ocr-db pointing at a .db database (e.g. Latin.db, under Subtitle Edit's OCR folder)",
                        options = new[] { "--ocr-db" },
                    },
                    new
                    {
                        id = "ollama", aliases = Array.Empty<string>(), type = "http",
                        isDefault = false, ready = (bool?)null,
                        requires = "A running Ollama instance with a vision model",
                        options = new[] { "--ollama-url", "--ollama-model", "--ocr-language", "--ocr-prompt" },
                    },
                    new
                    {
                        id = "llamacpp", aliases = Array.Empty<string>(), type = "http",
                        isDefault = false, ready = llamaCppInstalled && llamaCppModelCount > 0,
                        requires = $"llama-server ({(llamaCppInstalled ? "installed" : "not found")}) plus an OCR model ({llamaCppModelCount} installed) — download via Subtitle Edit's OCR window, or point --ocr-url at a running server",
                        options = new[] { "--ocr-model", "--ocr-url", "--ocr-language", "--ocr-prompt" },
                    },
                    new
                    {
                        id = "paddle", aliases = new[] { "paddleocr" }, type = "subprocess",
                        isDefault = false, ready = paddleInstalled,
                        requires = "`paddleocr` binary on PATH (`pip install paddleocr`)",
                        options = new[] { "--ocr-language" },
                    },
                },
                note = "Language codes differ per engine: Tesseract uses ISO 639-2 (eng, deu), Paddle uses short codes (en, de), Ollama and llama.cpp take a human-readable language name.",
            });
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]OCR engines (pass via --ocr-engine)[/]");
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Name[/]");
        table.AddColumn("[green]Type[/]");
        table.AddColumn("[cyan]Requirements[/]");

        var tesseract = tesseractInstalled ? "installed ✓" : "not on PATH";
        var paddle = paddleInstalled ? "installed ✓" : "not on PATH";

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

        var llamaCppServer = llamaCppInstalled ? "installed ✓" : "not found";
        table.AddRow(
            "[green]llamacpp[/]",
            "HTTP",
            $"llama-server ({llamaCppServer}) + OCR model ({llamaCppModelCount} installed) — download via SE's OCR window, or --ocr-url. --ocr-model, --ocr-url");
        table.AddRow(
            "[green]paddle[/] / [green]paddleocr[/]",
            "subprocess",
            $"`paddleocr` binary on PATH ({paddle}) — `pip install paddleocr`");

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[dim]Pass language via --ocr-language (Tesseract: ISO 639-2; Paddle: en/de/fr/...; Ollama/llama.cpp: human name).[/]");
    }
}
