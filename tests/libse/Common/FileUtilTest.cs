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

    /// <summary>
    /// A Blu-ray .sup saved under another extension (.sub) is recognised by its content - the
    /// segment chain, not just the two "PG" bytes, which also start plenty of text files.
    /// </summary>
    [Fact]
    public void IsBluRaySupByContentNeedsASegmentChain()
    {
        var sup = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_BDSUP_multi_image.sup"));
        WithTempFile(sup, path => Assert.True(FileUtil.IsBluRaySupByContent(path)));
        WithTempFile(Encoding.ASCII.GetBytes("PG-13 rated, 1" + Environment.NewLine + "00:00:01,000 --> 00:00:02,000" + Environment.NewLine + "Hi"),
            path => Assert.False(FileUtil.IsBluRaySupByContent(path)));
        WithTempFile(sup.Take(14).ToArray(), path => Assert.False(FileUtil.IsBluRaySupByContent(path)));
    }

    public static TheoryData<int> ReadAllTextSharedEncodings() => new TheoryData<int> { 0, 1, 2, 3, 4, 5 };

    private static Encoding MakeEncoding(int index) => index switch
    {
        0 => Encoding.UTF8,
        1 => new UTF8Encoding(false),
        2 => Encoding.Latin1,
        3 => Encoding.Unicode,
        4 => Encoding.BigEndianUnicode,
        _ => Encoding.UTF32,
    };

    [Theory]
    [MemberData(nameof(ReadAllTextSharedEncodings))]
    public void ReadAllTextShared_MatchesStreamReader(int encodingIndex)
    {
        var encoding = MakeEncoding(encodingIndex);
        var prefixes = new[]
        {
            new byte[0],
            new byte[] { 0xEF, 0xBB, 0xBF },
            new byte[] { 0xEF, 0xBB },
            new byte[] { 0xEF },
            new byte[] { 0xFF },
            new byte[] { 0xFF, 0xFE, 0x00 },
            new byte[] { 0xFF, 0xFE },
            new byte[] { 0xFE, 0xFF },
            new byte[] { 0xFF, 0xFE, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0xFE, 0xFF },
        };
        var payloads = new[]
        {
            new byte[0],
            new byte[] { 0x41 },
            Encoding.UTF8.GetBytes("1\r\n00:00:01,000 --> 00:00:02,000\r\nHej \u00e6\u00f8\u00e5 \u4e2d\r\n"),
            new byte[] { 0x41, 0x42, 0xC3 }, // UTF-8 sequence cut at the end
            new byte[] { 0x41, 0x00, 0x42 }, // odd length for UTF-16
            new byte[] { 0x41, 0xFF, 0x42, 0x80 }, // invalid UTF-8
        };

        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        try
        {
            foreach (var prefix in prefixes)
            {
                foreach (var payload in payloads)
                {
                    File.WriteAllBytes(fileName, prefix.Concat(payload).ToArray());
                    string expected;
                    Encoding expectedEncoding;
                    using (var reader = new StreamReader(fileName, encoding, true))
                    {
                        expected = reader.ReadToEnd();
                        expectedEncoding = reader.CurrentEncoding;
                    }

                    var actual = FileUtil.ReadAllTextShared(fileName, encoding, out var actualEncoding);

                    var label = $"{encoding.WebName} prefix {BitConverter.ToString(prefix)} payload {BitConverter.ToString(payload)}";
                    Assert.True(expected == actual, label);
                    Assert.True(expectedEncoding.CodePage == actualEncoding.CodePage, label);
                    Assert.True(expectedEncoding.GetPreamble().SequenceEqual(actualEncoding.GetPreamble()), label);
                }
            }
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
