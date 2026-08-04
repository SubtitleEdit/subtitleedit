using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Runs one real playback frame of <see cref="AudioVisualizer.Render"/>, headlessly but with the
/// real Skia backend, exactly the way the app's ~60 fps cursor timer drives it: advance the
/// playhead, scroll the view (center mode), refresh the paragraph window on every third frame
/// (the 50 ms position timer), then record the frame into a DrawingGroup drawing context.
///
/// Recording, deliberately not rasterizing: in the app, Render() records into the compositor's
/// deferred list on the UI thread and Skia rasterizes on the render thread - the playback
/// stutter this benchmark exists for was UI-thread saturation from the per-pixel geometry
/// build inside Render(). Rasterizing here (e.g. into a RenderTargetBitmap) would bury that
/// cost under raster work the UI thread never pays.
///
/// Three scenarios:
///  - StaticViewPlaybackFrame (baseline): the view stands still and only the cursor moves; the
///    caches hit on almost every frame on both old and new code, so this is the floor - the
///    unavoidable per-frame cost of re-recording the scene.
///  - CenterModePlaybackFrame: "center video position" is on, so StartPositionSeconds moves a
///    couple of pixels every frame. This is the pathological path - before the anchored caches,
///    every one of these frames missed the waveform/grid/timeline caches and re-ran the full
///    per-pixel geometry build; after, it should cost the same as the static baseline (the
///    rebuild runs once per 256 px block, ~every 2 s of playback at default zoom).
///  - ForcedRebuildFrame: jumps the view far enough every frame that the caches miss on both
///    old and new code. This is what one old center-mode frame cost - compare it against the
///    static baseline to isolate the geometry-build cost the anchored caches removed.
///
/// The waveform is speech-like (bursts of loud peaks) so the fancy style produces its glow
/// batches - the worst case called out in the cache comments.
/// </summary>
[MemoryDiagnoser]
public class AudioVisualizerRenderBenchmarks
{
    private const double HeightPx = 220;
    private const double FrameSeconds = 1 / 60.0;
    private const int PeaksPerSecond = 126; // Se.Settings.Waveform.WaveformMinimumSampleRate default
    private const int PeaksLengthSeconds = 3600;

    private static bool _avaloniaInitialized;

    private AudioVisualizer _audioVisualizer = null!;
    private List<SubtitleLineViewModel> _subtitles = null!;
    private readonly List<SubtitleLineViewModel> _noSelection = new();
    private double _positionSeconds;
    private double _viewSeconds;
    private int _frameIndex;

    [Params(1600, 3200)]
    public int WidthPx { get; set; }

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
        _viewSeconds = WidthPx / (double)PeaksPerSecond; // ZoomFactor is 1.0
        _positionSeconds = 10;
        _frameIndex = 0;
    }

    [Benchmark]
    public void CenterModePlaybackFrame()
    {
        _positionSeconds += FrameSeconds;
        if (_positionSeconds > PeaksLengthSeconds - _viewSeconds - 5)
        {
            _positionSeconds = 10;
        }

        var startSeconds = Math.Max(0, _positionSeconds - _viewSeconds / 2.0);
        if (++_frameIndex % 3 == 0)
        {
            // What the 50 ms position timer does: refresh the visible paragraph window.
            _audioVisualizer.SetPosition(startSeconds, _subtitles, _positionSeconds, -1, _noSelection);
        }
        else
        {
            // What the ~60 fps cursor timer does: glide the cursor and the centered scroll.
            _audioVisualizer.StartPositionSeconds = startSeconds;
            _audioVisualizer.CurrentVideoPositionSeconds = _positionSeconds;
        }

        RenderFrame();
    }

    [Benchmark]
    public void ForcedRebuildFrame()
    {
        // Alternate between two views ~100 s apart: different cache anchors on the new code,
        // different exact positions on the old, so both rebuild the full geometry every frame.
        // Everything else mirrors the other scenarios (paragraph refresh on every third frame)
        // so the difference against the static baseline is purely the geometry rebuild.
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

        RenderFrame();
    }

    [Benchmark(Baseline = true)]
    public void StaticViewPlaybackFrame()
    {
        const double viewStart = 10;
        _positionSeconds += FrameSeconds;
        if (_positionSeconds > viewStart + _viewSeconds - 1)
        {
            _positionSeconds = viewStart + 1;
        }

        if (++_frameIndex % 3 == 0)
        {
            _audioVisualizer.SetPosition(viewStart, _subtitles, _positionSeconds, -1, _noSelection);
        }
        else
        {
            _audioVisualizer.CurrentVideoPositionSeconds = _positionSeconds;
        }

        RenderFrame();
    }

    private void RenderFrame()
    {
        // Record only - see the class summary. The DrawingGroup context is a plain
        // display-list recorder, so this measures what Render() costs the UI thread.
        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        _audioVisualizer.Render(context);
    }

    /// <summary>
    /// Speech-like peaks: ~300 ms bursts of loud audio separated by ~200 ms of near-silence,
    /// with deterministic pseudo-noise inside the bursts. Loud enough that the fancy style
    /// crosses its medium/high amplitude thresholds and emits glow geometry.
    /// </summary>
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
