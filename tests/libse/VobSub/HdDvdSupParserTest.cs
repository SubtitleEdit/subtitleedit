using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;

namespace LibSETests.VobSub;

public class HdDvdSupParserTest
{
    private const byte Transparent = 0;
    private const byte White = 1;
    private const byte Red = 200; // needs the 8-bit colour code

    [Fact]
    public void ParsesTimesAreaAndPixels()
    {
        var stream = BuildStream(
            (Pts: 90_000, StopDelay: 90),     // 1.000 s, shown for 1.024 s
            (Pts: 180_000, StopDelay: null),  // 2.000 s, no stop - stays until the next one
            (Pts: 270_000, StopDelay: 180));  // 3.000 s, shown for 2.048 s

        var pictures = HdDvdSupParser.Parse(stream);

        Assert.Equal(3, pictures.Count);
        Assert.Equal(1000, pictures[0].StartTime.TotalMilliseconds);
        Assert.Equal(2024, pictures[0].EndTime.TotalMilliseconds);
        Assert.False(pictures[1].HasStopDisplay);
        Assert.Equal(2000, pictures[1].StartTime.TotalMilliseconds);
        Assert.Equal(3000, pictures[1].EndTime.TotalMilliseconds);
        Assert.Equal(5048, pictures[2].EndTime.TotalMilliseconds);
        Assert.Equal(new SKRectI(10, 20, 14, 23), pictures[0].ImageDisplayArea);

        // Rows: white x4 / transparent, red, red, transparent / white x4.
        using var bitmap = pictures[0].GetBitmap(crop: false);
        Assert.Equal(4, bitmap.Width);
        Assert.Equal(3, bitmap.Height);
        Assert.Equal(SKColors.White, bitmap.GetPixel(0, 0));
        Assert.Equal(0, bitmap.GetPixel(0, 1).Alpha);
        Assert.True(bitmap.GetPixel(1, 1).Red > 200 && bitmap.GetPixel(1, 1).Green < 60);
        Assert.Equal(0, bitmap.GetPixel(3, 1).Alpha);
        Assert.Equal(SKColors.White, bitmap.GetPixel(3, 2));
    }

    [Fact]
    public void CropsToVisiblePixels()
    {
        var pictures = HdDvdSupParser.Parse(BuildPacket(0, 90, oddRowOnly: true));

        using var bitmap = pictures[0].GetBitmap();

        Assert.Equal(2, bitmap.Width);
        Assert.Equal(1, bitmap.Height);
        Assert.Equal(new SKPointI(11, 21), pictures[0].ImagePosition);
    }

    [Fact]
    public void DetectsHdDvdButNotDvdOrBluRaySup()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sup");
        try
        {
            File.WriteAllBytes(fileName, BuildStream((Pts: 0, StopDelay: 90), (Pts: 90_000, StopDelay: 90)));

            Assert.True(HdDvdSupParser.IsHdDvdSup(fileName));
            Assert.False(FileUtil.IsSpDvdSup(fileName));
            Assert.False(FileUtil.IsBluRaySup(fileName));
            Assert.Equal(2, HdDvdSupParser.Parse(fileName).Count);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void SkipsGarbageBetweenPackets()
    {
        var one = BuildStream((Pts: 0, StopDelay: 90));
        var two = BuildStream((Pts: 90_000, StopDelay: 90));
        var stream = one.Concat(new byte[] { 0x53, 0x50, 1, 2, 3 }).Concat(two).ToArray();

        var pictures = HdDvdSupParser.Parse(stream);

        Assert.Equal(2, pictures.Count);
        Assert.Equal(1000, pictures[1].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void TruncatedStreamDoesNotThrow()
    {
        var stream = BuildStream((Pts: 0, StopDelay: 90));
        for (var length = 0; length < stream.Length; length++)
        {
            var pictures = HdDvdSupParser.Parse(stream.Take(length).ToArray());
            foreach (var picture in pictures)
            {
                using var bitmap = picture.GetBitmap();
            }
        }
    }

    private static byte[] BuildStream(params (long Pts, int? StopDelay)[] packets)
    {
        return packets.SelectMany(p => BuildPacket(p.Pts, p.StopDelay, false)).ToArray();
    }

    /// <summary>
    /// One "SP" packet with a 4x3 picture at (10,20). Rows 0 and 2 (even field) are white,
    /// row 1 (odd field) is transparent, red, red, transparent - or with
    /// <paramref name="oddRowOnly"/> the even rows are transparent too.
    /// </summary>
    private static byte[] BuildPacket(long pts, int? stopDelay, bool oddRowOnly)
    {
        var even = new BitWriter();
        for (var row = 0; row < 2; row++)
        {
            // run, 2-bit colour, long length of 0 = to end of line
            even.Write(1, 1);
            even.Write(0, 1);
            even.Write(oddRowOnly ? Transparent : White, 2);
            even.Write(1, 1);
            even.Write(0, 7);
            even.Align();
        }

        var odd = new BitWriter();
        odd.Write(0, 1); // single pixel, 2-bit colour
        odd.Write(0, 1);
        odd.Write(Transparent, 2);
        odd.Write(1, 1); // run, 8-bit colour, short length (3 bits + 2)
        odd.Write(1, 1);
        odd.Write(Red, 8);
        odd.Write(0, 1);
        odd.Write(0, 3);
        odd.Write(0, 1); // single pixel, 2-bit colour
        odd.Write(0, 1);
        odd.Write(Transparent, 2);
        odd.Align();

        var evenBytes = even.ToArray();
        var oddBytes = odd.ToArray();
        var evenOffset = 10;
        var oddOffset = evenOffset + evenBytes.Length;
        var firstSequence = oddOffset + oddBytes.Length;

        var firstCommands = new List<byte> { 0x01, 0x83 };
        for (var i = 0; i < 256; i++)
        {
            // Y, Cr, Cb
            firstCommands.AddRange(i switch
            {
                White => new byte[] { 235, 128, 128 },
                Red => new byte[] { 63, 240, 102 },
                _ => new byte[] { 16, 128, 128 },
            });
        }

        firstCommands.Add(0x84);
        for (var i = 0; i < 256; i++)
        {
            firstCommands.Add(i is White or Red ? (byte)0 : (byte)255);
        }

        // x1=10, x2=13, y1=20, y2=22 in 12-bit pairs
        firstCommands.AddRange(new byte[] { 0x85, 0x00, 0xA0, 0x0D, 0x01, 0x40, 0x16 });
        firstCommands.Add(0x86);
        firstCommands.AddRange(BigEndian(evenOffset));
        firstCommands.AddRange(BigEndian(oddOffset));
        firstCommands.Add(0xff);

        var secondSequence = firstSequence + 6 + firstCommands.Count;
        var unit = new List<byte> { 0, 0, 0, 0, 0, 0 };
        unit.AddRange(BigEndian(firstSequence));
        unit.AddRange(evenBytes);
        unit.AddRange(oddBytes);
        unit.AddRange(new byte[] { 0, 0 });
        unit.AddRange(BigEndian(stopDelay.HasValue ? secondSequence : firstSequence));
        unit.AddRange(firstCommands);
        if (stopDelay.HasValue)
        {
            unit.Add((byte)(stopDelay.Value >> 8));
            unit.Add((byte)stopDelay.Value);
            unit.AddRange(BigEndian(secondSequence));
            unit.Add(0x02);
            unit.Add(0xff);
        }

        var size = BigEndian(unit.Count);
        for (var i = 0; i < 4; i++)
        {
            unit[2 + i] = size[i];
        }

        var packet = new List<byte> { (byte)'S', (byte)'P' };
        packet.AddRange(BitConverter.GetBytes(pts));
        packet.AddRange(unit);
        return packet.ToArray();
    }

    private static byte[] BigEndian(int value) =>
        new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

    private sealed class BitWriter
    {
        private readonly List<byte> _bytes = new();
        private int _bitCount;

        public void Write(int value, int bits)
        {
            for (var i = bits - 1; i >= 0; i--)
            {
                if (_bitCount % 8 == 0)
                {
                    _bytes.Add(0);
                }

                if (((value >> i) & 1) == 1)
                {
                    _bytes[^1] |= (byte)(0x80 >> (_bitCount % 8));
                }

                _bitCount++;
            }
        }

        public void Align() => _bitCount = (_bitCount + 7) & ~7;

        public byte[] ToArray() => _bytes.ToArray();
    }
}
