using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibSETests.ContainerFormats;

public class MxfEssenceScanTest
{
    private static readonly byte[] HeaderPartitionPackKey = { 0x06, 0x0E, 0x2B, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x01, 0x02, 0x04, 0x00 };

    // generic container essence element keys: item type 0x15 = picture, 0x17 = data (e.g. SMPTE 429-5 timed text)
    private static readonly byte[] PictureElementKey = { 0x06, 0x0E, 0x2B, 0x34, 0x01, 0x02, 0x01, 0x01, 0x0D, 0x01, 0x03, 0x01, 0x15, 0x01, 0x05, 0x01 };
    private static readonly byte[] DataElementKey = { 0x06, 0x0E, 0x2B, 0x34, 0x01, 0x02, 0x01, 0x01, 0x0D, 0x01, 0x03, 0x01, 0x17, 0x01, 0x0B, 0x01 };

    private const string Srt = "1\n00:00:01,000 --> 00:00:02,000\nHello\n\n2\n00:00:03,000 --> 00:00:04,000\nWorld\n";

    /// <summary>
    /// Every picture and sound frame under 500 KB used to be read and decoded as text in search of
    /// subtitles - opening an hour of 50 Mbit/s broadcast MXF took minutes. Picture/sound elements
    /// are now skipped without reading them; subtitles in data elements are still found.
    /// </summary>
    [Fact]
    public void SkipsPictureAndSoundElementsWithoutReadingThem()
    {
        // a picture frame whose bytes happen to look like a subtitle - it must not be picked up either
        var picture = Encoding.UTF8.GetBytes(Srt).Concat(Enumerable.Repeat((byte)' ', 400_000)).ToArray();
        var file = new List<byte>();
        AddKlv(file, HeaderPartitionPackKey, new byte[88]);
        AddKlv(file, PictureElementKey, picture);
        AddKlv(file, DataElementKey, Encoding.UTF8.GetBytes(Srt));

        var stream = new CountingStream(file.ToArray());
        var parser = new MxfParser(stream);

        Assert.True(parser.IsValid);
        var subtitle = Assert.Single(parser.GetSubtitles());
        Assert.Contains("World", subtitle);
        Assert.True(stream.BytesRead < 100_000, $"read {stream.BytesRead} bytes - the picture element was read");
    }

    private static void AddKlv(List<byte> file, byte[] key, byte[] value)
    {
        file.AddRange(key);
        file.Add(0x83); // 3-byte BER length
        file.Add((byte)(value.Length >> 16));
        file.Add((byte)(value.Length >> 8));
        file.Add((byte)value.Length);
        file.AddRange(value);
    }

    private sealed class CountingStream : MemoryStream
    {
        public CountingStream(byte[] buffer) : base(buffer)
        {
        }

        public long BytesRead { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = base.Read(buffer, offset, count);
            BytesRead += read;
            return read;
        }

        public override int ReadByte()
        {
            var b = base.ReadByte();
            if (b >= 0)
            {
                BytesRead++;
            }

            return b;
        }
    }
}
