using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The original-subtitle overlay culls cues with a binary search on end times. Original cues are in
/// file order (by start), and a long cue overlapped by a shorter one ends after its successor, so a
/// search on the raw end times skipped the long cue whenever the viewport began inside it.
/// </summary>
public class AudioVisualizerOriginalOverlayCullTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double HeightPx = 200;

    [AvaloniaFact]
    public void LongCueOverlappedByShorterOne_IsDrawnWhenTheViewStartsInsideIt()
    {
        var av = new AudioVisualizer { WavePeaks = MakePeaks(60), ShowOriginalSubtitleOverlay = true };
        var window = new Window
        {
            Width = WidthPx,
            Height = HeightPx,
            Content = av,
        };

        window.Show();
        window.UpdateLayout();

        try
        {
            var noLines = new List<SubtitleLineViewModel>();
            var noSelection = new List<SubtitleLineViewModel>();

            // Viewport starts at 6.5 s: A covers all of it, B and C are both out of view.
            var longCue = new WaveformOriginalSubtitleCue(0, 30, "♪ music ♪");
            var shortCueInsideLong = new WaveformOriginalSubtitleCue(5, 6, "Hi.");
            var cueAfterView = new WaveformOriginalSubtitleCue(31, 32, "Bye.");

            av.SetPosition(6.5, noLines, 0, -1, noSelection);

            av.SetOriginalSubtitleCues(new[] { longCue });
            var onlyLongCue = Capture(window);

            av.SetOriginalSubtitleCues(new[] { longCue, shortCueInsideLong, cueAfterView });
            var withOverlap = Capture(window);

            Assert.Equal(onlyLongCue, withOverlap);

            // Control: the frame really depends on the long cue being drawn.
            av.SetOriginalSubtitleCues(new[] { shortCueInsideLong, cueAfterView });
            Assert.NotEqual(onlyLongCue, Capture(window));
        }
        finally
        {
            window.Close();
        }
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

    private static byte[] Capture(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();

        using var frame = window.CaptureRenderedFrame()!;
        using var stream = new MemoryStream();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        return stream.ToArray();
    }
}
