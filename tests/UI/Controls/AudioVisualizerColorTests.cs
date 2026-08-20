using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// Waveform color changes must take effect without restarting SE (#13897).
///
/// The fancy draw style batches columns by a quantized amplitude bucket and caches a pen per
/// bucket - both in the pen/gradient caches and in the pooled batch dictionary. None of those keys
/// carry the color, so every one of them has to be dropped when a waveform color changes,
/// otherwise the rebuilt geometry is stroked with pens still painted in the old color and only a
/// new control (i.e. a restart) shows the new one.
/// </summary>
public class AudioVisualizerColorTests
{
    private const int SampleRate = 126; // Se.Settings.Waveform.WaveformMinimumSampleRate default
    private const double WidthPx = 800;
    private const double HeightPx = 200;

    private static readonly Color OldColor = Color.FromRgb(0, 70, 0);
    private static readonly Color NewColor = Color.FromRgb(255, 0, 255);

    /// <summary>Mostly quiet peaks (drawn in the base waveform color) with an occasional loud one
    /// so the highest peak - and with it the low/medium/high color thresholds - is well above the
    /// quiet columns.</summary>
    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            var v = i % 500 == 0 ? (short)8000 : (short)500;
            peaks[i] = new WavePeak2(v, (short)-v);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    private static AudioVisualizer MakeMeasuredFancyVisualizer()
    {
        var av = new AudioVisualizer
        {
            WaveformDrawStyle = WaveformDrawStyle.Fancy,
            WavePeaks = MakePeaks(60),
            WaveformColor = OldColor,
            WaveformSelectedColor = Color.FromArgb(150, 0, 120, 255),
            WaveformFancyHighColor = Colors.Orange,
        };
        av.Measure(new Size(WidthPx, HeightPx));
        av.Arrange(new Rect(0, 0, WidthPx, HeightPx));
        av.SetPosition(0, new List<SubtitleLineViewModel>(), 0, 0, new List<SubtitleLineViewModel>());
        return av;
    }

    private static void RenderFrame(AudioVisualizer av)
    {
        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        av.Render(context);
    }

    /// <summary>Every color the cached waveform draw ops would actually paint with.</summary>
    private static List<Color> CachedPenColors(AudioVisualizer av)
    {
        var field = typeof(AudioVisualizer).GetField("_waveformCacheDraws", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var list = (System.Collections.IList)field.GetValue(av)!;
        var colors = new List<Color>();
        foreach (var draw in list)
        {
            var (pen, _) = (ValueTuple<IPen, Geometry>)draw!;
            switch (pen.Brush)
            {
                case IGradientBrush gradient:
                    foreach (var stop in gradient.GradientStops)
                    {
                        colors.Add(stop.Color);
                    }

                    break;
                case ISolidColorBrush solid:
                    colors.Add(solid.Color);
                    break;
            }
        }

        return colors;
    }

    [AvaloniaFact]
    public void FancyWaveform_UsesNewWaveformColor_AfterSettingsApply_Issue13897()
    {
        var av = MakeMeasuredFancyVisualizer();
        RenderFrame(av);
        Assert.Contains(OldColor, CachedPenColors(av));

        // What Settings -> OK does: push the new colors, then reset the caches.
        av.WaveformColor = NewColor;
        av.ResetCache();
        RenderFrame(av);

        var colors = CachedPenColors(av);
        Assert.DoesNotContain(OldColor, colors);
        Assert.Contains(NewColor, colors);
    }

    [AvaloniaFact]
    public void FancyWaveform_UsesNewWaveformColor_WithoutResetCache_Issue13897()
    {
        // The setter alone must be enough - no caller should have to remember ResetCache().
        var av = MakeMeasuredFancyVisualizer();
        RenderFrame(av);
        Assert.Contains(OldColor, CachedPenColors(av));

        av.WaveformColor = NewColor;
        RenderFrame(av);

        var colors = CachedPenColors(av);
        Assert.DoesNotContain(OldColor, colors);
        Assert.Contains(NewColor, colors);
    }

    [AvaloniaFact]
    public void FancyWaveform_UsesNewSelectedColor_Issue13897()
    {
        var av = MakeMeasuredFancyVisualizer();
        var selected = new SubtitleLineViewModel
        {
            Text = "text",
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromSeconds(5),
        };
        var lines = new List<SubtitleLineViewModel> { selected };
        av.SetPosition(0, lines, 0, 0, lines);

        var oldSelectedColor = Color.FromRgb(0, 120, 255);
        av.WaveformSelectedColor = oldSelectedColor;
        RenderFrame(av);
        Assert.Contains(oldSelectedColor, CachedPenColors(av));

        av.WaveformSelectedColor = NewColor;
        RenderFrame(av);

        var colors = CachedPenColors(av);
        Assert.DoesNotContain(oldSelectedColor, colors);
        Assert.Contains(NewColor, colors);
    }

    [AvaloniaFact]
    public void FancyWaveform_UsesNewFancyHighColor_Issue13897()
    {
        var av = MakeMeasuredFancyVisualizer();
        RenderFrame(av);
        Assert.Contains(Colors.Orange, CachedPenColors(av));

        av.WaveformFancyHighColor = NewColor;
        RenderFrame(av);

        var colors = CachedPenColors(av);
        Assert.DoesNotContain(Colors.Orange, colors);
        Assert.Contains(NewColor, colors);
    }
}
