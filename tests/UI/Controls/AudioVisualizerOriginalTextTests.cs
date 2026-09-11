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
/// "Toggle translation and original in video/audio preview" (#14252) also swaps the text drawn in
/// the waveform - the video preview alone showing the original, with the waveform still on the
/// translation, is what SE 4 never did.
/// </summary>
public class AudioVisualizerOriginalTextTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double HeightPx = 200;
    private const double LineStartSeconds = 100;

    [AvaloniaFact]
    public void ShowOriginalText_DrawsTheOriginalInsteadOfTheTranslation()
    {
        var av = new AudioVisualizer { WavePeaks = MakePeaks(300) };
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
            var line = new SubtitleLineViewModel
            {
                // Deliberately different lengths: the footer's CPS follows the drawn text, so the
                // frames below can only match if it swapped too.
                Text = "Vertaalde regel",
                OriginalText = "Source line",
                StartTime = TimeSpan.FromSeconds(LineStartSeconds),
                EndTime = TimeSpan.FromSeconds(LineStartSeconds + 3),
            };

            var lines = new List<SubtitleLineViewModel> { line };
            var noSelection = new List<SubtitleLineViewModel>();

            av.SetPosition(LineStartSeconds - 1, lines, 0, -1, noSelection);
            var translation = Capture(window);

            av.ShowOriginalText = true;
            av.InvalidateVisual();
            var original = Capture(window);

            Assert.NotEqual(translation, original);

            // ... and it is the original text that was drawn, not merely something else: the same
            // waveform with the original text in the ordinary Text property paints the same frame -
            // text, and the CPS in the paragraph footer with it.
            av.ShowOriginalText = false;
            line.Text = "Source line";
            av.InvalidateVisual();
            Assert.Equal(original, Capture(window));
        }
        finally
        {
            // A window left open keeps the app-wide activation and focused element for the rest
            // of the run, so a later test's key press or click lands here instead.
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
