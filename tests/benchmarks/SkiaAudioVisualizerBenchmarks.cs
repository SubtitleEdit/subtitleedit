using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Classic <see cref="AudioVisualizer"/> vs the experimental <see cref="SkiaAudioVisualizer"/>
/// on a "center video position" playback frame (the view scrolls a couple of pixels per frame).
///
///  - RecordFrame: what Render() costs the UI thread. For the classic renderer that is the
///    geometry/text recording (with its anchored caches); for Skia it is only the frame snapshot,
///    because all drawing happens later in the custom draw operation on the render thread.
///  - RecordAndRasterizeFrame: the whole frame on one thread - Render() plus actually rasterizing
///    it into a bitmap with the real Skia backend - so the render-thread work Skia moved off the
///    UI thread is counted too.
/// </summary>
[MemoryDiagnoser]
public class SkiaAudioVisualizerBenchmarks
{
    private const double HeightPx = 220;
    private const double FrameSeconds = 1 / 60.0;
    private const int PeaksPerSecond = 126;
    private const int PeaksLengthSeconds = 3600;

    private static bool _avaloniaInitialized;

    private AudioVisualizer _audioVisualizer = null!;
    private RenderTargetBitmap _bitmap = null!;
    private List<SubtitleLineViewModel> _subtitles = null!;
    private readonly List<SubtitleLineViewModel> _noSelection = new();
    private double _positionSeconds;
    private double _viewSeconds;
    private int _frameIndex;

    [Params(1600, 3200)]
    public int WidthPx { get; set; }

    [Params(WaveformDrawStyle.Classic, WaveformDrawStyle.Fancy)]
    public WaveformDrawStyle Style { get; set; }

    [Params(false, true)]
    public bool Skia { get; set; }

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

        _audioVisualizer = Skia ? new SkiaAudioVisualizer() : new AudioVisualizer();
        _audioVisualizer.WaveformDrawStyle = Style;
        _audioVisualizer.WavePeaks = MakeSpeechLikePeaks();
        _audioVisualizer.WaveformColor = Color.FromArgb(150, 144, 238, 144);
        _audioVisualizer.WaveformSelectedColor = Color.FromArgb(210, 254, 10, 10);
        _audioVisualizer.WaveformBackgroundColor = Colors.Black;
        _audioVisualizer.DrawGridLines = true;
        _audioVisualizer.Measure(new Size(WidthPx, HeightPx));
        _audioVisualizer.Arrange(new Rect(0, 0, WidthPx, HeightPx));

        _bitmap = new RenderTargetBitmap(new PixelSize(WidthPx, (int)HeightPx));
        _subtitles = SubtitleFactory.Make(2000);
        _viewSeconds = WidthPx / (double)PeaksPerSecond;
        _positionSeconds = 10;
        _frameIndex = 0;
    }

    [GlobalCleanup]
    public void Cleanup() => _bitmap.Dispose();

    [Benchmark]
    public void RecordFrame()
    {
        AdvanceCenterModeFrame();
        if (_audioVisualizer is SkiaAudioVisualizer skia)
        {
            // A DrawingGroup cannot hold custom draw operations, so take the same snapshot Render()
            // does and dispose the operation the way the compositor does when the next frame lands.
            using var drawOperation = skia.CreateDrawOperation();
            return;
        }

        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        _audioVisualizer.Render(context);
    }

    [Benchmark]
    public void RecordAndRasterizeFrame()
    {
        AdvanceCenterModeFrame();
        _bitmap.Render(_audioVisualizer);
    }

    private void AdvanceCenterModeFrame()
    {
        _positionSeconds += FrameSeconds;
        if (_positionSeconds > PeaksLengthSeconds - _viewSeconds - 5)
        {
            _positionSeconds = 10;
        }

        var startSeconds = Math.Max(0, _positionSeconds - _viewSeconds / 2.0);
        if (++_frameIndex % 3 == 0)
        {
            _audioVisualizer.SetPosition(startSeconds, _subtitles, _positionSeconds, -1, _noSelection);
        }
        else
        {
            _audioVisualizer.StartPositionSeconds = startSeconds;
            _audioVisualizer.CurrentVideoPositionSeconds = _positionSeconds;
        }
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
