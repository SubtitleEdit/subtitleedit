using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System.IO;
using System.Linq;
using System.Text;

namespace LibSETests.ContainerFormats;

/// <summary>
/// "stz2" holds the same sample sizes as "stsz" packed into 4, 8 or 16 bits each
/// (ISO/IEC 14496-12 8.7.3.3). Bento4's mp4compact rewrites files this way; without a
/// reader for the box the track came out with no sample sizes, and so no subtitles.
/// </summary>
public class Mp4CompactSampleSizeTest
{
    [Fact]
    public void CompactedFileReadsLikeTheOriginal()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MP4_stz2_compact.mp4");

        var parser = new MP4Parser(path);
        var track = Assert.Single(parser.GetSubtitleTracks());
        var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();

        Assert.Equal(5, paragraphs.Count);
        Assert.Equal("Line one plain", paragraphs[0].Text);
        Assert.Equal(1000, paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(2500, paragraphs[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal("日本語のテスト", paragraphs[4].Text);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(8)]
    [InlineData(4)]
    public void EveryFieldSizeIsRead(byte fieldSize)
    {
        // 4-bit fields only reach 15, so keep the samples short enough for every width
        var samples = new[] { "ab", "cd", "ef" };

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildStz2Mp4(samples, fieldSize));
            var parser = new MP4Parser(tempFile);
            var paragraphs = parser.GetSubtitleTracks()[0].Mdia.Minf.Stbl.GetParagraphs();

            Assert.Equal(samples.Length, paragraphs.Count);
            for (var i = 0; i < samples.Length; i++)
            {
                Assert.Equal(samples[i], paragraphs[i].Text);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static byte[] BuildStz2Mp4(string[] samples, byte fieldSize)
    {
        var sampleBytes = samples.Select(s =>
        {
            var text = Encoding.UTF8.GetBytes(s);
            var buf = new byte[2 + text.Length];
            buf[0] = (byte)(text.Length >> 8);
            buf[1] = (byte)text.Length;
            System.Buffer.BlockCopy(text, 0, buf, 2, text.Length);
            return buf;
        }).ToArray();

        var ftyp = Box("ftyp", Ascii("isom"), UInt32Be(512), Ascii("isom"), Ascii("iso2"), Ascii("mp41"));
        var mdat = Box("mdat", Concat(sampleBytes));
        var sampleDataOffset = (uint)(ftyp.Length + 8);

        var stsd = Box("stsd", new byte[4], UInt32Be(1), Box("tx3g"));
        var stts = Box("stts", new byte[4], UInt32Be(1), UInt32Be((uint)samples.Length), UInt32Be(1000));
        var stsc = Box("stsc", new byte[4], UInt32Be(1), UInt32Be(1), UInt32Be((uint)samples.Length), UInt32Be(1));
        var stco = Box("stco", new byte[4], UInt32Be(1), UInt32Be(sampleDataOffset));

        var sizes = sampleBytes.Select(b => b.Length).ToArray();
        byte[] entries;
        switch (fieldSize)
        {
            case 4:
                entries = new byte[(sizes.Length + 1) / 2];
                for (var i = 0; i < sizes.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        entries[i / 2] |= (byte)(sizes[i] << 4);
                    }
                    else
                    {
                        entries[i / 2] |= (byte)(sizes[i] & 0x0F);
                    }
                }

                break;
            case 8:
                entries = sizes.Select(s => (byte)s).ToArray();
                break;
            default:
                entries = Concat(sizes.Select(s => UInt16Be((ushort)s)).ToArray());
                break;
        }

        var stz2 = Box("stz2",
            new byte[4],                            // version + flags
            new byte[3],                            // reserved
            new[] { fieldSize },
            UInt32Be((uint)samples.Length),
            entries);

        var minf = Box("minf", Box("stbl", stsd, stts, stsc, stz2, stco));
        var hdlr = Box("hdlr", new byte[4], new byte[4], Ascii("sbtl"), new byte[12], new byte[] { 0 });
        var mdhd = Box("mdhd", new byte[4], new byte[4], new byte[4], UInt32Be(1000),
            UInt32Be(1000 * (uint)samples.Length), UInt16Be(0x55C4), UInt16Be(0));

        return Concat(ftyp, mdat, Box("moov", Box("trak", Box("mdia", hdlr, mdhd, minf))));
    }

    private static byte[] Box(string name, params byte[][] parts)
    {
        var total = 8;
        foreach (var p in parts) total += p.Length;
        var box = new byte[total];
        box[0] = (byte)(total >> 24);
        box[1] = (byte)(total >> 16);
        box[2] = (byte)(total >> 8);
        box[3] = (byte)total;
        Encoding.ASCII.GetBytes(name, 0, 4, box, 4);
        var off = 8;
        foreach (var p in parts)
        {
            System.Buffer.BlockCopy(p, 0, box, off, p.Length);
            off += p.Length;
        }

        return box;
    }

    private static byte[] Concat(params byte[][] parts)
    {
        var total = 0;
        foreach (var p in parts) total += p.Length;
        var result = new byte[total];
        var off = 0;
        foreach (var p in parts)
        {
            System.Buffer.BlockCopy(p, 0, result, off, p.Length);
            off += p.Length;
        }

        return result;
    }

    private static byte[] Ascii(string s) => Encoding.ASCII.GetBytes(s);

    private static byte[] UInt32Be(uint v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };

    private static byte[] UInt16Be(ushort v) => new[] { (byte)(v >> 8), (byte)v };
}
