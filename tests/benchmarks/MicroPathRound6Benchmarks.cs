using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Globalization;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The grid's time-code cells as the app actually drives them: every visible row formats its
/// start and end time and its duration once for the cell binding and a second time for the row's
/// accessible name, which is a MultiBinding over the same values through the same converter
/// instances (InitListViewAndEditBox). <see cref="GridCellConverterBenchmarks"/> converts each
/// value once and so cannot see that.
/// </summary>
[MemoryDiagnoser]
public class TimeCodeCellRepaintBenchmarks
{
    private const int Rows = 40; // a viewport, not the whole file - the grid virtualizes

    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    private TimeSpanToDisplayFullConverter _fullTime = null!;
    private TimeSpanToDisplayShortConverter _shortTime = null!;
    private TimeSpan[] _startTimes = null!;
    private TimeSpan[] _endTimes = null!;
    private TimeSpan[] _durations = null!;

    [GlobalSetup]
    public void Setup()
    {
        GridCellConverterBenchmarks.EnsureAvalonia();

        _fullTime = new TimeSpanToDisplayFullConverter();
        _shortTime = new TimeSpanToDisplayShortConverter();

        _startTimes = new TimeSpan[Rows];
        _endTimes = new TimeSpan[Rows];
        _durations = new TimeSpan[Rows];
        for (var i = 0; i < Rows; i++)
        {
            _startTimes[i] = TimeSpan.FromMilliseconds(i * 2400 + 137);
            _durations[i] = TimeSpan.FromMilliseconds(1500 + i % 900);
            _endTimes[i] = _startTimes[i] + _durations[i];
        }
    }

    /// <summary>One repaint: three cell bindings plus the accessible-name MultiBinding per row.</summary>
    [Benchmark]
    public object ViewportRepaint()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            // Cells
            last = _fullTime.Convert(_startTimes[i], typeof(string), null, _culture);
            last = _fullTime.Convert(_endTimes[i], typeof(string), null, _culture);
            last = _shortTime.Convert(_durations[i], typeof(string), null, _culture);

            // AutomationProperties.Name MultiBinding over the same three values
            last = _fullTime.Convert(_startTimes[i], typeof(string), null, _culture);
            last = _fullTime.Convert(_endTimes[i], typeof(string), null, _culture);
            last = _shortTime.Convert(_durations[i], typeof(string), null, _culture);
        }

        return last;
    }
}

/// <summary>
/// One playback frame of the waveform in two states the other render benchmarks do not cover:
/// zoomed far out, where a screenful holds many paragraph rectangles and their footers, and with a
/// video loaded whose waveform has not been generated yet (the "click to generate" hint). Both
/// render at the full ~60 fps rate because CurrentVideoPositionSeconds is an AffectsRender
/// property the cursor timer writes on every tick.
/// </summary>
[MemoryDiagnoser]
public class WaveformPerFrameTextBenchmarks
{
    private const double WidthPx = 1600;
    private const double HeightPx = 220;
    private const double FrameSeconds = 1 / 60.0;
    private const int PeaksPerSecond = 126;
    private const int PeaksLengthSeconds = 600;

    private static bool _avaloniaInitialized;

    private AudioVisualizer _zoomedOut = null!;
    private AudioVisualizer _noPeaks = null!;
    private List<SubtitleLineViewModel> _subtitles = null!;
    private readonly List<SubtitleLineViewModel> _noSelection = new();
    private RenderTargetBitmap _renderTarget = null!;
    private double _positionSeconds;
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

        Se.Settings.Waveform.WaveformShowNumberAndDuration = true;
        Se.Settings.Waveform.WaveformShowCps = true;

        _renderTarget = new RenderTargetBitmap(new PixelSize((int)WidthPx, (int)HeightPx), new Vector(96, 96));

        _zoomedOut = new AudioVisualizer
        {
            WaveformDrawStyle = WaveformDrawStyle.Classic,
            WavePeaks = MakeSpeechLikePeaks(),
            // n = ZoomFactor * SampleRate lands between 15 and 51: the band where each footer is
            // just "#N", and where a screenful holds a lot of paragraphs.
            ZoomFactor = 0.25,
        };
        Arrange(_zoomedOut);

        _noPeaks = new AudioVisualizer
        {
            ShowClickToGenerateHint = true,
            ClickToGenerateText = "Click here to generate the waveform",
        };
        Arrange(_noPeaks);

        _subtitles = SubtitleFactory.Make(2000);
        _positionSeconds = 10;
        _frameIndex = 0;
    }

    private static void Arrange(AudioVisualizer av)
    {
        av.Measure(new Size(WidthPx, HeightPx));
        av.Arrange(new Rect(0, 0, WidthPx, HeightPx));
    }

    [Benchmark]
    public void ZoomedOutParagraphFooters()
    {
        _positionSeconds += FrameSeconds;
        if (_positionSeconds > PeaksLengthSeconds - 60)
        {
            _positionSeconds = 10;
        }

        if (++_frameIndex % 3 == 0)
        {
            _zoomedOut.SetPosition(10, _subtitles, _positionSeconds, -1, _noSelection);
        }
        else
        {
            _zoomedOut.CurrentVideoPositionSeconds = _positionSeconds;
        }

        RenderFrame(_zoomedOut);
    }

    [Benchmark]
    public void ClickToGenerateHintFrame()
    {
        _positionSeconds += FrameSeconds;
        _noPeaks.CurrentVideoPositionSeconds = _positionSeconds;
        RenderFrame(_noPeaks);
    }

    private void RenderFrame(AudioVisualizer av)
    {
        using var context = _renderTarget.CreateDrawingContext();
        av.Render(context);
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

/// <summary>
/// The syntax highlighting converter over rows that carry colours - an HTML font tag and an ASSA
/// colour override. Those are the rows that re-parsed the tag and built a brush on every repaint;
/// <see cref="SyntaxHighlightingConverterBenchmarks"/> mixes them into a realistic file where they
/// are one row in ten, which hides the per-row cost.
/// </summary>
[MemoryDiagnoser]
public class ColoredSyntaxHighlightingBenchmarks
{
    private const int Rows = 200;

    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    private TextWithSubtitleSyntaxHighlightingConverter _converter = null!;
    private string[] _texts = null!;

    [GlobalSetup]
    public void Setup()
    {
        GridCellConverterBenchmarks.EnsureAvalonia();
        Se.Settings.Appearance.SubtitleGridFormattingType = (int)SubtitleGridFormattingTypes.ShowFormatting;
        _converter = new TextWithSubtitleSyntaxHighlightingConverter();

        // A colour-graded file: the same handful of font tags repeated down the whole subtitle,
        // which is what colour tagging looks like in practice.
        string[] sentences =
        {
            "<font color=\"#ff8800\" face=\"Arial\" size=\"20\">Somewhere in Denmark</font>",
            "<font color=\"#00ccff\">- Are you coming with us?</font>\r\n<font color=\"#00ccff\">- No.</font>",
            "{\\i1\\c&H00FF00&\\fnArial\\fs28}Do you have any idea{\\i0}\r\nwhat you have done?",
            "<font color=\"#ff8800\" face=\"Arial\" size=\"20\">He said: \"Never again.\"</font>",
        };

        _texts = new string[Rows];
        for (var i = 0; i < Rows; i++)
        {
            _texts[i] = sentences[i % sentences.Length];
        }
    }

    /// <summary>The default grid mode: the tags are applied and hidden.</summary>
    [Benchmark]
    public object ColoredRowsRepaint()
    {
        Se.Settings.Appearance.SubtitleGridFormattingType = (int)SubtitleGridFormattingTypes.ShowFormatting;
        return RunAll();
    }

    /// <summary>
    /// "Show tags" mode, where the tag text itself is coloured - the mode that runs the
    /// tokenizer's colour-value parsers (the edit box runs them per keystroke too).
    /// </summary>
    [Benchmark]
    public object ColoredRowsRepaintShowTags()
    {
        Se.Settings.Appearance.SubtitleGridFormattingType = (int)SubtitleGridFormattingTypes.ShowTags;
        return RunAll();
    }

    private object RunAll()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _converter.Convert(_texts[i], typeof(object), null, _culture);
        }

        return last;
    }
}
