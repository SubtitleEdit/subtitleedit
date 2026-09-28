using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class Mp4Cea708H264Test
{
    // Verifies CEA-708 (DTVCC) captions carried in H.264 SEI cc_data are
    // assembled into DTVCC packets, the primary caption service (1) is
    // extracted, and the bytes are decoded into paragraphs by Cea708.Decode.
    //
    // Two DTVCC packets, one per sample:
    //   sample 0 (PTS 0):     packet seq=0 size=2 → service-1 block_size=2
    //                         data: 'H' 'i'
    //   sample 1 (PTS 1s):    packet seq=1 size=3 → service-1 block_size=3
    //                         data: '!' 0x8A(HideWindows) 0x80(window-bitmap)
    //                         + 1 NULL-pad byte
    //
    // Decoder behaviour: 'H' 'i' '!' accumulate as SetText commands; HideWindows
    // triggers FlushText emitting "Hi!" and timestamps it from packet-0's PTS
    // (where state.StartLineIndex points) to packet-1's PTS.
    //
    // Expected: a single paragraph "Hi!" spanning 0 → 1000 ms.
    [Fact]
    public void TrunCea708_AssemblesTextFromDtvccTripletsAcrossFrames()
    {
        AssertHiFromSampleEntry("avc1", nalLengthSize: 4);
    }

    /// <summary>
    /// H.265 SEI (prefix SEI NAL, type 39, 2-byte NAL header) in an "hvc1" track - it used to be
    /// skipped, as only the H.264 SEI NAL type was recognized.
    /// </summary>
    [Theory]
    [InlineData("hvc1")]
    [InlineData("hev1")]
    public void Cea708FromHevcSei(string sampleEntry)
    {
        AssertHiFromSampleEntry(sampleEntry, nalLengthSize: 4);
    }

    /// <summary>
    /// The NAL unit length prefix size comes from avcC/hvcC (lengthSizeMinusOne) - it is not always 4.
    /// </summary>
    [Theory]
    [InlineData("avc1", 2)]
    [InlineData("hvc1", 2)]
    [InlineData("avc1", 1)]
    public void Cea708WithShortNalLengthPrefix(string sampleEntry, int nalLengthSize)
    {
        AssertHiFromSampleEntry(sampleEntry, nalLengthSize);
    }

    private static void AssertHiFromSampleEntry(string sampleEntry, int nalLengthSize)
    {
        var ccDataPerSample = new[]
        {
            // sample 0 → packet seq=0, size_code=2 (4 bytes total), service 1, block_size=2
            new[]
            {
                new CcTriplet(3, 0x02, 0x22),  // packet header + service block header
                new CcTriplet(2, 0x48, 0x69),  // service data 'H' 'i'
            },
            // sample 1 → packet seq=1, size_code=3 (6 bytes total), service 1, block_size=3
            new[]
            {
                new CcTriplet(3, 0x43, 0x23),  // packet header + service block header
                new CcTriplet(2, 0x21, 0x8A),  // service data '!' + HideWindows command
                new CcTriplet(2, 0x80, 0x00),  // window bitmap (HideWindows arg) + NULL pad
            },
        };

        const uint timeScale = 1000;
        const uint sampleTicks = 1000;

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildMp4WithCcData(ccDataPerSample, sampleTicks, timeScale, sampleEntry, nalLengthSize));

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

    // Build a minimal H.264 ("avc1") or H.265 ("hvc1"/"hev1") MP4 with one SEI per sample, each
    // SEI carrying N cc_data triplets (arbitrary cc_type — 2/3 for CEA-708, 0/1 for CEA-608), NAL
    // units prefixed with a nalLengthSize-byte length as declared in the avcC/hvcC configuration.
    private static byte[] BuildMp4WithCcData(CcTriplet[][] ccDataPerSample, uint sampleTicks, uint timeScale, string sampleEntry = "avc1", int nalLengthSize = 4)
    {
        var sampleCount = ccDataPerSample.Length;
        var isHevc = sampleEntry != "avc1";

        var sampleBytes = new byte[sampleCount][];
        for (var i = 0; i < sampleCount; i++)
        {
            var nal = BuildSeiNalWithCcData(ccDataPerSample[i], isHevc);
            var withLengthPrefix = new byte[nalLengthSize + nal.Length];
            for (var b = 0; b < nalLengthSize; b++)
            {
                withLengthPrefix[b] = (byte)(nal.Length >> (8 * (nalLengthSize - 1 - b)));
            }

            System.Buffer.BlockCopy(nal, 0, withLengthPrefix, nalLengthSize, nal.Length);
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

        // visual sample entry (78 bytes) + decoder configuration; only lengthSizeMinusOne is read:
        // avcC byte 4, hvcC byte 21 (low 2 bits)
        var configuration = new byte[isHevc ? 23 : 7];
        configuration[isHevc ? 21 : 4] = (byte)(0xFC | (nalLengthSize - 1));
        var videoEntry = Box(sampleEntry, new byte[78], Box(isHevc ? "hvcC" : "avcC", configuration));
        var stsd = Box("stsd",
            new byte[4],
            UInt32Be(1),
            videoEntry);

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

    // Build an H.264 SEI NAL (nal_unit_type=6) or H.265 prefix SEI NAL (nal_unit_type=39, 2-byte
    // header) containing an ATSC A/53 user_data_registered_itu_t_t35 payload with N cc_data triplets.
    // Each triplet is: marker byte (cc_valid=1, cc_type=N) + cc_data_1 + cc_data_2.
    private static byte[] BuildSeiNalWithCcData(CcTriplet[] triplets, bool isHevc)
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

        var header = isHevc
            ? new byte[] { 39 << 1, 0x01 }          // nal_unit_type = PREFIX_SEI, nuh_temporal_id_plus1 = 1
            : new byte[] { 0x06 };                  // nal_unit_type = SEI
        return Concat(header, sei.ToArray());
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
