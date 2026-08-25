using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using SkiaSharp;

namespace LibSETests.Core;

public class BugHunt20260823Round3Test
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)] // length % 3 == 2 - used to lose both bytes
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)] // length % 3 == 2
    [InlineData(8)] // length % 3 == 2
    [InlineData(64)]
    [InlineData(65)]
    public void UuEncode_RoundTripsEveryTailLength(int length)
    {
        var bytes = new byte[length];
        for (var i = 0; i < length; i++)
        {
            bytes[i] = (byte)(i + 1);
        }

        var decoded = UUEncoding.UUDecode(UUEncoding.UUEncode(bytes));

        // UUEncode pads the last group to three bytes, so the decoded array may be longer
        Assert.True(decoded.Length >= length);
        Assert.Equal(bytes, decoded.Take(length).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void Mp4Boxes_TooSmallSizeIsRejectedNotUnderflowed(ulong size)
    {
        using var ms = new MemoryStream(new byte[512]);
        Assert.NotNull(new Mdhd(ms, size));
        ms.Position = 0;
        Assert.NotNull(new Tfdt(ms, size));
        ms.Position = 0;
        Assert.NotNull(new Tfhd(ms, size));
    }

    [Fact]
    public void Mp4Boxes_HugeSizeIsRejected()
    {
        using var ms = new MemoryStream(new byte[512]);
        Assert.NotNull(new Mdhd(ms, uint.MaxValue));
    }

    [Fact]
    public void Mdhd_StillReadsAValidBox()
    {
        // version 0 mdhd: version/flags, creation, modification, timescale(1000), duration, "eng"
        var payload = new byte[24];
        payload[15] = 0xE8; // timescale low byte of 1000
        payload[14] = 0x03;
        // "eng" packed as 5+5+5 bits with the pad bit clear
        var packed = ((('e' - 0x60) & 0x1f) << 10) | ((('n' - 0x60) & 0x1f) << 5) | (('g' - 0x60) & 0x1f);
        payload[20] = (byte)(packed >> 8);
        payload[21] = (byte)(packed & 0xff);

        using var ms = new MemoryStream(payload);
        var mdhd = new Mdhd(ms, (ulong)payload.Length + 4);
        Assert.Equal("eng", mdhd.Iso639ThreeLetterCode);
        Assert.Equal(1000UL, mdhd.TimeScale);
    }

    [Fact]
    public void PmtDescriptor_CaDescriptorWithShortLengthIsRejected()
    {
        var data = new byte[] { 9, 1, 0, 0, 0, 0, 0, 0 };
        var d = new ProgramMapTableDescriptor(data, 0);
        Assert.Null(d.PrivateDataBytes);
    }

    [Fact]
    public void PmtDescriptor_ContentEndingExactlyAtBufferEndIsRead()
    {
        var data = new byte[] { 5, 2, 0x41, 0x42 };
        Assert.Equal("AB", new ProgramMapTableDescriptor(data, 0).ContentAsString);
    }

    [Fact]
    public void PmtDescriptor_TruncatedContentIsStillRejected()
    {
        var data = new byte[] { 5, 4, 0x41, 0x42 };
        Assert.Null(new ProgramMapTableDescriptor(data, 0).ContentAsString);
    }

    [Fact]
    public void TransportStreamSubtitle_WithNoSourceDoesNotThrow()
    {
        var s = new TransportStreamSubtitle();
        Assert.Equal(0, s.NumberOfImages);
        Assert.NotNull(s.GetBitmap());

        // GetScreenSize returns a struct, so "not null" asserted nothing at all (xUnit2002).
        // Pin the no-source fallback it actually returns instead.
        Assert.Equal(
            new SKSize(DvbSubPes.DefaultScreenWidth, DvbSubPes.DefaultScreenHeight),
            s.GetScreenSize());

        Assert.NotNull(s.GetPosition());
    }
}
