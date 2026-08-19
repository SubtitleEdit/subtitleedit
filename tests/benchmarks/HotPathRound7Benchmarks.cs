using Avalonia.Media;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using SkiaSharp;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The hot paths touched by the "scan for performance improvements" round: repeated ASSA header
/// parsing, the OCR fix engine's per-line replace lists, the WebVTT color tools, plain-text
/// sniffing, the generic importer and Subtitle.GetIndex.
///
/// Every benchmark here only uses API that exists on both sides of the change, so the same file
/// compiles against an unmodified checkout and can be run as the baseline.
/// </summary>
[MemoryDiagnoser]
public class HotPathRound7Benchmarks
{
    private string _assaHeaderFewStyles = null!;
    private string _assaHeaderManyStyles = null!;

    private string _webVttHeader = null!;
    private string[] _webVttTexts = null!;
    private List<SubtitleLineViewModel> _colorLines = null!;
    private readonly WebVTT _webVtt = new WebVTT();

    private string _plainTextFile = null!;
    private string _timeCodeFile = null!;

    private Subtitle _subtitle = null!;
    private Paragraph[] _missingParagraphs = null!;

    private List<string> _importerLines = null!;

    private OcrFixReplaceList2? _ocrList;
    private Subtitle _ocrSubtitle = null!;
    private string[] _ocrTexts = null!;
    private ISpellChecker _spellChecker = null!;

    [GlobalSetup]
    public void Setup()
    {
        _assaHeaderFewStyles = BuildAssaHeader(3);
        _assaHeaderManyStyles = BuildAssaHeader(60);

        _webVttHeader = BuildWebVttHeader(20);
        _webVttTexts = new string[200];
        for (var i = 0; i < _webVttTexts.Length; i++)
        {
            _webVttTexts[i] = i % 3 == 0
                ? "<c.color" + i % 20 + ">Some subtitle line number " + i + "</c>"
                : "Some subtitle line number " + i + " with no class at all";
        }

        GridCellConverterBenchmarks.EnsureAvalonia();
        _colorLines = SubtitleFactory.Make(300);

        _plainTextFile = WriteTempFile(BuildProse(120_000));
        _timeCodeFile = WriteTempFile(BuildSrtLikeText(1500));

        _subtitle = new Subtitle();
        for (var i = 0; i < 5000; i++)
        {
            _subtitle.Paragraphs.Add(new Paragraph("Line number " + i, i * 2000, i * 2000 + 1800) { Number = i + 1 });
        }

        // Probes that are not in the list, so the fallback scan runs to the end.
        _missingParagraphs = new Paragraph[50];
        for (var i = 0; i < _missingParagraphs.Length; i++)
        {
            _missingParagraphs[i] = new Paragraph("nowhere " + i, 100_000_000 + i, 100_000_100 + i);
        }

        _importerLines = BuildSrtLikeText(400).SplitToLines();

        _ocrSubtitle = new Subtitle();
        _ocrTexts = BuildOcrTexts();
        foreach (var t in _ocrTexts)
        {
            _ocrSubtitle.Paragraphs.Add(new Paragraph(t, 0, 1000));
        }

        _spellChecker = new EvenLengthSpellChecker();
        var listFile = FindEnglishReplaceList();
        if (listFile != null)
        {
            _ocrList = new OcrFixReplaceList2(listFile);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        TryDelete(_plainTextFile);
        TryDelete(_timeCodeFile);
    }

    // ------------------------------------------------------------------ ASSA styles

    /// <summary>What every ASSA save and every style dialog does: resolve all styles in the header.</summary>
    [Benchmark]
    public int AssaAllStylesFromHeader_Few() => AdvancedSubStationAlpha.GetSsaStylesFromHeader(_assaHeaderFewStyles).Count;

    [Benchmark]
    public int AssaAllStylesFromHeader_Many() => AdvancedSubStationAlpha.GetSsaStylesFromHeader(_assaHeaderManyStyles).Count;

    /// <summary>A single lookup, e.g. the burn-in and transparent-subtitle "Default" probe.</summary>
    [Benchmark]
    public object AssaSingleStyle() => AdvancedSubStationAlpha.GetSsaStyle("Default", _assaHeaderManyStyles);

    // ---------------------------------------------------------------------- WebVTT

    /// <summary>Colorizing a selection: the header is re-read for every line.</summary>
    [Benchmark]
    public int WebVttRemoveUnusedColorStyles()
    {
        var total = 0;
        foreach (var text in _webVttTexts)
        {
            total += WebVttHelper.RemoveUnusedColorStylesFromText(text, _webVttHeader).Length;
        }

        return total;
    }

    [Benchmark]
    public int WebVttGetOnlyColorStyle()
    {
        var found = 0;
        for (var i = 0; i < 200; i++)
        {
            if (WebVttHelper.GetOnlyColorStyle(SKColors.Red, _webVttHeader) != null)
            {
                found++;
            }
        }

        return found;
    }

    /// <summary>
    /// What the user actually triggers: set a color on a selection of WebVTT lines. Every line
    /// asked the helper for the header's styles several times over.
    /// </summary>
    [Benchmark]
    public int WebVttSetColorOnSelection()
    {
        var subtitle = new Subtitle { Header = _webVttHeader };
        var service = new ColorService();
        service.SetColor(_colorLines, Colors.Red, subtitle, _webVtt);
        return subtitle.Header.Length;
    }

    // ----------------------------------------------------------------- plain text

    [Benchmark]
    public bool IsPlainText_Prose() => FileUtil.IsPlainText(_plainTextFile);

    [Benchmark]
    public bool IsPlainText_TimeCodes() => FileUtil.IsPlainText(_timeCodeFile);

    // --------------------------------------------------------------- Subtitle index

    /// <summary>The fallback scan: probes that are not in the list at all.</summary>
    [Benchmark]
    public int SubtitleGetIndexMissing()
    {
        var sum = 0;
        foreach (var p in _missingParagraphs)
        {
            sum += _subtitle.GetIndex(p);
        }

        return sum;
    }

    // ------------------------------------------------------------------- importer

    [Benchmark]
    public int UnknownFormatImport() => new UnknownFormatImporter().AutoGuessImport(_importerLines, null)?.Paragraphs.Count ?? 0;

    // ------------------------------------------------------------------ OCR fixing

    /// <summary>One OCR pass over a subtitle: the replace lists are walked for every line.</summary>
    [Benchmark]
    public int OcrFixViaLineReplaceList()
    {
        if (_ocrList == null)
        {
            return 0;
        }

        var total = 0;
        for (var i = 0; i < _ocrTexts.Length; i++)
        {
            total += _ocrList.FixOcrErrorViaLineReplaceList(_ocrTexts[i], _ocrSubtitle, i, _spellChecker, new List<string>(), true).Length;
        }

        return total;
    }

    // ----------------------------------------------------------------------- setup

    private static string BuildAssaHeader(int styleCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Script Info]");
        sb.AppendLine("Title: Benchmark");
        sb.AppendLine("ScriptType: v4.00+");
        sb.AppendLine("PlayResX: 1920");
        sb.AppendLine("PlayResY: 1080");
        sb.AppendLine();
        sb.AppendLine("[V4+ Styles]");
        sb.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
        sb.AppendLine("Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1");
        for (var i = 1; i < styleCount; i++)
        {
            sb.AppendLine($"Style: Style{i},Verdana,{20 + i % 20},&H00FF{i % 100:00}00,&H0300FFFF,&H00101010,&H02000000,0,0,0,0,100,100,0,0,1,2,2,{1 + i % 9},{i % 50},{i % 50},{i % 50},1");
        }

        sb.AppendLine();
        sb.AppendLine("[Events]");
        sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
        return sb.ToString();
    }

    private static string BuildWebVttHeader(int styleCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();
        for (var i = 0; i < styleCount; i++)
        {
            sb.AppendLine("STYLE");
            sb.AppendLine($"::cue(.color{i}) {{ color: #{i:00}{i:00}{i:00} }}");
            sb.AppendLine();
        }

        sb.AppendLine("STYLE");
        sb.AppendLine("::cue(.red) { color: #ff0000 }");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildProse(int approximateLength)
    {
        var words = new[] { "the", "quick", "brown", "fox", "jumps", "over", "lazy", "dog", "and", "then", "walks", "away", "slowly" };
        var sb = new StringBuilder(approximateLength + 64);
        var i = 0;
        while (sb.Length < approximateLength)
        {
            sb.Append(words[i % words.Length]);
            sb.Append(i % 11 == 0 ? ".\n" : " ");
            i++;
        }

        return sb.ToString();
    }

    private static string BuildSrtLikeText(int cues)
    {
        var sb = new StringBuilder();
        for (var i = 1; i <= cues; i++)
        {
            var start = TimeSpan.FromMilliseconds(i * 2000);
            var end = TimeSpan.FromMilliseconds(i * 2000 + 1800);
            sb.AppendLine(i.ToString());
            sb.AppendLine($"{start:hh\\:mm\\:ss}\\,{start.Milliseconds:000} --> {end:hh\\:mm\\:ss}\\,{end.Milliseconds:000}".Replace("\\,", ","));
            sb.AppendLine("Subtitle line number " + i);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string[] BuildOcrTexts()
    {
        var seeds = new[]
        {
            "l am here and l said so",
            "I don't know what to do",
            "<i>l think so</i>",
            "- Hello there.\n- l'm fine, thanks.",
            "\"l said no\" he told me",
            "What? l don't know! l can't.",
            "ln the beginning there was light",
            "Wait a rninute, the rnan said",
            "Yes, l'll do it tomorrow morning",
            "♪ La la la la la ♪",
            "[door slams shut loudly]",
            "(whispering) l can't hear you",
            "Mr. Smith and Dr. Jones arrived",
            "...and then l simply left the room",
            "he said \"l'm sorry\" and walked out",
        };

        var texts = new string[300];
        for (var i = 0; i < texts.Length; i++)
        {
            texts[i] = seeds[i % seeds.Length];
        }

        return texts;
    }

    private static string WriteTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "se-bench-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, content, new UTF8Encoding(false));
        return path;
    }

    private static void TryDelete(string? path)
    {
        try
        {
            if (path != null && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static string? FindEnglishReplaceList()
    {
        foreach (var dir in new[]
                 {
                     Path.Combine(AppContext.BaseDirectory, "Dictionaries"),
                     "/Users/nikolajolsson/git/subtitleedit/src/ui/bin/Debug/net10.0/Dictionaries",
                 })
        {
            var file = Path.Combine(dir, "eng_OCRFixReplaceList.xml");
            if (File.Exists(file))
            {
                return file;
            }
        }

        return null;
    }

    private sealed class EvenLengthSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word) => (word?.Length ?? 0) % 2 == 0;

        public List<string> GetSuggestions(string word) => new List<string>();
    }
}
