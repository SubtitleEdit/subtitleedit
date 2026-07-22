using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

/// <summary>
/// Sample-table edge cases: a uniform stsz (no entry table) and a malformed stsc
/// with a repeated first_chunk. Both used to lose the whole track.
/// </summary>
public class Mp4StblMalformedTest
{
    // stsz carries a single sample_size for every sample when that field is non-zero,
    // and then no entry table follows (ISO/IEC 14496-12 8.7.3.2). The parser used to
    // read an entry table regardless, walking past the end of the box and leaving
    // SampleSizes empty - so the track produced no subtitles at all.
    [Fact]
    public void GetParagraphs_UniformStszSampleSize_ReturnsEverySample()
    {
        // Equal-length texts so a single uniform sample size is valid: 2-byte
        // length prefix + 3 bytes of UTF-8 = 5 bytes per sample.
        var samples = new[] { "AAA", "BBB", "CCC" };

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildTx3gMp4(samples, sampleDurationTicks: 1000, timeScale: 1000, uniformSampleSize: 5));

            var parser = new MP4Parser(tempFile);
            var tracks = parser.GetSubtitleTracks();
            Assert.Single(tracks);

            var paragraphs = tracks[0].Mdia.Minf.Stbl.GetParagraphs();
            Assert.Equal(samples, paragraphs.Select(p => p.Text).ToArray());
            Assert.Equal(0, paragraphs[0].StartTime.TotalMilliseconds, 1);
            Assert.Equal(1000, paragraphs[0].EndTime.TotalMilliseconds, 1);
            Assert.Equal(2000, paragraphs[2].StartTime.TotalMilliseconds, 1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // 14496-12 8.7.4 requires stsc first_chunk to increase, but files with a repeated
    // value exist. Building the lookup with ToDictionary threw ArgumentException on the
    // duplicate, and the exception escaped the Stbl constructor - taking down the parse
    // of the entire file, not just the one bad track.
    [Fact]
    public void GetParagraphs_DuplicateStscFirstChunk_DoesNotThrowAndKeepsFirstEntry()
    {
        var samples = new[] { "First", "Second", "Third" };

        var tempFile = Path.GetTempFileName();
        try
        {
            // Two entries both claiming first_chunk = 1. The first says all three
            // samples live in chunk 1; the duplicate contradicts it with 1.
            File.WriteAllBytes(tempFile, BuildTx3gMp4(
                samples,
                sampleDurationTicks: 1000,
                timeScale: 1000,
                stscEntries: new[] { (first: 1u, perChunk: 3u), (first: 1u, perChunk: 1u) }));

            var parser = new MP4Parser(tempFile);
            var tracks = parser.GetSubtitleTracks();
            Assert.Single(tracks);

            // First entry wins, so all three samples in the single chunk are read.
            var paragraphs = tracks[0].Mdia.Minf.Stbl.GetParagraphs();
            Assert.Equal(samples, paragraphs.Select(p => p.Text).ToArray());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Minimal single-chunk tx3g MP4. <paramref name="uniformSampleSize"/> non-zero writes
    /// stsz in uniform form (no entry table); <paramref name="stscEntries"/> overrides the
    /// default single sample-to-chunk entry.
    /// </summary>
    private static byte[] BuildTx3gMp4(
        string[] samples,
        uint sampleDurationTicks,
        uint timeScale,
        uint uniformSampleSize = 0,
        (uint first, uint perChunk)[]? stscEntries = null)
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
        var mdatPayload = Concat(sampleBytes);

        var ftyp = Box("ftyp",
            Ascii("isom"),
            UInt32Be(512),
            Ascii("isom"), Ascii("iso2"), Ascii("mp41"));

        var mdat = Box("mdat", mdatPayload);
        var sampleDataOffset = (uint)(ftyp.Length + 8); // ftyp + mdat header

        var tx3gEntry = Box("tx3g");
        var stsd = Box("stsd",
            new byte[4],
            UInt32Be(1),
            tx3gEntry);

        var stts = Box("stts",
            new byte[4],
            UInt32Be(1),
            UInt32Be((uint)samples.Length),
            UInt32Be(sampleDurationTicks));

        stscEntries ??= new[] { (first: 1u, perChunk: (uint)samples.Length) };
        var stsc = Box("stsc",
            new byte[4],
            UInt32Be((uint)stscEntries.Length),
            Concat(stscEntries
                .Select(e => Concat(UInt32Be(e.first), UInt32Be(e.perChunk), UInt32Be(1)))
                .ToArray()));

        // sample_size non-zero => uniform, and the entry table is omitted entirely.
        var stsz = uniformSampleSize != 0
            ? Box("stsz",
                new byte[4],
                UInt32Be(uniformSampleSize),
                UInt32Be((uint)samples.Length))
            : Box("stsz",
                new byte[4],
                UInt32Be(0),
                UInt32Be((uint)samples.Length),
                Concat(sampleBytes.Select(b => UInt32Be((uint)b.Length)).ToArray()));

        var stco = Box("stco",
            new byte[4],
            UInt32Be(1),
            UInt32Be(sampleDataOffset));

        var stbl = Box("stbl", stsd, stts, stsc, stsz, stco);
        var minf = Box("minf", stbl);

        var hdlr = Box("hdlr",
            new byte[4],
            new byte[4],
            Ascii("sbtl"),
            new byte[12],
            new byte[] { 0 });

        var mdhd = Box("mdhd",
            new byte[4],
            new byte[4],
            new byte[4],
            UInt32Be(timeScale),
            UInt32Be(sampleDurationTicks * (uint)samples.Length),
            UInt16Be(0x55C4),
            UInt16Be(0));

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
