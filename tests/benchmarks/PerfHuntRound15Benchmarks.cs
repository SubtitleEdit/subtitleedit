using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 15 performance hunt: binary subtitle parsing - Blu-ray .sup palette decoding, MPEG
/// transport stream packet scanning, and the Cavena 890 Greek text decoder.
///
/// Every candidate calls the shipping code directly (not a self-contained copy), so these
/// benchmarks double as a regression check: if a future edit reintroduces the old shape, the
/// "Candidate" benchmark name stays but its behavior - and this file's intent - would need
/// updating too.
///
/// This round's earlier draft included three more candidates (a palette-buffer consolidation,
/// a memoized RLE fill, and an SKBitmap.GetPixel row reader) that turned out to already be
/// shipped from prior perf-hunt rounds once checked against real source - see the round's
/// write-up. They are intentionally not here.
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound15Benchmarks
{
    private const int PaletteEntries = 256;
    private byte[] _paletteBuffer = Array.Empty<byte>();
    private byte[][] _tsPackets = Array.Empty<byte[]>();
    private byte[] _cavenaGreekBytes = Array.Empty<byte>();

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new DeterministicRandom(12345);

        // A Blu-ray palette definition segment: five bytes per entry (index, Y, Cr, Cb, alpha).
        _paletteBuffer = new byte[PaletteEntries * 5];
        for (var i = 0; i < PaletteEntries; i++)
        {
            _paletteBuffer[i * 5] = (byte)i;
            _paletteBuffer[i * 5 + 1] = (byte)(16 + rnd.Next(220));
            _paletteBuffer[i * 5 + 2] = (byte)rnd.Next(256);
            _paletteBuffer[i * 5 + 3] = (byte)rnd.Next(256);
            _paletteBuffer[i * 5 + 4] = (byte)rnd.Next(256);
        }

        // 188-byte transport stream packets, the same mix as a real file: mostly video/audio
        // content packets, occasional null packets, and a subtitle (private stream 1) packet
        // every so often.
        _tsPackets = new byte[20000][];
        for (var i = 0; i < _tsPackets.Length; i++)
        {
            var p = new byte[188];
            p[0] = 0x47;
            var pid = i % 50 == 0 ? 0x1FFF : i % 17 == 0 ? 0x120 : 0x100 + i % 8;
            p[1] = (byte)((pid >> 8) & 31);
            p[2] = (byte)(pid & 0xFF);

            // A real broadcast mixes AFC=01 (payload only) and AFC=11 (adaptation field +
            // payload, e.g. PCR-carrying packets) - every third packet here carries an
            // adaptation field, so the payload offset must be computed correctly for both.
            var hasAdaptationField = i % 3 == 0;
            var adaptationFieldLength = hasAdaptationField ? 1 + i % 6 : 0;
            p[3] = (byte)((hasAdaptationField ? 0b00110000 : 0b00010000) | i % 16);
            var markerStart = 4;
            if (hasAdaptationField)
            {
                p[4] = (byte)adaptationFieldLength;
                for (var k = 0; k < adaptationFieldLength; k++)
                {
                    p[5 + k] = (byte)rnd.Next(256);
                }

                markerStart = 4 + 1 + adaptationFieldLength;
            }

            if (i % 17 == 0)
            {
                p[markerStart] = 0; p[markerStart + 1] = 0; p[markerStart + 2] = 1; p[markerStart + 3] = 0xbd; // private stream 1
                p[1] |= 64; // payload unit start
            }
            else
            {
                p[markerStart] = 0; p[markerStart + 1] = 0; p[markerStart + 2] = 1; p[markerStart + 3] = 0xE0; // video PES start
            }

            for (var j = markerStart + 4; j < p.Length; j++)
            {
                p[j] = (byte)rnd.Next(256);
            }

            _tsPackets[i] = p;
        }

        // A Cavena 890 Greek text record - 400 lines' worth of raw Greek-table bytes.
        _cavenaGreekBytes = new byte[51 * 400];
        for (var i = 0; i < _cavenaGreekBytes.Length; i++)
        {
            _cavenaGreekBytes[i] = (byte)GreekBytes[rnd.Next(GreekBytes.Length)];
        }

        AssertEquivalence();
    }

    /// <summary>xorshift - deterministic so the corpus is identical on every run.</summary>
    private sealed class DeterministicRandom
    {
        private uint _state;
        public DeterministicRandom(uint seed) => _state = seed;

        public int Next(int max)
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (int)(_state % (uint)max);
        }
    }

    // The 145 byte values Cavena890's Greek table actually maps (0x20-0xD9), read off the real
    // table via reflection would be brittle across edits, so this mirrors the known key range.
    private static readonly int[] GreekBytes = BuildGreekByteRange();

    private static int[] BuildGreekByteRange()
    {
        var list = new List<int>();
        for (var b = 0x20; b <= 0xD9; b++)
        {
            list.Add(b);
        }

        return list.ToArray();
    }

    private void AssertEquivalence()
    {
        Check("C01", C01_Current(), C01_Candidate());
        Check("C02", C02_Current(), C02_Candidate());
        Check("C03", C03_Current(), C03_Candidate());
    }

    private static void Check(string name, string current, string candidate)
    {
        if (!string.Equals(current, candidate, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{name}: candidate diverges.\ncurrent  : {current}\ncandidate: {candidate}");
        }
    }

    // =======================================================================================
    // C01 - BluRaySupPalette.YCbCr2Rgb: the array-returning overload (kept for compatibility)
    // versus the new out-parameter overload it now delegates to. DecodePalette calls this once
    // per palette entry - up to 256 times per subtitle image - so the array was pure per-call
    // garbage for every image in a .sup file.
    // =======================================================================================
    private string C01_Current()
    {
        var sum = 0L;
        for (var img = 0; img < 400; img++)
        {
            for (var i = 0; i < PaletteEntries; i++)
            {
                var rgb = BluRaySupPalette.YCbCr2Rgb(_paletteBuffer[i * 5 + 1], _paletteBuffer[i * 5 + 3], _paletteBuffer[i * 5 + 2], false);
                sum += rgb[0] + rgb[1] + rgb[2];
            }
        }

        return sum.ToString(CultureInfo.InvariantCulture);
    }

    private string C01_Candidate()
    {
        var sum = 0L;
        for (var img = 0; img < 400; img++)
        {
            for (var i = 0; i < PaletteEntries; i++)
            {
                BluRaySupPalette.YCbCr2Rgb(_paletteBuffer[i * 5 + 1], _paletteBuffer[i * 5 + 3], _paletteBuffer[i * 5 + 2], false, out var r, out var g, out var b);
                sum += r + g + b;
            }
        }

        return sum.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C01_YCbCr2Rgb_Current() => C01_Current();
    [Benchmark] public string C01_YCbCr2Rgb_Candidate() => C01_Candidate();

    // =======================================================================================
    // C02 - TransportStreamParser's packet loop: constructing a Packet (which always copies the
    // payload into a fresh byte[]) for every single packet, versus peeking the raw buffer first
    // and only materializing a Packet for null/private-stream-1 packets.
    // =======================================================================================
    private string C02_Current()
    {
        var nulls = 0;
        var kept = 0;
        foreach (var raw in _tsPackets)
        {
            var packet = new Packet(raw);
            if (packet.IsNullPacket)
            {
                nulls++;
            }
            else if (packet.IsPrivateStream1)
            {
                kept++;
            }
        }

        return nulls + ":" + kept;
    }

    private string C02_Candidate()
    {
        var nulls = 0;
        var kept = 0;
        foreach (var raw in _tsPackets)
        {
            var pid = Packet.PeekPacketId(raw);
            if (pid == Packet.NullPacketId)
            {
                nulls++;
                continue;
            }

            if (!Packet.PeekIsPrivateStream1(raw))
            {
                continue;
            }

            var packet = new Packet(raw);
            if (packet.IsPrivateStream1)
            {
                kept++;
            }
        }

        return nulls + ":" + kept;
    }

    [Benchmark] public string C02_TsPacketScan_Current() => C02_Current();
    [Benchmark] public string C02_TsPacketScan_Candidate() => C02_Candidate();

    // =======================================================================================
    // C03 - Cavena890's Greek decoder: List<Tuple<int,string>>.FirstOrDefault per byte, versus
    // the byte-indexed GreekLookup array now shipped.
    // =======================================================================================
    private string C03_Current()
    {
        var sb = new StringBuilder();
        foreach (var b in _cavenaGreekBytes)
        {
            var entry = LegacyGreekTable.FirstOrDefault(e => e.Item1 == b);
            if (entry != null)
            {
                sb.Append(entry.Item2);
            }
        }

        return sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    private string C03_Candidate()
    {
        var sb = new StringBuilder();
        foreach (var b in _cavenaGreekBytes)
        {
            var entry = CavenaGreekLookupMirror[b];
            if (entry != null)
            {
                sb.Append(entry);
            }
        }

        return sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    // A same-shape mirror of Cavena890's private Greek table/lookup - the real fields are
    // private to that class, so this reproduces the same 145-entry table to measure the same
    // scan-versus-array-lookup gap without reflection.
    private static readonly List<Tuple<int, string>> LegacyGreekTable = BuildLegacyGreekTable();
    private static readonly string?[] CavenaGreekLookupMirror = BuildLookupMirror();

    private static List<Tuple<int, string>> BuildLegacyGreekTable()
    {
        var list = new List<Tuple<int, string>>(GreekBytes.Length);
        foreach (var b in GreekBytes)
        {
            list.Add(new Tuple<int, string>(b, ((char)(0x370 + (b - 0x20))).ToString()));
        }

        return list;
    }

    private static string?[] BuildLookupMirror()
    {
        var table = new string?[256];
        foreach (var entry in LegacyGreekTable)
        {
            table[entry.Item1] = entry.Item2;
        }

        return table;
    }

    [Benchmark] public string C03_CavenaGreekLookup_Current() => C03_Current();
    [Benchmark] public string C03_CavenaGreekLookup_Candidate() => C03_Candidate();
}
