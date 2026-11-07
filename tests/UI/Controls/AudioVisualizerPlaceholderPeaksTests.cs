using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Controls;

/// <summary>
/// Issue #16710: with "auto-generate waveform" off, the timeline gets silent placeholder peaks so
/// the subtitles can be seen and edited without extracting any audio. The "click to generate"
/// hint shrinks to a small label at the top - only a click on that label generates the waveform,
/// the rest of the timeline edits the subtitles as usual.
/// </summary>
public class AudioVisualizerPlaceholderPeaksTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly bool _snapToFrames = Se.Settings.Waveform.SnapToFrames;
    private readonly bool _snapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;
    private readonly int _sampleRate = Se.Settings.Waveform.WaveformMinimumSampleRate;

    private const double WidthPx = 800;
    private const double HeightPx = 200;

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        Se.Settings.Waveform.SnapToFrames = _snapToFrames;
        Se.Settings.Waveform.SnapToShotChanges = _snapToShotChanges;
        Se.Settings.Waveform.WaveformMinimumSampleRate = _sampleRate;
    }

    private static SubtitleLineViewModel Line(double startSeconds, double endSeconds) => new()
    {
        Text = "text",
        StartTime = TimeSpan.FromSeconds(startSeconds),
        EndTime = TimeSpan.FromSeconds(endSeconds),
    };

    private (Window Window, AudioVisualizer Av) Open(bool useSkia, WavePeakData2? peaks, params SubtitleLineViewModel[] lines)
    {
        Se.Settings.Waveform.SnapToFrames = false;
        Se.Settings.Waveform.SnapToShotChanges = false;

        AudioVisualizer av = useSkia ? new SkiaAudioVisualizer() : new AudioVisualizer();
        av.WavePeaks = peaks;
        av.Width = WidthPx;
        av.Height = HeightPx;
        av.ClickToGenerateText = "Click to generate waveform";
        av.ShowClickToGenerateHint = true;
        av.SetPosition(0, new List<SubtitleLineViewModel>(lines), 0, 0, new List<SubtitleLineViewModel>());

        var window = new Window { Width = WidthPx, Height = HeightPx, Content = av };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (window, av);
    }

    [Fact]
    public void CreatePlaceholder_CoversTheDurationAndIsMarked()
    {
        Se.Settings.Waveform.WaveformMinimumSampleRate = 126;

        var peaks = WavePeakData2.CreatePlaceholder(90.5);

        Assert.True(peaks.IsPlaceholder);
        Assert.Equal(126, peaks.SampleRate);
        Assert.True(peaks.LengthInSeconds >= 90.5);
        Assert.Equal(0, peaks.HighestPeak);
    }

    [Fact]
    public void SmpteConversion_KeepsThePlaceholder()
    {
        var peaks = WavePeakData2.CreatePlaceholder(60);

        Assert.Same(peaks, AudioVisualizer.ToSmpteDropFrameTime(peaks));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Placeholder_DragMovesTheCueInsteadOfGenerating(bool useSkia)
    {
        var line = Line(1, 3);
        var (window, av) = Open(useSkia, WavePeakData2.CreatePlaceholder(60), line);
        var requested = 0;
        av.OnGenerateWaveformRequested += (_, _) => requested++;
        var sampleRate = av.WavePeaks!.SampleRate;

        // Middle of the cue (2 s), well below the label at the top.
        var x = 2 * sampleRate;
        window.MouseDown(new Point(x, 150), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(x + sampleRate, 150), RawInputModifiers.LeftMouseButton);
        window.MouseUp(new Point(x + sampleRate, 150), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, requested);
        Assert.Equal(2, line.StartTime.TotalSeconds, 2);
        Assert.Equal(4, line.EndTime.TotalSeconds, 2);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Placeholder_ClickOnLabelRequestsGeneration(bool useSkia)
    {
        var line = Line(1, 3);
        var (window, av) = Open(useSkia, WavePeakData2.CreatePlaceholder(60), line);
        var requested = 0;
        av.OnGenerateWaveformRequested += (_, _) => requested++;

        // The label is centered horizontally near the top.
        window.MouseDown(new Point(WidthPx / 2, 14), MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(new Point(WidthPx / 2, 14), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, requested);
        Assert.Equal(1, line.StartTime.TotalSeconds, 3);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoPeaks_ClickAnywhereRequestsGeneration(bool useSkia)
    {
        var (window, av) = Open(useSkia, null);
        var requested = 0;
        av.OnGenerateWaveformRequested += (_, _) => requested++;

        window.MouseDown(new Point(100, 150), MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(new Point(100, 150), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, requested);
    }

    [AvaloniaFact]
    public void Placeholder_HasNoSilenceToFind()
    {
        var (_, av) = Open(false, WavePeakData2.CreatePlaceholder(60));

        Assert.True(av.HasPlaceholderPeaks);
        Assert.Equal(-1, av.FindDataBelowThreshold(10, 0.5));
        Assert.Equal(-1, av.FindDataBelowThresholdBack(10, 0.5));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Placeholder_RendersWithoutThrowing(bool useSkia)
    {
        var (window, _) = Open(useSkia, WavePeakData2.CreatePlaceholder(60), Line(1, 3), Line(4, 6));

        var frame = window.CaptureRenderedFrame();

        Assert.NotNull(frame);
    }
}
