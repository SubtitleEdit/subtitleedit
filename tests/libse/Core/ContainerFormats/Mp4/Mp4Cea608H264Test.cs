using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class Mp4Cea608H264Test
{
    [Fact]
    public void CheckForMoovVideoCea608_BFrameReordering_DecodesCharsInPresentationOrder()
    {
        // Regression: H.264 video with B-frames stores samples in decode order, but
        // CEA-608 cc_data must be processed in presentation order. Without parsing
        // ctts, byte pairs from B-frames arrive out of order and the caption text
        // is scrambled. Seen on a real file where the parser produced
        // "es [wavan rooverad]bubbhe" instead of "[waves roll and bubble overhead]".
        //
        // Test design: 9 single-NAL H.264-style samples each carrying one CEA-608
        // byte pair. Decode order swaps the two character pairs ("Hi" and "!!"),
        // so a naive parser produces "!!Hi" while a ctts-aware parser yields "Hi!!".
        var pairsInPtsOrder = new (byte a, byte b)[]
        {
            (0x14, 0x70), // PAC (row 15, col 0)
            (0x14, 0x20), // RCL — switch to pop-on
            (0x14, 0x2E), // ENM — clear non-displayed buffer
            (0x14, 0x70), // PAC — position cursor for chars
            (0x48, 0x69), // 'H' 'i'
            (0x21, 0x21), // '!' '!'
            (0x14, 0x2F), // EOC — flip memories; caption now displayed
            (0x14, 0x2C), // EDM — clear displayed; emits DisplayScreen with prior text
            (0x00, 0x00),
        };

        // Storage (decode) order: swap PTS indices 4 and 5. Storage-order chars
        // would be "!!Hi"; PTS-order chars are "Hi!!".
        var storageToPts = new[] { 0, 1, 2, 3, 5, 4, 6, 7, 8 };

        const uint timeScale = 1000;
        const uint sampleTicks = 100;

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildH264Mp4WithCea608(pairsInPtsOrder, storageToPts, sampleTicks, timeScale));

            var parser = new MP4Parser(tempFile);
            Assert.NotNull(parser.TrunCea608Subtitle);
            var paragraphs = parser.TrunCea608Subtitle.Paragraphs;
            Assert.Single(paragraphs);
            Assert.Equal("Hi!!", paragraphs[0].Text);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Build a minimal H.264-in-MP4 with one length-prefixed SEI NAL per sample,
    /// each NAL carrying a single CEA-608 cc_data byte pair (NTSC F1).
    /// The ctts box maps decode indices to presentation timestamps so storage
    /// order can be permuted relative to PTS order.
    /// </summary>
    private static byte[] BuildH264Mp4WithCea608(
        (byte a, byte b)[] pairsInPtsOrder,
        int[] storageToPts,
        uint sampleTicks,
        uint timeScale)
    {
        var sampleCount = storageToPts.Length;

        // Build one sample = [4-byte big-endian NAL length][NAL header 0x06][SEI payload]
        var sampleBytes = new byte[sampleCount][];
        for (var i = 0; i < sampleCount; i++)
        {
            var (a, b) = pairsInPtsOrder[storageToPts[i]];
            var nal = BuildSeiNalWithCcDataPair(a, b);
            var withLengthPrefix = new byte[4 + nal.Length];
            WriteUInt32Be(withLengthPrefix, 0, (uint)nal.Length);
            System.Buffer.BlockCopy(nal, 0, withLengthPrefix, 4, nal.Length);
            sampleBytes[i] = withLengthPrefix;
        }
        var sampleSizes = sampleBytes.Select(s => (uint)s.Length).ToArray();
        var mdatPayload = Concat(sampleBytes);

        // Composition offsets: ctts[i] = pts(storage_i) - dts(storage_i),
        // where dts(storage_i) is the cumulative sample duration in storage order
        // and pts(storage_i) = storageToPts[i] * sampleTicks (offset by sampleTicks
        // so all ctts values remain non-negative — letting us use ctts version 0).
        var cttsOffsets = new uint[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var dts = (uint)i * sampleTicks;
            var pts = (uint)(storageToPts[i] + 1) * sampleTicks;
            cttsOffsets[i] = pts - dts;
        }

        var ftyp = Box("ftyp",
            Ascii("isom"),
            UInt32Be(512),
            Ascii("isom"), Ascii("iso2"), Ascii("avc1"), Ascii("mp41"));

        var mdat = Box("mdat", mdatPayload);
        var sampleDataOffset = (uint)(ftyp.Length + 8); // ftyp + mdat header

        // Stub avc1 sample entry — parser only inspects the box name.
        var avc1Entry = Box("avc1");
        var stsd = Box("stsd",
            new byte[4],                            // version + flags
            UInt32Be(1),                            // entry_count
            avc1Entry);

        var stts = Box("stts",
            new byte[4],
            UInt32Be(1),
            UInt32Be((uint)sampleCount),            // sample_count
            UInt32Be(sampleTicks));                 // sample_delta

        // Encode ctts as N entries of (sample_count=1, offset=ctts[i]) to keep
        // the layout flexible; a real encoder would run-length-encode.
        var cttsEntries = new byte[8 * sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            WriteUInt32Be(cttsEntries, i * 8, 1);
            WriteUInt32Be(cttsEntries, i * 8 + 4, cttsOffsets[i]);
        }
        var ctts = Box("ctts",
            new byte[4],                            // version 0 + flags
            UInt32Be((uint)sampleCount),
            cttsEntries);

        var stsc = Box("stsc",
            new byte[4],
            UInt32Be(1),                            // 1 entry
            UInt32Be(1),                            // first_chunk
            UInt32Be((uint)sampleCount),            // samples_per_chunk
            UInt32Be(1));                           // sample_description_index

        var stsz = Box("stsz",
            new byte[4],
            UInt32Be(0),                            // per-sample sizes follow
            UInt32Be((uint)sampleCount),
            Concat(sampleSizes.Select(UInt32Be).ToArray()));

        var stco = Box("stco",
            new byte[4],
            UInt32Be(1),                            // 1 chunk
            UInt32Be(sampleDataOffset));

        var stbl = Box("stbl", stsd, stts, ctts, stsc, stsz, stco);
        var minf = Box("minf", stbl);

        var hdlr = Box("hdlr",
            new byte[4],
            new byte[4],
            Ascii("vide"),                          // handler_type = video
            new byte[12],
            new byte[] { 0 });

        var mdhd = Box("mdhd",
            new byte[4],
            new byte[4],                            // creation_time
            new byte[4],                            // modification_time
            UInt32Be(timeScale),
            UInt32Be(sampleTicks * (uint)sampleCount),
            UInt16Be(0x55C4),                       // language = "und"
            UInt16Be(0));

        var mdia = Box("mdia", hdlr, mdhd, minf);
        var trak = Box("trak", mdia);
        var moov = Box("moov", trak);

        return Concat(ftyp, mdat, moov);
    }

    /// <summary>
    /// Build an H.264 SEI NAL (nal_unit_type=6) containing a single ATSC A/53
    /// cc_data triplet for the given CEA-608 byte pair on NTSC F1 (cc_type=0).
    /// Returns the NAL body (NAL header + SEI bytes); the caller adds the
    /// 4-byte big-endian length prefix.
    /// </summary>
    private static byte[] BuildSeiNalWithCcDataPair(byte ccData1, byte ccData2)
    {
        // SEI payload (everything after the NAL header byte):
        //   payloadType         = 4  (user_data_registered_itu_t_t35)
        //   payloadSize         = 14
        //   itu_t_t35_country   = 0xB5 (USA)
        //   itu_t_t35_provider  = 0x00 0x31 (ATSC)
        //   user_identifier     = "GA94"
        //   user_data_type_code = 0x03 (cc_data)
        //   flags               = process_cc_data_flag | reserved | cc_count(1)
        //   em_data             = 0xFF
        //   cc_data triplet     = marker(valid, type=0) | cc_data_1 | cc_data_2
        //   marker_bits         = 0xFF
        var sei = new byte[]
        {
            0x04,
            14,
            0xB5, 0x00, 0x31,
            0x47, 0x41, 0x39, 0x34,
            0x03,
            0xC1,
            0xFF,
            0xFC, ccData1, ccData2,
            0xFF,
        };
        var nal = new byte[1 + sei.Length];
        nal[0] = 0x06;                              // nal_unit_type = SEI
        System.Buffer.BlockCopy(sei, 0, nal, 1, sei.Length);
        return nal;
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
