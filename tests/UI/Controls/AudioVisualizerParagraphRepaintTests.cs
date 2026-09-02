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
/// The blank waveform in issue #14218: after Options/OK the waveform kept the time ruler and the
/// cursor but drew no paragraphs at all - no boxes, no text, no number/duration/CPS - until
/// playback was resumed, which brought them all back.
///
/// The paragraph set the waveform draws is rebuilt on the 50 ms position timer and lives in plain
/// fields, so a rebuild of it asks for no repaint. While the video is paused nothing else moves an
/// AffectsRender property either, so whatever was on screen when the set was last painted just
/// stays there. Options/OK produced exactly that: the rebuilt video player reports 0 for a moment,
/// "center video position" scrolls the waveform to the start (16 ms cursor timer), the paragraph
/// reload empties the list, and the frame painted when the player lands back on the real position
/// shows the right time range with nothing in it. The reload a tick later fixed the list but not
/// the picture.
/// </summary>
public class AudioVisualizerParagraphRepaintTests : IDisposable
{
    // A window left open outlives the test: it keeps the application-wide activation and focused
    // element, so a later test's click or key press is delivered to it instead. Closing here rather
    // than at the end of each test also covers the tests that stop early on a failed assertion.
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
    private const double HeightPx = 200;
    private const double LineStartSeconds = 100;

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(200, -200);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    private static byte[] Capture(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();

        using var frame = window.CaptureRenderedFrame()!;
        using var stream = new MemoryStream();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        return stream.ToArray();
    }

    [AvaloniaFact]
    public void ParagraphReloadRepaintsWhenTheDrawnSetChanged_Issue14218()
    {
        var av = new AudioVisualizer { WavePeaks = MakePeaks(300) };
        var window = new Window
        {
            Width = WidthPx,
            Height = HeightPx,
            Content = av,
        };

        _windows.Add(window);
        window.Show();
        window.UpdateLayout();

        var lines = new List<SubtitleLineViewModel>
        {
            new()
            {
                Text = "- Where is Arturo?" + Environment.NewLine + "- He was here.",
                StartTime = TimeSpan.FromSeconds(LineStartSeconds),
                EndTime = TimeSpan.FromSeconds(LineStartSeconds + 3),
            },
        };

        var noSelection = new List<SubtitleLineViewModel>();

        // The waveform is scrolled to the start of the file - what "center video position" does
        // while the rebuilt player still reports 0 - so the line is far outside the drawn window.
        av.SetPosition(0, lines, 0, -1, noSelection);
        Capture(window);

        // The player lands back where the user was. The cursor timer moves the view, which is an
        // AffectsRender property, so this frame is painted - with the paragraph set still empty.
        av.StartPositionSeconds = LineStartSeconds - 1;
        var withoutParagraphs = Capture(window);

        // The next reload brings the line back into the set. Nothing else changes here (same view,
        // same cursor - the video is paused), so unless the reload asks for the repaint itself the
        // waveform goes on showing the frame above.
        av.SetPosition(LineStartSeconds - 1, lines, 0, -1, noSelection);
        var withParagraphs = Capture(window);

        Assert.NotEqual(withoutParagraphs, withParagraphs);
    }

    [AvaloniaFact]
    public void ParagraphReloadRepaintsWhenTheSelectionChanged_Issue14218()
    {
        // Same hole on the selection half of the reload: AllSelectedParagraphs is an AffectsRender
        // property, but only its identity is - the reload mutates the list in place, so a selection
        // that arrives while the video is paused repaints nothing on its own.
        var av = new AudioVisualizer { WavePeaks = MakePeaks(300) };
        var window = new Window
        {
            Width = WidthPx,
            Height = HeightPx,
            Content = av,
        };

        _windows.Add(window);
        window.Show();
        window.UpdateLayout();

        var lines = new List<SubtitleLineViewModel>
        {
            new()
            {
                Text = "Where are you, Arturo?",
                StartTime = TimeSpan.FromSeconds(LineStartSeconds),
                EndTime = TimeSpan.FromSeconds(LineStartSeconds + 3),
            },
        };

        av.SetPosition(LineStartSeconds - 1, lines, 0, -1, new List<SubtitleLineViewModel>());
        var unselected = Capture(window);

        av.SetPosition(LineStartSeconds - 1, lines, 0, -1, new List<SubtitleLineViewModel>(lines));
        var selected = Capture(window);

        Assert.NotEqual(unselected, selected);
    }
}
