using SkiaSharp;
using System.IO.Compression;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Encodes a bitmap as an 8-bit palette-indexed PNG (PNG color type 3) with a tRNS chunk
/// holding the per-palette-entry alpha. Blu-ray authoring tools (Scenarist, DoStudio, ...)
/// expect BDN XML image sets in indexed color, which is what "BDN/xml 8-bit" writes.
/// Skia can only encode 32-bit RGBA PNGs (Skia dropped its indexed color type), so the
/// chunks are written by hand here.
/// The quantizer is the one SE4 used (NikseBitmap.ConvertTo8BitsPerPixel): a greedy
/// nearest-color palette, index 0 reserved for transparent.
/// </summary>
public static class Png8BitEncoder
{
    /// <summary>Palette entries, index 0 included - one less than the 256 PNG allows, as SE4 did.</summary>
    private const int MaxPaletteColors = 255;

    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
    private static readonly uint[] CrcTable = CreateCrcTable();

    public static byte[] Encode(SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        if (bitmap.Width <= 0 || bitmap.Height <= 0)
        {
            // PNG has no zero-sized images; hand back a single transparent pixel instead.
            return Write(1, 1, new byte[1], new List<SKColor> { SKColors.Transparent });
        }

        var width = bitmap.Width;
        var height = bitmap.Height;
        var palette = new List<SKColor> { SKColors.Transparent };
        var indexes = new byte[width * height];

        using (var straight = ToStraightBgra8888(bitmap))
        {
            var pixels = straight.GetPixelSpan();
            var rowBytes = straight.RowBytes;

            // Anti-aliased text repeats the same few hundred colors over and over, and the
            // nearest-color search below is a linear scan of the palette - without this the
            // per-image cost is width * height * palette-size.
            var seen = new Dictionary<uint, byte>();

            for (var y = 0; y < height; y++)
            {
                var row = y * rowBytes;
                for (var x = 0; x < width; x++)
                {
                    var offset = row + (x * 4);
                    var b = pixels[offset];
                    var g = pixels[offset + 1];
                    var r = pixels[offset + 2];
                    var a = pixels[offset + 3];

                    if (a < 5)
                    {
                        continue; // index 0 - the array is already zeroed
                    }

                    var key = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
                    if (seen.TryGetValue(key, out var cached))
                    {
                        indexes[(y * width) + x] = cached;
                        continue;
                    }

                    var color = new SKColor(r, g, b, a);
                    var index = FindBestMatch(color, palette, out var maxDiff);

                    if ((index < 0 && palette.Count < MaxPaletteColors) ||
                        (palette.Count < 200 && maxDiff > 5) ||
                        (palette.Count < MaxPaletteColors && maxDiff > 15))
                    {
                        index = palette.Count;
                        palette.Add(color);
                    }

                    // index can still be -1 when the palette is full and nothing came close;
                    // fall back to transparent, like SE4 did.
                    var paletteIndex = (byte)(index >= 0 ? index : 0);
                    indexes[(y * width) + x] = paletteIndex;
                    seen[key] = paletteIndex;
                }
            }
        }

        return Write(width, height, indexes, palette);
    }

    /// <summary>
    /// Straight (non-premultiplied) BGRA copy - the palette must hold the real colors, not
    /// colors already scaled by their alpha.
    /// </summary>
    private static SKBitmap ToStraightBgra8888(SKBitmap bitmap)
    {
        var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        var copy = new SKBitmap(info);
        using (var image = SKImage.FromBitmap(bitmap))
        {
            if (image != null && image.ReadPixels(info, copy.GetPixels(), copy.RowBytes, 0, 0))
            {
                return copy;
            }
        }

        copy.Dispose();

        // Unconvertible color type - a plain copy still gives four bytes per pixel to walk.
        return bitmap.Copy(SKColorType.Bgra8888) ?? new SKBitmap(info);
    }

    private static int FindBestMatch(SKColor color, List<SKColor> palette, out int maxDiff)
    {
        var smallestDiff = 1000;
        var smallestDiffIndex = -1;
        for (var i = 0; i < palette.Count; i++)
        {
            var pc = palette[i];
            var diff = Math.Abs(pc.Alpha - color.Alpha) +
                       Math.Abs(pc.Red - color.Red) +
                       Math.Abs(pc.Green - color.Green) +
                       Math.Abs(pc.Blue - color.Blue);
            if (diff < smallestDiff)
            {
                smallestDiff = diff;
                smallestDiffIndex = i;
                if (smallestDiff < 4)
                {
                    maxDiff = smallestDiff;
                    return smallestDiffIndex;
                }
            }
        }

        maxDiff = smallestDiff;
        return smallestDiffIndex;
    }

    private static byte[] Write(int width, int height, byte[] indexes, List<SKColor> palette)
    {
        using var stream = new MemoryStream();
        stream.Write(PngSignature, 0, PngSignature.Length);

        var header = new byte[13];
        WriteUInt32BigEndian(header, 0, (uint)width);
        WriteUInt32BigEndian(header, 4, (uint)height);
        header[8] = 8; // bit depth
        header[9] = 3; // color type: palette
        header[10] = 0; // compression: deflate
        header[11] = 0; // filter: adaptive
        header[12] = 0; // interlace: none
        WriteChunk(stream, "IHDR", header);

        var plte = new byte[palette.Count * 3];
        for (var i = 0; i < palette.Count; i++)
        {
            plte[i * 3] = palette[i].Red;
            plte[(i * 3) + 1] = palette[i].Green;
            plte[(i * 3) + 2] = palette[i].Blue;
        }

        WriteChunk(stream, "PLTE", plte);

        // tRNS may stop at the last non-opaque entry; everything after it is opaque by default.
        var alphaCount = palette.Count;
        while (alphaCount > 0 && palette[alphaCount - 1].Alpha == 255)
        {
            alphaCount--;
        }

        if (alphaCount > 0)
        {
            var trns = new byte[alphaCount];
            for (var i = 0; i < alphaCount; i++)
            {
                trns[i] = palette[i].Alpha;
            }

            WriteChunk(stream, "tRNS", trns);
        }

        WriteChunk(stream, "IDAT", Deflate(width, height, indexes));
        WriteChunk(stream, "IEND", Array.Empty<byte>());

        return stream.ToArray();
    }

    private static byte[] Deflate(int width, int height, byte[] indexes)
    {
        // One filter byte per scan line. The PNG spec recommends leaving palette images
        // unfiltered (filter type 0) - the bytes are indexes, so deltas mean nothing.
        var raw = new byte[(width + 1) * height];
        for (var y = 0; y < height; y++)
        {
            Buffer.BlockCopy(indexes, y * width, raw, (y * (width + 1)) + 1, width);
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, true))
        {
            zlib.Write(raw, 0, raw.Length);
        }

        return compressed.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        var length = new byte[4];
        WriteUInt32BigEndian(length, 0, (uint)data.Length);
        stream.Write(length, 0, length.Length);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes, 0, typeBytes.Length);
        stream.Write(data, 0, data.Length);

        var crc = Crc32(Crc32(0xffffffff, typeBytes), data) ^ 0xffffffff;
        var crcBytes = new byte[4];
        WriteUInt32BigEndian(crcBytes, 0, crc);
        stream.Write(crcBytes, 0, crcBytes.Length);
    }

    private static void WriteUInt32BigEndian(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static uint Crc32(uint crc, byte[] data)
    {
        foreach (var b in data)
        {
            crc = CrcTable[(crc ^ b) & 0xff] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] CreateCrcTable()
    {
        var table = new uint[256];
        for (var n = 0; n < 256; n++)
        {
            var c = (uint)n;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xedb88320 ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
