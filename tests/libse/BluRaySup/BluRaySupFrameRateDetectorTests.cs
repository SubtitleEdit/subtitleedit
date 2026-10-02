using Nikse.SubtitleEdit.Core.BluRaySup;
using BluRaySupCore = Nikse.SubtitleEdit.Core.BluRaySup.Core;

namespace LibSETests.BluRaySup;

/// <summary>
/// The PCS frame_rate byte of a sup is often wrong, so the frame rate is taken from the grid the
/// start times are on and the byte only trusted when they fit it.
/// </summary>
public class BluRaySupFrameRateDetectorTests
{
    private const int Code23976 = 0x10;
    private const int Code25 = 0x30;

    /// <summary>Cue start PTS on the frame grid of fps, a few seconds apart, optionally stored in whole ms.</summary>
    private static List<long> FramePts(double fps, int count, bool wholeMilliseconds, long origin = 0)
    {
        var random = new Random(count);
        var pts = new List<long>(count);
        long frame = 10;
        for (var i = 0; i < count; i++)
        {
            frame += random.Next(20, 200);
            var exact = origin + frame * 90000.0 / fps;
            pts.Add(wholeMilliseconds
                ? (long)(Math.Round(exact / 90.0) * 90.0)
                : (long)Math.Floor(exact));
        }

        return pts;
    }

    [Fact]
    public void Timing23976With25Byte_Is23976()
    {
        // What the export dialog's default 25 fps profile wrote over 23.976 timing.
        var pts = FramePts(BluRaySupCore.Fps24P, 500, wholeMilliseconds: true);
        Assert.Equal(BluRaySupCore.Fps24P, BluRaySupFrameRateDetector.Detect(pts, Code25));
    }

    [Fact]
    public void Timing25With23976Byte_Is25()
    {
        var pts = FramePts(25, 500, wholeMilliseconds: true);
        Assert.Equal(25.0, BluRaySupFrameRateDetector.Detect(pts, Code23976));
    }

    [Fact]
    public void Disc23976With23976Byte_Is23976()
    {
        // A disc rip: exact frame PTS, the stream not starting on a frame at PTS 0.
        var pts = FramePts(BluRaySupCore.Fps24P, 800, wholeMilliseconds: false, origin: 1234);
        Assert.Equal(BluRaySupCore.Fps24P, BluRaySupFrameRateDetector.Detect(pts, Code23976));
    }

    [Fact]
    public void Timing25With25Byte_KeepsDeclared25()
    {
        var pts = FramePts(25, 300, wholeMilliseconds: true);
        Assert.Equal(25.0, BluRaySupFrameRateDetector.Detect(pts, Code25));
    }

    [Fact]
    public void Timing25WithUnknownByte_PrefersLowerRateOnTie()
    {
        // Every 25 fps PTS is also on the 50 grid.
        var pts = FramePts(25, 300, wholeMilliseconds: true);
        Assert.Equal(25.0, BluRaySupFrameRateDetector.Detect(pts, 0));
    }

    [Fact]
    public void Timing2997WithWrongByte_Is2997()
    {
        var pts = FramePts(BluRaySupCore.FpsNtsc, 500, wholeMilliseconds: true);
        Assert.Equal(BluRaySupCore.FpsNtsc, BluRaySupFrameRateDetector.Detect(pts, Code25));
    }

    [Fact]
    public void RandomTimes_IsZero()
    {
        var random = new Random(42);
        var pts = new List<long>();
        for (var i = 0; i < 500; i++)
        {
            pts.Add(random.NextInt64(0, 90000L * 3600 * 2));
        }

        Assert.Equal(0, BluRaySupFrameRateDetector.Detect(pts, Code25));
    }

    [Fact]
    public void WholeSecondTimes_AreAmbiguous()
    {
        // On the 24, 25 and 50 grids alike - neither family can be told apart, so no guess.
        var pts = new List<long>();
        for (var i = 1; i <= 30; i++)
        {
            pts.Add(i * 7 * 90000L);
        }

        Assert.Equal(0, BluRaySupFrameRateDetector.Detect(pts, Code23976));
    }

    [Fact]
    public void FewTimes_KeepDeclared()
    {
        Assert.Equal(25.0, BluRaySupFrameRateDetector.Detect(new List<long> { 1001, 50_000 }, Code25));
        Assert.Equal(25.0, BluRaySupFrameRateDetector.Detect(new List<long>(), Code25));
    }
}
