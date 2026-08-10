using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System.Text;

namespace LibUiLogicTests.Export;

public class Png8BitEncoderTests
{
    private static SKBitmap MakeBitmap(int width, int height, Func<int, int, SKColor> pixel)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, pixel(x, y));
            }
        }

        return bitmap;
    }

    private static List<(string Type, byte[] Data)> ReadChunks(byte[] png)
    {
        var chunks = new List<(string, byte[])>();
        var offset = 8; // signature
        while (offset + 8 <= png.Length)
        {
            var length = (png[offset] << 24) | (png[offset + 1] << 16) | (png[offset + 2] << 8) | png[offset + 3];
            var type = Encoding.ASCII.GetString(png, offset + 4, 4);
            var data = new byte[length];
            Array.Copy(png, offset + 8, data, 0, length);
            chunks.Add((type, data));
            offset += 12 + length; // length + type + data + crc
        }

        return chunks;
    }

    [Fact]
    public void WritesIndexedColorPngWithPaletteAndTransparency()
    {
        using var bitmap = MakeBitmap(8, 4, (x, _) => x < 4 ? SKColors.Transparent : SKColors.White);

        var png = Png8BitEncoder.Encode(bitmap);

        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png.Take(8).ToArray());

        var chunks = ReadChunks(png);
        Assert.Equal(new[] { "IHDR", "PLTE", "tRNS", "IDAT", "IEND" }, chunks.Select(c => c.Type).ToArray());

        var header = chunks[0].Data;
        Assert.Equal(8, (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3]); // width
        Assert.Equal(4, (header[4] << 24) | (header[5] << 16) | (header[6] << 8) | header[7]); // height
        Assert.Equal(8, header[8]); // bit depth
        Assert.Equal(3, header[9]); // color type: palette

        // Index 0 is the fully transparent entry, white was added as index 1.
        Assert.Equal(2 * 3, chunks[1].Data.Length);
        Assert.Equal(0, chunks[2].Data[0]);
    }

    [Fact]
    public void DecodesBackToTheOriginalColors()
    {
        var colors = new[] { SKColors.Transparent, SKColors.White, SKColors.Black, new SKColor(255, 0, 0, 128) };
        using var bitmap = MakeBitmap(4, 3, (x, _) => colors[x]);

        var png = Png8BitEncoder.Encode(bitmap);

        using var decoded = SKBitmap.Decode(png);
        Assert.NotNull(decoded);
        Assert.Equal(4, decoded.Width);
        Assert.Equal(3, decoded.Height);
        for (var y = 0; y < 3; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var actual = decoded.GetPixel(x, y);
                if (colors[x].Alpha == 0)
                {
                    Assert.Equal(0, actual.Alpha);
                    continue;
                }

                Assert.Equal(colors[x].Alpha, actual.Alpha);
                Assert.Equal(colors[x].Red, actual.Red);
                Assert.Equal(colors[x].Green, actual.Green);
                Assert.Equal(colors[x].Blue, actual.Blue);
            }
        }
    }

    [Fact]
    public void PremultipliedInputKeepsItsStraightColors()
    {
        // A half transparent white pixel is stored as (128,128,128,128) when premultiplied -
        // the palette must hold the straight color, or semi-transparent anti-aliased edges
        // come out too dark.
        using var bitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Premul));
        bitmap.SetPixel(0, 0, new SKColor(255, 255, 255, 128));

        var png = Png8BitEncoder.Encode(bitmap);

        using var decoded = SKBitmap.Decode(png);
        var pixel = decoded.GetPixel(0, 0);
        Assert.Equal(128, pixel.Alpha);
        Assert.True(pixel.Red >= 254, $"Expected white, got {pixel}");
        Assert.True(pixel.Green >= 254, $"Expected white, got {pixel}");
        Assert.True(pixel.Blue >= 254, $"Expected white, got {pixel}");
    }

    [Fact]
    public void ManyColorsStayWithinAnEightBitPalette()
    {
        // A gradient has far more than 255 distinct colors; the quantizer must still fit.
        using var bitmap = MakeBitmap(64, 64, (x, y) => new SKColor((byte)(x * 4), (byte)(y * 4), (byte)((x + y) * 2)));

        var png = Png8BitEncoder.Encode(bitmap);

        var chunks = ReadChunks(png);
        var plte = chunks.First(c => c.Type == "PLTE").Data;
        Assert.True(plte.Length % 3 == 0);
        Assert.InRange(plte.Length / 3, 1, 256);

        using var decoded = SKBitmap.Decode(png);
        Assert.Equal(64, decoded.Width);
        Assert.Equal(64, decoded.Height);
    }

    [Fact]
    public void EmptyBitmapGivesASinglePixelPng()
    {
        using var bitmap = new SKBitmap(0, 0);

        var png = Png8BitEncoder.Encode(bitmap);

        using var decoded = SKBitmap.Decode(png);
        Assert.NotNull(decoded);
        Assert.Equal(1, decoded.Width);
        Assert.Equal(1, decoded.Height);
    }
}
