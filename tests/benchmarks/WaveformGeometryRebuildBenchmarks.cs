using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Isolates the waveform geometry rebuild (the per-pixel BuildWaveFormFancy/Classic loops):
/// every frame jumps the view ~100 s, so the anchored waveform cache misses and Render()
/// re-runs the full build - a trimmed-down ForcedRebuildFrame from
/// AudioVisualizerRenderBenchmarks, at one width, kept lean so before/after runs of a
/// build-loop change are quick. Classic is the drift control for changes that only touch
/// the fancy loop (its ratio to its own baseline should stay ~1.0).
/// </summary>
[MemoryDiagnoser]
public class WaveformGeometryRebuildBenchmarks
{
    private const double HeightPx = 220;
    private const int WidthPx = 3200;
    private const int PeaksPerSecond = 126;
    private const int PeaksLengthSeconds = 3600;

    private static bool _avaloniaInitialized;

    private AudioVisualizer _audioVisualizer = null!;
    private List<SubtitleLineViewModel> _subtitles = null!;
    private readonly List<SubtitleLineViewModel> _noSelection = new();
    private double _viewSeconds;
    private int _frameIndex;

    [Params(WaveformDrawStyle.Classic, WaveformDrawStyle.Fancy)]
    public WaveformDrawStyle Style { get; set; }

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

        _audioVisualizer = new AudioVisualizer
        {
            WaveformDrawStyle = Style,
            WavePeaks = MakeSpeechLikePeaks(),
        };
        _audioVisualizer.Measure(new Size(WidthPx, HeightPx));
        _audioVisualizer.Arrange(new Rect(0, 0, WidthPx, HeightPx));

        _subtitles = SubtitleFactory.Make(2000);
        _viewSeconds = WidthPx / (double)PeaksPerSecond;
        _frameIndex = 0;
    }

    [Benchmark]
    public void RebuildFrame()
    {
        _frameIndex++;
        var startSeconds = (_frameIndex & 1) == 0 ? 100.0 : 200.0;
        var cursorSeconds = startSeconds + _viewSeconds / 2.0;
        if (_frameIndex % 3 == 0)
        {
            _audioVisualizer.SetPosition(startSeconds, _subtitles, cursorSeconds, -1, _noSelection);
        }
        else
        {
            _audioVisualizer.StartPositionSeconds = startSeconds;
            _audioVisualizer.CurrentVideoPositionSeconds = cursorSeconds;
        }

        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        _audioVisualizer.Render(context);
    }

    private static WavePeakData2 MakeSpeechLikePeaks()
    {
        var random = new Random(42);
        var peaks = new WavePeak2[PeaksPerSecond * PeaksLengthSeconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            var seconds = i / (double)PeaksPerSecond;
            var inBurst = seconds % 0.5 < 0.3;
            var amplitude = inBurst ? random.Next(4000, 28000) : random.Next(0, 900);
            peaks[i] = new WavePeak2((short)amplitude, (short)-random.Next(amplitude / 2, amplitude + 1));
        }

        return new WavePeakData2(PeaksPerSecond, peaks);
    }
}
