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
        Assert.Equal(900, WaveFileExtractor.GetWaveformDelayMilliseconds(0.9, false));
    }

    [Fact]
    public void Delay_TransportStream_IsZero()
    {
        Assert.Equal(0, WaveFileExtractor.GetWaveformDelayMilliseconds(1.4, true));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0.0)]
    [InlineData(-0.021333)]
    public void Delay_NoPositiveStart_IsZero(double? start)
    {
        Assert.Equal(0, WaveFileExtractor.GetWaveformDelayMilliseconds(start, false));
    }
}
