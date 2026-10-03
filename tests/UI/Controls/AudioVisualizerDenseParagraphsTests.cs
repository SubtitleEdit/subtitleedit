using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// Issue #15587: anime ASSA files have bursts of tens of thousands of frame-by-frame typesetting
/// lines. The waveform loads the view plus 15 s on each side and stopped after 250 lines, so a burst
/// in the left margin used up the whole budget and the dialogue on screen was never drawn.
/// </summary>
public class AudioVisualizerDenseParagraphsTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double HeightPx = 200;

    [AvaloniaFact]
    public void LinesAfterADenseBurstInTheMargin_AreLoaded()
    {
        var av = new AudioVisualizer { WavePeaks = MakePeaks(500) };
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
            // Same shape as the reported file: ~66,000 lines of 40 ms between 6:23 and 6:31.5,
            // then normal dialogue from 6:33.12.
            var lines = new List<SubtitleLineViewModel>();
            const int burstCount = 66_000;
            for (var i = 0; i < burstCount; i++)
            {
                var start = 383.0 + i * 8.5 / burstCount;
                lines.Add(MakeLine(start, start + 0.04, string.Empty));
            }

            var dialogue = MakeLine(393.12, 396.46, "is two tickets fer that luxury cruise");
            var nextDialogue = MakeLine(396.6, 398.0, "everyone's buzzin' about lately!");
            lines.Add(dialogue);
            lines.Add(nextDialogue);

            av.SetPosition(393, lines, 393, -1, new List<SubtitleLineViewModel>());

            var loaded = new List<SubtitleLineViewModel>();
            av.CopyDisplayableParagraphs(loaded);

            Assert.Contains(dialogue, loaded);
            Assert.Contains(nextDialogue, loaded);

            // The burst is still thinned: 200 as-is, then about one per 90 ms of its 8.5 s.
            Assert.InRange(loaded.Count, 200, 400);
        }
        finally
        {
            window.Close();
        }
    }

    private static SubtitleLineViewModel MakeLine(double startSeconds, double endSeconds, string text)
    {
        return new SubtitleLineViewModel
        {
            Text = text,
            StartTime = TimeSpan.FromSeconds(startSeconds),
            EndTime = TimeSpan.FromSeconds(endSeconds),
        };
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
}
