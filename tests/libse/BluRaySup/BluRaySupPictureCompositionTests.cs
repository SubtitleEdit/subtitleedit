using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.Text;

namespace LibSETests.BluRaySup;

/// <summary>
/// A Blu-ray display set can compose two objects, each in a window of its own, from one
/// palette. The writer lays the palettes of the objects end to end in that one palette and
/// encodes each object against its own range.
/// </summary>
public class BluRaySupPictureCompositionTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "bluraysupcomposition_" + Guid.NewGuid().ToString("N"));

    public BluRaySupPictureCompositionTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private sealed record Segment(long Pts, byte Type, byte[] Payload);

    private static List<Segment> ReadSegments(byte[] sup)
    {
        var segments = new List<Segment>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            Assert.Equal(0x50, sup[position]);
            Assert.Equal(0x47, sup[position + 1]);
            var pts = ((long)sup[position + 2] << 24) | ((long)sup[position + 3] << 16) | ((long)sup[position + 4] << 8) | sup[position + 5];
            var size = (sup[position + 11] << 8) + sup[position + 12];
            segments.Add(new Segment(pts, sup[position + 10], sup.Skip(position + 13).Take(size).ToArray()));
            position += 13 + size;
        }

        Assert.Equal(sup.Length, position);
        return segments;
    }

    private static SKBitmap Solid(int width, int height, SKColor color)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
        return bitmap;
    }

    /// <summary>
    /// A bitmap with <paramref name="colors"/> distinct colours - one per column.
    /// </summary>
    private static SKBitmap Gradient(int colors)
    {
        var bitmap = new SKBitmap(new SKImageInfo(colors, 10, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        for (var x = 0; x < colors; x++)
        {
            for (var y = 0; y < 10; y++)
            {
                bitmap.SetPixel(x, y, new SKColor((byte)x, (byte)(255 - x), 128));
            }
        }

        return bitmap;
    }

    private static BluRaySupPicture Picture() => new()
    {
        Width = 1280,
        Height = 720,
        StartTime = 1000,
        EndTime = 3000,
        CompositionNumber = 10,
    };

    private static BluRaySupCompositionObject Object(SKBitmap bitmap, SKColor fontColor, int x, int y) => new()
    {
        Bitmap = bitmap,
        FontColor = fontColor,
        X = x,
        Y = y,
    };

    private List<BluRaySupParser.PcsData> Parse(byte[] sup)
    {
        var fileName = Path.Combine(_dir, Guid.NewGuid().ToString("N") + ".sup");
        File.WriteAllBytes(fileName, sup);
        return BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());
    }

    [Fact]
    public void TwoObjects_AreComposedInTwoWindowsFromOnePalette()
    {
        using var bottom = Solid(300, 80, SKColors.White);
        using var top = Solid(200, 40, SKColors.Red);
        var picture = Picture();

        var sup = BluRaySupPicture.CreateSupFrame(picture, new[]
        {
            Object(bottom, SKColors.White, 490, 590),
            Object(top, SKColors.Red, 540, 50),
        }, 25);

        var segments = ReadSegments(sup);
        Assert.Equal(new byte[] { 0x16, 0x17, 0x14, 0x15, 0x15, 0x80, 0x16, 0x17, 0x80 }, segments.Select(s => s.Type));

        var pcs = segments[0].Payload;
        Assert.Equal(2, pcs[10]);
        Assert.Equal(490, (pcs[15] << 8) + pcs[16]);
        Assert.Equal(590, (pcs[17] << 8) + pcs[18]);
        Assert.Equal(540, (pcs[23] << 8) + pcs[24]);
        Assert.Equal(50, (pcs[25] << 8) + pcs[26]);

        var wds = segments[1].Payload;
        Assert.Equal(2, wds[0]);
        Assert.Equal(0, wds[1]);
        Assert.Equal(1, wds[10]);

        // White, transparent, red, transparent - one palette, two ranges.
        var pds = segments[2].Payload;
        Assert.Equal(2 + 4 * 5, pds.Length);
        Assert.Equal(255, pds[2 + 0 * 5 + 4]);
        Assert.Equal(0, pds[2 + 1 * 5 + 4]);
        Assert.Equal(255, pds[2 + 2 * 5 + 4]);
        Assert.Equal(0, pds[2 + 3 * 5 + 4]);

        Assert.Equal(12, picture.NextCompositionNumber);
        Assert.Equal(11, (segments[6].Payload[5] << 8) + segments[6].Payload[6]);
    }

    [Fact]
    public void TwoObjects_DecodeToBothCaptionsInPlace()
    {
        using var bottom = Solid(300, 80, SKColors.White);
        using var top = Solid(200, 40, SKColors.Red);

        var sup = BluRaySupPicture.CreateSupFrame(Picture(), new[]
        {
            Object(bottom, SKColors.White, 490, 590),
            Object(top, SKColors.Red, 540, 50),
        }, 25);

        var subtitle = Assert.Single(Parse(sup));
        Assert.Equal(1000, subtitle.StartTimeCode.TotalMilliseconds, 1);
        Assert.Equal(3000, subtitle.EndTimeCode.TotalMilliseconds, 1);
        Assert.Equal(490, subtitle.GetPosition().Left);
        Assert.Equal(50, subtitle.GetPosition().Top);

        using var bitmap = subtitle.GetBitmap();
        Assert.Equal(300, bitmap.Width);
        Assert.Equal(620, bitmap.Height);
        var red = bitmap.GetPixel(540 - 490 + 100, 20);
        Assert.Equal(255, red.Alpha);
        Assert.True(red.Red > 200 && red.Green < 60 && red.Blue < 60, $"expected red, got {red}");
        var white = bitmap.GetPixel(150, 619);
        Assert.Equal(255, white.Alpha);
        Assert.True(white.Red > 200 && white.Green > 200 && white.Blue > 200, $"expected white, got {white}");
        Assert.Equal(0, bitmap.GetPixel(150, 300).Alpha);
    }

    [Fact]
    public void WithoutClear_TheDisplaySetEndsWithTheCaption()
    {
        using var bitmap = Solid(300, 80, SKColors.White);
        var picture = Picture();

        var sup = BluRaySupPicture.CreateSupFrame(picture, new[] { Object(bitmap, SKColors.White, 490, 590) }, 25, writeClear: false);

        var segments = ReadSegments(sup);
        Assert.Equal(new byte[] { 0x16, 0x17, 0x14, 0x15, 0x80 }, segments.Select(s => s.Type));
        Assert.Equal(11, picture.NextCompositionNumber);
    }

    [Fact]
    public void PalettesThatDoNotFitTogether_ShareTheBudget()
    {
        using var left = Gradient(200);
        using var right = Gradient(200);

        var sup = BluRaySupPicture.CreateSupFrame(Picture(), new[]
        {
            Object(left, SKColors.White, 100, 100),
            Object(right, SKColors.White, 100, 400),
        }, 25);

        var pds = ReadSegments(sup).First(s => s.Type == 0x14).Payload;
        var entries = (pds.Length - 2) / 5;
        Assert.InRange(entries, 2, BluRaySupPicture.MaxPaletteEntries);

        // Both still decode - a pixel of each keeps roughly its colour.
        var subtitle = Assert.Single(Parse(sup));
        using var bitmap = subtitle.GetBitmap();
        Assert.Equal(200, bitmap.Width);
        Assert.Equal(310, bitmap.Height);
        var a = bitmap.GetPixel(150, 5);
        var b = bitmap.GetPixel(150, 305);
        Assert.Equal(255, a.Alpha);
        Assert.Equal(255, b.Alpha);
        Assert.InRange(a.Red, 120, 180);
        Assert.InRange(b.Red, 120, 180);
    }

    [Fact]
    public void EncodedBitmap_BringsThePixelsBackExactly()
    {
        // Anti-aliased edges and a transparent hole: the encoder writes single pixels, runs of
        // colour 0, long and short runs and end of line codes.
        using var bitmap = new SKBitmap(new SKImageInfo(320, 60, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        for (var y = 0; y < 60; y++)
        {
            for (var x = 0; x < 320; x++)
            {
                var inHole = x > 100 && x < 150 && y > 20 && y < 40;
                var edge = x < 5 || y < 5;
                bitmap.SetPixel(x, y, inHole ? SKColors.Transparent : edge ? new SKColor(200, 100, 50, (byte)(40 + x % 7 * 30)) : SKColors.White);
            }
        }

        var picture = Picture();
        BluRaySupPicture.CreateSupFrame(picture, bitmap, SKColors.White, 25, 50, 0, BluRayContentAlignment.BottomCenter);

        var encoded = picture.EncodedBitmap;
        Assert.NotNull(encoded);
        Assert.Equal(320, encoded.Width);
        Assert.Equal(60, encoded.Height);
        Assert.True(encoded.Rle.Length < 320 * 60 / 4, $"the RLE should be a lot smaller than the pixels, was {encoded.Rle.Length}");

        using var decoded = encoded.ToBitmap();
        Assert.Equal(320, decoded.Width);
        Assert.Equal(60, decoded.Height);
        for (var y = 0; y < 60; y += 3)
        {
            for (var x = 0; x < 320; x += 7)
            {
                var expected = bitmap.GetPixel(x, y);
                var actual = decoded.GetPixel(x, y);
                if (expected.Alpha == 0)
                {
                    Assert.Equal(0, actual.Alpha);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }
            }
        }
    }

    [Fact]
    public void MoreThanTwoObjects_IsRejected()
    {
        using var bitmap = Solid(10, 10, SKColors.White);
        var objects = Enumerable.Range(0, 3).Select(i => Object(bitmap, SKColors.White, 0, i * 20)).ToList();

        Assert.Throws<ArgumentException>(() => BluRaySupPicture.CreateSupFrame(Picture(), objects, 25));
        Assert.Throws<ArgumentException>(() => BluRaySupPicture.CreateSupFrame(Picture(), new List<BluRaySupCompositionObject>(), 25));
    }

    [Fact]
    public void OneObject_IsWrittenAsTheSingleBitmapOverloadWritesIt()
    {
        using var bitmap = Solid(300, 80, SKColors.White);

        var viaObjects = BluRaySupPicture.CreateSupFrame(Picture(), new[] { Object(bitmap, SKColors.White, 490, 590) }, 25);
        var viaBitmap = BluRaySupPicture.CreateSupFrame(Picture(), bitmap, SKColors.White, 25, 50, 0, BluRayContentAlignment.BottomCenter);

        Assert.Equal(viaBitmap, viaObjects);
    }
}
