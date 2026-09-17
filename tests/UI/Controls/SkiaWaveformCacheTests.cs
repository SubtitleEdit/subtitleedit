using Avalonia;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The Skia renderer caches the scenery (background, grid, waveform) in an offscreen layer anchored
/// to a 256 device pixel block and blits it translated while the view scrolls inside that block -
/// which is what center-mode playback does on every frame. A cache that drifts by a pixel, or that
/// is not invalidated when the view changes, would show up as a waveform that no longer lines up
/// with the paragraphs and the cursor drawn over it, so these compare the cached output against the
/// same frames drawn without a cache.
/// </summary>
public class SkiaWaveformCacheTests
{
    private const int Width = 800;
    private const int Height = 200;
    private const int SampleRate = 126;

    [AvaloniaTheory]
    [InlineData(WaveformDrawStyle.Classic, 1f)]
    [InlineData(WaveformDrawStyle.Classic, 2f)]
    [InlineData(WaveformDrawStyle.Fancy, 1f)]
    [InlineData(WaveformDrawStyle.Fancy, 2f)]
    public void CachedSceneryMatchesUncachedDrawing(WaveformDrawStyle style, float scale)
    {
        // Sub-pixel steps inside one anchor block, then across block boundaries, then a far jump:
        // the cache has to translate, rebuild at the boundary, and rebuild after the jump.
        var starts = new List<double>();
        for (var i = 0; i < 12; i++)
        {
            starts.Add(10 + i * 0.017);
        }

        starts.Add(12.7);
        starts.Add(13.0);
        starts.Add(600.0);

        foreach (var start in starts)
        {
            var uncached = Render(style, SkiaWaveformCacheMode.None, scale, new[] { start });
            var cached = Render(style, SkiaWaveformCacheMode.Image, scale, new[] { start });
            AssertSamePicture(uncached, cached, style, $"start {start}");
        }

        // Scrolling through every position must end on the same picture as jumping straight there:
        // a cache that drifts while translating would only show up this way.
        AssertSamePicture(
            Render(style, SkiaWaveformCacheMode.None, scale, new[] { starts[^1] }),
            Render(style, SkiaWaveformCacheMode.Image, scale, starts.ToArray()),
            style,
            "scrolled through");
    }

    /// <summary>
    /// Classic style has to be pixel exact. Fancy style draws a semi-transparent gradient, and going
    /// through the offscreen layer rounds its alpha one extra time, so a handful of channels land
    /// one or two levels off - invisible, but not bit identical.
    /// </summary>
    private static void AssertSamePicture(byte[] expected, byte[] actual, WaveformDrawStyle style, string what)
    {
        Assert.Equal(expected.Length, actual.Length);
        var differing = 0;
        var maxDelta = 0;
        for (var i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                differing++;
                maxDelta = Math.Max(maxDelta, Math.Abs(expected[i] - actual[i]));
            }
        }

        if (style == WaveformDrawStyle.Classic)
        {
            Assert.True(differing == 0, $"{what}: {differing} bytes differ (max delta {maxDelta})");
            return;
        }

        Assert.True(maxDelta <= 2, $"{what}: max channel delta {maxDelta}");
        Assert.True(differing < expected.Length / 20, $"{what}: {differing} of {expected.Length} bytes differ");
    }

    /// <summary>Renders each start position in order - so a cache sees a real scroll - and returns the last frame.</summary>
    private static byte[] Render(WaveformDrawStyle style, SkiaWaveformCacheMode mode, float scale, double[] starts)
    {
        var previousMode = SkiaWaveformRenderer.CacheMode;
        SkiaWaveformRenderer.CacheMode = mode;
        try
        {
            var av = new SkiaAudioVisualizer
            {
                WavePeaks = MakePeaks(),
                WaveformDrawStyle = style,
                DrawGridLines = true,
                WaveformColor = Avalonia.Media.Color.FromArgb(150, 144, 238, 144),
                WaveformSelectedColor = Avalonia.Media.Color.FromArgb(210, 254, 10, 10),
                WaveformBackgroundColor = Avalonia.Media.Color.FromArgb(90, 70, 70, 70),
                WaveformFancyHighColor = Avalonia.Media.Colors.Orange,
            };
            av.Measure(new Size(Width, Height));
            av.Arrange(new Rect(0, 0, Width, Height));

            var lines = MakeLines();
            var selection = new List<SubtitleLineViewModel> { lines[4] };
            using var surface = SKSurface.Create(new SKImageInfo((int)(Width * scale), (int)(Height * scale), SKColorType.Bgra8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            foreach (var start in starts)
            {
                av.SetPosition(start, lines, start + 2, 4, selection);
                using var operation = av.CreateDrawOperation()!;
                canvas.Clear(SKColors.Black);
                canvas.Save();
                canvas.Scale(scale);
                operation.RenderTo(canvas);
                canvas.Restore();
            }

            using var image = surface.Snapshot();
            return image.PeekPixels().GetPixelSpan().ToArray();
        }
        finally
        {
            SkiaWaveformRenderer.CacheMode = previousMode;
        }
    }

    private static List<SubtitleLineViewModel> MakeLines()
    {
        var lines = new List<SubtitleLineViewModel>();
        for (var i = 0; i < 300; i++)
        {
            lines.Add(new SubtitleLineViewModel
            {
                Number = i + 1,
                Text = "Line " + i + "\nsecond line of text",
                StartTime = TimeSpan.FromSeconds(1 + i * 2.5),
                EndTime = TimeSpan.FromSeconds(3 + i * 2.5),
            });
        }

        return lines;
    }

    private static WavePeakData2 MakePeaks()
    {
        var random = new Random(42);
        var peaks = new WavePeak2[SampleRate * 700];
        for (var i = 0; i < peaks.Length; i++)
        {
            var seconds = i / (double)SampleRate;
            var amplitude = seconds % 0.5 < 0.3 ? random.Next(4000, 28000) : random.Next(0, 900);
            peaks[i] = new WavePeak2((short)amplitude, (short)-random.Next(amplitude / 2, amplitude + 1));
        }

        return new WavePeakData2(SampleRate, peaks);
    }
}
