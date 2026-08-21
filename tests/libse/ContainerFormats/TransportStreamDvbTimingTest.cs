using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using System.IO;
using System.Linq;

namespace LibSETests.ContainerFormats;

public class TransportStreamDvbTimingTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    /// <summary>
    /// The DVB timing loop turns absolute PTS into video-relative times by subtracting the first
    /// video timestamp. A subtitle that starts a millisecond *before* that first video frame used
    /// to fall into the "different epoch" rebase, which computes a replacement offset from the
    /// previous subtitle and then keeps it - so one millisecond of muxer jitter pushed the second
    /// cue of this stream from 0 s out to 7 s, and every later cue with it.
    /// </summary>
    [Fact]
    public void SubtitleStartingJustBeforeVideoDoesNotShiftTheStream()
    {
        var parser = new TransportStreamParser();
        parser.Parse(FilePath("sample_TS_dvbsub_at_video_start.ts"), null);

        var pid = Assert.Single(parser.SubtitlePacketIds);
        var subtitles = parser.GetDvbSubtitles(pid);
        Assert.Equal(2, subtitles.Count);

        // Both cues sit at the very start of the stream; neither may be pushed out.
        Assert.Equal(0UL, subtitles[0].StartMilliseconds);
        Assert.Equal(0UL, subtitles[1].StartMilliseconds);
        Assert.True(subtitles[1].EndMilliseconds < 3000,
            $"second cue should end within the first seconds, was {subtitles[1].EndMilliseconds} ms");
    }

    /// <summary>
    /// 204-byte packets (188 bytes plus Reed-Solomon parity) as written by DVB capture cards.
    /// The parity is skipped, so the same stream must read back identically to its 188-byte form.
    /// </summary>
    [Fact]
    public void Rs204StreamReadsTheSameAsThe188ByteForm()
    {
        var plain = new TransportStreamParser();
        plain.Parse(FilePath("sample_TS_dvbsub_at_video_start.ts"), null);

        var rs204 = new TransportStreamParser();
        rs204.Parse(FilePath("sample_TS_dvbsub_rs204.ts"), null);

        Assert.Equal(plain.SubtitlePacketIds, rs204.SubtitlePacketIds);

        var expected = plain.GetDvbSubtitles(plain.SubtitlePacketIds[0]);
        var actual = rs204.GetDvbSubtitles(rs204.SubtitlePacketIds[0]);
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected.Select(p => p.StartMilliseconds), actual.Select(p => p.StartMilliseconds));
        Assert.Equal(expected.Select(p => p.EndMilliseconds), actual.Select(p => p.EndMilliseconds));
    }

    [Fact]
    public void Rs204FileIsRecognisedAsATransportStream()
    {
        Assert.True(FileUtil.IsTransportStream(FilePath("sample_TS_dvbsub_rs204.ts")));
        Assert.True(FileUtil.IsTransportStream(FilePath("sample_TS_dvbsub_at_video_start.ts")));
    }

    [Theory]
    [InlineData(204, true)]  // Reed-Solomon packets
    [InlineData(188, false)] // a plain transport stream must not be taken for RS204
    [InlineData(192, false)] // M2TS has its own detection
    public void IsRs204TransportStream_MatchesOnlyThe204ByteStride(int stride, bool expected)
    {
        var data = new byte[stride * 3 + 8];
        for (var i = 0; i + 1 < data.Length; i += stride)
        {
            data[i] = Packet.SynchronizationByte;
        }

        using var ms = new MemoryStream(data);
        Assert.Equal(expected, TransportStreamParser.IsRs204TransportStream(ms));
    }
}
