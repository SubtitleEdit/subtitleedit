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
/// Issue #14306: with "mouse-wheel sets video position" turned off, the wheel still jumped the
/// video to the cursor after any Ctrl shortcut was used over the waveform.
///
/// The wheel handler read a Ctrl/Alt/Shift/Meta mirror maintained from KeyDown/KeyUp. A shortcut
/// handler that swallows the key-up leaves the mirror stuck down, and the wheel then took the
/// Ctrl branch ("scroll and set the video position at the cursor") with no Ctrl held. It now
/// reads the modifiers off the wheel event itself, like every other pointer handler in the
/// control, so a stale mirror cannot reach it.
/// </summary>
public class AudioVisualizerWheelModifierTests : IDisposable
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

    private (Window Window, AudioVisualizer Av) Open()
    {
        var av = new AudioVisualizer { WavePeaks = MakePeaks(200), Width = WidthPx, Height = 200 };
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
        av.Focus();
        Dispatcher.UIThread.RunJobs();
        return (window, av);
    }

    /// <summary>Runs an action with "mouse-wheel sets video position" forced to <paramref name="on"/>.</summary>
    private static void WithWheelSetsVideoPosition(bool on, Action body)
    {
        var old = Se.Settings.Waveform.MouseWheelSetsVideoPosition;
        Se.Settings.Waveform.MouseWheelSetsVideoPosition = on;
        try
        {
            body();
        }
        finally
        {
            Se.Settings.Waveform.MouseWheelSetsVideoPosition = old;
        }
    }

    // Ctrl goes down over the waveform and the matching key-up never arrives, which is what a
    // shortcut handler that marks the key event handled looks like from here.
    private static void PressCtrlWithoutRelease(Window window)
    {
        window.KeyPress(Key.LeftCtrl, RawInputModifiers.Control, PhysicalKey.ControlLeft, null);
        Dispatcher.UIThread.RunJobs();
    }

    // The reported bug, with the setting off: after a Ctrl shortcut the wheel repositioned the
    // video even though no modifier was held while scrolling.
    [AvaloniaFact]
    public void Wheel_AfterCtrlShortcut_DoesNotMoveTheVideo()
    {
        WithWheelSetsVideoPosition(false, () =>
        {
            var (window, av) = Open();
            var moved = 0;
            av.OnVideoPositionChanged += (_, _) => moved++;

            PressCtrlWithoutRelease(window);
            window.MouseWheel(WheelPoint, new Vector(0, 1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, moved);
        });
    }

    // The same stale Ctrl with the setting on: it suppressed the step-seek the setting exists for
    // and did the Ctrl "set position at cursor" instead.
    [AvaloniaFact]
    public void Wheel_AfterCtrlShortcut_StillStepsTheVideo_WhenSettingIsOn()
    {
        WithWheelSetsVideoPosition(true, () =>
        {
            var (window, av) = Open();
            av.CurrentVideoPositionSeconds = 100;
            var positions = new List<double>();
            av.OnVideoPositionChanged += (_, e) => positions.Add(e.PositionInSeconds);

            PressCtrlWithoutRelease(window);
            window.MouseWheel(WheelPoint, new Vector(0, 1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Single(positions);
            // A step off the current position, not a jump to wherever the pointer happens to be.
            Assert.True(Math.Abs(positions[0] - 100) < 1,
                $"expected a step near 100s, got {positions[0]}s");
        });
    }

    // Ctrl genuinely held while scrolling keeps setting the video position at the cursor.
    [AvaloniaFact]
    public void Wheel_WithCtrlActuallyHeld_MovesTheVideo()
    {
        WithWheelSetsVideoPosition(false, () =>
        {
            var (window, av) = Open();
            var moved = 0;
            av.OnVideoPositionChanged += (_, _) => moved++;

            window.MouseWheel(WheelPoint, new Vector(0, 1), RawInputModifiers.Control);
            Dispatcher.UIThread.RunJobs();

            Assert.True(moved > 0, "Ctrl+wheel should still set the video position");
        });
    }

    // Without any modifier the wheel only scrolls the view - the baseline the reporter had before
    // touching a shortcut.
    [AvaloniaFact]
    public void Wheel_WithNoModifier_OnlyScrolls()
    {
        WithWheelSetsVideoPosition(false, () =>
        {
            var (window, av) = Open();
            var moved = 0;
            var scrolled = 0;
            av.OnVideoPositionChanged += (_, _) => moved++;
            av.OnHorizontalScroll += (_, _) => scrolled++;

            window.MouseWheel(WheelPoint, new Vector(0, 1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, moved);
            Assert.True(scrolled > 0, "the wheel should scroll the waveform");
        });
    }
}
