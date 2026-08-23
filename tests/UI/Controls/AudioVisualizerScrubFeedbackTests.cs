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
using System;
using System.Collections.Generic;

namespace UITests.Controls;

/// <summary>
/// The scrub-during-edge-drag feedback loop behind issue #13955 ("grabbing and sliding in the
/// waveform is mostly impossible to control" - the grabbed edge "flies about 10 subtitles to the
/// right").
///
/// An edge drag scrubs the video to the edge being dragged. A consumer that reacts by scrolling
/// the waveform to that position - which is what "center video position (also when paused)" does -
/// closes a loop: the drag delta is measured in absolute waveform time (#13600), so the scroll is
/// added to the delta, the edge moves further, the view scrolls further. Whole-paragraph moves
/// never scrub, which is exactly why the reporter saw them behave while edge drags did not.
///
/// MainViewModel breaks the loop by not recentering while <see cref="AudioVisualizer.IsEditingWithPointer"/>
/// is set. These tests pin the two facts that guard depends on: the flag is already true when the
/// scrub fires, and a consumer that does scroll mid-drag really does amplify the drag.
/// </summary>
public class AudioVisualizerScrubFeedbackTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double LineStart = 100;
    private const double LineEnd = 102;
    private const double ViewStart = 98;

    // The line's right edge, in px from the left of the view.
    private const double RightEdgeX = (LineEnd - ViewStart) * SampleRate; // 504
    private const double MiddleX = (LineStart + 1 - ViewStart) * SampleRate; // 378

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    private static (Window Window, AudioVisualizer Av) Open()
    {
        var av = new AudioVisualizer { WavePeaks = MakePeaks(200), Width = WidthPx, Height = 200 };
        var line = new SubtitleLineViewModel
        {
            Text = "text",
            StartTime = TimeSpan.FromSeconds(LineStart),
            EndTime = TimeSpan.FromSeconds(LineEnd),
        };

        av.SetPosition(ViewStart, new List<SubtitleLineViewModel> { line }, 0, 0, new List<SubtitleLineViewModel>());

        var window = new Window { Width = WidthPx, Height = 200, Content = av };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (window, av);
    }

    private static void DragBy(Window window, double fromX, params double[] toXs)
    {
        window.MouseDown(new Point(fromX, 100), MouseButton.Left, RawInputModifiers.None);
        foreach (var x in toXs)
        {
            window.MouseMove(new Point(x, 100), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
        }

        window.MouseUp(new Point(toXs[^1], 100), MouseButton.Left, RawInputModifiers.None);
    }

    /// <summary>Runs an action with the drag-scrubs-the-video setting on.</summary>
    private static void WithScrubOnMove(Action body)
    {
        var old = Se.Settings.Waveform.SetVideoPositionOnMoveStartEnd;
        Se.Settings.Waveform.SetVideoPositionOnMoveStartEnd = true;
        try
        {
            body();
        }
        finally
        {
            Se.Settings.Waveform.SetVideoPositionOnMoveStartEnd = old;
        }
    }

    // The contract MainViewModel's guard is built on: by the time the scrub asks for a new video
    // position, the control already reports that a pointer edit is in progress. If this ever
    // regressed, the guard would silently stop guarding and #13955 would come back.
    [AvaloniaFact]
    public void EdgeDrag_ScrubFires_WhileIsEditingWithPointerIsAlreadyTrue()
    {
        WithScrubOnMove(() =>
        {
            var (window, av) = Open();
            var fireCount = 0;
            var everFiredWhileIdle = false;
            av.OnVideoPositionChanged += (_, _) =>
            {
                fireCount++;
                if (!av.IsEditingWithPointer)
                {
                    everFiredWhileIdle = true;
                }
            };

            DragBy(window, RightEdgeX, RightEdgeX + 10, RightEdgeX + 11, RightEdgeX + 12);

            Assert.True(fireCount > 0, "the edge drag should scrub the video");
            Assert.False(everFiredWhileIdle, "every scrub during a drag must report IsEditingWithPointer");
            window.Close();
        });
    }

    // A consumer that leaves the view alone - i.e. what MainViewModel now does mid-drag - lets the
    // edge track the pointer exactly: 12 px of travel is 12 px of travel.
    [AvaloniaFact]
    public void EdgeDrag_ConsumerLeavesTheViewAlone_EdgeTracksThePointer()
    {
        WithScrubOnMove(() =>
        {
            var (window, av) = Open();
            var line = av.SelectedParagraph!;
            var half = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;

            // The guarded consumer: skip the recenter while the pointer owns the waveform.
            av.OnVideoPositionChanged += (_, e) =>
            {
                if (!av.IsEditingWithPointer)
                {
                    av.StartPositionSeconds = Math.Max(0, e.PositionInSeconds - half);
                }
            };

            DragBy(window, RightEdgeX, RightEdgeX + 10, RightEdgeX + 11, RightEdgeX + 12);

            Assert.Equal(LineEnd + 12.0 / SampleRate, line.EndTime.TotalSeconds, 3);
            Assert.Equal(ViewStart, av.StartPositionSeconds, 6); // the view never moved
            window.Close();
        });
    }

    // Why the guard has to exist: an unguarded recenter turns 12 px of pointer travel into
    // seconds of edge travel. This is the runaway the reporter described.
    [AvaloniaFact]
    public void EdgeDrag_ConsumerRecentersMidDrag_AmplifiesTheDragRunaway()
    {
        WithScrubOnMove(() =>
        {
            var (window, av) = Open();
            var line = av.SelectedParagraph!;
            var half = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;

            av.OnVideoPositionChanged += (_, e) =>
                av.StartPositionSeconds = Math.Max(0, e.PositionInSeconds - half);

            DragBy(window, RightEdgeX, RightEdgeX + 10, RightEdgeX + 11, RightEdgeX + 12);

            var intended = 12.0 / SampleRate;               // ~0.095 s
            var actual = line.EndTime.TotalSeconds - LineEnd;
            Assert.True(actual > intended * 10,
                $"expected the unguarded loop to amplify the drag, but it moved {actual:0.###} s for {intended:0.###} s of pointer travel");
            window.Close();
        });
    }

    // The asymmetry the reporter noticed: dragging a whole paragraph "goes at the slow pace",
    // because it never scrubs, so the loop cannot start there.
    [AvaloniaFact]
    public void WholeParagraphMove_DoesNotScrubTheVideo()
    {
        WithScrubOnMove(() =>
        {
            var (window, av) = Open();
            var firedDuringMove = false;
            av.OnVideoPositionChanged += (_, _) => firedDuringMove = true;

            // Press first: the single-click action sets the video position on its own, which is
            // not the scrub under test.
            window.MouseDown(new Point(MiddleX, 100), MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            firedDuringMove = false;

            foreach (var x in new[] { MiddleX + 10, MiddleX + 11, MiddleX + 12 })
            {
                window.MouseMove(new Point(x, 100), RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
            }

            Assert.False(firedDuringMove, "a whole-paragraph move should not scrub the video");

            window.MouseUp(new Point(MiddleX + 12, 100), MouseButton.Left, RawInputModifiers.None);
            window.Close();
        });
    }
}
