using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UITests.Controls;

/// <summary>
/// Waveform drag behavior for issue #13600.
///
/// The drag delta is anchored in absolute waveform time, not pixels: when the view scrolls
/// mid-drag (the position timer jumps the view while the video plays, or the user wheel-scrolls
/// without releasing), the dragged edge must keep following the pointer - SE4 behaved this way,
/// and it is what makes "extend an end cue while the video plays" workable without releasing and
/// re-grabbing the edge once per screenful.
///
/// The classic draw style must also not rebuild its cached per-pixel waveform geometry when a
/// selected line's times change (i.e. on every pointer move of a drag) - the selection is painted
/// as a clipped overlay instead. The fancy style bakes selection into per-column colors, so it
/// still rebuilds.
/// </summary>
public class AudioVisualizerDragTests
{
    private const int SampleRate = 126; // Se.Settings.Waveform.WaveformMinimumSampleRate default
    private const double WidthPx = 800;
    private const double HeightPx = 200;

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    private static SubtitleLineViewModel Line(double startSeconds, double endSeconds) => new()
    {
        Text = "text",
        StartTime = TimeSpan.FromSeconds(startSeconds),
        EndTime = TimeSpan.FromSeconds(endSeconds),
    };

    private static (Window Window, AudioVisualizer Av, List<SubtitleLineViewModel> Lines) Open(params SubtitleLineViewModel[] lines)
    {
        var av = new AudioVisualizer
        {
            WavePeaks = MakePeaks(60),
            Width = WidthPx,
            Height = HeightPx,
        };

        var list = new List<SubtitleLineViewModel>(lines);
        av.SetPosition(0, list, 0, 0, new List<SubtitleLineViewModel>());

        var window = new Window
        {
            Width = WidthPx,
            Height = HeightPx,
            Content = av,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        return (window, av, list);
    }

    [AvaloniaFact]
    public void ResizeRight_FollowsPointer_WhileViewIsStatic()
    {
        // One line 1-3 s; at zoom 1 the right edge sits at x = 3 * 126 = 378.
        var (window, av, _) = Open(Line(1, 3));
        var line = av.SelectedParagraph!;

        window.MouseDown(new Point(378, 100), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(478, 100), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // +100 px = +100/126 s.
        Assert.Equal(3 + 100.0 / SampleRate, line.EndTime.TotalSeconds, 2);

        window.MouseUp(new Point(478, 100), MouseButton.Left, RawInputModifiers.None);
        window.Close();
    }

    [AvaloniaFact]
    public void ResizeRight_FollowsScroll_WhenViewScrollsMidDrag()
    {
        // The #13600 regression: the view scrolling under an active drag must carry the dragged
        // edge along with the pointer. With the old pixel-delta math the edge stayed frozen at
        // the pre-scroll time and the drag went dead at the window border.
        var (window, av, _) = Open(Line(1, 3));
        var line = av.SelectedParagraph!;

        window.MouseDown(new Point(378, 100), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(478, 100), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // What the position timer does when the playhead crosses the view edge during playback.
        av.StartPositionSeconds = 2;
        window.MouseMove(new Point(479, 100), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // End = original 3 s + pointer delta (101/126 s) + the 2 s the view scrolled.
        Assert.Equal(3 + 101.0 / SampleRate + 2, line.EndTime.TotalSeconds, 2);

        window.MouseUp(new Point(479, 100), MouseButton.Left, RawInputModifiers.None);
        window.Close();
    }

    [AvaloniaFact]
    public void MoveWholeLine_FollowsScroll_WhenViewScrollsMidDrag()
    {
        var (window, av, _) = Open(Line(1, 3));
        var line = av.SelectedParagraph!;

        // Grab the middle of the line (x = 2 s * 126 = 252) and drag right.
        window.MouseDown(new Point(252, 100), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(302, 100), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1 + 50.0 / SampleRate, line.StartTime.TotalSeconds, 2);

        av.StartPositionSeconds = 2;
        window.MouseMove(new Point(303, 100), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1 + 51.0 / SampleRate + 2, line.StartTime.TotalSeconds, 2);
        Assert.Equal(2, line.Duration.TotalSeconds, 2); // duration preserved by a whole-line move

        window.MouseUp(new Point(303, 100), MouseButton.Left, RawInputModifiers.None);
        window.Close();
    }

    /// <summary>Ages the "last pointer edit" stamp past the 500 ms grace, i.e. simulates half a
    /// second of a held-down drag in which the time codes did not change.</summary>
    private static void ExpirePointerEditGrace(AudioVisualizer av)
    {
        var field = typeof(AudioVisualizer).GetField("_lastPointerEditMs", BindingFlags.NonPublic | BindingFlags.Instance)!;
        field.SetValue(av, Environment.TickCount64 - 5000);
    }

    [AvaloniaFact]
    public void IsEditingWithPointer_StaysTrue_WhenDragPausesMidDrag_Issue13636()
    {
        // Undo change detection is suppressed for the whole drag, not just 500 ms after the last
        // time-code-changing move. Holding still mid-drag (or fine-tuning inside one pixel column,
        // where the same-X early return never stamps) used to reopen the window, and the next
        // detection tick banked the half-finished times as an undo entry - so undo stepped back
        // through the middle of the drag instead of to before it.
        var (window, av, _) = Open(Line(1, 3));

        window.MouseDown(new Point(378, 100), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(478, 100), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.True(av.IsEditingWithPointer);

        ExpirePointerEditGrace(av);
        Assert.True(av.IsEditingWithPointer);

        // The on-video preview settle keeps the plain 500 ms timing: a wasted refresh mid-drag is
        // cheap, and a held pause is when the user wants to see the frame they are aiming at.
        Assert.False(av.IsPointerEditSettling);

        window.MouseUp(new Point(478, 100), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // Released: only the 500 ms tail is left, and that one has been aged out - so the settled
        // times are free to be snapshotted, once.
        Assert.False(av.IsEditingWithPointer);

        window.Close();
    }

    [AvaloniaFact]
    public void IsEditingWithPointer_False_AfterEscapeCancelsDrag()
    {
        var (window, av, _) = Open(Line(1, 3));

        window.MouseDown(new Point(378, 100), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(478, 100), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        Dispatcher.UIThread.RunJobs();
        ExpirePointerEditGrace(av);

        Assert.False(av.IsEditingWithPointer);

        window.Close();
    }

    [AvaloniaFact]
    public void IsEditingWithPointer_False_WhenPointerMovesWithNoButtonDown()
    {
        // Self-heal for a drag that never delivered a release: any later button-less move over the
        // waveform clears the flag, so change detection can never stay switched off.
        var (window, av, _) = Open(Line(1, 3));

        window.MouseDown(new Point(378, 100), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(478, 100), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        ExpirePointerEditGrace(av);
        Assert.True(av.IsEditingWithPointer);

        window.MouseMove(new Point(500, 100), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        ExpirePointerEditGrace(av);

        Assert.False(av.IsEditingWithPointer);

        window.Close();
    }

    [AvaloniaFact]
    public void IsEditingWithPointer_False_ForAPlainClick()
    {
        // A press that starts no drag (empty area click on a waveform with no line under it is a
        // new-selection drag, so click the middle of the line and release without moving) must not
        // suppress change detection at all.
        var (window, av, _) = Open(Line(1, 3));

        window.MouseDown(new Point(252, 100), MouseButton.Left, RawInputModifiers.None);
        Assert.True(av.IsEditingWithPointer); // press established a Moving drag
        window.MouseUp(new Point(252, 100), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.False(av.IsEditingWithPointer);

        window.Close();
    }

    private static Geometry FirstCachedWaveformGeometry(AudioVisualizer av)
    {
        var field = typeof(AudioVisualizer).GetField("_waveformCacheDraws", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var list = (System.Collections.IList)field.GetValue(av)!;
        Assert.True(list.Count > 0);
        var item = (ValueTuple<IPen, Geometry>)list[0]!;
        return item.Item2;
    }

    private static void RenderFrame(AudioVisualizer av)
    {
        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        av.Render(context);
    }

    private static AudioVisualizer MakeMeasuredVisualizer(WaveformDrawStyle style, out SubtitleLineViewModel selectedLine)
    {
        var av = new AudioVisualizer
        {
            WaveformDrawStyle = style,
            WavePeaks = MakePeaks(60),
        };
        av.Measure(new Size(WidthPx, HeightPx));
        av.Arrange(new Rect(0, 0, WidthPx, HeightPx));

        var lines = new List<SubtitleLineViewModel> { Line(1, 3) };
        av.SetPosition(0, lines, 0, 0, new List<SubtitleLineViewModel>());
        selectedLine = lines[0];
        return av;
    }

    [AvaloniaFact]
    public void ClassicWaveformCache_SurvivesSelectedTimeChange()
    {
        // Dragging a selected line changes its times on every pointer move; the classic style
        // must replay the cached geometry (selection is a draw-time overlay), not rebuild it.
        var av = MakeMeasuredVisualizer(WaveformDrawStyle.Classic, out var line);

        RenderFrame(av);
        var before = FirstCachedWaveformGeometry(av);

        line.EndTime = line.EndTime + TimeSpan.FromSeconds(0.2);
        RenderFrame(av);
        var after = FirstCachedWaveformGeometry(av);

        Assert.Same(before, after);
    }

    [AvaloniaFact]
    public void FancyWaveformCache_RebuildsOnSelectedTimeChange()
    {
        // The fancy style bakes the selection into per-column colors, so a selected-time change
        // must still invalidate the cached geometry.
        var av = MakeMeasuredVisualizer(WaveformDrawStyle.Fancy, out var line);

        RenderFrame(av);
        var before = FirstCachedWaveformGeometry(av);

        line.EndTime = line.EndTime + TimeSpan.FromSeconds(0.2);
        RenderFrame(av);
        var after = FirstCachedWaveformGeometry(av);

        Assert.NotSame(before, after);
    }
}
