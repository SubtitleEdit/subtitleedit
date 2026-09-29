using System.Collections.ObjectModel;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustDuration;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 26. Every class calls the real (public or internal) method, so the same file is run once
// against the commit before the change and once after. ControlAutoBreak is the drift control.

/// <summary>Opening a raw DVD .vob: all video packs between subtitle packs are skipped.</summary>
[MemoryDiagnoser]
public class Round26VobSubBenchmarks
{
    private byte[] _vob = Array.Empty<byte>();

    [Params(2000)]
    public int Packs { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(26);
        using var ms = new MemoryStream();
        for (var i = 0; i < Packs; i++)
        {
            var pack = new byte[2048];
            byte[] header = [0, 0, 1, 0xBA, 0x44, 0, 0x04, 0, 0x04, 0x01, 0x01, 0x89, 0xC3, 0xF8];
            header.CopyTo(pack, 0);
            random.NextBytes(pack.AsSpan(14 + 6));
            if (i % 20 == 5)
            {
                // private stream 1 with a PTS and sub-picture stream id 0x20
                byte[] pes = [0, 0, 1, 0xBD, 0x07, 0xEC, 0x81, 0x80, 0x05, 0x21, 0x00, 0x01, 0x00, 0x01, 0x20];
                pes.CopyTo(pack, 14);
            }
            else
            {
                byte[] pes = [0, 0, 1, 0xE0, 0x07, 0xEC];
                pes.CopyTo(pack, 14);
            }

            ms.Write(pack);
        }

        _vob = ms.ToArray();
    }

    [Benchmark]
    public int OpenVob()
    {
        var parser = new VobSubParser(true);
        parser.Open(new MemoryStream(_vob, false));
        return parser.VobSubPacks.Count;
    }
}

/// <summary>Fix names dialog / batch: which names from names.xml + en_names.xml occur with the wrong casing.</summary>
[MemoryDiagnoser]
public class Round26FixNamesBenchmarks
{
    private Subtitle _subtitle = null!;
    private List<string> _names = null!;

    [Params(5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = BenchmarkSubtitles.Build(Lines);
        _names = new NameList(Configuration.DictionariesDirectory, "en_US", false, string.Empty).GetAllNames();
    }

    [Benchmark]
    public int FindNames()
    {
        return FixNamesLogic.FindNames(_subtitle, _names, "Abc, jones", "en_US").Count;
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

/// <summary>Auto-translate CPU work around the engine call (echo engine, so no network).</summary>
[MemoryDiagnoser]
public class Round26TranslateBenchmarks
{
    private Subtitle _subtitle = null!;
    private readonly TranslationPair _source = new("English", "en", "en");
    private readonly TranslationPair _target = new("Danish", "da", "da");

    [Params(3000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        MergeAndSplitHelper.AbbreviationsForLanguage = _ => new HashSet<string> { "Mrs.", "Dr." };
        Configuration.Settings.Tools.AutoTranslateDelaySeconds = 0;
        _subtitle = BenchmarkSubtitles.Build(Lines);
    }

    [Benchmark]
    public async Task<int> Merged()
    {
        var rows = await new DoAutoTranslate().DoTranslate(_subtitle, _source, _target, new EchoTranslator(), CancellationToken.None);
        return rows.Count;
    }

    [Benchmark]
    public async Task<int> SingleLine()
    {
        var translate = new DoAutoTranslate { TranslateEachLineSeparately = true };
        var rows = await translate.DoTranslate(_subtitle, _source, _target, new EchoTranslator(), CancellationToken.None);
        return rows.Count;
    }

    private sealed class EchoTranslator : IAutoTranslator
    {
        public string Name => "Echo";
        public string Url => string.Empty;
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;
        public void Initialize() { }
        public List<TranslationPair> GetSupportedSourceLanguages() => [];
        public List<TranslationPair> GetSupportedTargetLanguages() => [];
        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken) => Task.FromResult(text);
    }
}

/// <summary>Image subtitle editor, Adjust all durations.</summary>
[MemoryDiagnoser]
public class Round26BinaryAdjustDurationBenchmarks
{
    private List<BinarySubtitleItem> _items = null!;

    [Params(10000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _items = new List<BinarySubtitleItem>(Lines);
        for (var i = 0; i < Lines; i++)
        {
            _items.Add(new BinarySubtitleItem(TimeSpan.FromMilliseconds(i * 3000), TimeSpan.FromMilliseconds(i * 3000 + 1500)));
        }
    }

    [Benchmark]
    public TimeSpan AdjustAllFixed()
    {
        var vm = new BinaryAdjustDurationViewModel
        {
            SelectedAdjustType = new BinaryAdjustDurationDisplay { Type = BinaryAdjustDurationType.Fixed },
            AdjustFixed = 2,
        };
        vm.AdjustDuration(_items);
        return _items[^1].EndTime;
    }
}

/// <summary>Speech to text post-processing, merge short lines (AutoBalanceLines).</summary>
[MemoryDiagnoser]
public class Round26SpeechToTextBalanceBenchmarks
{
    private Subtitle _subtitle = null!;

    [Params(10000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = new Subtitle();
        for (var i = 0; i < Lines; i++)
        {
            _subtitle.Paragraphs.Add(new Paragraph("so we went down to the", i * 1000, i * 1000 + 950));
        }
    }

    [Benchmark]
    public int AutoBalanceLines()
    {
        return new SpeechToTextPostProcessor("en").AutoBalanceLines(_subtitle, "en").Paragraphs.Count;
    }
}

/// <summary>Autodetect cost of formats that parse any input as XML (5000-line TTML and SubRip files).</summary>
[MemoryDiagnoser]
public class Round26XmlDetectBenchmarks
{
    private List<string> _ttmlLines = null!;
    private List<string> _srtLines = null!;

    [Params(5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var subtitle = BenchmarkSubtitles.Build(Lines);
        _ttmlLines = new TimedText10().ToText(subtitle, "x").SplitToLines();
        _srtLines = new SubRip().ToText(subtitle, "x").SplitToLines();
    }

    [Benchmark]
    public int BelleNuitTtml() => Load(new BelleNuitSubtitler(), _ttmlLines);

    [Benchmark]
    public int BelleNuitSrt() => Load(new BelleNuitSubtitler(), _srtLines);

    [Benchmark]
    public int Unknown67Ttml() => Load(new UnknownSubtitle67(), _ttmlLines);

    [Benchmark]
    public int Unknown67Srt() => Load(new UnknownSubtitle67(), _srtLines);

    private static int Load(SubtitleFormat format, List<string> lines)
    {
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, null);
        return subtitle.Paragraphs.Count;
    }
}

/// <summary>
/// Self-contained old/new shapes for view-model code that needs a window: the Set sync point
/// 150 ms tick sort, and the Merge lines with same time codes preview loop (MergeItems scan per
/// merge + a List&lt;int&gt;.Contains bookkeeping list that was never read).
/// </summary>
[MemoryDiagnoser]
public class Round26ViewModelShapeBenchmarks
{
    private List<SubtitleLineViewModel> _lines = new();
    private List<SubtitleLineViewModel>? _sortedLines;
    private readonly AdvancedSubStationAlpha _assa = new();

    [Params(10000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var subtitle = BenchmarkSubtitles.Build(Lines);
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            // pairs of lines with the same time codes
            var p = subtitle.Paragraphs[i];
            p.StartTime.TotalMilliseconds = i / 2 * 2000;
            p.EndTime.TotalMilliseconds = i / 2 * 2000 + 1800;
        }

        _lines = subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, _assa)).ToList();
        var oldItems = MergePreviewOld();
        var newItems = MergePreviewNew();
        if (oldItems.Count != newItems.Count || oldItems.Where((t, i) => t.MergedText != newItems[i].MergedText || t.Lines != newItems[i].Lines).Any())
        {
            throw new InvalidOperationException("old and new merge previews disagree");
        }
    }

    [Benchmark]
    public int SetSyncPointTickOld()
    {
        return _lines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList().Count;
    }

    [Benchmark]
    public int SetSyncPointTickNew()
    {
        return (_sortedLines ??= _lines.OrderBy(p => p.StartTime.TotalMilliseconds).ToList()).Count;
    }

    [Benchmark]
    public int MergeSameTimeCodesOld() => MergePreviewOld().Count;

    [Benchmark]
    public int MergeSameTimeCodesNew() => MergePreviewNew().Count;

    private List<MergeDisplayItem> MergePreviewOld()
    {
        var mergeItems = new List<MergeDisplayItem>();
        var mergedIndexes = new List<int>();
        SubtitleLineViewModel? p = null;
        var singleMergeSubtitles = new List<SubtitleLineViewModel>();
        var mergedText = string.Empty;
        for (var i = 1; i < _lines.Count; i++)
        {
            if (singleMergeSubtitles.Count == 0)
            {
                p = _lines[i - 1];
            }

            var next = _lines[i];
            if (p != null && MergeSameTimeCodesViewModel.QualifiesForMerge(p, next, 250) && IsFixAllowed(mergeItems, p))
            {
                AddToGroup(singleMergeSubtitles, p, next, ref mergedText);
                if (!mergedIndexes.Contains(i))
                {
                    mergedIndexes.Add(i);
                }

                if (!mergedIndexes.Contains(i - 1))
                {
                    mergedIndexes.Add(i - 1);
                }
            }
            else if (singleMergeSubtitles.Count > 0)
            {
                FlushGroup(mergeItems, singleMergeSubtitles, ref mergedText);
            }
        }

        if (singleMergeSubtitles.Count > 0)
        {
            FlushGroup(mergeItems, singleMergeSubtitles, ref mergedText);
        }

        return mergeItems;
    }

    private List<MergeDisplayItem> MergePreviewNew()
    {
        var mergeItems = new List<MergeDisplayItem>();
        SubtitleLineViewModel? p = null;
        var singleMergeSubtitles = new List<SubtitleLineViewModel>();
        var mergedText = string.Empty;
        for (var i = 1; i < _lines.Count; i++)
        {
            if (singleMergeSubtitles.Count == 0)
            {
                p = _lines[i - 1];
            }

            var next = _lines[i];
            if (p != null && MergeSameTimeCodesViewModel.QualifiesForMerge(p, next, 250))
            {
                AddToGroup(singleMergeSubtitles, p, next, ref mergedText);
            }
            else if (singleMergeSubtitles.Count > 0)
            {
                FlushGroup(mergeItems, singleMergeSubtitles, ref mergedText);
            }
        }

        if (singleMergeSubtitles.Count > 0)
        {
            FlushGroup(mergeItems, singleMergeSubtitles, ref mergedText);
        }

        return mergeItems;
    }

    private static void AddToGroup(List<SubtitleLineViewModel> group, SubtitleLineViewModel p, SubtitleLineViewModel next, ref string mergedText)
    {
        if (!group.Contains(p))
        {
            group.Add(p);
        }

        if (!group.Contains(next))
        {
            group.Add(next);
        }

        if (group.Count == 2)
        {
            mergedText = p.Text;
        }

        mergedText = Utilities.AutoBreakLine(MergeSameTimeCodesViewModel.GetMergedLines(mergedText, next.Text, true), "en");
    }

    private static void FlushGroup(List<MergeDisplayItem> mergeItems, List<SubtitleLineViewModel> group, ref string mergedText)
    {
        var groupName = (mergeItems.Count + 1).ToString();
        foreach (var svm in group)
        {
            _ = new SubtitleLineViewModel(svm) { Extra = groupName };
        }

        mergeItems.Add(new MergeDisplayItem(true, group, mergedText, groupName));
        group.Clear();
        mergedText = string.Empty;
    }

    private static bool IsFixAllowed(List<MergeDisplayItem> mergeItems, SubtitleLineViewModel p)
    {
        foreach (var mi in mergeItems.Where(p => !p.Apply))
        {
            foreach (var line in mi.LinesToMerge)
            {
                if (line.Id == p.Id)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
