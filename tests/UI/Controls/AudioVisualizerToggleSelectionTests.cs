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
/// Multi-select in the waveform (#17119): Ctrl+click (Cmd or Shift+click on macOS) on a line adds it
/// to / removes it from the selection. Ctrl+click on a line used to set the end of the selected line
/// at the click instead, and Shift+click on macOS toggled and then also moved a selected line's start.
/// A modifier click on the one selected line itself still sets its start/end.
/// </summary>
public class AudioVisualizerToggleSelectionTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly bool _snapToFrames = Se.Settings.Waveform.SnapToFrames;
    private readonly bool _snapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;

    private const int SampleRate = 126;

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        Se.Settings.Waveform.SnapToFrames = _snapToFrames;
        Se.Settings.Waveform.SnapToShotChanges = _snapToShotChanges;
    }

    [Theory]
    [InlineData(KeyModifiers.Control, false, true)]
    [InlineData(KeyModifiers.Control | KeyModifiers.Shift, false, false)]
    [InlineData(KeyModifiers.Shift, false, false)]
    [InlineData(KeyModifiers.Control | KeyModifiers.Alt, false, false)]
    [InlineData(KeyModifiers.None, false, false)]
    [InlineData(KeyModifiers.Meta, true, true)]
    [InlineData(KeyModifiers.Shift, true, true)]
    [InlineData(KeyModifiers.Meta | KeyModifiers.Shift, true, false)]
    [InlineData(KeyModifiers.Control, true, false)]
    [InlineData(KeyModifiers.None, true, false)]
    public void ToggleModifierPerPlatform(KeyModifiers modifiers, bool isMac, bool expected)
    {
        Assert.Equal(expected, AudioVisualizer.IsToggleSelectionModifier(modifiers, isMac));
    }

    private static SubtitleLineViewModel Line(double start, double end) => new()
    {
        Text = "text",
        StartTime = TimeSpan.FromSeconds(start),
        EndTime = TimeSpan.FromSeconds(end),
    };

    private (Window Window, AudioVisualizer Av, List<SubtitleLineViewModel> Toggled) Open(bool useSkia, SubtitleLineViewModel[] lines, SubtitleLineViewModel selected)
    {
        Se.Settings.Waveform.SnapToFrames = false;
        Se.Settings.Waveform.SnapToShotChanges = false;

        AudioVisualizer av = useSkia ? new SkiaAudioVisualizer() : new AudioVisualizer();
        var peaks = new WavePeak2[SampleRate * 60];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(200, -200);
        }

        av.WavePeaks = new WavePeakData2(SampleRate, peaks);
        av.Width = 1000;
        av.Height = 200;
        av.SetPosition(0, new List<SubtitleLineViewModel>(lines), 0, 0, new List<SubtitleLineViewModel> { selected });
        var toggled = new List<SubtitleLineViewModel>();
        av.OnToggleSelection += (_, e) => toggled.Add(e.Paragraph);

        var window = new Window { Width = 1000, Height = 200, Content = av };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (window, av, toggled);
    }

    private static void Click(Window window, double seconds, RawInputModifiers modifiers)
    {
        var point = new Point(seconds * SampleRate, 120);
        window.MouseDown(point, MouseButton.Left, modifiers);
        window.MouseUp(point, MouseButton.Left, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    private static RawInputModifiers ToggleModifier => OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control;

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToggleClickOnAnotherLine_TogglesIt_AndLeavesTheSelectedLineAlone(bool useSkia)
    {
        var first = Line(1, 3);
        var second = Line(4, 6);
        var (window, _, toggled) = Open(useSkia, new[] { first, second }, first);

        Click(window, 5, ToggleModifier);

        Assert.Equal(new[] { second.Id }, toggled.Select(p => p.Id));
        Assert.Equal(1, first.StartTime.TotalSeconds, 3);
        Assert.Equal(3, first.EndTime.TotalSeconds, 3);
        Assert.Equal(4, second.StartTime.TotalSeconds, 3);
        Assert.Equal(6, second.EndTime.TotalSeconds, 3);
    }

    [AvaloniaFact]
    public void MacShiftClickOnAnotherLine_OnlyTogglesIt()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        var first = Line(1, 3);
        var second = Line(4, 6);
        var (window, av, toggled) = Open(false, new[] { first, second }, second);

        Click(window, 2, RawInputModifiers.Shift);

        Assert.Equal(new[] { first.Id }, toggled.Select(p => p.Id));

        // The Shift+click used to toggle and then also set the start of the (now first) selected
        // line - the clicked one - at the click.
        foreach (var line in av.AllSelectedParagraphs.Concat(new[] { first, second }))
        {
            Assert.Contains(line.StartTime.TotalSeconds, new[] { 1.0, 4.0 });
            Assert.Contains(line.EndTime.TotalSeconds, new[] { 3.0, 6.0 });
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToggleClickOnTheOneSelectedLine_StillSetsItsEnd(bool useSkia)
    {
        var first = Line(1, 3);
        var (window, _, toggled) = Open(useSkia, new[] { first }, first);

        Click(window, 2.5, ToggleModifier);

        Assert.Empty(toggled);
        Assert.Equal(1, first.StartTime.TotalSeconds, 3);
        Assert.Equal(2.5, first.EndTime.TotalSeconds, 2);
    }
}
