using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The experimental SkiaSharp renderer draws from a custom draw operation on the render thread
/// instead of recording Avalonia geometry, so a mistake there shows up as an empty control rather
/// than as an exception. These check that a frame actually reaches the canvas, and that the things
/// the ~60 fps timers move (the cursor, the view) still change the picture.
/// </summary>
public class SkiaAudioVisualizerRenderTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double HeightPx = 200;

    [AvaloniaFact]
    public void DrawsTheWaveformAndFollowsTheCursorAndTheView()
    {
        var av = new SkiaAudioVisualizer
        {
            WavePeaks = MakePeaks(60),
            WaveformColor = Avalonia.Media.Color.FromRgb(0, 255, 0),
            WaveformBackgroundColor = Avalonia.Media.Color.FromRgb(0, 0, 0),
            WaveformCursorColor = Avalonia.Media.Color.FromRgb(0, 255, 255),
        };

        var window = new Window { Width = WidthPx, Height = HeightPx, Content = av };
        window.Show();
        window.UpdateLayout();

        try
        {
            var lines = new List<SubtitleLineViewModel>
            {
                new()
                {
                    Number = 1,
                    Text = "Hello there",
                    StartTime = TimeSpan.FromSeconds(2),
                    EndTime = TimeSpan.FromSeconds(4),
                },
            };

            av.SetPosition(0, lines, 1, -1, new List<SubtitleLineViewModel>());
            var frame = Capture(window);

            // Something other than the background was drawn.
            var blank = new SkiaAudioVisualizer { WaveformBackgroundColor = Avalonia.Media.Color.FromRgb(0, 0, 0) };
            var blankWindow = new Window { Width = WidthPx, Height = HeightPx, Content = blank };
            blankWindow.Show();
            blankWindow.UpdateLayout();
            try
            {
                Assert.NotEqual(Capture(blankWindow), frame);
            }
            finally
            {
                blankWindow.Close();
            }

            // The cursor timer only moves this property - it has to repaint.
            av.CurrentVideoPositionSeconds = 3;
            var moved = Capture(window);
            Assert.NotEqual(frame, moved);

            // ...and so does scrolling the view.
            av.StartPositionSeconds = 10;
            Assert.NotEqual(moved, Capture(window));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// Every frame is rented from a pool and returned when the compositor disposes the operation;
    /// a frame that is never returned (or returned twice) would leak or be drawn while reused.
    /// </summary>
    [AvaloniaFact]
    public void DisposingDrawOperationsRecyclesTheirFrames()
    {
        var av = new SkiaAudioVisualizer { WavePeaks = MakePeaks(10) };
        av.Measure(new Avalonia.Size(WidthPx, HeightPx));
        av.Arrange(new Avalonia.Rect(0, 0, WidthPx, HeightPx));

        var first = av.CreateDrawOperation();
        Assert.NotNull(first);
        first!.Dispose();

        for (var i = 0; i < 10; i++)
        {
            var operation = av.CreateDrawOperation();
            Assert.NotNull(operation);
            operation!.Dispose();
        }

        // A control with no size has nothing to snapshot.
        var unmeasured = new SkiaAudioVisualizer();
        Assert.Null(unmeasured.CreateDrawOperation());
    }

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            var v = (short)(i % 20 == 0 ? 9000 : 400);
            peaks[i] = new WavePeak2(v, (short)-v);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    private static byte[] Capture(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();

        using var frame = window.CaptureRenderedFrame()!;
        using var stream = new MemoryStream();
        frame.Save(stream);
        return stream.ToArray();
    }
}
