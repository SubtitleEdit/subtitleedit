using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// One real playback frame of <see cref="AudioVisualizer.Render"/> with the spectrogram shown -
/// the sibling scenario of <see cref="AudioVisualizerRenderBenchmarks"/>, which only ever runs
/// the waveform. Every other draw method in the control has an anchored per-block cache;
/// DrawSpectrogram was the one that rebuilt its whole bitmap on every frame (a new SKBitmap, N
/// blits into it and a full per-pixel premultiply walk into a fresh WriteableBitmap), so this
/// stands in for what the ~60 fps cursor timer pays while the spectrogram is visible.
///
/// Rasterized rather than recorded (see RenderFrame): the DrawingGroup recorder that
/// AudioVisualizerRenderBenchmarks uses does not implement DrawBitmap, the one call
/// DrawSpectrogram makes. The raster cost is the same in all three scenarios below.
///
/// Scenarios:
///  - StaticViewPlaybackFrame (baseline): the view stands still (plain playback, waveform not
///    centered) and only the cursor moves.
///  - CenterModePlaybackFrame: "center video position" is on, so the view scrolls a few
///    spectrogram columns every frame - the pathological path for an exact-position cache key.
///  - WaveformOnlyPlaybackFrame: the same static frame with the spectrogram hidden, as the floor.
/// </summary>
[MemoryDiagnoser]
public class SpectrogramRenderBenchmarks
{
    private const double WidthPx = 1600;
    private const double HeightPx = 220;
    private const double FrameSeconds = 1 / 60.0;
    private const int PeaksPerSecond = 126; // Se.Settings.Waveform.WaveformMinimumSampleRate default
    private const int PeaksLengthSeconds = 600;

    // What WavePeakGenerator2 uses: fft size 256 (image height 128), 1024 columns per image,
    // one column per 256 samples at 44.1 kHz.
    private const int FftSize = 256;
    private const int SpectrogramImageWidth = 1024;
    private const double SampleDuration = FftSize / 44100.0;

    private static bool _avaloniaInitialized;

    private AudioVisualizer _withSpectrogram = null!;
    private AudioVisualizer _waveformOnly = null!;
    private List<SubtitleLineViewModel> _subtitles = null!;
    private readonly List<SubtitleLineViewModel> _noSelection = new();
    private double _centerPositionSeconds;
    private double _staticPositionSeconds;
    private double _viewSeconds;
    private int _frameIndex;

    private const double StaticViewStart = 10;

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

        _renderTarget = new RenderTargetBitmap(new PixelSize((int)WidthPx, (int)HeightPx), new Vector(96, 96));
        _withSpectrogram = MakeVisualizer(withSpectrogram: true);
        _waveformOnly = MakeVisualizer(withSpectrogram: false);

        _subtitles = SubtitleFactory.Make(2000);
        _viewSeconds = WidthPx / (double)PeaksPerSecond; // ZoomFactor is 1.0
        _centerPositionSeconds = 10;
        _staticPositionSeconds = StaticViewStart + 1;
        _frameIndex = 0;
    }

    private static AudioVisualizer MakeVisualizer(bool withSpectrogram)
    {
        var av = new AudioVisualizer
        {
            WaveformDrawStyle = WaveformDrawStyle.Classic,
            WavePeaks = MakeSpeechLikePeaks(),
        };

        if (withSpectrogram)
        {
            av.SetSpectrogram(MakeSpectrogram());
            av.SetDisplayMode(WaveformDisplayMode.WaveformAndSpectrogram);
        }

        av.Measure(new Size(WidthPx, HeightPx));
        av.Arrange(new Rect(0, 0, WidthPx, HeightPx));
        return av;
    }

    [Benchmark]
    public void CenterModePlaybackFrame()
    {
        _centerPositionSeconds += FrameSeconds;
        if (_centerPositionSeconds > PeaksLengthSeconds - _viewSeconds - 5)
        {
            _centerPositionSeconds = 10;
        }

        Drive(_withSpectrogram, Math.Max(0, _centerPositionSeconds - _viewSeconds / 2.0), _centerPositionSeconds);
        RenderFrame(_withSpectrogram);
    }

    [Benchmark(Baseline = true)]
    public void StaticViewPlaybackFrame()
    {
        AdvanceStatic();
        Drive(_withSpectrogram, StaticViewStart, _staticPositionSeconds);
        RenderFrame(_withSpectrogram);
    }

    [Benchmark]
    public void WaveformOnlyPlaybackFrame()
    {
        AdvanceStatic();
        Drive(_waveformOnly, StaticViewStart, _staticPositionSeconds);
        RenderFrame(_waveformOnly);
    }

    private void AdvanceStatic()
    {
        _staticPositionSeconds += FrameSeconds;
        if (_staticPositionSeconds > StaticViewStart + _viewSeconds - 1)
        {
            _staticPositionSeconds = StaticViewStart + 1;
        }
    }

    private void Drive(AudioVisualizer av, double startSeconds, double positionSeconds)
    {
        // Every third frame is the 50 ms position timer refreshing the paragraph window; the
        // other two are the ~60 fps cursor timer gliding the cursor and the centered scroll.
        if (++_frameIndex % 3 == 0)
        {
            av.SetPosition(startSeconds, _subtitles, positionSeconds, -1, _noSelection);
        }
        else
        {
            av.StartPositionSeconds = startSeconds;
            av.CurrentVideoPositionSeconds = positionSeconds;
        }
    }

    private RenderTargetBitmap _renderTarget = null!;

    private void RenderFrame(AudioVisualizer av)
    {
        // Rasterizing rather than recording into a DrawingGroup (which AudioVisualizerRenderBenchmarks
        // can do): the DrawingGroup recorder does not implement DrawBitmap, which is exactly the call
        // DrawSpectrogram makes. The raster cost is identical across all three scenarios here, so the
        // differences between them are still purely the per-frame spectrogram work.
        using var context = _renderTarget.CreateDrawingContext();
        av.Render(context);
    }

    /// <summary>
    /// Spectrogram images shaped exactly like the generator's: Rgba8888/Premul, 1024 columns
    /// wide and fft/2 tall, with a deterministic pattern so nothing is a uniform block.
    /// </summary>
    private static SpectrogramData2 MakeSpectrogram()
    {
        const int height = FftSize / 2;
        var imageCount = (int)(PeaksLengthSeconds / (SampleDuration * SpectrogramImageWidth)) + 2;
        var images = new List<SKBitmap>(imageCount);
        var buffer = new byte[SpectrogramImageWidth * height * 4];
        for (var i = 0; i < imageCount; i++)
        {
            for (var p = 0; p < buffer.Length; p += 4)
            {
                var v = (byte)((p / 4 + i * 37) % 251);
                buffer[p] = v;
                buffer[p + 1] = (byte)(255 - v);
                buffer[p + 2] = (byte)(v / 2);
                buffer[p + 3] = 255;
            }

            var bmp = new SKBitmap(SpectrogramImageWidth, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            Marshal.Copy(buffer, 0, bmp.GetPixels(), buffer.Length);
            images.Add(bmp);
        }

        return new SpectrogramData2(FftSize, SpectrogramImageWidth, SampleDuration, images);
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
