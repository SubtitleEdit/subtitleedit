using Nikse.SubtitleEdit.Core.Common;
using System.Text;

namespace LibSETests.Common;

public class FileUtilTest
{
    private static byte[] MakeRawPgsSegment(byte segmentType, int payloadSize)
    {
        var segment = new byte[3 + payloadSize];
        segment[0] = segmentType;
        segment[1] = (byte)(payloadSize >> 8);
        segment[2] = (byte)(payloadSize & 0xff);
        return segment;
    }

    private static void WithTempFile(byte[] content, Action<string> assert)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, content);
            assert(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void IsRawPgsSegmentStreamDetectsRawSegmentChain()
    {
        // PCS + WDS + PDS + ODS + END - the segment sequence of one display set as it
        // appears in a Matroska S_HDMV/PGS track extracted in raw mode (issue #12683).
        var bytes = MakeRawPgsSegment(0x16, 19)
            .Concat(MakeRawPgsSegment(0x17, 10))
            .Concat(MakeRawPgsSegment(0x14, 50))
            .Concat(MakeRawPgsSegment(0x15, 200))
            .Concat(MakeRawPgsSegment(0x80, 0))
            .ToArray();

        WithTempFile(bytes, path => Assert.True(FileUtil.IsRawPgsSegmentStream(path)));
    }

    [Fact]
    public void IsRawPgsSegmentStreamRejectsBluRaySup()
    {
        // A proper standalone .sup starts with the "PG" magic, not a bare segment type.
        var bytes = new byte[] { 0x50, 0x47, 0, 0, 0, 0, 0, 0, 0, 0, 0x16, 0, 19 }
            .Concat(new byte[19])
            .ToArray();

        WithTempFile(bytes, path => Assert.False(FileUtil.IsRawPgsSegmentStream(path)));
    }

    [Fact]
    public void IsRawPgsSegmentStreamRejectsText()
    {
        var bytes = Encoding.UTF8.GetBytes("1\n00:00:01,000 --> 00:00:02,000\nHello\n");

        WithTempFile(bytes, path => Assert.False(FileUtil.IsRawPgsSegmentStream(path)));
    }

    [Fact]
    public void IsRawPgsSegmentStreamRejectsSingleSegmentFollowedByGarbage()
    {
        // One plausible segment header followed by non-PGS bytes must not count -
        // requiring a chain of valid segments keeps false positives out.
        var bytes = new byte[] { 0x16, 0x00, 0x13 }
            .Concat(Enumerable.Repeat((byte)'0', 1000))
            .ToArray();

        WithTempFile(bytes, path => Assert.False(FileUtil.IsRawPgsSegmentStream(path)));
    }

    // The UTF-16LE BOM is FF FE; ReadAllLinesShared used to test FE FF (the UTF-16BE BOM),
    // so the BOM was never stripped and U+FEFF leaked into the first line.
    [Fact]
    public void ReadAllLinesSharedUtf16LeWithBomStripsBom()
    {
        var path = Path.GetTempFileName();
        try
        {
            var bytes = Encoding.Unicode.GetPreamble()
                .Concat(Encoding.Unicode.GetBytes("Hello" + Environment.NewLine + "World"))
                .ToArray();
            File.WriteAllBytes(path, bytes);

            var lines = FileUtil.ReadAllLinesShared(path, Encoding.Unicode);

            Assert.Equal("Hello", lines[0]);
            Assert.Equal("World", lines[1]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
