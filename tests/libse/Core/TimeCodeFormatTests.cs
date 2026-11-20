using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// The five frame-based formatters share one implementation, so their output is pinned together -
/// including the "frames round up to a whole second" carry.
/// </summary>
public class TimeCodeFormatTests : IDisposable
{
    private readonly double _frameRate = Configuration.Settings.General.CurrentFrameRate;
    private readonly bool _smpte = Configuration.Settings.General.CurrentVideoIsSmpte;

    public TimeCodeFormatTests()
    {
        UseFrameRate(25);
    }

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _frameRate;
        Configuration.Settings.General.CurrentVideoIsSmpte = _smpte;
    }

    /// <summary>Frame rate for one test; Dispose puts the previous one back.</summary>
    private static void UseFrameRate(double frameRate, bool smpte = false)
    {
        Configuration.Settings.General.CurrentFrameRate = frameRate;
        Configuration.Settings.General.CurrentVideoIsSmpte = smpte;
    }

    [Theory]
    // totalMs,   HHMMSSFF,      HHMMSS,     DropFrame,      SSFF,    PeriodFF
    // 25 fps has no drop-frame mode, so the DropFrame column repeats the plain label.
    [InlineData(0, "00:00:00:00", "00:00:00", "00:00:00:00", "00:00", "00:00:00.00")]
    [InlineData(500, "00:00:00:13", "00:00:00", "00:00:00:13", "00:13", "00:00:00.13")]
    // 999 ms rounds to 25 frames at 25 fps, i.e. a whole second - every formatter must carry.
    [InlineData(999, "00:00:01:00", "00:00:01", "00:00:01:00", "01:00", "00:00:01.00")]
    [InlineData(59_999, "00:01:00:00", "00:01:00", "00:01:00:00", "60:00", "00:01:00.00")]
    [InlineData(3_600_000, "01:00:00:00", "01:00:00", "01:00:00:00", "00:00", "01:00:00.00")]
    [InlineData(90_061_500, "25:01:01:13", "25:01:01", "25:01:01:13", "01:13", "25:01:01.13")]
    public void FrameFormatters_Match(double totalMilliseconds, string hhmmssff, string hhmmss,
        string dropFrame, string ssff, string periodFf)
    {
        var tc = new TimeCode(totalMilliseconds);

        Assert.Equal(hhmmssff, tc.ToHHMMSSFF());
        Assert.Equal(hhmmss, tc.ToHHMMSS());
        Assert.Equal(dropFrame, tc.ToDropFrameString());
        Assert.Equal(ssff, tc.ToSSFF());
        Assert.Equal(periodFf, tc.ToHHMMSSPeriodFF());
    }

    /// <summary>
    /// Pinned because its one caller (Avid Caption) only moves to ToDropFrameString() in a later
    /// change - until then this has to keep changing nothing but the separator.
    /// </summary>
    [Fact]
    public void LegacyDropFrameName_StillOnlyChangesTheSeparator()
    {
        UseFrameRate(29.97);

        // Plain numbers with a ';' - which is what drop frame is not.
        Assert.Equal("00:01:00;00", new TimeCode(60_000).ToHHMMSSFFDropFrame());
        Assert.Equal("00:00:59;28", new TimeCode(60_000).ToDropFrameString());
    }

    /// <summary>
    /// A drop-frame label names the frame on screen, not the wall clock rounded onto a 30 fps
    /// grid: 29.97 x 60 s is 1798.2 frames, so one minute shows frame 1798, labelled 00:00:59;28.
    /// </summary>
    [Theory]
    [InlineData(29.97, 0, "00:00:00;00")]
    [InlineData(29.97, 60_000, "00:00:59;28")]
    [InlineData(29.97, 60_060, "00:01:00;02")]
    [InlineData(29.97, 60_090, "00:01:00;03")]
    [InlineData(29.97, 600_000, "00:10:00;00")]
    [InlineData(29.97, 3_600_000, "01:00:00;00")]
    [InlineData(59.94, 60_000, "00:00:59;56")]
    [InlineData(59.94, 60_060, "00:01:00;04")]
    [InlineData(59.94, 600_000, "00:10:00;00")]
    [InlineData(59.94, 3_600_000, "01:00:00;00")]
    public void DropFrameFormatter_UsesTheFrameOnScreen(double frameRate, double totalMilliseconds, string expected)
    {
        UseFrameRate(frameRate);
        Assert.Equal(expected, new TimeCode(totalMilliseconds).ToDropFrameString());
    }

    /// <summary>
    /// Rates without a drop-frame mode get plain labels. 23.976 is 1000/1001 too, but ST 12-1
    /// gives it none; 60 sits closest to 59.94, where a loose tolerance would claim drop frame.
    /// </summary>
    [Theory]
    [InlineData(23.976)]
    [InlineData(25)]
    [InlineData(60)]
    public void DropFrameFormatter_FallsBackToPlainLabelsWithoutDropFrameMode(double frameRate)
    {
        UseFrameRate(frameRate);

        Assert.False(TimeCode.IsDropFrameRate(frameRate));
        Assert.DoesNotContain(";", new TimeCode(60_000).ToDropFrameString());
    }

    /// <summary>
    /// Both rate checks use the tolerance the frame conversion uses, so a rate near 29.97 is
    /// neither numbered nor labelled as 29.97. A 0.01 tolerance would claim drop frame here.
    /// </summary>
    [Theory]
    [InlineData(29.97, 2)]
    [InlineData(59.94, 4)]
    [InlineData(29.972, 0)]
    [InlineData(59.946, 0)]
    public void DropFrameRate_ToleranceMatchesTheConversion(double frameRate, int skipCount)
    {
        Assert.Equal(skipCount, TimeCode.DropFrameSkipCount(frameRate));
        Assert.Equal(skipCount > 0, TimeCode.IsDropFrameRate(frameRate));
    }

    /// <summary>In SMPTE timing the labels count on the time code clock, not on media time.</summary>
    [Fact]
    public void DropFrameLabelsFollowTheSmpteClock()
    {
        UseFrameRate(29.97, smpte: true);

        Assert.Equal("01:00:03;18", new TimeCode(3_600_000).ToDropFrameString());
        Assert.InRange(TimeCode.FromDropFrameLabel(1, 0, 0, 0).TotalMilliseconds, 3_596_399, 3_596_401);
    }

    /// <summary>Without SMPTE timing the same numbers stay on the media clock.</summary>
    [Fact]
    public void DropFrameLabelsStayOnTheMediaClock()
    {
        UseFrameRate(29.97);

        Assert.Equal("01:00:00;00", new TimeCode(3_600_000).ToDropFrameString());
        // 107892 frames, 3.6 ms short of the hour - the residual drop frame allows.
        Assert.InRange(TimeCode.FromDropFrameLabel(1, 0, 0, 0).TotalMilliseconds, 3_599_996, 3_600_000);
    }

    /// <summary>A negative time keeps its sign, as every other formatter does.</summary>
    [Fact]
    public void DropFrameFormatter_KeepsTheSign()
    {
        UseFrameRate(29.97);
        Assert.Equal("-00:00:59;28", new TimeCode(-60_000).ToDropFrameString());
    }

    /// <summary>Zero has one spelling, so a step that lands on it carries no sign.</summary>
    [Fact]
    public void DropFrameFormatter_StepsAcrossZeroWithoutASign()
    {
        UseFrameRate(29.97);

        Assert.Equal("-00:00:00;00", new TimeCode(-10).ToDropFrameString());
        Assert.Equal("-00:00:00:00", new TimeCode(-10).ToHHMMSSFF());
        Assert.Equal("00:00:00;00", new TimeCode(-10).ToNextFrameString());
        Assert.Equal("-00:00:00;01", new TimeCode(-60).ToNextFrameString());
    }

    /// <summary>
    /// Write then read lands on the same frame, or every save/load walks the subtitles away from
    /// the video. Swept over ten minutes so every minute boundary is hit.
    /// </summary>
    [Theory]
    [InlineData(29.97)]
    [InlineData(59.94)]
    public void DropFrameLabel_RoundTripsToTheSameFrame(double frameRate)
    {
        UseFrameRate(frameRate);

        var framesPerSecond = (int)Math.Round(frameRate);
        var drop = TimeCode.DropFrameSkipCount(frameRate);
        var halfFrame = 500.0 / frameRate;
        // On the 1000/1001 clock the implementation uses - a rounded 29.97 fails by a whole frame.
        var exactFrameRate = framesPerSecond * 1000.0 / 1001.0;

        for (var frame = 0; frame < 40_000; frame++)
        {
            var milliseconds = frame * 1000.0 / exactFrameRate;
            var label = new TimeCode(milliseconds).ToDropFrameString();
            var parts = label.Split(':', ';');
            var seconds = int.Parse(parts[2]);
            var frameField = int.Parse(parts[3]);

            Assert.True(frameField < framesPerSecond, $"{label} has an impossible frame field");
            // Labels 00..(drop-1) do not exist at the start of every minute except every tenth.
            Assert.True(seconds != 0 || int.Parse(parts[1]) % 10 == 0 || frameField >= drop,
                $"{label} is a label no frame ever gets at {frameRate} fps");

            var back = TimeCode.FromDropFrameLabel(int.Parse(parts[0]), int.Parse(parts[1]), seconds, frameField);
            Assert.True(Math.Abs(back.TotalMilliseconds - milliseconds) <= halfFrame,
                $"frame {frame}: {milliseconds} ms -> {label} -> {back.TotalMilliseconds} ms at {frameRate} fps");
        }
    }

    /// <summary>
    /// The label one frame on, which a one-frame event needs for its out point. Counted from the
    /// label rather than by adding a frame duration: at 23.976 the 41.708 ms step is truncated to
    /// 41 or 42 ms and can round back onto the label it started from.
    /// </summary>
    [Theory]
    [InlineData(29.97, 60_060, "00:01:00;03")]     // label 1802, frame 1800
    [InlineData(29.97, 60_027, "00:01:00;02")]     // 00:00:59;29 is the last label of minute 0
    [InlineData(29.97, 599_966, "00:10:00;00")]    // minute 10 is a tenth one - nothing skipped
    [InlineData(59.94, 60_059, "00:01:00;05")]     // four labels skipped, 00-03
    [InlineData(23.976, 60_146.2, "00:01:00:05")]  // plain: 146 ms and 187 ms are both frame 4
    [InlineData(25, 59_960, "00:01:00:00")]        // 59:24 carries into the next second
    [InlineData(29.97, -60_000, "-00:00:59;27")]   // negative runs toward zero, not away
    [InlineData(29.97, -60_060, "-00:00:59;29")]   // and borrows over the labels 00-01 skip
    [InlineData(29.97, -10, "00:00:00;00")]        // -0.x frames: one on is zero, not a sign more
    [InlineData(59.94, -10, "00:00:00;00")]        // ;01 steps onto zero - no signed zero either
    public void NextFrameLabel_CountsOneLabelOn(double frameRate, double totalMilliseconds, string expected)
    {
        UseFrameRate(frameRate);
        Assert.Equal(expected, new TimeCode(totalMilliseconds).ToNextFrameString());
    }

    /// <summary>
    /// At a rate without a drop-frame mode the plain label repeats numbers of its own - 00:00:21:00
    /// belongs to no frame at 23.976 - so the step promises a label of its own, not the next one's.
    /// </summary>
    [Fact]
    public void NextFrameLabel_CountsOnFromAPlainLabel()
    {
        UseFrameRate(23.976);

        var frame503 = new TimeCode(503 * 1001.0 / 24.0);
        var frame504 = new TimeCode(504 * 1001.0 / 24.0);

        Assert.Equal("00:00:20:23", frame503.ToDropFrameString());
        Assert.Equal("00:00:21:01", frame504.ToDropFrameString());
        Assert.Equal("00:00:21:00", frame503.ToNextFrameString());
    }

    /// <summary>
    /// The hour field is only bounded by the caller's parser, so a corrupt label must not overflow
    /// the frame arithmetic on the way back out.
    /// </summary>
    [Fact]
    public void DropFrameLabel_SurvivesAnHourFieldNoFileHas()
    {
        UseFrameRate(29.97);

        var label = TimeCode.FromDropFrameLabel(999_999, 0, 0, 0).ToDropFrameString();
        Assert.Equal(new TimeCode(TimeCode.MaxTimeTotalMilliseconds).ToDropFrameString(), label);
    }

    /// <summary>ToSSFF carries into seconds only: 59.999 s is "60:00", not "00:00".</summary>
    [Fact]
    public void ToSSFF_CarriesIntoSecondsOnly()
    {
        Assert.Equal("60:00", new TimeCode(59_999).ToSSFF());
        Assert.Equal("00:00", new TimeCode(3_600_000).ToSSFF());
    }

    [Theory]
    [InlineData(-1000, "-00:00:01:00")]
    [InlineData(-90_000, "-00:01:30:00")]
    public void FrameFormatters_PrefixASingleSign(double totalMilliseconds, string expected)
    {
        Assert.Equal(expected, new TimeCode(totalMilliseconds).ToHHMMSSFF());
    }

    [Theory]
    [InlineData(-1000, "-00:00:01,000")]
    [InlineData(-3_723_456, "-01:02:03,456")]
    public void ToString_PrefixesASingleSign(double totalMilliseconds, string expected)
    {
        Assert.Equal(expected, new TimeCode(totalMilliseconds).ToString(false));
    }

    [Fact]
    public void ToShortStringHHMMSSFF_DropsLeadingZeroGroups()
    {
        Assert.Equal("00:00", new TimeCode(0).ToShortStringHHMMSSFF());
        Assert.Equal("01:00", new TimeCode(999).ToShortStringHHMMSSFF());
        Assert.Equal("01:00:00", new TimeCode(59_999).ToShortStringHHMMSSFF());
        Assert.Equal("01:00:00:00", new TimeCode(3_600_000).ToShortStringHHMMSSFF());
    }
}
