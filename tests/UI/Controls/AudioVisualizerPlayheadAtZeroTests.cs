using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The waveform cursor was hidden at exactly 0 s, so it vanished on Stop and on a freshly opened
/// video, and only came back once playback moved off the start. With peaks loaded 0 is a real
/// position and the cursor shows on the left edge; without peaks (a closed video resets the
/// position to 0) there is still no timeline to draw it on.
/// </summary>
public class AudioVisualizerPlayheadAtZeroTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const int WidthPx = 400;
    private const int HeightPx = 100;
    private const int EdgeColumns = 2;

    [AvaloniaFact]
    public void CursorAtZero_IsDrawnOnTheLeftEdge()
    {
        var atZero = RenderLeftEdge(withPeaks: true, positionSeconds: 0);
        var elsewhere = RenderLeftEdge(withPeaks: true, positionSeconds: 2); // well right of the edge

        Assert.NotEqual(elsewhere, atZero);
    }

    [AvaloniaFact]
    public void CursorWithoutPeaks_IsNotDrawn()
    {
        var atZero = RenderLeftEdge(withPeaks: false, positionSeconds: 0);
        var elsewhere = RenderLeftEdge(withPeaks: false, positionSeconds: 2);

        Assert.Equal(elsewhere, atZero);
    }

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(200, -200);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    // Renders the control and returns the leftmost pixel columns of every row. Only the cursor
    // depends on the position, so these columns differ between two positions exactly when one
    // of them draws the cursor on the edge.
    private static byte[] RenderLeftEdge(bool withPeaks, double positionSeconds)
    {
        var av = new AudioVisualizer
        {
            WavePeaks = withPeaks ? MakePeaks(10) : null,
            CurrentVideoPositionSeconds = positionSeconds,
        };
        av.Measure(new Size(WidthPx, HeightPx));
        av.Arrange(new Rect(0, 0, WidthPx, HeightPx));

        using var bitmap = new RenderTargetBitmap(new PixelSize(WidthPx, HeightPx), new Vector(96, 96));
        using (var context = bitmap.CreateDrawingContext())
        {
            av.Render(context);
        }

        var pixels = new byte[WidthPx * HeightPx * 4];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(0, 0, WidthPx, HeightPx), handle.AddrOfPinnedObject(), pixels.Length, WidthPx * 4);
        }
        finally
        {
            handle.Free();
        }

        var edge = new byte[HeightPx * EdgeColumns * 4];
        for (var y = 0; y < HeightPx; y++)
        {
            Array.Copy(pixels, y * WidthPx * 4, edge, y * EdgeColumns * 4, EdgeColumns * 4);
        }

        return edge;
    }
}
