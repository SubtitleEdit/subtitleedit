using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class Mp4Cea708H264Test
{
    // Verifies CEA-708 (DTVCC) captions carried in H.264 SEI cc_data are
    // assembled, fed to the Cea708 decoder, and surfaced as paragraphs.
    //
    // Two samples are emitted with one SEI each:
    //   sample 0 (PTS 0):   cc_type=2 [0x48 'H', 0x69 'i']
    //   sample 1 (PTS 1s):  cc_type=2 [0x21 '!', 0x8A HideWindows]
    //                       cc_type=2 [0x80 window-bitmap, 0x00 padding]
    //
    // Decoder behaviour: 'H' 'i' '!' are accumulated as SetText commands
    // (no flush). 0x8A = HideWindows triggers FlushText, emitting "Hi!".
    // The 0x80 byte after HideWindows is the window bitmap (consumed by the
    // command), 0x00 is harmless C0 padding.
    //
    // Expected: a single paragraph "Hi!" spanning frame 0 → frame 1.
    [Fact]
    public void TrunCea708_AssemblesTextFromDtvccTripletsAcrossFrames()
    {
        var ccDataPerSample = new[]
        {
            // sample 0: cc_type=2 triplet carrying 'H' 'i'
            new[] { new CcTriplet(2, 0x48, 0x69) },
            // sample 1: two cc_type=2 triplets carrying '!', HideWindows, window-bitmap, padding
            new[]
            {
                new CcTriplet(2, 0x21, 0x8A),
                new CcTriplet(2, 0x80, 0x00),
            },
        };

        const uint timeScale = 1000;
        const uint sampleTicks = 1000;

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildH264Mp4WithCcData(ccDataPerSample, sampleTicks, timeScale));

            var parser = new MP4Parser(tempFile);
            Assert.NotNull(parser.TrunCea708Subtitle);
            var paragraphs = parser.TrunCea708Subtitle.Paragraphs;
            Assert.Single(paragraphs);
            Assert.Equal("Hi!", paragraphs[0].Text);
            Assert.Equal(0, paragraphs[0].StartTime.TotalMilliseconds, 1);
            Assert.Equal(1000, paragraphs[0].EndTime.TotalMilliseconds, 1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private record CcTriplet(byte CcType, byte Data1, byte Data2);

    // Build a minimal H.264-in-MP4 with one SEI per sample, each SEI carrying
    // N cc_data triplets (arbitrary cc_type — 2/3 for CEA-708, 0/1 for CEA-608).
    private static byte[] BuildH264Mp4WithCcData(CcTriplet[][] ccDataPerSample, uint sampleTicks, uint timeScale)
    {
        var sampleCount = ccDataPerSample.Length;

        var sampleBytes = new byte[sampleCount][];
        for (var i = 0; i < sampleCount; i++)
        {
            var nal = BuildSeiNalWithCcData(ccDataPerSample[i]);
            var withLengthPrefix = new byte[4 + nal.Length];
            WriteUInt32Be(withLengthPrefix, 0, (uint)nal.Length);
            System.Buffer.BlockCopy(nal, 0, withLengthPrefix, 4, nal.Length);
            sampleBytes[i] = withLengthPrefix;
        }
        var sampleSizes = sampleBytes.Select(s => (uint)s.Length).ToArray();
        var mdatPayload = Concat(sampleBytes);

        var ftyp = Box("ftyp",
            Ascii("isom"),
            UInt32Be(512),
            Ascii("isom"), Ascii("iso2"), Ascii("avc1"), Ascii("mp41"));

        var mdat = Box("mdat", mdatPayload);
        var sampleDataOffset = (uint)(ftyp.Length + 8);

        var avc1Entry = Box("avc1");
        var stsd = Box("stsd",
            new byte[4],
            UInt32Be(1),
            avc1Entry);

        var stts = Box("stts",
            new byte[4],
            UInt32Be(1),
            UInt32Be((uint)sampleCount),
            UInt32Be(sampleTicks));

        var stsc = Box("stsc",
            new byte[4],
            UInt32Be(1),
            UInt32Be(1),
            UInt32Be((uint)sampleCount),
            UInt32Be(1));

        var stsz = Box("stsz",
            new byte[4],
            UInt32Be(0),
            UInt32Be((uint)sampleCount),
            Concat(sampleSizes.Select(UInt32Be).ToArray()));

        var stco = Box("stco",
            new byte[4],
            UInt32Be(1),
            UInt32Be(sampleDataOffset));

        var stbl = Box("stbl", stsd, stts, stsc, stsz, stco);
        var minf = Box("minf", stbl);

        var hdlr = Box("hdlr",
            new byte[4],
            new byte[4],
            Ascii("vide"),
            new byte[12],
            new byte[] { 0 });

        var mdhd = Box("mdhd",
            new byte[4],
            new byte[4],
            new byte[4],
            UInt32Be(timeScale),
            UInt32Be(sampleTicks * (uint)sampleCount),
            UInt16Be(0x55C4),
            UInt16Be(0));

        var mdia = Box("mdia", hdlr, mdhd, minf);
        var trak = Box("trak", mdia);
        var moov = Box("moov", trak);

        return Concat(ftyp, mdat, moov);
    }

    // Build an H.264 SEI NAL (nal_unit_type=6) containing an ATSC A/53
    // user_data_registered_itu_t_t35 payload with N cc_data triplets.
    // Each triplet is: marker byte (cc_valid=1, cc_type=N) + cc_data_1 + cc_data_2.
    private static byte[] BuildSeiNalWithCcData(CcTriplet[] triplets)
    {
        var ccCount = triplets.Length;

        var payload = new System.Collections.Generic.List<byte>
        {
            // SEI payload header
            0xB5, 0x00, 0x31,                       // itu_t_t35 country + provider (USA + ATSC)
            0x47, 0x41, 0x39, 0x34,                 // user_identifier = "GA94"
            0x03,                                   // user_data_type_code = cc_data
            (byte)(0xC0 | (ccCount & 0x1F)),        // flags: process_cc_data_flag=1, reserved=1, cc_count
            0xFF,                                   // em_data
        };
        foreach (var t in triplets)
        {
            payload.Add((byte)(0xFC | (t.CcType & 0x03)));  // marker bits + cc_valid=1 + cc_type
            payload.Add(t.Data1);
            payload.Add(t.Data2);
        }
        payload.Add(0xFF);                          // marker_bits

        var sei = new System.Collections.Generic.List<byte>
        {
            0x04,                                   // payloadType = user_data_registered_itu_t_t35
            (byte)payload.Count,                    // payloadSize
        };
        sei.AddRange(payload);

        var nal = new byte[1 + sei.Count];
        nal[0] = 0x06;                              // nal_unit_type = SEI
        for (var i = 0; i < sei.Count; i++)
        {
            nal[1 + i] = sei[i];
        }
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
