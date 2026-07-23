using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.IO;
using System.Text;

namespace LibSETests.BluRaySup;

public class BluRaySupParserTest
{
    private const long Pts1 = 90000;  // 1 second in 90 kHz ticks
    private const long Pts2 = 180000; // 2 seconds

    private static void WriteSegment(MemoryStream ms, byte type, long pts, byte[] payload)
    {
        ms.WriteByte(0x50); // 'P'
        ms.WriteByte(0x47); // 'G'
        ms.WriteByte((byte)((pts >> 24) & 0xFF));
        ms.WriteByte((byte)((pts >> 16) & 0xFF));
        ms.WriteByte((byte)((pts >> 8) & 0xFF));
        ms.WriteByte((byte)(pts & 0xFF));
        ms.WriteByte(0); // DTS (unused)
        ms.WriteByte(0);
        ms.WriteByte(0);
        ms.WriteByte(0);
        ms.WriteByte(type);
        ms.WriteByte((byte)(payload.Length >> 8));
        ms.WriteByte((byte)(payload.Length & 0xFF));
        ms.Write(payload, 0, payload.Length);
    }

    private static byte[] MakePcsPayload(byte compositionObjectCount, bool withObject)
    {
        var payload = new byte[withObject ? 19 : 11];
        payload[0] = 0x07; // width 1920
        payload[1] = 0x80;
        payload[2] = 0x04; // height 1080
        payload[3] = 0x38;
        payload[4] = 0x10; // fps type
        payload[5] = 0x00; // composition number
        payload[6] = 0x01;
        payload[7] = withObject ? (byte)0x80 : (byte)0x00; // epoch start / normal
        payload[8] = 0x00; // no palette update
        payload[9] = 0x00; // palette id
        payload[10] = compositionObjectCount;
        if (withObject)
        {
            payload[11] = 0x00; // object id 0
            payload[12] = 0x00;
            payload[13] = 0x00; // window id
            payload[14] = 0x00; // not forced
            payload[15] = 0x00; // x = 100
            payload[16] = 0x64;
            payload[17] = 0x00; // y = 200
            payload[18] = 0xC8;
        }

        return payload;
    }

    private static byte[] MakeOdsPayload()
    {
        // 8x2 bitmap, palette entry 1: eight single pixels + end-of-line marker per row
        var rle = new byte[20];
        for (var row = 0; row < 2; row++)
        {
            for (var x = 0; x < 8; x++)
            {
                rle[row * 10 + x] = 0x01;
            }
            // rle[row * 10 + 8] and [+ 9] stay 0x00 0x00 = end of line
        }

        var payload = new byte[11 + rle.Length];
        payload[0] = 0x00; // object id 0
        payload[1] = 0x00;
        payload[2] = 0x00; // version
        payload[3] = 0xC0; // first and last in sequence
        payload[4] = 0x00; // object data length (3 bytes, unused by parser)
        payload[5] = 0x00;
        payload[6] = (byte)(rle.Length + 4);
        payload[7] = 0x00; // width 8
        payload[8] = 0x08;
        payload[9] = 0x00; // height 2
        payload[10] = 0x02;
        rle.CopyTo(payload, 11);
        return payload;
    }

    private static byte[] MakePdsPayload()
    {
        return new byte[]
        {
            0x00, 0x00,                   // palette id, version
            0x00, 0xEB, 0x80, 0x80, 0xFF, // entry 0: white, opaque
            0x01, 0x10, 0x80, 0x80, 0xFF, // entry 1: black, opaque
        };
    }

    private static byte[] MakeWdsPayload()
    {
        return new byte[]
        {
            0x01,                   // one window
            0x00,                   // window id
            0x00, 0x64, 0x00, 0xC8, // x, y
            0x00, 0x08, 0x00, 0x02, // width, height
        };
    }

    private static MemoryStream MakeSupStream(byte firstPcsObjectCount)
    {
        var ms = new MemoryStream();
        WriteSegment(ms, 0x16, Pts1, MakePcsPayload(firstPcsObjectCount, withObject: true));
        WriteSegment(ms, 0x17, Pts1, MakeWdsPayload());
        WriteSegment(ms, 0x14, Pts1, MakePdsPayload());
        WriteSegment(ms, 0x15, Pts1, MakeOdsPayload());
        WriteSegment(ms, 0x80, Pts1, System.Array.Empty<byte>());
        WriteSegment(ms, 0x16, Pts2, MakePcsPayload(0, withObject: false)); // display set clear
        WriteSegment(ms, 0x80, Pts2, System.Array.Empty<byte>());
        ms.Position = 0;
        return ms;
    }

    private static List<BluRaySupParser.PcsData> Parse(MemoryStream ms)
    {
        return BluRaySupParser.ParseBluRaySup(
            ms,
            new StringBuilder(),
            false,
            new Dictionary<int, List<PaletteInfo>>(),
            new Dictionary<int, List<BluRaySupParser.OdsData>>());
    }

    [Fact]
    public void ParsesMinimalSupStream()
    {
        using var ms = MakeSupStream(firstPcsObjectCount: 1);
        var pcsList = Parse(ms);

        var pcs = Assert.Single(pcsList);
        Assert.Equal(Pts1, pcs.StartTime);
        Assert.Equal(Pts2, pcs.EndTime);
        Assert.Equal(1920, pcs.Size.Width);
        Assert.Equal(1080, pcs.Size.Height);

        var pcsObject = Assert.Single(pcs.PcsObjects);
        Assert.Equal(0, pcsObject.ObjectId);
        Assert.False(pcsObject.IsForced);
        Assert.Equal(100, pcsObject.Origin.X);
        Assert.Equal(200, pcsObject.Origin.Y);

        var odsList = Assert.Single(pcs.BitmapObjects);
        var ods = Assert.Single(odsList);
        Assert.Equal(8, ods.Width);
        Assert.Equal(2, ods.Height);
        Assert.Equal(20, ods.Fragment.ImagePacketSize);

        Assert.Single(pcs.PaletteInfos);

        using var bitmap = BluRaySupParser.SupDecoder.DecodeImage(pcsObject, odsList, pcs.PaletteInfos);
        Assert.Equal(8, bitmap.Width);
        Assert.Equal(2, bitmap.Height);
    }

    // A PCS can declare more composition objects than fit in the segment; the
    // parser must only read objects that lie inside segment.Size instead of
    // reading past the segment (the shared read buffer may hold stale bytes
    // from a previous, larger segment).
    [Fact]
    public void MalformedCompositionObjectCountIsBoundedBySegmentSize()
    {
        using var ms = MakeSupStream(firstPcsObjectCount: 3); // only 1 object present
        var pcsList = Parse(ms);

        var pcs = Assert.Single(pcsList);
        var pcsObject = Assert.Single(pcs.PcsObjects);
        Assert.Equal(0, pcsObject.ObjectId);
        Assert.Equal(100, pcsObject.Origin.X);
        Assert.Equal(200, pcsObject.Origin.Y);
    }

    // Segments share one read buffer, so a small segment after a large one must
    // not see the large segment's leftover bytes. The clearing PCS (11 bytes)
    // follows the 31-byte ODS - if stale data leaked, the clear would grow
    // extra composition objects and survive into the result.
    [Fact]
    public void SmallSegmentAfterLargeSegmentDoesNotReadStaleData()
    {
        using var ms = MakeSupStream(firstPcsObjectCount: 1);
        var pcsList = Parse(ms);

        Assert.Single(pcsList); // the empty clearing PCS was removed, not inflated
    }

    // Palette entry 0 transparent, entry 1 opaque - "00 84 00" is a run of four palette-0 pixels.
    private static byte[] MakeOdsPayloadWithTransparentRun()
    {
        var rle = new byte[18];
        for (var row = 0; row < 2; row++)
        {
            var o = row * 9;
            rle[o] = 0x01;
            rle[o + 1] = 0x01;
            rle[o + 2] = 0x01;
            rle[o + 3] = 0x01;
            rle[o + 4] = 0x00; // 00 84 00 -> four times palette entry 0
            rle[o + 5] = 0x84;
            rle[o + 6] = 0x00;
            // rle[o + 7] and [o + 8] stay 0x00 0x00 = end of line
        }

        var payload = new byte[11 + rle.Length];
        payload[3] = 0xC0; // first and last in sequence
        payload[6] = (byte)(rle.Length + 4);
        payload[8] = 0x08; // width 8
        payload[10] = 0x02; // height 2
        rle.CopyTo(payload, 11);
        return payload;
    }

    private static byte[] MakePdsPayloadWithTransparentEntry()
    {
        return new byte[]
        {
            0x00, 0x00,                   // palette id, version
            0x00, 0xEB, 0x80, 0x80, 0x00, // entry 0: transparent (alpha 0)
            0x01, 0x10, 0x80, 0x80, 0xFF, // entry 1: black, opaque
        };
    }

    // Transparent palette entries are forced to black (BluRaySupParser.DecodePalette), so an
    // alpha-less bitmap turns the whole background into a solid black block - see issue #12761.
    [Fact]
    public void DecodedImageKeepsTransparency()
    {
        var ms = new MemoryStream();
        WriteSegment(ms, 0x16, Pts1, MakePcsPayload(1, withObject: true));
        WriteSegment(ms, 0x17, Pts1, MakeWdsPayload());
        WriteSegment(ms, 0x14, Pts1, MakePdsPayloadWithTransparentEntry());
        WriteSegment(ms, 0x15, Pts1, MakeOdsPayloadWithTransparentRun());
        WriteSegment(ms, 0x80, Pts1, System.Array.Empty<byte>());
        WriteSegment(ms, 0x16, Pts2, MakePcsPayload(0, withObject: false));
        WriteSegment(ms, 0x80, Pts2, System.Array.Empty<byte>());
        ms.Position = 0;

        var pcs = Assert.Single(Parse(ms));
        using var bitmap = pcs.GetBitmap();

        Assert.NotEqual(SKAlphaType.Opaque, bitmap.AlphaType);
        Assert.Equal(255, bitmap.GetPixel(0, 0).Alpha); // drawn pixel
        Assert.Equal(0, bitmap.GetPixel(4, 0).Alpha);   // transparent run
        Assert.Equal(0, bitmap.GetPixel(7, 1).Alpha);

        // the symptom users see is in the encoded png, so pin that too
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var decoded = SKBitmap.Decode(data.ToArray());
        Assert.Equal(255, decoded.GetPixel(0, 0).Alpha);
        Assert.Equal(0, decoded.GetPixel(4, 0).Alpha);

        ms.Dispose();
    }
}
