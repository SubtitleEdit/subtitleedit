using Avalonia.Media;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The grid re-reads these getters on every cell repaint (scrolling the subtitle grid,
/// any row invalidation), for every visible row.
/// </summary>
[MemoryDiagnoser]
public class SubtitleLineViewModelBenchmarks
{
    private List<SubtitleLineViewModel> _lines = new();

    /// <summary>Rows visible in the grid at once - that is how many getters run per repaint.</summary>
    [Params(30)]
    public int VisibleRows { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Se.Settings.General.ColorTextTooLong = true;
        Se.Settings.General.ColorTextTooManyLines = true;
        Se.Settings.General.ColorTextTooWide = false; // HarfBuzz shaping; measured separately
        _lines = SubtitleFactory.Make(VisibleRows);
    }

    [Benchmark]
    public IBrush TextBackgroundBrush()
    {
        IBrush? last = null;
        for (var i = 0; i < _lines.Count; i++)
        {
            last = _lines[i].TextBackgroundBrush;
        }

        return last!;
    }

    [Benchmark]
    public double CharactersPerSecond()
    {
        var sum = 0.0;
        for (var i = 0; i < _lines.Count; i++)
        {
            sum += _lines[i].CharactersPerSecond;
        }

        return sum;
    }
}
