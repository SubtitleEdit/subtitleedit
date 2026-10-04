using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 29. Round29MergeTwoSubtitlesBenchmarks calls the real method, so it is run once against
// the commit before the change and once after; the row lookup compares both shapes in one class.

/// <summary>Merge two subtitles (SubRip output): each line of the first finds its overlapping line in the second.</summary>
[MemoryDiagnoser]
public class Round29MergeTwoSubtitlesBenchmarks
{
    private Subtitle _first = null!;
    private Subtitle _second = null!;

    [Params(10000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _first = new Subtitle();
        _second = new Subtitle();
        for (var i = 0; i < Lines; i++)
        {
            _first.Paragraphs.Add(new Paragraph("First " + i, i * 3000, i * 3000 + 2000));
            _second.Paragraphs.Add(new Paragraph("Second " + i, i * 3000 + 100, i * 3000 + 2100));
            if (i % 50 == 0)
            {
                // a line only the second subtitle has, in a gap of the first
                _second.Paragraphs.Add(new Paragraph("Extra " + i, i * 3000 + 2300, i * 3000 + 2900));
            }
        }
    }

    [Benchmark]
    public int Merge() => MergeTwoSubtitlesViewModel.BuildSubRipMerge(_first, _second).Paragraphs.Count;

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

/// <summary>
/// Copying a dialog's result (Visual sync, translate/fix selected lines) back onto the selected
/// rows: old = <c>Subtitles.FirstOrDefault(p =&gt; p.Id == id)</c> per line, new = one id dictionary.
/// </summary>
[MemoryDiagnoser]
public class Round29RowsByIdBenchmarks
{
    private List<SubtitleLineViewModel> _rows = null!;
    private List<Guid> _selectedIds = null!;

    [Params(10000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rows = BenchmarkSubtitles.Build(Lines).Paragraphs.Select(p => new SubtitleLineViewModel(p, new Nikse.SubtitleEdit.Core.SubtitleFormats.SubRip())).ToList();
        _selectedIds = _rows.Select(p => p.Id).ToList(); // select all
        if (Old() != New())
        {
            throw new InvalidOperationException("Different rows");
        }
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var hash = 0;
        foreach (var id in _selectedIds)
        {
            var row = _rows.FirstOrDefault(p => p.Id == id);
            if (row != null)
            {
                hash = hash * 31 + row.Number;
            }
        }

        return hash;
    }

    [Benchmark]
    public int New()
    {
        var rowsById = new Dictionary<Guid, SubtitleLineViewModel>(_rows.Count);
        foreach (var row in _rows)
        {
            rowsById.TryAdd(row.Id, row);
        }

        var hash = 0;
        foreach (var id in _selectedIds)
        {
            if (rowsById.TryGetValue(id, out var row))
            {
                hash = hash * 31 + row.Number;
            }
        }

        return hash;
    }
}
