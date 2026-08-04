using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Export;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// OcrFixReplaceList2.ReplaceWord runs once per matching replace-list entry per OCR'd line.
/// </summary>
[MemoryDiagnoser]
public class OcrReplaceWordBenchmarks
{
    private const string Line = "l am not sure what you mean, l said to him.";
    private const string LineNoMatch = "The quick brown fox jumps over the lazy dog near the river bank today.";

    [Benchmark]
    public string ReplaceWord_Hit() => OcrFixReplaceList2.ReplaceWord(Line, "l", "I");

    [Benchmark]
    public string ReplaceWord_FirstCharOnlyHits() => OcrFixReplaceList2.ReplaceWord(LineNoMatch, "t", "T");
}

/// <summary>
/// ItalicTextMerger.MergeWithItalicTags runs once per OCR'd subtitle line.
/// </summary>
[MemoryDiagnoser]
public class ItalicTextMergerBenchmarks
{
    private List<BinaryOcrMatcher.CompareMatch> _chars = new();

    [GlobalSetup]
    public void Setup()
    {
        _chars = new List<BinaryOcrMatcher.CompareMatch>();
        const string text = "This is a sample line with some italic words in it here";
        for (var i = 0; i < text.Length; i++)
        {
            var italic = i > 20 && i < 35;
            _chars.Add(new BinaryOcrMatcher.CompareMatch(text[i].ToString(), italic, 0, null));
        }
    }

    [Benchmark]
    public string MergeWithItalicTags() => ItalicTextMerger.MergeWithItalicTags(_chars);
}

/// <summary>
/// SpellChecker.IsNumber runs once per word per grid repaint with live spell check on;
/// nearly all words are plain letters and can never match any of the three regexes.
/// </summary>
[MemoryDiagnoser]
public class SpellCheckerIsNumberBenchmarks
{
    private sealed class Probe : SpellChecker
    {
        public static bool IsNumberProbe(string word) => IsNumber(word);
    }

    [Benchmark] public bool IsNumber_Word() => Probe.IsNumberProbe("subtitle");
    [Benchmark] public bool IsNumber_Number() => Probe.IsNumberProbe("1,234.56");
}

/// <summary>
/// CustomTextFormatter.GenerateCustomText - export via a custom format template; GetTimeCode
/// runs three times per paragraph.
/// </summary>
[MemoryDiagnoser]
public class CustomTextFormatterBenchmarks
{
    private CustomFormatTemplate _template = new();
    private List<Paragraph> _paragraphs = new();

    [GlobalSetup]
    public void Setup()
    {
        _template = new CustomFormatTemplate
        {
            Name = "bench",
            Extension = ".txt",
            FormatHeader = string.Empty,
            FormatParagraph = "{number}\t{start}\t{end}\t{duration}\t{text}\r\n",
            FormatFooter = string.Empty,
            FormatTimeCode = "hh:mm:ss,zzz",
            FormatNewLine = " ",
        };

        _paragraphs = new List<Paragraph>();
        for (var i = 0; i < 500; i++)
        {
            _paragraphs.Add(new Paragraph(
                "Hello there, this is subtitle line " + i + Environment.NewLine + "with a second line.",
                i * 2500,
                i * 2500 + 2000));
        }
    }

    [Benchmark]
    public string GenerateCustomText() => CustomTextFormatter.GenerateCustomText(_template, _paragraphs, "title", "video.mkv");
}
