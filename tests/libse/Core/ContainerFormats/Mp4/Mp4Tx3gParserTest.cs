using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class Mp4Tx3gParserTest
{
    [Fact]
    public void GetParagraphs_MultipleTextSamplesInSingleChunk_ReturnsEverySample()
    {
        // Regression test: when a tx3g track packs several text samples
        // into one chunk (stsc = [(1, N, 1)]), GetParagraphs used to
        // allocate a single Paragraph per chunk and let later samples
        // overwrite earlier ones. Only the last non-empty sample survived.
        var samples = new[] { "First line", "Second line", "Third line" };
        const uint timeScale = 1000;
        const uint sampleDurationTicks = 1000;

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildSingleChunkTx3gMp4(samples, sampleDurationTicks, timeScale));

            var parser = new MP4Parser(tempFile);
            var tracks = parser.GetSubtitleTracks();
            Assert.Single(tracks);

            var paragraphs = tracks[0].Mdia.Minf.Stbl.GetParagraphs();
            Assert.Equal(samples.Length, paragraphs.Count);
            for (var i = 0; i < samples.Length; i++)
            {
                Assert.Equal(samples[i], paragraphs[i].Text);
                Assert.Equal(i * 1000.0, paragraphs[i].StartTime.TotalMilliseconds, 1);
                Assert.Equal((i + 1) * 1000.0, paragraphs[i].EndTime.TotalMilliseconds, 1);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Build a minimal MP4 with a single sbtl/tx3g track whose samples are
    /// packed into one chunk — the layout that triggered the regression.
    /// </summary>
    private static byte[] BuildSingleChunkTx3gMp4(string[] samples, uint sampleDurationTicks, uint timeScale)
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
        var sampleSizes = sampleBytes.Select(b => (uint)b.Length).ToArray();
        var mdatPayload = Concat(sampleBytes);

        var ftyp = Box("ftyp",
            Ascii("isom"),
            UInt32Be(512),
            Ascii("isom"), Ascii("iso2"), Ascii("mp41"));

        var mdat = Box("mdat", mdatPayload);
        var sampleDataOffset = (uint)(ftyp.Length + 8); // ftyp + mdat header

        // tx3g sample entry — parser only reads the box size+name, so an empty body is fine.
        var tx3gEntry = Box("tx3g");
        var stsd = Box("stsd",
            new byte[4],                         // version + flags
            UInt32Be(1),                         // entry_count
            tx3gEntry);

        var stts = Box("stts",
            new byte[4],
            UInt32Be(1),                         // 1 run
            UInt32Be((uint)samples.Length),      // sample_count
            UInt32Be(sampleDurationTicks));      // sample_delta

        var stsc = Box("stsc",
            new byte[4],
            UInt32Be(1),                         // 1 entry
            UInt32Be(1),                         // first_chunk
            UInt32Be((uint)samples.Length),      // samples_per_chunk
            UInt32Be(1));                        // sample_description_index

        var stsz = Box("stsz",
            new byte[4],
            UInt32Be(0),                         // sample_size = 0 => per-sample sizes follow
            UInt32Be((uint)samples.Length),
            Concat(sampleSizes.Select(UInt32Be).ToArray()));

        var stco = Box("stco",
            new byte[4],
            UInt32Be(1),                         // 1 chunk
            UInt32Be(sampleDataOffset));

        var stbl = Box("stbl", stsd, stts, stsc, stsz, stco);
        var minf = Box("minf", stbl);

        var hdlr = Box("hdlr",
            new byte[4],
            new byte[4],                         // pre_defined
            Ascii("sbtl"),
            new byte[12],                        // reserved
            new byte[] { 0 });                   // empty UTF-8 name

        var mdhd = Box("mdhd",
            new byte[4],                         // version + flags (v0)
            new byte[4],                         // creation_time
            new byte[4],                         // modification_time
            UInt32Be(timeScale),
            UInt32Be(sampleDurationTicks * (uint)samples.Length),
            UInt16Be(0x55C4),                    // language = "und"
            UInt16Be(0));                        // pre_defined

        var mdia = Box("mdia", hdlr, mdhd, minf);
        var trak = Box("trak", mdia);
        var moov = Box("moov", trak);

        return Concat(ftyp, mdat, moov);
    }

    private static byte[] Box(string name, params byte[][] parts)
    {
        var total = 8;
        foreach (var p in parts) total += p.Length;
        var box = new byte[total];
        WriteUInt32Be(box, 0, (uint)total);
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

    private static void WriteUInt32Be(byte[] dst, int off, uint v)
    {
        dst[off] = (byte)(v >> 24);
        dst[off + 1] = (byte)(v >> 16);
        dst[off + 2] = (byte)(v >> 8);
        dst[off + 3] = (byte)v;
    }
}
