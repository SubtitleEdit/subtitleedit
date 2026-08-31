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
/// Issue #14318: with the waveform's "Center" toggle on and the video paused, scrolling the
/// waveform slid the play-head across the view instead of keeping it pinned to the middle.
///
/// #12864 made a scroll in center mode seek the video to the new view center, but only while
/// playing - paused scrolling was deliberately left free. "Center video position also while
/// paused" is the setting that says centering applies when paused too, and the wheel handler
/// was the one place ignoring it: with the play-head pinned to the middle, scrolling the view
/// has to carry the video with it or the pin breaks as soon as the wheel turns.
/// </summary>
public class AudioVisualizerCenterScrollTests : IDisposable
{
    // A window left open outlives the test: it keeps the application-wide activation and focused
    // element, so a later test's input is delivered to it instead.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double ViewStart = 98;
    private static readonly Point WheelPoint = new(400, 100);

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    private (AudioVisualizer Av, Window Window) Open(bool isPlaying)
    {
        var av = new AudioVisualizer
        {
            WavePeaks = MakePeaks(400),
            Width = WidthPx,
            Height = 200,
            GetIsVideoPlaying = () => isPlaying,
        };
        var line = new SubtitleLineViewModel
        {
            Text = "text",
            StartTime = TimeSpan.FromSeconds(100),
            EndTime = TimeSpan.FromSeconds(102),
        };

        av.SetPosition(ViewStart, new List<SubtitleLineViewModel> { line }, 0, 0, new List<SubtitleLineViewModel>());

        var window = new Window { Width = WidthPx, Height = 200, Content = av };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (av, window);
    }

    private static void WithWaveformSettings(bool center, bool alsoWhenPaused, Action body)
    {
        var waveform = Se.Settings.Waveform;
        var oldCenter = waveform.CenterVideoPosition;
        var oldPaused = waveform.CenterVideoPositionAlsoWhenPaused;
        var oldWheelSets = waveform.MouseWheelSetsVideoPosition;

        waveform.CenterVideoPosition = center;
        waveform.CenterVideoPositionAlsoWhenPaused = alsoWhenPaused;
        // This test is about the plain scroll path, not the step-the-video-per-notch mode.
        waveform.MouseWheelSetsVideoPosition = false;
        try
        {
            body();
        }
        finally
        {
            waveform.CenterVideoPosition = oldCenter;
            waveform.CenterVideoPositionAlsoWhenPaused = oldPaused;
            waveform.MouseWheelSetsVideoPosition = oldWheelSets;
        }
    }

    // The report: centered + paused + "also while paused" on, the scroll should carry the video
    // so the play-head stays in the middle of the view.
    [AvaloniaFact]
    public void PausedScroll_SeeksToTheViewCentre_WhenAlsoWhenPausedIsOn()
    {
        WithWaveformSettings(center: true, alsoWhenPaused: true, () =>
        {
            var (av, window) = Open(isPlaying: false);
            double? seeked = null;
            av.OnVideoPositionChanged += (_, e) => seeked = e.PositionInSeconds;

            window.MouseWheel(WheelPoint, new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.NotNull(seeked);

            // The play-head lands in the middle of wherever the view ended up - that is what
            // "stays centered" means.
            var expected = av.StartPositionSeconds + (av.EndPositionSeconds - av.StartPositionSeconds) / 2;
            Assert.Equal(expected, seeked!.Value, 2);
        });
    }

    // #12864 deliberately left paused scrolling free for anyone who has not opted into
    // centering-while-paused. That has to stay true.
    [AvaloniaFact]
    public void PausedScroll_DoesNotSeek_WhenAlsoWhenPausedIsOff()
    {
        WithWaveformSettings(center: true, alsoWhenPaused: false, () =>
        {
            var (av, window) = Open(isPlaying: false);
            var seeks = 0;
            var scrolls = 0;
            av.OnVideoPositionChanged += (_, _) => seeks++;
            av.OnHorizontalScroll += (_, _) => scrolls++;

            window.MouseWheel(WheelPoint, new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, seeks);
            Assert.True(scrolls > 0, "the view should still scroll");
        });
    }

    // The #12864 behaviour itself: while playing, a scroll seeks regardless of the paused setting.
    [AvaloniaFact]
    public void PlayingScroll_StillSeeks_WithAlsoWhenPausedOff()
    {
        WithWaveformSettings(center: true, alsoWhenPaused: false, () =>
        {
            var (av, window) = Open(isPlaying: true);
            var seeks = 0;
            av.OnVideoPositionChanged += (_, _) => seeks++;

            window.MouseWheel(WheelPoint, new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(seeks > 0, "center mode while playing should still seek on scroll");
        });
    }

    // With the Center toggle off nothing seeks, paused setting or not.
    [AvaloniaFact]
    public void ScrollWithCentreOff_NeverSeeks()
    {
        WithWaveformSettings(center: false, alsoWhenPaused: true, () =>
        {
            var (av, window) = Open(isPlaying: false);
            var seeks = 0;
            av.OnVideoPositionChanged += (_, _) => seeks++;

            window.MouseWheel(WheelPoint, new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, seeks);
        });
    }
}
