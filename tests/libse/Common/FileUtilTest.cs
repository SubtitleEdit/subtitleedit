using Nikse.SubtitleEdit.Core.Common;
using System.Text;

namespace LibSETests.Common;

public class FileUtilTest
{
    private static readonly byte[] MxfHeaderPartitionPackId = { 0x06, 0x0E, 0x2B, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02 };

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

    // The Header Partition PackId is 11 bytes and may sit anywhere in the first 64 KB, so the
    // interesting offsets are the two edges of the search window. The byte-at-a-time loop this
    // replaced stopped at start offset count - 12, which missed a pack ending on the last byte.
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(2048 - 12)]
    [InlineData(2048 - 11)] // the pack ends on the very last byte read
    public void IsMaterialExchangeFormatFindsPackIdAtAnyOffset(int offset)
    {
        var bytes = new byte[2048];
        MxfHeaderPartitionPackId.CopyTo(bytes, offset);

        WithTempFile(bytes, path => Assert.True(FileUtil.IsMaterialExchangeFormat(path)));
    }

    [Fact]
    public void IsMaterialExchangeFormatRejectsFileWithoutPackId()
    {
        var bytes = new byte[2048];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i % 251);
        }

        // A truncated pack (one byte short of the full signature) must not match either.
        MxfHeaderPartitionPackId.AsSpan(0, MxfHeaderPartitionPackId.Length - 1).CopyTo(bytes.AsSpan(100));
        bytes[100 + MxfHeaderPartitionPackId.Length - 1] = 0xFF;

        WithTempFile(bytes, path => Assert.False(FileUtil.IsMaterialExchangeFormat(path)));
    }

    [Fact]
    public void IsMaterialExchangeFormatRejectsFileShorterThanHundredBytes()
    {
        // The pack is there, but a file this short cannot be an MXF - the size guard wins.
        var bytes = new byte[99];
        MxfHeaderPartitionPackId.CopyTo(bytes, 0);

        WithTempFile(bytes, path => Assert.False(FileUtil.IsMaterialExchangeFormat(path)));
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
