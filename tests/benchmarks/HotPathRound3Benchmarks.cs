using Avalonia;
using Avalonia.Headless;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Reflection;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Find/Count/ReplaceAll walk every line of the subtitle. Regex mode built an identity
/// List&lt;int&gt; index map per line (only needed when the line contains '\r'), and the
/// whole-word paths compiled a new Regex per line.
/// </summary>
[MemoryDiagnoser]
public class FindServiceRound3Benchmarks
{
    private FindService _service = null!;
    private List<string> _lines = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sentences = new[]
        {
            "It was the best of times, it was the worst of times.",
            "Are you coming with us?",
            "- No.\n- Then stay here and wait for the others.",
            "I told you already, this is not going to work out the way you think it will.",
            "Hello there.",
            "Somewhere in Denmark,\r\na quiet evening begins.",
        };

        _lines = new List<string>(2000);
        for (var i = 0; i < 2000; i++)
        {
            _lines.Add(sentences[i % sentences.Length]);
        }

        _service = new FindService();
    }

    [Benchmark]
    public int CountRegex()
    {
        return _service.Count(@"t[he]+\b", _lines, false, FindService.FindMode.RegularExpression);
    }

    [Benchmark]
    public int FindAllRegex()
    {
        _service.Initialize(_lines, 0, false, FindService.FindMode.RegularExpression);
        return _service.FindAll(@"\bth\w+").Count;
    }

    [Benchmark]
    public int FindAllWholeWord()
    {
        _service.Initialize(_lines, 0, true, FindService.FindMode.CaseInsensitive);
        return _service.FindAll("the").Count;
    }

    [Benchmark]
    public int ReplaceAllWholeWord()
    {
        // Replacing "the" with itself keeps the run idempotent across iterations while
        // still exercising the full per-line whole-word replace path.
        _service.Initialize(_lines, 0, true, FindService.FindMode.CaseInsensitive);
        return _service.ReplaceAll("the", "the");
    }
}

/// <summary>
/// The Compare window diffs every changed line pair with a repeated longest-common-substring
/// walk that went near-cubic on repetitive text; same greedy result now comes from an
/// O(n*m)-per-round suffix-run table.
/// </summary>
[MemoryDiagnoser]
public class TextDiffRound3Benchmarks
{
    private Func<string, string, int, List<(int pos1, int pos2, int length)>> _findCommonSubstrings = null!;
    private (string a, string b)[] _pairs = null!;
    private string _longA = null!;
    private string _longB = null!;

    [GlobalSetup]
    public void Setup()
    {
        var type = typeof(FindService).Assembly.GetType("Nikse.SubtitleEdit.Features.Files.Compare.TextDiffHighlighter")!;
        var method = type.GetMethod("FindLongestCommonSubstrings", BindingFlags.NonPublic | BindingFlags.Static)!;
        _findCommonSubstrings = method.CreateDelegate<Func<string, string, int, List<(int, int, int)>>>();

        _pairs = new[]
        {
            ("It was the best of times, it was the worst of times.",
             "It was the best of days, it was the worst of days."),
            ("I told you already, this is not going to work out.",
             "I told you before, this is never going to work out."),
            ("- No.\n- Then stay here and wait for the others.",
             "- No!\n- Then wait here and stay with the others."),
            // Repetitive text is the old walk's worst case (every position matches every position).
            ("la la la la la la la la la la la la la la la la la la",
             "la la la la la la la la la la la la la la la la laa la"),
            ("Somewhere in Denmark, a quiet evening begins to fall.",
             "Somewhere in Sweden, a quiet morning begins to dawn."),
        };

        _longA = string.Concat(Enumerable.Repeat("ha ha ha he he ", 30));
        _longB = _longA.Remove(200, 3).Insert(320, "hi hi ");
    }

    [Benchmark]
    public int DiffLinePairs()
    {
        var total = 0;
        foreach (var (a, b) in _pairs)
        {
            total += _findCommonSubstrings(a, b, 3).Count;
        }

        return total;
    }

    [Benchmark]
    public int DiffRepetitiveLongPair()
    {
        // Long repetitive text: every position matches every position and the common runs
        // are long, the shape that sent the naive walk cubic (a UI freeze in the Compare
        // window / Multiple Replace preview).
        return _findCommonSubstrings(_longA, _longB, 3).Count;
    }
}

/// <summary>
/// Auto-translate merges rows until the URL-encoded batch limit; the size check re-encoded
/// the whole accumulated text (plus a throwaway concat) for every visited row, and the
/// sentence-end bookkeeping materialized the accumulated StringBuilder per merged row.
/// </summary>
[MemoryDiagnoser]
public class MergeSplitRound3Benchmarks
{
    private TranslateRow[] _rows = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sentences = new[]
        {
            "and then we walked along the shore for a while",
            "because nobody had told them what happened that night",
            "which was exactly what she had been afraid of",
            "so the others kept going without looking back",
            "until the lights of the harbour finally came into view.",
        };

        _rows = new TranslateRow[400];
        for (var i = 0; i < _rows.Length; i++)
        {
            _rows[i] = new TranslateRow
            {
                Number = i + 1,
                Show = TimeSpan.FromMilliseconds(i * 2500),
                Hide = TimeSpan.FromMilliseconds(i * 2500 + 2200),
                Text = sentences[i % sentences.Length],
            };
        }
    }

    [Benchmark]
    public int MergeMultipleLines()
    {
        var result = MergeAndSplitHelper.MergeMultipleLines(_rows, 0, 10000, false, false);
        return result.Text.Length;
    }
}

/// <summary>
/// nOCR runs several shape predicates per unmatched glyph; each one re-scanned the whole
/// bitmap for empty rows into a fresh bool[Height]. Bitmap construction happens inside the
/// benchmark so per-instance caching is measured from cold, like real matching.
/// </summary>
[MemoryDiagnoser]
public class BinaryOcrPredicateRound3Benchmarks
{
    private (int width, int height, List<(int x, int y)> pixels)[] _shapes = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _shapes = new (int, int, List<(int, int)>)[60];
        for (var i = 0; i < _shapes.Length; i++)
        {
            var width = random.Next(6, 40);
            var height = random.Next(12, 60);
            var pixels = new List<(int, int)>();
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (random.Next(4) == 0)
                    {
                        pixels.Add((x, y));
                    }
                }
            }

            _shapes[i] = (width, height, pixels);
        }
    }

    [Benchmark]
    public int AllPredicates()
    {
        var hits = 0;
        foreach (var (width, height, pixels) in _shapes)
        {
            var bitmap = new BinaryOcrBitmap(width, height);
            foreach (var (x, y) in pixels)
            {
                bitmap.SetPixel(x, y);
            }

            if (bitmap.IsPeriod()) hits++;
            if (bitmap.IsPeriodAtTop(20)) hits++;
            if (bitmap.IsComma()) hits++;
            if (bitmap.IsApostrophe()) hits++;
            if (bitmap.IsLowercaseI(out _)) hits++;
            if (bitmap.IsLowercaseJ()) hits++;
            if (bitmap.IsColon()) hits++;
            if (bitmap.IsDash()) hits++;
            if (bitmap.IsExclamationMark()) hits++;
            if (bitmap.IsLowercaseL()) hits++;
            if (bitmap.IsC()) hits++;
            if (bitmap.IsO()) hits++;
        }

        return hits;
    }
}

/// <summary>
/// Fix common errors: select-all toggled IsSelected per visible item, and every set re-ran
/// the full summary/chip recount - O(visible x fixes x chips) per click.
/// </summary>
[MemoryDiagnoser]
public class FixesSelectAllRound3Benchmarks
{
    private static bool _avaloniaInitialized;
    private FixCommonErrorsViewModel _viewModel = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (!_avaloniaInitialized)
        {
            _avaloniaInitialized = true;
            AppBuilder.Configure<Application>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .UseSkia()
                .WithInterFont()
                .SetupWithoutStarting();
        }

        var actions = new[]
        {
            "Fix empty lines", "Fix overlapping display times", "Fix short display times",
            "Fix long display times", "Fix invalid italic tags", "Fix unneeded spaces",
        };

        _viewModel = new FixCommonErrorsViewModel(null!, null!, null!);
        for (var i = 0; i < 1500; i++)
        {
            var p = new Paragraph($"Line {i} text goes here.", i * 2000, i * 2000 + 1500) { Number = i + 1 };
            // Goes through the same path the scan uses, so the per-item PropertyChanged
            // subscription that drives the summary recount is attached.
            _viewModel.AddFixToListView(p, actions[i % actions.Length], "before", "after", i % 3 != 0);
        }
    }

    [Benchmark]
    public int SelectAllToggle()
    {
        // Alternates select-all/deselect-all; each call is one full bulk pass.
        _viewModel.FixesSelectAll();
        return _viewModel.Fixes.Count;
    }
}

/// <summary>
/// Unchanged code path used as the run-to-run noise yardstick - if this one moves, the
/// difference is machine noise, not the optimizations.
/// </summary>
[MemoryDiagnoser]
public class NoiseYardstickRound3Benchmarks
{
    private string _text = null!;

    [GlobalSetup]
    public void Setup()
    {
        _text = string.Concat(Enumerable.Repeat("It was the best of times, æøå 1234. ", 200));
    }

    [Benchmark]
    public int UrlEncodeLengthYardstick()
    {
        return Utilities.UrlEncodeLength(_text);
    }
}
