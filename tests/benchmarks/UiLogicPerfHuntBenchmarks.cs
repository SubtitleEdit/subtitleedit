using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.UiLogic.Export;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// NikseBitmapImageSplitter2.IsBitmapsAlike is the inner loop of binary OCR matching -
/// BinaryOcrMatcher.FindBestMatch runs it against up to 9 size-shifted variants of every
/// database glyph for every unmatched character.
/// </summary>
[MemoryDiagnoser]
public class IsBitmapsAlikeBenchmarks
{
    private NikseBitmap2 _glyphA = null!;
    private NikseBitmap2 _glyphB = null!;
    private NikseBitmap2 _glyphDifferent = null!;

    public static NikseBitmap2 MakeGlyph(int width, int height, int seed)
    {
        var data = new byte[width * height * 4];
        for (var i = 0; i < data.Length; i += 4)
        {
            // deterministic pseudo-glyph: some opaque white "strokes", rest transparent
            var pixel = i / 4;
            var on = (pixel * 31 + seed * 17) % 7 < 2;
            data[i] = (byte)(on ? 255 : 0);     // B
            data[i + 1] = (byte)(on ? 255 : 0); // G
            data[i + 2] = (byte)(on ? 255 : 0); // R
            data[i + 3] = (byte)(on ? 255 : 0); // A
        }

        return new NikseBitmap2(width, height, data);
    }

    [GlobalSetup]
    public void Setup()
    {
        _glyphA = MakeGlyph(28, 42, seed: 1);
        _glyphB = MakeGlyph(28, 42, seed: 1);
        _glyphDifferent = MakeGlyph(28, 42, seed: 5);
    }

    [Benchmark]
    public int IsBitmapsAlike_Same() => NikseBitmapImageSplitter2.IsBitmapsAlike(_glyphA, _glyphB);

    [Benchmark]
    public int IsBitmapsAlike_Different() => NikseBitmapImageSplitter2.IsBitmapsAlike(_glyphA, _glyphDifferent);
}

/// <summary>
/// BinaryOcrBitmap's ctor (per-pixel GetAlpha loop + MurmurHash) runs once per glyph per
/// OCR'd character - and, before the fix, once per matching expanded-DB entry as well.
/// </summary>
[MemoryDiagnoser]
public class BinaryOcrBitmapCtorBenchmarks
{
    private NikseBitmap2 _glyph = null!;

    [GlobalSetup]
    public void Setup() => _glyph = IsBitmapsAlikeBenchmarks.MakeGlyph(28, 42, seed: 3);

    [Benchmark]
    public BinaryOcrBitmap Construct() => new BinaryOcrBitmap(_glyph);
}

/// <summary>
/// The expanded-match passes in BinaryOcrMatcher.GetCompareMatch iterate the whole
/// CompareImagesExpanded list per glyph; before the fix they reconstructed the same
/// following-glyph BinaryOcrBitmap for every candidate DB entry.
/// </summary>
[MemoryDiagnoser]
public class BinaryOcrExpandedMatchBenchmarks
{
    private BinaryOcrMatcher _matcher = null!;
    private BinaryOcrDb _db = null!;
    private List<ImageSplitterItem2> _list = null!;

    [GlobalSetup]
    public void Setup()
    {
        _matcher = new BinaryOcrMatcher();
        _db = new BinaryOcrDb("bench.db", loadCompareImages: false);

        // A line of 8 glyphs, all 28x42-ish - realistic: glyph sizes cluster tightly,
        // so the +-3 size gate of the "allow for error %" pass passes often.
        _list = new List<ImageSplitterItem2>();
        for (var i = 0; i < 8; i++)
        {
            _list.Add(new ImageSplitterItem2(i * 30, 0, IsBitmapsAlikeBenchmarks.MakeGlyph(28, 42, seed: 100 + i)));
        }

        // 300 expanded entries whose dimensions match the target but whose expanded-list
        // hashes never match, so every entry walks its ExpandedList and (pre-fix)
        // constructs fresh BinaryOcrBitmaps for the following glyphs each time.
        for (var i = 0; i < 300; i++)
        {
            var entry = new BinaryOcrBitmap(IsBitmapsAlikeBenchmarks.MakeGlyph(28, 42, seed: 100), false, 2, "ab", 0, 0);
            entry.ExpandedList = new List<BinaryOcrBitmap>
            {
                new BinaryOcrBitmap(IsBitmapsAlikeBenchmarks.MakeGlyph(28, 42, seed: 1000 + i), false, 0, "b", 30, 0),
            };
            _db.CompareImagesExpanded.Add(entry);
        }
    }

    [Benchmark]
    public BinaryOcrMatcher.CompareMatch? GetCompareMatch_ExpandedScan()
    {
        return _matcher.GetCompareMatch(_list[0], out _, _list, 0, _db, maxErrorPercent: 6.0);
    }
}

/// <summary>
/// SpellCheckWordLists.Split runs per line in the spell-check underline transformer and
/// syntax-highlighting converter, i.e. on grid repaints with live spell check enabled.
/// </summary>
[MemoryDiagnoser]
public class SpellCheckSplitBenchmarks
{
    private const string Line = "It's very important to understand, that the world - as we know it - isn't just a place which accepts our norms!";

    [Benchmark]
    public List<SpellCheckWord> Split() => SpellCheckWordLists.Split(Line);
}

/// <summary>
/// UnknownWordGuesser.CreateGuessesFromLetters runs once per unknown OCR word.
/// </summary>
[MemoryDiagnoser]
public class UnknownWordGuesserBenchmarks
{
    [Benchmark]
    public int Guesses_English() => UnknownWordGuesser.CreateGuessesFromLetters("understandthe", "eng").Count();

    [Benchmark]
    public int Guesses_SkippedName() => UnknownWordGuesser.CreateGuessesFromLetters("Johnson", "eng").Count();

    [Benchmark]
    public int Guesses_Compounding() => UnknownWordGuesser.CreateGuessesFromLetters("vaudevilleveteraan", "nld").Count();
}

/// <summary>
/// The curly-brace escape scans in CustomTextFormatter run per exported paragraph;
/// GetCurlyBeginIndexesReversed/GetCurlyEndIndexesReversed were O(len * matches).
/// </summary>
[MemoryDiagnoser]
public class CustomTextFormatterCurlyBenchmarks
{
    private CustomFormatTemplate _template = null!;
    private List<Paragraph> _paragraphs = null!;

    [GlobalSetup]
    public void Setup()
    {
        _template = new CustomFormatTemplate
        {
            Name = "bench",
            Extension = ".txt",
            FormatHeader = string.Empty,
            FormatFooter = string.Empty,
            FormatParagraph = "{{literal}} {4} -> {0} --> {1} {2} {{more literals}} {16}",
            FormatTimeCode = "hh:mm:ss,zzz",
            FormatNewLine = "|",
        };

        _paragraphs = new List<Paragraph>();
        for (var i = 0; i < 200; i++)
        {
            _paragraphs.Add(new Paragraph($"Line {i} with some text{Environment.NewLine}and a second line", i * 2000, i * 2000 + 1800));
        }
    }

    [Benchmark]
    public string GenerateWithLiteralCurlies() => CustomTextFormatter.GenerateCustomText(_template, _paragraphs, "title", "video.mkv");
}
