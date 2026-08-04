using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using System.IO;

namespace LibSETests.ContainerFormats;

public class DvbSubPesTest
{
    // Builds a small private_stream_1 PES packet. declaredLength lets tests lie
    // about the PES packet length field to hit the bad-subs clamping path.
    private static byte[] MakePesPacket(int totalSize, int? declaredLength = null)
    {
        var buffer = new byte[totalSize];
        buffer[0] = 0x00;
        buffer[1] = 0x00;
        buffer[2] = 0x01;
        buffer[3] = 0xBD; // private_stream_1
        var length = declaredLength ?? totalSize - 6;
        buffer[4] = (byte)(length >> 8);
        buffer[5] = (byte)(length & 0xFF);
        buffer[6] = 0x00;
        buffer[7] = 0x80; // PTS present
        buffer[8] = 5;    // header data length
        buffer[9] = 0x21; // PTS (5 bytes)
        buffer[10] = 0x12;
        buffer[11] = 0x34;
        buffer[12] = 0x56;
        buffer[13] = 0x79;
        buffer[14] = 0x20; // sub picture stream id
        for (var i = 15; i < totalSize; i++)
        {
            buffer[i] = (byte)i; // recognizable payload
        }

        return buffer;
    }

    private static byte[] GetData(DvbSubPes pes)
    {
        using (var ms = new MemoryStream())
        {
            pes.WriteToStream(ms);
            return ms.ToArray();
        }
    }

    // The (byte[], int, int) constructor exists so an oversized buffer (e.g. one
    // rented from ArrayPool) can be passed with an explicit valid-data length.
    // Parsing must match the exact-size buffer bit for bit.
    [Fact]
    public void OversizedBufferWithExplicitLengthMatchesExactBuffer()
    {
        var exact = MakePesPacket(30);
        var oversized = new byte[100];
        exact.CopyTo(oversized, 0);
        for (var i = exact.Length; i < oversized.Length; i++)
        {
            oversized[i] = 0xEE; // garbage that must never be read
        }

        var pesExact = new DvbSubPes(exact, 0);
        var pesOversized = new DvbSubPes(oversized, 0, exact.Length);

        Assert.Equal(pesExact.StreamId, pesOversized.StreamId);
        Assert.Equal(pesExact.Length, pesOversized.Length);
        Assert.Equal(pesExact.HeaderDataLength, pesOversized.HeaderDataLength);
        Assert.Equal(pesExact.PresentationTimestamp, pesOversized.PresentationTimestamp);
        Assert.Equal(pesExact.SubPictureStreamId, pesOversized.SubPictureStreamId);
        Assert.Equal(GetData(pesExact), GetData(pesOversized));
    }

    // A declared PES length larger than the available data is clamped to the
    // valid length, not to the physical buffer size - otherwise garbage from an
    // oversized buffer would end up in the retained data.
    [Fact]
    public void BadDeclaredLengthClampsToValidLengthNotBufferSize()
    {
        var exact = MakePesPacket(30, declaredLength: 100);
        var oversized = new byte[100];
        exact.CopyTo(oversized, 0);
        for (var i = exact.Length; i < oversized.Length; i++)
        {
            oversized[i] = 0xEE;
        }

        var pesExact = new DvbSubPes(exact, 0);
        var pesOversized = new DvbSubPes(oversized, 0, exact.Length);

        var data = GetData(pesOversized);
        Assert.Equal(GetData(pesExact), data);
        Assert.DoesNotContain((byte)0xEE, data);
    }

    [Fact]
    public void TwoArgumentConstructorMatchesExplicitFullLength()
    {
        var buffer = MakePesPacket(30);

        var pes = new DvbSubPes(buffer, 0);
        var pesExplicit = new DvbSubPes(buffer, 0, buffer.Length);

        Assert.Equal(pes.Length, pesExplicit.Length);
        Assert.Equal(pes.PresentationTimestamp, pesExplicit.PresentationTimestamp);
        Assert.Equal(GetData(pes), GetData(pesExplicit));
    }

    // Data shorter than the fixed PES header must leave the object empty for
    // any length smaller than index + 9, even when the buffer itself is larger.
    [Fact]
    public void LengthShorterThanFixedHeaderLeavesObjectEmpty()
    {
        var buffer = MakePesPacket(30);

        var pes = new DvbSubPes(buffer, 0, 8);

        Assert.Equal(0, pes.Length);
        Assert.Null(pes.PresentationTimestamp);
        Assert.Empty(GetData(pes));
    }
}
