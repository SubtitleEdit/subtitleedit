using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Diagnostics;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The waveform extends the cursor tick's motion by the time elapsed since the tick when it
/// renders (#14909 follow-up): a 16 ms tick against a 16.7 ms frame otherwise draws a frame
/// with two ticks of motion every ~25 frames. The extension must stop where the estimator
/// stopped, must not outrun a stalled tick, and must never draw the cursor behind where it was.
/// </summary>
public class AudioVisualizerRenderTimePlayheadTests
{
    private const int SampleRate = 100;

    private static AudioVisualizer MakeVisualizer(double position, double start)
    {
        var peaks = new WavePeak2[SampleRate * 60];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(200, -200);
        }

        return new AudioVisualizer
        {
            Width = 400,
            Height = 100,
            WavePeaks = new WavePeakData2(SampleRate, peaks),
            ZoomFactor = 1.0,
            CurrentVideoPositionSeconds = position,
            StartPositionSeconds = start,
        };
    }

    private static long TimestampSecondsAgo(double seconds, long now)
    {
        return now - (long)(seconds * Stopwatch.Frequency);
    }

    [AvaloniaFact]
    public void ExtendsTheTicksMotionByTheElapsedTime()
    {
        var now = Stopwatch.GetTimestamp();
        var av = MakeVisualizer(position: 10, start: 8);
        av.SetPlayheadMotion(TimestampSecondsAgo(0.010, now), velocity: 1.0, scrollsView: true);

        var (position, start) = av.GetRenderTimePlayhead(now);

        Assert.Equal(10.010, position, 4);
        Assert.Equal(8.010, start, 4);
    }

    [AvaloniaFact]
    public void DoesNotMoveTheViewWhenTheTickDidNotCenterIt()
    {
        var now = Stopwatch.GetTimestamp();
        var av = MakeVisualizer(position: 10, start: 8);
        av.SetPlayheadMotion(TimestampSecondsAgo(0.010, now), velocity: 1.0, scrollsView: false);

        var (position, start) = av.GetRenderTimePlayhead(now);

        Assert.Equal(10.010, position, 4);
        Assert.Equal(8, start);
    }

    [AvaloniaFact]
    public void HoldsWhenTheEstimatorDidNotAdvance()
    {
        var now = Stopwatch.GetTimestamp();
        var av = MakeVisualizer(position: 10, start: 8);
        av.SetPlayheadMotion(TimestampSecondsAgo(0.010, now), velocity: 0, scrollsView: true);

        var (position, start) = av.GetRenderTimePlayhead(now);

        Assert.Equal(10, position);
        Assert.Equal(8, start);
    }

    [AvaloniaFact]
    public void AStalledTickIsExtendedByAtMostTwoTicks()
    {
        var now = Stopwatch.GetTimestamp();
        var av = MakeVisualizer(position: 10, start: 8);
        av.SetPlayheadMotion(TimestampSecondsAgo(0.300, now), velocity: 1.0, scrollsView: true);

        var (position, _) = av.GetRenderTimePlayhead(now);

        Assert.InRange(position, 10.030, 10.035);
    }

    [AvaloniaFact]
    public void APositionWrittenByAnotherPathIsDrawnAsIs()
    {
        var now = Stopwatch.GetTimestamp();
        var av = MakeVisualizer(position: 10, start: 8);
        av.SetPlayheadMotion(TimestampSecondsAgo(0.010, now), velocity: 1.0, scrollsView: true);
        av.CurrentVideoPositionSeconds = 4; // a wheel scrub or click since the tick

        var (position, start) = av.GetRenderTimePlayhead(now);

        Assert.Equal(4, position);
        Assert.Equal(8, start);
    }

    [AvaloniaFact]
    public void NeverDrawsTheCursorBehindThePreviousFrame()
    {
        var now = Stopwatch.GetTimestamp();
        var av = MakeVisualizer(position: 10, start: 8);

        // Frame 1: a tick 16 ms ago at 1.2x (drift correction) - drawn ahead of the tick.
        av.SetPlayheadMotion(TimestampSecondsAgo(0.016, now), velocity: 1.2, scrollsView: true);
        var (first, _) = av.GetRenderTimePlayhead(now);
        Assert.Equal(10.0192, first, 4);

        // Frame 2: the next tick landed just behind that, at 1.0x and only 1 ms ago.
        av.CurrentVideoPositionSeconds = 10.016;
        av.StartPositionSeconds = 8.016;
        av.SetPlayheadMotion(TimestampSecondsAgo(0.001, now), velocity: 1.0, scrollsView: true);
        var (second, start) = av.GetRenderTimePlayhead(now);

        Assert.Equal(first, second, 4);
        Assert.Equal(8.016 + (first - 10.016), start, 4);
    }

    [AvaloniaFact]
    public void ABackwardSeekResetsTheHold()
    {
        var now = Stopwatch.GetTimestamp();
        var av = MakeVisualizer(position: 10, start: 8);
        av.SetPlayheadMotion(TimestampSecondsAgo(0.016, now), velocity: 1.0, scrollsView: true);
        av.GetRenderTimePlayhead(now);

        av.CurrentVideoPositionSeconds = 2;
        av.StartPositionSeconds = 0;
        av.SetPlayheadMotion(TimestampSecondsAgo(0.005, now), velocity: 1.0, scrollsView: true);
        var (position, start) = av.GetRenderTimePlayhead(now);

        Assert.Equal(2.005, position, 4);
        Assert.Equal(0, start); // the view is at the start, so the cursor moves instead
    }
}
