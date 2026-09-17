using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The Skia waveform renderer used to shape a whole subtitle line with one HarfBuzz call. HarfBuzz
/// picks one direction per buffer and does no bidi reordering, so in a Hebrew line "abc" came out
/// as "cba" and "12" as "21" - unlike the classic renderer, whose Avalonia FormattedText does full
/// bidi. These render through <see cref="SkiaTextCache"/> and compare pixels.
/// </summary>
public class SkiaTextCacheBidiTests
{
    private const float Size = 16f;
    private const int Width = 160;
    private const int Height = 24;

    [Fact]
    public void HebrewLineDrawsLatinWordAndDigitsInReadingOrder()
    {
        var cache = new SkiaTextCache();
        cache.SetFont("Arial", false);

        var line = cache.Get("שלום 12 abc", Size, rightToLeft: true);
        Assert.NotNull(line.Blob);

        // Expected: the runs in visual order, each shaped on its own in its own direction and
        // placed side by side from the left. The cache does not shape a whitespace-only string, so
        // the width of the space between "abc" and "12" is taken from shaped text around it.
        var space = cache.Get("abc 12", Size).Width - cache.Get("abc12", Size).Width;
        Assert.True(space > 0);
        var expected = Draw(canvas =>
        {
            var x = 0f;
            foreach (var (text, rtl) in new[] { ("abc", false), (null, true), ("12", false), ("שלום ", true) })
            {
                if (text == null)
                {
                    x += space;
                    continue;
                }

                var run = cache.Get(text, Size, rtl);
                DrawBlob(canvas, run, x);
                x += run.Width;
            }
        });

        var actual = Draw(canvas => DrawBlob(canvas, line, 0));
        Assert.True(HasInk(actual), "nothing was drawn - no font with Latin glyphs available");
        Assert.Equal(expected, actual);

        // ...and not what one right to left shaping pass over the whole line gives (the old output,
        // with "abc" mirrored to "cba" and "12" to "21").
        var typeface = SKTypeface.FromFamilyName("Arial") ?? SKTypeface.Default;
        using var font = new SKFont(typeface, Size) { Subpixel = true, Edging = SKFontEdging.Antialias };
        using var shaper = new SKShaper(typeface);
        var single = Draw(canvas =>
        {
            using var buffer = new HarfBuzzSharp.Buffer();
            buffer.AddUtf16("שלום 12 abc");
            buffer.Direction = HarfBuzzSharp.Direction.RightToLeft;
            buffer.GuessSegmentProperties();
            var result = shaper.Shape(buffer, font);
            using var builder = new SKTextBlobBuilder();
            var run = builder.AllocatePositionedRun(font, result.Codepoints.Length);
            for (var i = 0; i < result.Codepoints.Length; i++)
            {
                run.Glyphs[i] = (ushort)result.Codepoints[i];
                run.Positions[i] = result.Points[i];
            }

            using var blob = builder.Build();
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawText(blob, 0, line.Baseline, paint);
        });
        Assert.NotEqual(single, actual);
    }

    [Fact]
    public void LeftToRightLineIsUnchangedByTheBidiPath()
    {
        var cache = new SkiaTextCache();
        cache.SetFont("Arial", false);

        // "abc 12" has no right to left letters: an explicit right to left paragraph (a right to
        // left UI) must still read "abc 12", not "12 abc".
        var ltr = Draw(canvas => DrawBlob(canvas, cache.Get("abc 12", Size, rightToLeft: false), 0));
        var rtl = Draw(canvas => DrawBlob(canvas, cache.Get("abc 12", Size, rightToLeft: true), 0));
        Assert.True(HasInk(ltr));
        Assert.Equal(ltr, rtl);
    }

    private static void DrawBlob(SKCanvas canvas, SkiaTextCache.ShapedText text, float x)
    {
        if (text.Blob == null)
        {
            return;
        }

        using var paint = new SKPaint { Color = SKColors.White };
        canvas.DrawText(text.Blob, x, text.Baseline, paint);
    }

    private static byte[] Draw(Action<SKCanvas> draw)
    {
        using var bitmap = new SKBitmap(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Black);
        draw(canvas);
        canvas.Flush();
        return bitmap.Bytes;
    }

    private static bool HasInk(byte[] pixels)
    {
        foreach (var b in pixels)
        {
            if (b != 0 && b != 255)
            {
                return true;
            }
        }

        return false;
    }
}
