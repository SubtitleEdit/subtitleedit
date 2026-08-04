using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// AudioVisualizer.DrawParagraphs builds a hash set of the selected paragraphs on every frame so
/// the per-paragraph "am I selected" probe is O(1). Only paragraphs inside the visible window are
/// ever probed, so hashing the whole selection is waste - these two benchmarks are the before and
/// after of that filter.
/// </summary>
[MemoryDiagnoser]
public class WaveformSelectionBenchmarks
{
    private readonly HashSet<SubtitleLineViewModel> _set = new();
    private List<SubtitleLineViewModel> _selection = new();
    private double _windowStartMs;
    private double _windowEndMs;

    [Params(1, 100, 5000)]
    public int SelectedLines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _selection = SubtitleFactory.Make(Math.Max(SelectedLines, 1));

        // A ~40 second window in the middle, i.e. what fits on screen.
        var middle = _selection[_selection.Count / 2];
        _windowStartMs = middle.StartTime.TotalMilliseconds;
        _windowEndMs = _windowStartMs + 40_000;
    }

    [Benchmark(Baseline = true)]
    public int HashWholeSelection()
    {
        _set.Clear();
        foreach (var selected in _selection)
        {
            _set.Add(selected);
        }

        return _set.Count;
    }

    [Benchmark]
    public int HashVisibleWindowOnly()
    {
        _set.Clear();
        for (var i = 0; i < _selection.Count; i++)
        {
            var selected = _selection[i];
            if (selected.EndTime.TotalMilliseconds >= _windowStartMs &&
                selected.StartTime.TotalMilliseconds <= _windowEndMs)
            {
                _set.Add(selected);
            }
        }

        return _set.Count;
    }
}
