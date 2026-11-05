using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 34: per-frame waste in the layout 14 timeline, recorded like
/// <see cref="AudioVisualizerRenderBenchmarks"/>, plus the ffmpeg preview snapshot that is
/// rebuilt on the UI thread after every settled edit.
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound34Benchmarks
{
    private const int WidthPx = 1600;
    private const double HeightPx = 220;
    private const int PeaksPerSecond = 126;
    private const int PeaksLengthSeconds = 1800;

    private static bool _avaloniaInitialized;

    private AudioVisualizer _waveform = null!;
    private TimelineTracks _timeline = null!;
    private List<SubtitleLineViewModel> _subtitles = null!;
    private readonly List<SubtitleLineViewModel> _noSelection = new();
    private Subtitle _plainSubtitle = null!;
    private Subtitle _taggedSubtitle = null!;
    private int _frameIndex;

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

        _subtitles = SubtitleFactory.Make(5000);

        var peaks = new WavePeak2[PeaksPerSecond * PeaksLengthSeconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        _waveform = new AudioVisualizer
        {
            WaveformDrawStyle = WaveformDrawStyle.Classic,
            WavePeaks = new WavePeakData2(PeaksPerSecond, peaks),
        };
        _waveform.Measure(new Size(WidthPx, HeightPx));
        _waveform.Arrange(new Rect(0, 0, WidthPx, HeightPx));
        _waveform.SetPosition(10, _subtitles, 11, -1, _noSelection);

        // Layout 14 after "select all": a selection of every line, no original row.
        _timeline = new TimelineTracks
        {
            Source = _waveform,
            GetAllParagraphs = () => _subtitles,
            ShowVideoRow = false,
        };
        _timeline.Measure(new Size(WidthPx, TimelineTracks.TotalHeight));
        _timeline.Arrange(new Rect(0, 0, WidthPx, TimelineTracks.TotalHeight));

        _plainSubtitle = new Subtitle();
        _taggedSubtitle = new Subtitle();
        for (var i = 0; i < 5000; i++)
        {
            _plainSubtitle.Paragraphs.Add(new Paragraph("It was the best of times,\r\nit was the worst of times.", i * 3000, i * 3000 + 2500));
            _taggedSubtitle.Paragraphs.Add(new Paragraph("<i>It was the best of times,</i>\r\n{\\an8}it was the worst of times.", i * 3000, i * 3000 + 2500));
        }
    }

    [Benchmark]
    public void TimelineFrameAllSelected()
    {
        if (_frameIndex++ == 0)
        {
            _waveform.SetPosition(10, _subtitles, 11, -1, _subtitles);
        }

        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        _timeline.Render(context);
    }

    [Benchmark]
    public int FfmpegPreviewBuildPlainText() => FfmpegPreviewSubtitle.Build(_plainSubtitle, null, false).Count;

    /// <summary>Control: tagged lines take the full path before and after.</summary>
    [Benchmark]
    public int FfmpegPreviewBuildTagged() => FfmpegPreviewSubtitle.Build(_taggedSubtitle, null, false).Count;
}
