using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;

namespace LibSETests.ContainerFormats;

public class PacketizedElementaryStreamTest
{
    // A truncated pack (e.g. a PES packet with declared length 0) used to throw reading
    // the fixed header; the constructor now leaves the object empty instead.
    [Fact]
    public void TruncatedBufferDoesNotThrow()
    {
        var pes = new PacketizedElementaryStream(new byte[5], 0);
        Assert.Equal(0, pes.Length);
        Assert.Null(pes.SubPictureStreamId);
    }

    [Fact]
    public void PrivateStream1WithTruncatedHeaderDataDoesNotThrow()
    {
        // Fixed header claims private_stream_1 with a header-data length pointing past the buffer.
        var buffer = new byte[]
        {
            0x00, 0x00, 0x01, 0xBD, // start code + stream id private_stream_1
            0x00, 0x00,             // declared length 0
            0x80, 0x80,             // flags
            0xFF,                   // header data length far beyond the buffer
        };

        var pes = new PacketizedElementaryStream(buffer, 0);
        Assert.Null(pes.SubPictureStreamId);
    }
}
