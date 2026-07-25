using System.Collections.ObjectModel;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// MainViewModel's 50 ms position timer refills a scratch list with every subtitle line and then
/// re-checks that it is still start-time sorted, 20 times a second, before handing it to the
/// waveform. This measures what that per-tick pass actually costs.
/// </summary>
[MemoryDiagnoser]
public class WaveformBufferBenchmarks
{
    private ObservableCollection<SubtitleLineViewModel> _subtitles = new();
    private readonly List<SubtitleLineViewModel> _buffer = new();

    [Params(1000, 5000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _subtitles = new ObservableCollection<SubtitleLineViewModel>(SubtitleFactory.Make(Lines));
        _buffer.Capacity = Lines;
    }

    [Benchmark]
    public int RefillAndSortCheck()
    {
        var subtitle = _buffer;
        subtitle.Clear();
        if (subtitle.Capacity < _subtitles.Count)
        {
            subtitle.Capacity = _subtitles.Count;
        }

        for (var i = 0; i < _subtitles.Count; i++)
        {
            subtitle.Add(_subtitles[i]);
        }

        ListSortUtil.SortIfNeeded(subtitle, static (a, b) => a.StartTime.Ticks.CompareTo(b.StartTime.Ticks));
        return subtitle.Count;
    }
}
