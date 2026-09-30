using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;

namespace LibSETests.BluRaySup;

/// <summary>
/// A Blu-ray sup is written on the frame grid (issue #15478): times are snapped to the nearest
/// frame and the frame converted to 90 kHz with the exact frame duration, rounded down so a
/// strict decoder never shows a caption a frame late.
/// </summary>
public class BluRaySupPictureFrameGridTests
{
    private sealed record Segment(long Pts, byte Type);

    private static List<Segment> ReadSegments(byte[] sup)
    {
        var segments = new List<Segment>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            var pts = ((long)sup[position + 2] << 24) | ((long)sup[position + 3] << 16) | ((long)sup[position + 4] << 8) | sup[position + 5];
            var size = (sup[position + 11] << 8) + sup[position + 12];
            segments.Add(new Segment(pts, sup[position + 10]));
            position += 13 + size;
        }

        Assert.Equal(sup.Length, position);
        return segments;
    }

    private static List<long> PcsPts(BluRaySupPicture picture, double fps, List<BluRaySupFadeStep>? fadeSteps = null)
    {
        using var bitmap = new SKBitmap(100, 20);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
        }

        var sup = BluRaySupPicture.CreateSupFrame(picture, new[]
        {
            new BluRaySupCompositionObject { Bitmap = bitmap, FontColor = SKColors.White, X = 100, Y = 500, FadeSteps = fadeSteps ?? new List<BluRaySupFadeStep>() },
        }, fps);
        return ReadSegments(sup).Where(s => s.Type == 0x16).Select(s => s.Pts).ToList();
    }

    private static BluRaySupPicture Picture(long startMs, long endMs) => new()
    {
        Width = 1280,
        Height = 720,
        StartTime = startMs,
        EndTime = endMs,
    };

    [Theory]
    [InlineData(27861, 2507505)]   // frame 668
    [InlineData(29947, 2695192)]   // frame 718 - exact 2695192.5
    [InlineData(32491, 2924171)]   // frame 779 - exact 2924171.25
    [InlineData(0, 0)]
    public void MillisecondsToPts_23976_IsOnTheFrameGrid(long ms, long expectedPts)
    {
        Assert.Equal(expectedPts, BluRaySupPicture.MillisecondsToPts(ms, 23.976));
        Assert.Equal(expectedPts, BluRaySupPicture.MillisecondsToPts(ms, 24000.0 / 1001));
    }

    [Fact]
    public void MillisecondsToPts_DoesNotDriftOverHours()
    {
        // Frame 172 627 at 23.976 is 7200.4 s; a rounded frame duration would be ticks off.
        const long frame = 172627;
        var ms = (long)Math.Round(frame * 1001 / 24.0);
        Assert.Equal(frame * 90000 * 1001 / 24000, BluRaySupPicture.MillisecondsToPts(ms, 23.976));
    }

    [Theory]
    [InlineData(25.0, 1010, 90000)]      // snapped to the frame at 1000 ms
    [InlineData(29.97, 1001, 90090)]     // frame 30 = 1.001 s
    [InlineData(59.94, 1000, 90090)]     // frame 60 = 1.001 s
    [InlineData(24.0, 1000, 90000)]
    public void MillisecondsToPts_OtherRates(double fps, long ms, long expectedPts)
    {
        Assert.Equal(expectedPts, BluRaySupPicture.MillisecondsToPts(ms, fps));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(double.NaN)]
    public void MillisecondsToPts_WithoutFrameRate_ConvertsAsIs(double fps)
    {
        Assert.Equal(27861 * 90, BluRaySupPicture.MillisecondsToPts(27861, fps));
    }

    [Theory]
    [InlineData(0x10, 24000.0 / 1001)]
    [InlineData(0x20, 24.0)]
    [InlineData(0x30, 25.0)]
    [InlineData(0x40, 30000.0 / 1001)]
    [InlineData(0x60, 50.0)]
    [InlineData(0x70, 60000.0 / 1001)]
    [InlineData(0x00, 0.0)]
    public void GetFrameRate_FromPcsCode(int code, double expected)
    {
        Assert.Equal(expected, BluRaySupPicture.GetFrameRate(code), 6);
    }

    [Fact]
    public void CreateSupFrame_WritesStartAndClearOnTheFrameGrid()
    {
        var pts = PcsPts(Picture(29947, 32491), 23.976);
        Assert.Equal(new long[] { 2695192, 2924171 }, pts);
    }

    [Fact]
    public void CreateSupFrame_ShorterThanHalfAFrame_StaysUpOneFrame()
    {
        var pts = PcsPts(Picture(1000, 1010), 25);
        Assert.Equal(new long[] { 90000, 93600 }, pts);
    }

    [Fact]
    public void CreateSupFrame_FadeStepsOnOneFrame_AreWrittenOnce()
    {
        // 23.976: 1090 and 1100 ms are both frame 26, 1150 is frame 28.
        var steps = new List<BluRaySupFadeStep>
        {
            new(1000, 0),
            new(1090, 30),
            new(1100, 60),
            new(1150, 100),
        };

        var pts = PcsPts(Picture(1000, 3000), 23.976, steps);

        Assert.Equal(new long[]
        {
            BluRaySupPicture.MillisecondsToPts(1000, 23.976),
            BluRaySupPicture.MillisecondsToPts(1100, 23.976),
            BluRaySupPicture.MillisecondsToPts(1150, 23.976),
            BluRaySupPicture.MillisecondsToPts(3000, 23.976),
        }, pts);
    }
}
