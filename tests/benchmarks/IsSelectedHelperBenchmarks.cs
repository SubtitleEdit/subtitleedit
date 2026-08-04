using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Mirrors what the waveform's per-pixel loop does: reset the helper with the current
/// selection, then probe it once per horizontal pixel with a monotonically increasing
/// sample position. Runs on every waveform geometry rebuild (scroll / zoom / resize /
/// selection change), so with "select all" on a large subtitle it is squarely in the
/// scrolling hot path.
/// </summary>
[MemoryDiagnoser]
public class IsSelectedHelperBenchmarks
{
    private const int SampleRate = 100;
    private const int WidthPixels = 1600;

    private readonly IsSelectedHelper _helper = new();
    private List<SubtitleLineViewModel> _selection = new();
    private double _startSample;
    private double _samplesPerPixel;

    [Params(1, 100, 5000)]
    public int SelectedLines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var all = SubtitleFactory.Make(Math.Max(SelectedLines, 1));
        _selection = all;

        // Show a ~40 second window somewhere in the middle of the selection.
        var middle = all[all.Count / 2];
        _startSample = middle.StartTime.TotalSeconds * SampleRate;
        _samplesPerPixel = 40.0 * SampleRate / WidthPixels;
    }

    [Benchmark]
    public void ResetOnly() => _helper.Reset(_selection, SampleRate, (int)_startSample, (int)(_startSample + WidthPixels * _samplesPerPixel));

    [Benchmark]
    public int ResetAndScan()
    {
        _helper.Reset(_selection, SampleRate, (int)_startSample, (int)(_startSample + WidthPixels * _samplesPerPixel));

        var selected = 0;
        for (var x = 0; x < WidthPixels; x++)
        {
            var pos = (int)(_startSample + x * _samplesPerPixel);
            if (_helper.IsSelected(pos))
            {
                selected++;
            }
        }

        return selected;
    }
}
