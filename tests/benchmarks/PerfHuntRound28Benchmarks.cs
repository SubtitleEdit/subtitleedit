using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.Compare;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 28. The format classes call the real LoadSubtitle, so the same file is run once against
// the commit before the change and once after. ControlAutoBreak is the drift control.

/// <summary>
/// Opening a two hour MacCaption (.mcc) file: ~180k frame lines of CEA-708 caption data.
/// </summary>
[MemoryDiagnoser]
public class Round28MccLoadBenchmarks
{
    private List<string> _lines = null!;

    [Params(1500)]
    public int Captions { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        var subtitle = new Subtitle();
        for (var i = 0; i < Captions; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length], i * 4000 + 1000, i * 4000 + 3500));
        }

        _lines = new MacCaption10().ToText(subtitle, string.Empty).SplitToLines();
    }

    [Benchmark]
    public int LoadMcc()
    {
        var subtitle = new Subtitle();
        new MacCaption10().LoadSubtitle(subtitle, _lines, "a.mcc");
        return subtitle.Paragraphs.Count;
    }

    /// <summary>Drift control - untouched by this round.</summary>
    [Benchmark]
    public int ControlAutoBreak()
    {
        var total = 0;
        for (var i = 0; i < 3000; i++)
        {
            total += Utilities.AutoBreakLine(BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length]).Length;
        }

        return total;
    }
}

/// <summary>Formats that looked up the paragraph just added (or its index) by a list scan.</summary>
[MemoryDiagnoser]
public class Round28FormatLoadBenchmarks
{
    private List<string> _cheetah = null!;
    private List<string> _timeCodesOnly3 = null!;
    private List<string> _nci = null!;

    [Params(5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        var subtitle = BenchmarkSubtitles.Build(Lines);

        // pop-on captions without end codes - every end time comes from the next caption
        _cheetah = new CheetahCaptionAsc().ToText(subtitle, string.Empty).SplitToLines()
            .Where(line => !line.StartsWith("*E ", StringComparison.Ordinal)).ToList();
        _timeCodesOnly3 = new TimeCodesOnly3().ToText(subtitle, string.Empty).SplitToLines();
        _nci = new NciTimedRollUpCaptions().ToText(subtitle, string.Empty).SplitToLines();
    }

    [Benchmark]
    public int LoadCheetahCaptionAsc() => Load(new CheetahCaptionAsc(), _cheetah);

    [Benchmark]
    public int LoadTimeCodesOnly3() => Load(new TimeCodesOnly3(), _timeCodesOnly3);

    [Benchmark]
    public int LoadNciTimedRollUpCaptions() => Load(new NciTimedRollUpCaptions(), _nci);

    private static int Load(SubtitleFormat format, List<string> lines)
    {
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, null);
        return subtitle.Paragraphs.Count;
    }
}

/// <summary>
/// Compare window line-up with the HH:MM:SS:FF time format: the aligner asks whether two times
/// are equal for every pair of lines in its dynamic program. Old = four TimeCode display strings
/// per pair (the CompareViewModel.IsTimeEqual shape), new = one display string per distinct time.
/// </summary>
[MemoryDiagnoser]
public class Round28CompareAlignBenchmarks
{
    private List<CompareAligner.Line> _left = null!;
    private List<CompareAligner.Line> _right = null!;
    private bool _oldFormat;

    [Params(3000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _oldFormat = Configuration.Settings.General.UseTimeFormatHHMMSSFF;
        Configuration.Settings.General.UseTimeFormatHHMMSSFF = true;
        _left = new List<CompareAligner.Line>();
        _right = new List<CompareAligner.Line>();
        for (var i = 0; i < Lines; i++)
        {
            var start = TimeSpan.FromMilliseconds(i * 2500);
            var end = start + TimeSpan.FromMilliseconds(2000);
            _left.Add(new CompareAligner.Line($"line number {i} in the original", start, end));

            // every 100th line unchanged (a text anchor), the rest reworded and retimed by 120 ms
            _right.Add(i % 100 == 0
                ? new CompareAligner.Line($"line number {i} in the original", start, end)
                : new CompareAligner.Line($"reworded text {i * 7} of the edit", start + TimeSpan.FromMilliseconds(120), end + TimeSpan.FromMilliseconds(120)));
        }

        var oldPairs = CompareAligner.Align(_left, _right, IsTimeEqualOld);
        var newPairs = AlignWithCache();
        if (!oldPairs.SequenceEqual(newPairs))
        {
            throw new InvalidOperationException("Line-up differs");
        }
    }

    [GlobalCleanup]
    public void Cleanup() => Configuration.Settings.General.UseTimeFormatHHMMSSFF = _oldFormat;

    [Benchmark(Baseline = true)]
    public int AlignOld() => CompareAligner.Align(_left, _right, IsTimeEqualOld).Count;

    [Benchmark]
    public int AlignNew() => AlignWithCache().Count;

    private List<CompareAligner.Pair> AlignWithCache()
    {
        var displayStrings = new Dictionary<long, string>();
        string GetDisplayString(TimeSpan t)
        {
            if (!displayStrings.TryGetValue(t.Ticks, out var s))
            {
                s = new TimeCode(t).ToDisplayString();
                displayStrings.Add(t.Ticks, s);
            }

            return s;
        }

        return CompareAligner.Align(_left, _right, (t1, t2) => GetDisplayString(t1) == GetDisplayString(t2));
    }

    private static bool IsTimeEqualOld(TimeSpan t1, TimeSpan t2) =>
        new TimeCode(t1).ToDisplayString() == new TimeCode(t2).ToDisplayString();
}
