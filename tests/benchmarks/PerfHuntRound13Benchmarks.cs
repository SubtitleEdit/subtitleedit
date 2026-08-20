using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 13 candidate shoot-out: the OCR bitmap primitives in
/// <c>libuilogic/Ocr/NikseBitmap2.cs</c> (every one of them still walks the BGRA buffer one
/// byte at a time) plus the spectrogram build in <c>ui/Logic/Media/WaveToVisualizer2.cs</c>.
///
/// Self-contained on purpose (see the benchmark-verification notes): the shipped
/// implementation and each candidate live here with their own direct call site, so there is no
/// stash baseline, no worktree drift and no megamorphic delegate site. GlobalSetup asserts the
/// candidate is byte-for-byte equivalent to the current code over an adversarial battery
/// before any timing runs.
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound13Benchmarks
{
    // A cropped Blu-ray subtitle line: wide, short, mostly transparent with anti-aliased glyphs.
    private const int LineWidth = 1400;
    private const int LineHeight = 60;

    private byte[] _line = null!;      // the line image
    private byte[] _scratch = null!;   // writable copy for the mutating benchmarks
    private byte[] _blank = null!;     // fully transparent line image (worst case for the scans)

    // Spectrogram: one chunk as SpectrogramDrawer.Draw() sees it.
    private const int Nfft = 256;
    private const int ImageWidth = 1024;
    private byte[] _bitmap = null!;
    private uint[] _paletteRgba = null!;
    private byte[] _colorIndexes = null!;
    private float[] _rawSamples = null!;
    private double[] _chunkSamples = null!;

    private static readonly Vector<byte> AlphaMask = MakeLaneMask(3);
    private static readonly Vector<byte> RgbMask = MakeLaneMask(0, 1, 2);

    private static Vector<byte> MakeLaneMask(params int[] lanes)
    {
        var bytes = new byte[Vector<byte>.Count];
        for (var i = 0; i < bytes.Length; i += 4)
        {
            foreach (var lane in lanes)
            {
                bytes[i + lane] = 255;
            }
        }

        return new Vector<byte>(bytes);
    }

    [GlobalSetup]
    public void Setup()
    {
        _line = MakeSubtitleLine(LineWidth, LineHeight, 1);
        _scratch = new byte[_line.Length];
        _blank = new byte[LineWidth * LineHeight * 4];

        _bitmap = new byte[ImageWidth * (Nfft / 2) * 4];
        _paletteRgba = new uint[256];
        var palette = new byte[256 * 4];
        for (var i = 0; i < 256; i++)
        {
            palette[i * 4] = (byte)i;                 // R
            palette[i * 4 + 1] = (byte)(255 - i);     // G
            palette[i * 4 + 2] = (byte)(i * 3 % 256); // B
            palette[i * 4 + 3] = 255;                 // A
            _paletteRgba[i] = MemoryMarshal.Read<uint>(palette.AsSpan(i * 4, 4));
        }

        _colorIndexes = new byte[ImageWidth * (Nfft / 2)];
        var rnd = new Random(7);
        for (var i = 0; i < _colorIndexes.Length; i++)
        {
            _colorIndexes[i] = (byte)rnd.Next(256);
        }

        _rawSamples = new float[Nfft * ImageWidth];
        for (var i = 0; i < _rawSamples.Length; i++)
        {
            _rawSamples[i] = (float)Math.Sin(i * 0.01) * 0.5f;
        }

        _chunkSamples = new double[Nfft * ImageWidth];

        AssertEquivalence();
    }

    /// <summary>
    /// Transparent background, anti-aliased white glyph blobs, a few coloured pixels; the same
    /// mix a real cropped subtitle line has.
    /// </summary>
    private static byte[] MakeSubtitleLine(int width, int height, int seed)
    {
        var rnd = new Random(seed);
        var data = new byte[width * height * 4];
        for (var glyph = 0; glyph < width / 22; glyph++)
        {
            var gx = glyph * 22 + 3;
            var gw = 12 + rnd.Next(4);
            var gy = 8 + rnd.Next(4);
            var gh = height - gy - 8;
            for (var y = gy; y < gy + gh; y++)
            {
                for (var x = gx; x < gx + gw && x < width; x++)
                {
                    var edge = x == gx || x == gx + gw - 1 || y == gy || y == gy + gh - 1;
                    if (!edge && rnd.Next(3) == 0)
                    {
                        continue; // hollow parts of the glyph
                    }

                    var i = (y * width + x) * 4;
                    var v = edge ? (byte)rnd.Next(40, 200) : (byte)rnd.Next(200, 256);
                    data[i] = v;
                    data[i + 1] = v;
                    data[i + 2] = v;
                    data[i + 3] = edge ? (byte)rnd.Next(1, 255) : (byte)255;
                }
            }
        }

        return data;
    }

    // ---------------------------------------------------------------- equivalence

    private void AssertEquivalence()
    {
        var cases = new List<(byte[] data, int width, int height)>
        {
            (_line, LineWidth, LineHeight),
            (_blank, LineWidth, LineHeight),
            (MakeSubtitleLine(31, 47, 2), 31, 47),   // odd width, not a vector multiple
            (MakeSubtitleLine(1, 9, 3), 1, 9),       // one pixel wide
            (MakeSubtitleLine(5, 1, 4), 5, 1),       // one row
            (MakeSubtitleLine(17, 3, 5), 17, 3),
            (new byte[4], 1, 1),                     // single transparent pixel
        };

        foreach (var (data, width, height) in cases)
        {
            var widthX4 = width * 4;

            for (var y = 0; y < height; y++)
            {
                Check(Cur_IsLineTransparent(data, width, widthX4, y) == New_IsLineTransparent(data, widthX4, y), $"IsLineTransparent y={y} w={width}");
                Check(Cur_IsHorizontalLineTransparent(data, widthX4, y) == New_IsHorizontalLineTransparent(data, widthX4, y), $"IsHorizontalLineTransparent y={y} w={width}");
            }

            Check(Cur_IsImageOnlyTransparent(data) == New_IsImageOnlyTransparent(data), "IsImageOnlyTransparent");
            Check(Cur_CalcBottomTransparent(data, widthX4, height) == New_CalcBottomTransparent(data, widthX4, height), "CalcBottomTransparent");

            for (var margin = 0; margin < 4; margin++)
            {
                Check(Cur_CropTopTransparent(data, widthX4, height, margin) == New_CropTopTransparent(data, widthX4, height, margin), $"CropTopTransparent margin={margin}");
            }

            Cur_FindOpaqueSides(data, width, height, out var curLeft, out var curRight);
            New_FindOpaqueSides(data, width, height, out var newLeft, out var newRight);
            Check(curLeft == newLeft && curRight == newRight, $"FindOpaqueSides w={width} cur=({curLeft},{curRight}) new=({newLeft},{newRight})");

            CheckMutation(data, Cur_Fill, New_Fill, "Fill");
            CheckMutation(data, Cur_InvertColors, New_InvertColors, "InvertColors");
            CheckMutation(data, Cur_ReplaceTransparentWith, New_ReplaceTransparentWith, "ReplaceTransparentWith");
            CheckMutation(data, Cur_MakeOneColor, New_MakeOneColor, "MakeOneColor");
            foreach (var minRgb in new[] { 0, 1, 30, 300, 600, 765, 766 })
            {
                var expected = (byte[])data.Clone();
                Cur_MakeTwoColor(expected, minRgb);
                var actual = (byte[])data.Clone();
                New_MakeTwoColor(actual, minRgb);
                Check(expected.AsSpan().SequenceEqual(actual), $"MakeTwoColor minRgb={minRgb} w={width}");
            }
        }

        // Spectrogram pixel write shape.
        var refBitmap = new byte[_bitmap.Length];
        var newBitmap = new byte[_bitmap.Length];
        Cur_WritePixels(refBitmap, _colorIndexes, _paletteRgba, ImageWidth, Nfft / 2);
        New_WritePixels(newBitmap, _colorIndexes, _paletteRgba, ImageWidth, Nfft / 2);
        Check(refBitmap.AsSpan().SequenceEqual(newBitmap), "spectrogram pixel write");

        // float[] -> double[] chunk copy.
        var refChunk = new double[_chunkSamples.Length];
        var newChunk = new double[_chunkSamples.Length];
        foreach (var len in new[] { 0, 1, 3, 7, 8, 9, 33, _chunkSamples.Length })
        {
            Array.Clear(refChunk);
            Array.Clear(newChunk);
            Cur_CopyChunk(_rawSamples, 0, refChunk, len);
            New_CopyChunk(_rawSamples, 0, newChunk, len);
            Check(refChunk.AsSpan().SequenceEqual(newChunk), $"chunk copy len={len}");
        }
    }

    private static void CheckMutation(byte[] data, Action<byte[]> current, Action<byte[]> candidate, string what)
    {
        var expected = (byte[])data.Clone();
        current(expected);
        var actual = (byte[])data.Clone();
        candidate(actual);
        Check(expected.AsSpan().SequenceEqual(actual), what);
    }

    private static void Check(bool ok, string what)
    {
        if (!ok)
        {
            throw new InvalidOperationException($"Candidate differs from current implementation: {what}");
        }
    }

    // ================================================================ 1. IsLineTransparent

    private static bool Cur_IsLineTransparent(byte[] data, int width, int widthX4, int y)
    {
        var max = width * 4 + y * widthX4 + 3;
        for (var pos = y * widthX4 + 3; pos < max; pos += 4)
        {
            if (data[pos] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool New_IsLineTransparent(byte[] data, int widthX4, int y)
    {
        return RowIsTransparent(data.AsSpan(y * widthX4, widthX4), 0);
    }

    /// <summary>
    /// True when no pixel in the row has alpha above <paramref name="threshold"/>. One vector
    /// load per 4 or 8 pixels instead of one byte load per pixel; the mask keeps only the alpha
    /// lane of each BGRA quad, so it is byte-order neutral.
    /// </summary>
    private static bool RowIsTransparent(ReadOnlySpan<byte> row, byte threshold)
    {
        var i = 0;
        var step = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated && row.Length >= step)
        {
            var limit = new Vector<byte>(threshold);
            for (; i <= row.Length - step; i += step)
            {
                var alpha = new Vector<byte>(row.Slice(i, step)) & AlphaMask;
                if (Vector.GreaterThanAny(alpha, limit))
                {
                    return false;
                }
            }
        }

        for (var pos = i + 3; pos < row.Length; pos += 4)
        {
            if (row[pos] > threshold)
            {
                return false;
            }
        }

        return true;
    }

    [Benchmark] public bool IsLineTransparent_Current() { var r = false; for (var y = 0; y < LineHeight; y++) { r ^= Cur_IsLineTransparent(_line, LineWidth, LineWidth * 4, y); } return r; }
    [Benchmark] public bool IsLineTransparent_Simd() { var r = false; for (var y = 0; y < LineHeight; y++) { r ^= New_IsLineTransparent(_line, LineWidth * 4, y); } return r; }

    // ================================================== 2. IsHorizontalLineTransparent / bottom

    private static bool Cur_IsHorizontalLineTransparent(byte[] data, int widthX4, int y)
    {
        var yOffset = y * widthX4 + 3;
        var max = yOffset + widthX4;
        for (var pos = yOffset; pos < max; pos += 4)
        {
            if (data[pos] > 1)
            {
                return false;
            }
        }

        return true;
    }

    private static bool New_IsHorizontalLineTransparent(byte[] data, int widthX4, int y)
    {
        return RowIsTransparent(data.AsSpan(y * widthX4, widthX4), 1);
    }

    private static int Cur_CalcBottomTransparent(byte[] data, int widthX4, int height)
    {
        var y = height - 1;
        for (; y > 0; y--)
        {
            if (!Cur_IsHorizontalLineTransparent(data, widthX4, y))
            {
                break;
            }
        }

        return height - y;
    }

    private static int New_CalcBottomTransparent(byte[] data, int widthX4, int height)
    {
        var y = height - 1;
        for (; y > 0; y--)
        {
            if (!New_IsHorizontalLineTransparent(data, widthX4, y))
            {
                break;
            }
        }

        return height - y;
    }

    [Benchmark] public int CalcBottomTransparent_Current() => Cur_CalcBottomTransparent(_blank, LineWidth * 4, LineHeight);
    [Benchmark] public int CalcBottomTransparent_Simd() => New_CalcBottomTransparent(_blank, LineWidth * 4, LineHeight);

    // ================================================================ 3. IsImageOnlyTransparent

    private static bool Cur_IsImageOnlyTransparent(byte[] data)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool New_IsImageOnlyTransparent(byte[] data) => RowIsTransparent(data, 0);

    [Benchmark] public bool IsImageOnlyTransparent_Current() => Cur_IsImageOnlyTransparent(_blank);
    [Benchmark] public bool IsImageOnlyTransparent_Simd() => New_IsImageOnlyTransparent(_blank);

    // ================================================================ 4. CropTopTransparent

    private static int Cur_CropTopTransparent(byte[] data, int widthX4, int height, int minimumMargin)
    {
        var done = false;
        var newTop = 0;
        var y = 0;
        var width = widthX4 / 4;
        while (!done && y < height)
        {
            var x = 0;
            while (!done && x < width)
            {
                var alpha = data[x * 4 + y * widthX4 + 3];
                if (alpha > 10)
                {
                    done = true;
                    newTop = y - minimumMargin;
                    if (newTop < 0)
                    {
                        newTop = 0;
                    }
                }

                x++;
            }

            y++;
        }

        return newTop;
    }

    private static int New_CropTopTransparent(byte[] data, int widthX4, int height, int minimumMargin)
    {
        for (var y = 0; y < height; y++)
        {
            if (!RowIsTransparent(data.AsSpan(y * widthX4, widthX4), 10))
            {
                var newTop = y - minimumMargin;
                return newTop < 0 ? 0 : newTop;
            }
        }

        return 0;
    }

    [Benchmark] public int CropTopTransparent_Current() => Cur_CropTopTransparent(_line, LineWidth * 4, LineHeight, 2);
    [Benchmark] public int CropTopTransparent_Simd() => New_CropTopTransparent(_line, LineWidth * 4, LineHeight, 2);

    // ================================================================ 5. CropTransparentSides

    private static void Cur_FindOpaqueSides(byte[] data, int width, int height, out int left, out int right)
    {
        left = -1;
        right = -1;
        var widthX4 = width * 4;
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (data[x * 4 + y * widthX4 + 3] != 0)
                {
                    if (left < 0)
                    {
                        left = x;
                    }

                    right = x;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Same left/right opaque bounds, but read row-major (the buffer's own order) and only over
    /// the part of each row that could still improve the bounds: everything left of the current
    /// left edge and everything right of the current right edge. Both shrink to nothing after
    /// the first few rows, and the column-major walk's cache-line-per-pixel access is gone.
    /// </summary>
    private static void New_FindOpaqueSides(byte[] data, int width, int height, out int left, out int right)
    {
        left = -1;
        right = -1;
        var widthX4 = width * 4;
        for (var y = 0; y < height; y++)
        {
            if (left == 0 && right == width - 1)
            {
                return;
            }

            var row = data.AsSpan(y * widthX4, widthX4);
            var prefixEnd = left < 0 ? width : left; // exclusive, in pixels
            var x = FirstOpaque(row.Slice(0, prefixEnd * 4));
            if (x >= 0)
            {
                left = x;
                if (right < x)
                {
                    right = x;
                }
            }

            var suffixStart = right + 1;
            if (suffixStart < width)
            {
                x = LastOpaque(row.Slice(suffixStart * 4));
                if (x >= 0)
                {
                    right = suffixStart + x;
                    if (left < 0)
                    {
                        left = right;
                    }
                }
            }
        }
    }

    private static int FirstOpaque(ReadOnlySpan<byte> row)
    {
        var i = 0;
        var step = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated && row.Length >= step)
        {
            for (; i <= row.Length - step; i += step)
            {
                if ((new Vector<byte>(row.Slice(i, step)) & AlphaMask) != Vector<byte>.Zero)
                {
                    for (var pos = i + 3; pos < i + step; pos += 4)
                    {
                        if (row[pos] != 0)
                        {
                            return pos >> 2;
                        }
                    }
                }
            }
        }

        for (var pos = i + 3; pos < row.Length; pos += 4)
        {
            if (row[pos] != 0)
            {
                return pos >> 2;
            }
        }

        return -1;
    }

    private static int LastOpaque(ReadOnlySpan<byte> row)
    {
        var step = Vector<byte>.Count;
        var i = row.Length;
        if (Vector.IsHardwareAccelerated && row.Length >= step)
        {
            for (; i - step >= 0; i -= step)
            {
                if ((new Vector<byte>(row.Slice(i - step, step)) & AlphaMask) != Vector<byte>.Zero)
                {
                    for (var pos = i - 1; pos >= i - step; pos -= 4)
                    {
                        if (row[pos] != 0)
                        {
                            return pos >> 2;
                        }
                    }
                }
            }
        }

        for (var pos = i - 1; pos >= 0; pos -= 4)
        {
            if (row[pos] != 0)
            {
                return pos >> 2;
            }
        }

        return -1;
    }

    [Benchmark] public int CropTransparentSides_Current() { Cur_FindOpaqueSides(_line, LineWidth, LineHeight, out var l, out var r); return l + r; }
    [Benchmark] public int CropTransparentSides_RowMajor() { New_FindOpaqueSides(_line, LineWidth, LineHeight, out var l, out var r); return l + r; }

    // The mutating benchmarks below restore _scratch from _line first; this measures that
    // restore alone so the copy can be subtracted from both sides of each pair.
    [Benchmark] public void Scratch_CopyOnly() => _line.CopyTo(_scratch, 0);

    // ================================================================ 6. Fill

    private const byte FillB = 20, FillG = 40, FillR = 60, FillA = 200;

    private static void Cur_Fill(byte[] data)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            data[i] = FillB;
            data[i + 1] = FillG;
            data[i + 2] = FillR;
            data[i + 3] = FillA;
        }
    }

    private static void New_Fill(byte[] data)
    {
        MemoryMarshal.Cast<byte, uint>(data.AsSpan()).Fill(Pack(FillB, FillG, FillR, FillA));
    }

    private static uint Pack(byte b, byte g, byte r, byte a)
    {
        Span<byte> bytes = stackalloc byte[4];
        bytes[0] = b;
        bytes[1] = g;
        bytes[2] = r;
        bytes[3] = a;
        return MemoryMarshal.Read<uint>(bytes);
    }

    [Benchmark] public void Fill_Current() { _line.CopyTo(_scratch, 0); Cur_Fill(_scratch); }
    [Benchmark] public void Fill_SpanFill() { _line.CopyTo(_scratch, 0); New_Fill(_scratch); }

    // ================================================================ 7. InvertColors

    private static void Cur_InvertColors(byte[] data)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            data[i] = (byte)~data[i];
            data[i + 1] = (byte)~data[i + 1];
            data[i + 2] = (byte)~data[i + 2];
        }
    }

    private static void New_InvertColors(byte[] data)
    {
        var i = 0;
        var step = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated && data.Length >= step)
        {
            var span = data.AsSpan();
            for (; i <= data.Length - step; i += step)
            {
                (new Vector<byte>(span.Slice(i, step)) ^ RgbMask).CopyTo(span.Slice(i, step));
            }
        }

        for (; i < data.Length; i += 4)
        {
            data[i] = (byte)~data[i];
            data[i + 1] = (byte)~data[i + 1];
            data[i + 2] = (byte)~data[i + 2];
        }
    }

    [Benchmark] public void InvertColors_Current() { _line.CopyTo(_scratch, 0); Cur_InvertColors(_scratch); }
    [Benchmark] public void InvertColors_Simd() { _line.CopyTo(_scratch, 0); New_InvertColors(_scratch); }

    // ================================================================ 8. ReplaceTransparentWith

    private static void Cur_ReplaceTransparentWith(byte[] data)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] < 10)
            {
                data[i] = FillB;
                data[i + 1] = FillG;
                data[i + 2] = FillR;
                data[i + 3] = FillA;
            }
        }
    }

    private static void New_ReplaceTransparentWith(byte[] data)
    {
        var replacement = Pack(FillB, FillG, FillR, FillA);
        var i = 0;
        var step = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated && BitConverter.IsLittleEndian && data.Length >= step)
        {
            var span = data.AsSpan();
            var alphaMask = new Vector<uint>(0xFF000000u);
            var limit = new Vector<uint>(10u << 24);
            var replacementVector = new Vector<uint>(replacement);
            for (; i <= data.Length - step; i += step)
            {
                var raw = Vector.AsVectorUInt32(new Vector<byte>(span.Slice(i, step)));
                var isTransparent = Vector.LessThan(raw & alphaMask, limit);
                Vector.AsVectorByte(Vector.ConditionalSelect(isTransparent, replacementVector, raw)).CopyTo(span.Slice(i, step));
            }
        }

        var pixels = MemoryMarshal.Cast<byte, uint>(data.AsSpan());
        for (var p = i / 4; p < pixels.Length; p++)
        {
            if (data[p * 4 + 3] < 10)
            {
                pixels[p] = replacement;
            }
        }
    }

    [Benchmark] public void ReplaceTransparentWith_Current() { _line.CopyTo(_scratch, 0); Cur_ReplaceTransparentWith(_scratch); }
    [Benchmark] public void ReplaceTransparentWith_Simd() { _line.CopyTo(_scratch, 0); New_ReplaceTransparentWith(_scratch); }

    // ================================================================ 9. MakeOneColor

    private static void Cur_MakeOneColor(byte[] data)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i] > 20)
            {
                data[i] = FillB;
                data[i + 1] = FillG;
                data[i + 2] = FillR;
                data[i + 3] = FillA;
            }
            else
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 0;
                data[i + 3] = 0;
            }
        }
    }

    private static void New_MakeOneColor(byte[] data)
    {
        var color = Pack(FillB, FillG, FillR, FillA);
        var i = 0;
        var step = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated && BitConverter.IsLittleEndian && data.Length >= step)
        {
            var span = data.AsSpan();
            var lowByte = new Vector<uint>(0x000000FFu);
            var limit = new Vector<uint>(20u);
            var colorVector = new Vector<uint>(color);
            for (; i <= data.Length - step; i += step)
            {
                var raw = Vector.AsVectorUInt32(new Vector<byte>(span.Slice(i, step)));
                var keep = Vector.GreaterThan(raw & lowByte, limit);
                Vector.AsVectorByte(Vector.ConditionalSelect(keep, colorVector, Vector<uint>.Zero)).CopyTo(span.Slice(i, step));
            }
        }

        var pixels = MemoryMarshal.Cast<byte, uint>(data.AsSpan());
        for (var p = i / 4; p < pixels.Length; p++)
        {
            pixels[p] = data[p * 4] > 20 ? color : 0u;
        }
    }

    [Benchmark] public void MakeOneColor_Current() { _line.CopyTo(_scratch, 0); Cur_MakeOneColor(_scratch); }
    [Benchmark] public void MakeOneColor_Simd() { _line.CopyTo(_scratch, 0); New_MakeOneColor(_scratch); }

    // ================================================================ 10. MakeTwoColor

    private static readonly byte[] TwoColorBackground = { 10, 20, 30, 255 };
    private static readonly byte[] TwoColorForeground = { 250, 240, 230, 255 };

    private static void Cur_MakeTwoColor(byte[] data, int minRgb)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] < 1 || data[i] + data[i + 1] + data[i + 2] < minRgb)
            {
                Buffer.BlockCopy(TwoColorBackground, 0, data, i, 4);
            }
            else
            {
                Buffer.BlockCopy(TwoColorForeground, 0, data, i, 4);
            }
        }
    }

    private static void New_MakeTwoColor(byte[] data, int minRgb)
    {
        var background = MemoryMarshal.Read<uint>(TwoColorBackground);
        var foreground = MemoryMarshal.Read<uint>(TwoColorForeground);
        var i = 0;
        var step = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated && BitConverter.IsLittleEndian && minRgb >= 0 && data.Length >= step + 2)
        {
            var lowByte = new Vector<uint>(0x000000FFu);
            var alphaMask = new Vector<uint>(0xFF000000u);
            var limit = new Vector<uint>((uint)minRgb);
            var backgroundVector = new Vector<uint>(background);
            var foregroundVector = new Vector<uint>(foreground);
            for (; i + step + 2 <= data.Length; i += step)
            {
                var raw = Vector.AsVectorUInt32(new Vector<byte>(data, i));
                var green = Vector.AsVectorUInt32(new Vector<byte>(data, i + 1)) & lowByte;
                var red = Vector.AsVectorUInt32(new Vector<byte>(data, i + 2)) & lowByte;
                var sum = (raw & lowByte) + green + red;
                var isBackground = Vector.Equals(raw & alphaMask, Vector<uint>.Zero) | Vector.LessThan(sum, limit);
                Vector.AsVectorByte(Vector.ConditionalSelect(isBackground, backgroundVector, foregroundVector)).CopyTo(data, i);
            }
        }

        var pixels = MemoryMarshal.Cast<byte, uint>(data.AsSpan());
        for (var p = i / 4; p < pixels.Length; p++)
        {
            var b = p * 4;
            pixels[p] = data[b + 3] < 1 || data[b] + data[b + 1] + data[b + 2] < minRgb ? background : foreground;
        }
    }

    [Benchmark] public void MakeTwoColor_Current() { _line.CopyTo(_scratch, 0); Cur_MakeTwoColor(_scratch, 300); }
    [Benchmark] public void MakeTwoColor_Simd() { _line.CopyTo(_scratch, 0); New_MakeTwoColor(_scratch, 300); }

    // ================================================================ 11. spectrogram pixel write

    private static unsafe void Cur_WritePixels(byte[] bitmap, byte[] colorIndexes, uint[] palette, int width, int height)
    {
        var stride = width * 4;
        fixed (byte* pixels = bitmap)
        fixed (uint* pal = palette)
        {
            for (var x = 0; x < width; x++)
            {
                var xOffset = x * 4;
                for (var y = 0; y < height; y++)
                {
                    var color = pal[colorIndexes[x * height + y]];
                    var pixelY = height - y - 1;
                    var pixel = pixels + pixelY * stride + xOffset;
                    pixel[0] = (byte)color;
                    pixel[1] = (byte)(color >> 8);
                    pixel[2] = (byte)(color >> 16);
                    pixel[3] = (byte)(color >> 24);
                }
            }
        }
    }

    /// <summary>
    /// One 32-bit store per pixel instead of four byte stores, and the destination address is
    /// walked backwards a row at a time instead of being recomputed with a multiply per pixel.
    /// </summary>
    private static unsafe void New_WritePixels(byte[] bitmap, byte[] colorIndexes, uint[] palette, int width, int height)
    {
        var stride = width * 4;
        fixed (byte* pixels = bitmap)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = x * height;
                var dst = pixels + (height - 1) * stride + x * 4;
                for (var y = 0; y < height; y++)
                {
                    *(uint*)dst = palette[colorIndexes[offset + y]];
                    dst -= stride;
                }
            }
        }
    }

    [Benchmark] public void SpectrogramWritePixels_Current() => Cur_WritePixels(_bitmap, _colorIndexes, _paletteRgba, ImageWidth, Nfft / 2);
    [Benchmark] public void SpectrogramWritePixels_PackedStore() => New_WritePixels(_bitmap, _colorIndexes, _paletteRgba, ImageWidth, Nfft / 2);

    // ================================================================ 12. float[] -> double[] chunk

    private static void Cur_CopyChunk(float[] source, int offset, double[] destination, int count)
    {
        for (var i = 0; i < count; i++)
        {
            destination[i] = source[offset + i];
        }
    }

    private static void New_CopyChunk(float[] source, int offset, double[] destination, int count)
    {
        var src = source.AsSpan(offset, count);
        var dst = destination.AsSpan(0, count);
        var i = 0;
        var step = Vector<float>.Count;
        if (Vector.IsHardwareAccelerated && count >= step)
        {
            for (; i <= count - step; i += step)
            {
                Vector.Widen(new Vector<float>(src.Slice(i, step)), out var low, out var high);
                low.CopyTo(dst.Slice(i, Vector<double>.Count));
                high.CopyTo(dst.Slice(i + Vector<double>.Count, Vector<double>.Count));
            }
        }

        for (; i < count; i++)
        {
            dst[i] = src[i];
        }
    }

    [Benchmark] public void SpectrogramChunkCopy_Current() => Cur_CopyChunk(_rawSamples, 0, _chunkSamples, _chunkSamples.Length);
    [Benchmark] public void SpectrogramChunkCopy_Widen() => New_CopyChunk(_rawSamples, 0, _chunkSamples, _chunkSamples.Length);
}
