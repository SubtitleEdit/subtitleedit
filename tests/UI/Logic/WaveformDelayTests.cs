using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

public class WaveformDelayTests
{
    [Fact]
    public void ParseStartSeconds_MpegProgramStream()
    {
        var log = "Input #0, mpeg, from 'EHD.mpeg':\n  Duration: 00:44:14.00, start: 0.900000, bitrate: 741 kb/s\n";
        Assert.Equal(0.9, FfmpegMediaInfo2.ParseStartSeconds(log)!.Value, 6);
    }

    [Fact]
    public void ParseStartSeconds_Negative()
    {
        var log = "  Duration: 00:01:00.02, start: -0.021333, bitrate: 128 kb/s\n";
        Assert.Equal(-0.021333, FfmpegMediaInfo2.ParseStartSeconds(log)!.Value, 6);
    }

    [Fact]
    public void ParseStartSeconds_NoStart_ReturnsNull()
    {
        Assert.Null(FfmpegMediaInfo2.ParseStartSeconds("  Duration: N/A, bitrate: N/A\n"));
    }

    [Fact]
    public void Delay_ProgramStream_IsStartTime()
    {
        Assert.Equal(900, WaveFileExtractor.GetTimelineOffsetMilliseconds(0.9, false));
    }

    [Fact]
    public void Delay_TransportStream_IsZero()
    {
        Assert.Equal(0, WaveFileExtractor.GetTimelineOffsetMilliseconds(1.4, true));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0.0)]
    [InlineData(-0.021333)]
    public void Delay_NoPositiveStart_IsZero(double? start)
    {
        Assert.Equal(0, WaveFileExtractor.GetTimelineOffsetMilliseconds(start, false));
    }

    [Fact]
    public void ToFfmpegRange_SubtractsOffset()
    {
        var (start, duration) = WaveFileExtractor.ToFfmpegRange(15.886, 2.0, 900);
        Assert.Equal(14.986, start, 6);
        Assert.Equal(2.0, duration, 6);
    }

    [Fact]
    public void ToFfmpegRange_BeforeFfmpegZero_KeepsTheRest()
    {
        var (start, duration) = WaveFileExtractor.ToFfmpegRange(0.5, 2.0, 900);
        Assert.Equal(0, start, 6);
        Assert.Equal(1.6, duration, 6);
    }

    [Fact]
    public void ToFfmpegRange_NoOffset_Unchanged()
    {
        Assert.Equal((3.25, 1.5), WaveFileExtractor.ToFfmpegRange(3.25, 1.5, 0));
    }
}
