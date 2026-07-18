using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;

namespace LibSETests.ContainerFormats;

public class StblTest
{
    private static Stbl ParseSingleBox(byte[] boxBytes)
    {
        using var ms = new MemoryStream(boxBytes);
        return new Stbl(ms, (ulong)ms.Length, 90000, "text", null);
    }

    private static byte[] MakeBox(string name, params byte[][] content)
    {
        var payload = content.SelectMany(b => b).ToArray();
        var size = 8 + payload.Length;
        var bytes = new List<byte>
        {
            (byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size,
        };
        bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(name));
        bytes.AddRange(payload);
        return bytes.ToArray();
    }

    private static byte[] UInt32(uint value) => new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

    // A malformed stts run-length (sampleCount 0xFFFFFFFF) used to expand toward a
    // 4-billion-entry list; the expansion is now capped.
    [Fact]
    public void SttsHugeSampleCountIsCapped()
    {
        var box = MakeBox("stts",
            UInt32(0),            // version + flags
            UInt32(1),            // entry count
            UInt32(0xFFFFFFFF),   // sample count
            UInt32(1));           // sample delta

        var stbl = ParseSingleBox(box);

        Assert.Equal(5_000_000, stbl.Ssts.Count);
    }

    // ctts has the same run-length expansion and gets the same cap.
    [Fact]
    public void CttsHugeSampleCountIsCapped()
    {
        var box = MakeBox("ctts",
            UInt32(0),            // version + flags
            UInt32(1),            // entry count
            UInt32(0xFFFFFFFF),   // sample count
            UInt32(2));           // sample offset

        var stbl = ParseSingleBox(box);

        Assert.Equal(5_000_000, stbl.Ctts.Count);
    }

    // A declared entry count far beyond the actual box payload must stop at the buffer,
    // not throw or spin through 4 billion iterations.
    [Fact]
    public void StcoEntryCountBeyondBufferIsBounded()
    {
        var box = MakeBox("stco",
            UInt32(0),            // version + flags
            UInt32(0xFFFFFFFF),   // declared entry count
            UInt32(0x1234));      // a single actual chunk offset

        var stbl = ParseSingleBox(box);

        Assert.True(stbl.ChunkOffsets.Count <= 2);
        Assert.Equal(0x1234u, stbl.ChunkOffsets[0]);
    }
}
