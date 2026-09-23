using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 25. The first three classes call the real public methods, so they are run once against
// the commit before the change and once after (same file both times). The SUP class holds the
// old and new loop shapes side by side, and [GlobalSetup] throws when they disagree.

/// <summary>Change casing / STT sentence casing: StrippableText.FixCasing over every line with the full name list.</summary>
[MemoryDiagnoser]
public class Round25FixCasingBenchmarks
{
    private Subtitle _subtitle = null!;
    private FixCasing _fixCasing = null!;

    [Params(2000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = BenchmarkSubtitles.Build(Lines);
        _fixCasing = new FixCasing("en") { FixNormal = true };
    }

    [Benchmark]
    public int FixCasingNormal()
    {
        var subtitle = new Subtitle(_subtitle);
        _fixCasing.Fix(subtitle);
        return subtitle.Paragraphs.Count;
    }

    /// <summary>Drift control - untouched by this round.</summary>
    [Benchmark]
    public int ControlAutoBreak()
    {
        var total = 0;
        foreach (var p in _subtitle.Paragraphs)
        {
            total += Utilities.AutoBreakLine(p.Text).Length;
        }

        return total;
    }
}

/// <summary>OCR fix line pass with the Croatian list (1829 regular expressions) and the English one.</summary>
[MemoryDiagnoser]
public class Round25OcrRegexBenchmarks
{
    private string _hrvFile = string.Empty;
    private string _engFile = string.Empty;
    private OcrFixReplaceList2 _hrv = null!;
    private OcrFixReplaceList2 _eng = null!;
    private readonly Subtitle _subtitle = new();
    private readonly NoSpellChecker _spellChecker = new();
    private string[] _hrvLines = Array.Empty<string>();

    private static readonly string[] HrvSentences =
    {
        "Ne znam što bi trebao reći.",
        "- Gdje si bio cijelu noć?\n- Kod kuće, spavao sam.",
        "<i>Rekao sam ti da to neće uspjeti.</i>",
        "Moramo ići prije nego što padne mrak.",
        "Zašto mi nisi rekla istinu?",
        "Ovo je najbolji dan u mom životu!",
        "Sutra ujutro idemo u grad.",
        "Ne mogu vjerovati da si to učinio.",
    };

    [GlobalSetup]
    public void Setup()
    {
        var dictionaries = Path.Combine(BenchmarkSubtitles.FindDataDirectory(), "Dictionaries");
        _hrvFile = Path.Combine(dictionaries, "hrv_OCRFixReplaceList.xml");
        _engFile = Path.Combine(dictionaries, "eng_OCRFixReplaceList.xml");
        _hrv = new OcrFixReplaceList2(_hrvFile);
        _eng = new OcrFixReplaceList2(_engFile);
        _hrvLines = Enumerable.Range(0, 500).Select(i => HrvSentences[i % HrvSentences.Length]).ToArray();
        _hrv.FixOcrErrorViaLineReplaceList(_hrvLines[0], _subtitle, 0, _spellChecker, new List<string>(), true);
        _eng.FixOcrErrorViaLineReplaceList("warm up", _subtitle, 0, _spellChecker, new List<string>(), true);
    }

    /// <summary>500 Croatian lines through the line replace pass on a warmed-up list.</summary>
    [Benchmark]
    public int HrvLines500()
    {
        var total = 0;
        for (var i = 0; i < _hrvLines.Length; i++)
        {
            total += _hrv.FixOcrErrorViaLineReplaceList(_hrvLines[i], _subtitle, i, _spellChecker, new List<string>(), true).Length;
        }

        return total;
    }

    /// <summary>Load the list and fix the first line - what starting an OCR run costs.</summary>
    [Benchmark]
    public int HrvLoadAndFirstLine()
    {
        var list = new OcrFixReplaceList2(_hrvFile);
        return list.FixOcrErrorViaLineReplaceList(_hrvLines[0], _subtitle, 0, _spellChecker, new List<string>(), true).Length;
    }

    [Benchmark]
    public int EngLines500()
    {
        var total = 0;
        for (var i = 0; i < 500; i++)
        {
            var text = BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length];
            total += _eng.FixOcrErrorViaLineReplaceList(text, _subtitle, i, _spellChecker, new List<string>(), true).Length;
        }

        return total;
    }

    private sealed class NoSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word) => true;

        public List<string> GetSuggestions(string word) => new();
    }
}

/// <summary>MergeLinesWithSameTimeCodes (seconv /MergeSameTimeCodes, ISMT DFXP load): every other line shares its time codes.</summary>
[MemoryDiagnoser]
public class Round25MergeSameTimeCodesBenchmarks
{
    private Subtitle _subtitle = null!;

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _subtitle = new Subtitle();
        for (var i = 0; i < Lines; i++)
        {
            var start = i / 2 * 3000.0;
            _subtitle.Paragraphs.Add(new Paragraph("Line " + i, start, start + 2000));
        }

        _subtitle.Renumber();
    }

    [Benchmark]
    public int Merge()
    {
        var merged = MergeLinesWithSameTimeCodes.Merge(_subtitle, new List<int>(), out var merges, true, false, false, 250, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());
        return merged.Paragraphs.Count + merges;
    }
}

/// <summary>
/// BluRaySupParser fade merge: groups of identical consecutive images. The duplicate checks
/// scanned the whole growing removeIndices list for every pair (old) vs a set (new).
/// </summary>
[MemoryDiagnoser]
public class Round25SupFadeMergeBenchmarks
{
    private bool[] _same = Array.Empty<bool>();

    // Pictures in the SUP; every subtitle fades in and out over 8 identical frames.
    [Params(2000, 16000)]
    public int Pictures { get; set; }

    private sealed class DeleteIndex
    {
        public int Number { get; set; }
        public int Index { get; set; }
    }

    [GlobalSetup]
    public void Setup()
    {
        _same = new bool[Pictures];
        for (var i = 0; i < Pictures; i++)
        {
            _same[i] = i % 8 != 0; // picture i equals picture i - 1 except at a new subtitle
        }

        var oldResult = Old();
        var newResult = New();
        if (oldResult.Count != newResult.Count ||
            oldResult.Zip(newResult).Any(p => p.First.Number != p.Second.Number || p.First.Index != p.Second.Index))
        {
            throw new InvalidOperationException("old and new disagree");
        }
    }

    [Benchmark(Baseline = true)]
    public int OldAny() => Old().Count;

    [Benchmark]
    public int NewSet() => New().Count;

    private List<DeleteIndex> Old()
    {
        var removeIndices = new List<DeleteIndex>();
        var deleteNo = 0;
        for (var pcsIndex = _same.Length - 1; pcsIndex > 0; pcsIndex--)
        {
            if (_same[pcsIndex])
            {
                if (!removeIndices.Any(p => p.Number == deleteNo && p.Index == pcsIndex - 1))
                {
                    removeIndices.Add(new DeleteIndex { Number = deleteNo, Index = pcsIndex - 1 });
                }
                if (!removeIndices.Any(p => p.Number == deleteNo && p.Index == pcsIndex))
                {
                    removeIndices.Add(new DeleteIndex { Number = deleteNo, Index = pcsIndex });
                }
            }
            else
            {
                deleteNo++;
            }
        }

        return removeIndices;
    }

    private List<DeleteIndex> New()
    {
        var removeIndices = new List<DeleteIndex>();
        var removeIndexSet = new HashSet<(int Number, int Index)>();
        var deleteNo = 0;
        for (var pcsIndex = _same.Length - 1; pcsIndex > 0; pcsIndex--)
        {
            if (_same[pcsIndex])
            {
                if (removeIndexSet.Add((deleteNo, pcsIndex - 1)))
                {
                    removeIndices.Add(new DeleteIndex { Number = deleteNo, Index = pcsIndex - 1 });
                }
                if (removeIndexSet.Add((deleteNo, pcsIndex)))
                {
                    removeIndices.Add(new DeleteIndex { Number = deleteNo, Index = pcsIndex });
                }
            }
            else
            {
                deleteNo++;
            }
        }

        return removeIndices;
    }
}

/// <summary>
/// Real public methods, run before and after like the classes above: Compare's word split, the
/// Fix Common Errors overlap pass, and what one Remove-text-for-HI preview paid for its settings.
/// </summary>
[MemoryDiagnoser]
public class Round25ToolPassBenchmarks
{
    private Subtitle _subtitle = null!;
    private string[] _texts = Array.Empty<string>();

    [Params(20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = BenchmarkSubtitles.Build(Lines);
        _texts = _subtitle.Paragraphs.Select(p => p.Text).ToArray();
    }

    /// <summary>Compare: two SplitForChangedCalc calls per line pair per refresh.</summary>
    [Benchmark]
    public int CompareSplit()
    {
        var total = 0;
        foreach (var text in _texts)
        {
            total += Utilities.SplitForChangedCalc(text, false, false, false).Length;
            total += Utilities.SplitForChangedCalc(text, false, true, false).Length;
        }

        return total;
    }

    /// <summary>Fix Common Errors "overlapping display times" over a file with no overlaps.</summary>
    [Benchmark]
    public int FixOverlappingDisplayTimes()
    {
        var subtitle = new Subtitle(_subtitle);
        new FixOverlappingDisplayTimes().Fix(subtitle, new EmptyFixCallback());
        return subtitle.Paragraphs.Count;
    }

    /// <summary>
    /// What every HI preview paid before this round (language detection + names.xml); the
    /// dialog now does it once per working subtitle.
    /// </summary>
    [Benchmark]
    public int HiSettingsFromSubtitle() => new RemoveTextForHISettings(_subtitle).UppercaseWhitelist.Count;
}

/// <summary>
/// Per-tick work removed from two preview timers, old shape vs new shape side by side: visual
/// sync sorted all lines twice per 150 ms tick, and the ASSA override-tag dialog tagged and
/// serialized the whole subtitle every 500 ms.
/// </summary>
[MemoryDiagnoser]
public class Round25PreviewTickBenchmarks
{
    private List<SubtitleLineViewModel> _lines = new();
    private List<SubtitleLineViewModel> _selected = new();
    private List<SubtitleLineViewModel>? _sortedLines;
    private (string Tag, bool All, bool Selected, bool Forward)? _previewKey;
    private readonly AdvancedSubStationAlpha _assa = new();

    [Params(20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var subtitle = BenchmarkSubtitles.Build(Lines);
        _lines = subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, _assa)).ToList();
        _selected = _lines.Skip(100).Take(10).ToList();
        if (!VisualSyncTickNew().SequenceEqual(VisualSyncTickOld()))
        {
            throw new InvalidOperationException("old and new disagree");
        }
    }

    [Benchmark(Baseline = true)]
    public List<SubtitleLineViewModel> VisualSyncTickOld()
    {
        _ = _lines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        return _lines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
    }

    [Benchmark]
    public List<SubtitleLineViewModel> VisualSyncTickNew()
    {
        _ = _sortedLines ??= _lines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        return _sortedLines ??= _lines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
    }

    [Benchmark]
    public int AssaOverrideTickOld()
    {
        var subtitle = AssaApplyCustomOverrideTagsViewModel.BuildTaggedSubtitle(null, null, _lines, _selected, "{\\b1}", false, true, false, _assa);
        return _assa.ToText(subtitle, string.Empty).Length;
    }

    [Benchmark]
    public int AssaOverrideTickNew()
    {
        var key = ("{\\b1}", false, true, false);
        if (_previewKey == key)
        {
            return 0;
        }

        _previewKey = key;
        return AssaOverrideTickOld();
    }
}
