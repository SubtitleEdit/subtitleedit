using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

/// <summary>
/// The start time code read from "ffmpeg -i" output for "Set video offset" - "From video" (#17009).
/// The logs are what ffmpeg prints for files made with "-timecode".
/// </summary>
public class FfmpegStartTimeCodeTests
{
    private const string MovLog =
        "Input #0, mov,mp4,m4a,3gp,3g2,mj2, from 'tc.mov':\n" +
        "  Metadata:\n" +
        "    major_brand     : qt  \n" +
        "  Duration: 00:00:01.00, start: 0.000000, bitrate: 439 kb/s\n" +
        "  Stream #0:0[0x1]: Video: mpeg2video (Main) (m2v1 / 0x3176326D), yuv420p(tv, progressive), 320x240 [SAR 1:1 DAR 4:3], 435 kb/s, 25 fps, 25 tbr, 12800 tbn (default)\n" +
        "      Metadata:\n" +
        "        handler_name    : VideoHandler\n" +
        "        timecode        : 10:00:00:12\n" +
        "  Stream #0:1[0x2]: Data: none (tmcd / 0x64636D74) (default)\n" +
        "      Metadata:\n" +
        "        handler_name    : TimeCodeHandler\n" +
        "        timecode        : 10:00:00:12\n";

    [Fact]
    public void Mov_TimeCodeTrack_FramesAtVideoFrameRate()
    {
        var tc = FfmpegMediaInfo2.ParseStartTimeCode(MovLog, 30);
        Assert.NotNull(tc);
        Assert.Equal("10:00:00:12", tc.Text);
        Assert.Equal(TimeSpan.FromHours(10) + TimeSpan.FromMilliseconds(480), tc.Offset);
    }

    [Fact]
    public void Mxf_FileLevelTimeCode()
    {
        var log =
            "Input #0, mxf, from 'tc.mxf':\n" +
            "  Metadata:\n" +
            "    operational_pattern_ul: 060e2b34.04010101.0d010201.01010900\n" +
            "    timecode        : 01:00:00:00\n" +
            "  Duration: 00:00:01.00, start: 0.000000, bitrate: 639 kb/s\n" +
            "  Stream #0:0: Video: mpeg2video (4:2:2), yuv422p(tv, bt709, top first), 1920x1080 [SAR 1:1 DAR 16:9], 25 fps, 25 tbr, 25 tbn\n";
        Assert.Equal(TimeSpan.FromHours(1), FfmpegMediaInfo2.ParseStartTimeCode(log, 25)!.Offset);
    }

    [Fact]
    public void DropFrame_SemicolonSeparator()
    {
        var log =
            "  Stream #0:0[0x1]: Video: mpeg2video (Main), yuv420p, 320x240, 29.97 fps, 29.97 tbr, 30k tbn\n" +
            "      Metadata:\n" +
            "        timecode        : 01:00:00;15\n";
        var tc = FfmpegMediaInfo2.ParseStartTimeCode(log, 25);
        Assert.NotNull(tc);
        Assert.Equal("01:00:00;15", tc.Text);
        Assert.Equal(3_600_000 + 501, (long)tc.Offset.TotalMilliseconds); // 15 / 29.97 s
    }

    [Fact]
    public void NoFrameRateInLog_UsesFallback()
    {
        var tc = FfmpegMediaInfo2.ParseStartTimeCode("    timecode        : 00:00:10:12\n", 24);
        Assert.Equal(10_500, (long)tc!.Offset.TotalMilliseconds);
    }

    [Fact]
    public void NoTimeCode_ReturnsNull()
    {
        var log = "  Duration: 00:00:01.00, start: 0.000000, bitrate: 439 kb/s\n" +
                  "  Stream #0:0: Video: h264, yuv420p, 1920x1080, 25 fps, 25 tbr\n";
        Assert.Null(FfmpegMediaInfo2.ParseStartTimeCode(log, 25));
    }

    [Theory]
    [InlineData("    timecode        : 10:61:00:00\n")] // minutes out of range
    [InlineData("    timecode        : 10:00:00:25\n")] // frame 25 does not exist at 25 fps
    public void InvalidTimeCode_ReturnsNull(string log)
    {
        Assert.Null(FfmpegMediaInfo2.ParseStartTimeCode(log, 25));
    }
}
