using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Subtitle-level bulk operations. The "selected indices" ones are what "adjust display time" and
/// "set fixed duration" run when the user has a large selection - typically Ctrl+A.
/// </summary>
[MemoryDiagnoser]
public class SubtitleBenchmarks
{
    private List<Paragraph> _source = new();
    private List<int> _allIndices = new();

    [Params(1000, 5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _source = new List<Paragraph>(Lines);
        var startMs = 1000.0;
        for (var i = 0; i < Lines; i++)
        {
            var text = i % 9 == 0 ? "   " : "Line number " + (i + 1);
            _source.Add(new Paragraph(text, startMs, startMs + 1800));
            startMs += 2000;
        }

        _allIndices = Enumerable.Range(0, Lines).ToList();
    }

    private Subtitle Fresh()
    {
        var s = new Subtitle();
        foreach (var p in _source)
        {
            s.Paragraphs.Add(new Paragraph(p, false));
        }

        return s;
    }

    [Benchmark]
    public int AdjustDisplayTimeUsingPercent()
    {
        var s = Fresh();
        s.AdjustDisplayTimeUsingPercent(120, _allIndices);
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public int SetFixedDuration()
    {
        var s = Fresh();
        s.SetFixedDuration(_allIndices, 2000);
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public int RemoveEmptyLines()
    {
        var s = Fresh();
        return s.RemoveEmptyLines();
    }

    [Benchmark]
    public int GetFastHashCode()
    {
        var s = Fresh();
        return s.GetFastHashCode(string.Empty);
    }

    /// <summary>Baseline for the three above: how much of them is just building the subtitle.</summary>
    [Benchmark]
    public int BuildOnly() => Fresh().Paragraphs.Count;
}
