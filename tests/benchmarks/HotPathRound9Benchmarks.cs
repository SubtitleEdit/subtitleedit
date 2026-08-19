using Avalonia;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NikseBitmap = Nikse.SubtitleEdit.Core.Common.NikseBitmap;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 9: span / SIMD rewrites of byte-, sample- and pixel-level loops in src/ui.
/// Each candidate is measured as (Old = faithful replica of the previous code) vs
/// (New = the real, now-internal implementation), on the same synthesized data.
///
/// Cluster 1 - the video-open waveform/spectrogram path (WaveToVisualizer2):
///   peak-file load, highest-peak reduction, per-chunk sample conversion, chunk min/max.
/// Cluster 2 - per-image OCR pixel loops:
///   BDN make-transparent, Paddle transparent-to-black, Tesseract ink percent, invert colors.
/// Cluster 3 - the per-frame spectrogram blit (SKBitmap -> Avalonia WriteableBitmap).
/// </summary>
[MemoryDiagnoser]
public class HotPathRound9Benchmarks
{
    private const int PeakCount = 1_000_000;          // ~2.2 h of audio at 126 peaks/s
    private const int ChunkSampleCount = 762;         // 48000 Hz / 126 peaks/s * 2 slots
    private const int SpectroChunkShorts = 65_536;
    private const int ImageWidth = 1280;
    private const int ImageHeight = 720;

    private WavePeak2[] _peaks = null!;
    private byte[] _peakFileBytes = null!;
    private WavePeak2[] _peakLoadTarget = null!;
    private short[] _monoChunk = null!;
    private byte[] _monoChunkBytes = null!;
    private short[] _stereoChunk = null!;
    private byte[] _stereoChunkBytes = null!;
    private float[] _chunkSamples = null!;
    private float[] _spectroTarget = null!;
    private SKBitmap _bdnBitmap = null!;
    private SKBitmap _paddleBitmapOld = null!;
    private SKBitmap _paddleBitmapNew = null!;
    private SKBitmap _invertBitmap = null!;
    private SKBitmap _premulBitmap = null!;
    private NikseBitmap _inkBitmap = null!;

    private static bool _avaloniaInitialized;

    [GlobalSetup]
    public void Setup()
    {
        if (!_avaloniaInitialized)
        {
            _avaloniaInitialized = true;
            AppBuilder.Configure<Application>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .UseSkia()
                .SetupWithoutStarting();
        }

        var random = new Random(42);

        _peaks = new WavePeak2[PeakCount];
        for (var i = 0; i < _peaks.Length; i++)
        {
            _peaks[i] = new WavePeak2((short)random.Next(0, short.MaxValue), (short)random.Next(short.MinValue, 0));
        }

        _peakFileBytes = MemoryMarshal.AsBytes(_peaks.AsSpan()).ToArray();
        _peakLoadTarget = new WavePeak2[PeakCount + 5];

        _monoChunk = new short[ChunkSampleCount / 2];
        _stereoChunk = new short[ChunkSampleCount];
        for (var i = 0; i < _stereoChunk.Length; i++)
        {
            _stereoChunk[i] = (short)random.Next(short.MinValue, short.MaxValue);
            if (i < _monoChunk.Length)
            {
                _monoChunk[i] = (short)random.Next(short.MinValue, short.MaxValue);
            }
        }

        _monoChunkBytes = MemoryMarshal.AsBytes(_monoChunk.AsSpan()).ToArray();
        _stereoChunkBytes = MemoryMarshal.AsBytes(_stereoChunk.AsSpan()).ToArray();
        _chunkSamples = new float[ChunkSampleCount];
        _spectroTarget = new float[SpectroChunkShorts];

        _bdnBitmap = MakeTestBitmap(random, opaqueBackground: true);
        _paddleBitmapOld = MakeTestBitmap(random, opaqueBackground: false);
        _paddleBitmapNew = MakeTestBitmap(random, opaqueBackground: false);
        _invertBitmap = MakeTestBitmap(random, opaqueBackground: true);
        _premulBitmap = MakeTestBitmap(random, opaqueBackground: true, premul: true);
        _inkBitmap = new NikseBitmap(MakeTestBitmap(random, opaqueBackground: true));
    }

    private static SKBitmap MakeTestBitmap(Random random, bool opaqueBackground, bool premul = false)
    {
        var info = new SKImageInfo(ImageWidth, ImageHeight, SKColorType.Bgra8888,
            premul ? SKAlphaType.Premul : SKAlphaType.Unpremul);
        var bmp = new SKBitmap(info);
        unsafe
        {
            var ptr = (uint*)bmp.GetPixels();
            var count = ImageWidth * ImageHeight;
            for (var i = 0; i < count; i++)
            {
                // ~70% background-colored pixels, rest random "text" pixels, some transparent.
                var roll = random.Next(100);
                if (roll < 70)
                {
                    ptr[i] = opaqueBackground ? 0xFF101010 : 0x00000000;
                }
                else if (roll < 80)
                {
                    ptr[i] = 0x30FFFFFF; // semi-transparent
                }
                else
                {
                    ptr[i] = (uint)random.Next() | 0xFF000000;
                }
            }
        }

        return bmp;
    }

    // ---------------------------------------------------------------- 1: highest peak

    [Benchmark]
    public int HighestPeak_Old()
    {
        // Replica of the old CalculateHighestPeak: IList enumeration + per-peak Abs.
        IList<WavePeak2> peaks = _peaks;
        var highestPeak = 0;
        foreach (var peak in peaks)
        {
            int abs = peak.Abs;
            if (abs > highestPeak)
            {
                highestPeak = abs;
            }
        }

        return highestPeak;
    }

    [Benchmark]
    public int HighestPeak_New() => WavePeakData2.CalculateHighestPeak(_peaks);

    // ---------------------------------------------------------------- 2: peak file load

    [Benchmark]
    public int LoadPeaksStereo_Old()
    {
        var data = _peakFileBytes;
        var peaks = _peakLoadTarget;
        var peakIndex = 0;
        var byteIndex = 0;
        while (byteIndex < data.Length)
        {
            short max = Unsafe.ReadUnaligned<short>(ref data[byteIndex]);
            byteIndex += 2;
            short min = Unsafe.ReadUnaligned<short>(ref data[byteIndex]);
            byteIndex += 2;
            peaks[peakIndex++] = new WavePeak2(max, min);
        }

        return peakIndex;
    }

    [Benchmark]
    public int LoadPeaksStereo_New()
    {
        var data = _peakFileBytes;
        var src = MemoryMarshal.Cast<byte, WavePeak2>(data.AsSpan(0, data.Length - data.Length % 4));
        src.CopyTo(_peakLoadTarget);
        return src.Length;
    }

    private delegate int ReadSampleDataValue(byte[] data, ref int index);

    private static readonly ReadSampleDataValue ReadValue16Bit = static (byte[] data, ref int index) =>
    {
        var result = Unsafe.ReadUnaligned<short>(ref data[index]);
        index += 2;
        return result;
    };

    // ---------------------------------------------------------------- 4: spectrogram chunk, 16-bit stereo

    [Benchmark]
    public float SpectroChunkStereo_Old()
    {
        // Replica of the old spectrogram conversion: delegate per channel per sample.
        var data = _stereoChunkBytes;
        var target = _spectroTarget;
        const double scale = 0.5 / short.MaxValue;
        var offset = 0;
        var dataByteOffset = 0;
        while (dataByteOffset < data.Length)
        {
            double value = 0D;
            for (var iChannel = 0; iChannel < 2; iChannel++)
            {
                value += ReadValue16Bit(data, ref dataByteOffset);
            }

            target[offset] = (float)(value * scale);
            offset += 1;
        }

        return target[0];
    }

    [Benchmark]
    public float SpectroChunkStereo_New()
    {
        const double scale = 0.5 / short.MaxValue;
        var shorts = MemoryMarshal.Cast<byte, short>(_stereoChunkBytes.AsSpan());
        WavePeakGenerator2.ConvertSpectrogramChunk16BitStereo(shorts, _spectroTarget.AsSpan(0, shorts.Length / 2), scale);
        return _spectroTarget[0];
    }

    [Benchmark]
    public float SpectroChunkMono_Old()
    {
        var data = _monoChunkBytes;
        var target = _spectroTarget;
        const double scale = 1.0 / short.MaxValue;
        var offset = 0;
        var dataByteOffset = 0;
        while (dataByteOffset < data.Length)
        {
            double value = 0D;
            value += ReadValue16Bit(data, ref dataByteOffset);
            target[offset] = (float)(value * scale);
            offset += 1;
        }

        return target[0];
    }

    [Benchmark]
    public float SpectroChunkMono_New()
    {
        const double scale = 1.0 / short.MaxValue;
        var shorts = MemoryMarshal.Cast<byte, short>(_monoChunkBytes.AsSpan());
        WavePeakGenerator2.ConvertSpectrogramChunk16BitMono(shorts, _spectroTarget.AsSpan(0, shorts.Length), scale);
        return _spectroTarget[0];
    }

    // ---------------------------------------------------------------- 5: chunk min/max

    [Benchmark]
    public int CalculatePeak_Old()
    {
        var chunk = _chunkSamples;
        float max = chunk[0];
        float min = chunk[0];
        for (var i = 1; i < chunk.Length; i++)
        {
            float value = chunk[i];
            max = Math.Max(max, value);
            min = Math.Min(min, value);
        }

        return (short)(short.MaxValue * max) - (short)(short.MaxValue * min);
    }

    [Benchmark]
    public int CalculatePeak_New()
    {
        var peak = WavePeakGenerator2.CalculatePeak(_chunkSamples, _chunkSamples.Length);
        return peak.Max - peak.Min;
    }

    // ---------------------------------------------------------------- 6: SKBitmap -> Avalonia (premul)

    [Benchmark]
    public int ToAvaloniaPremul_Old()
    {
        // Replica of the old per-pixel branch loop (premul source).
        var skBitmap = _premulBitmap;
        var bitmap = new WriteableBitmap(
            new PixelSize(skBitmap.Width, skBitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using (var lockedBitmap = bitmap.Lock())
        {
            unsafe
            {
                var width = skBitmap.Width;
                var height = skBitmap.Height;
                var srcStride = skBitmap.RowBytes;
                var dstStride = lockedBitmap.RowBytes;
                var srcBase = (byte*)skBitmap.GetPixels();
                var dstBase = (byte*)lockedBitmap.Address;
                for (var y = 0; y < height; y++)
                {
                    var srcRow = (uint*)(srcBase + (y * srcStride));
                    var dstRow = (uint*)(dstBase + (y * dstStride));
                    for (var x = 0; x < width; x++)
                    {
                        var pixel = srcRow[x];
                        var a = pixel >> 24;
                        if (a == 255)
                        {
                            dstRow[x] = pixel;
                        }
                        else if (a == 0)
                        {
                            dstRow[x] = 0;
                        }
                        else
                        {
                            dstRow[x] = pixel; // premul source: copy as-is
                        }
                    }
                }
            }
        }

        var result = bitmap.PixelSize.Width;
        bitmap.Dispose();
        return result;
    }

    [Benchmark]
    public int ToAvaloniaPremul_New()
    {
        var bitmap = _premulBitmap.ToAvaloniaBitmap();
        var result = bitmap.PixelSize.Width;
        bitmap.Dispose();
        return result;
    }

    // ---------------------------------------------------------------- 7: BDN make transparent

    [Benchmark]
    public int BdnMakeTransparent_Old()
    {
        // Replica of the old GetPixel/SetPixel double loop (column-major, per-pixel interop).
        var original = _bdnBitmap;
        var imageInfo = new SKImageInfo(original.Width, original.Height, SKColorType.Rgba8888);
        var transparent = new SKBitmap(imageInfo);
        using (var canvas = new SKCanvas(transparent))
        {
            canvas.Clear(SKColors.Transparent);
            var bgColor = original.GetPixel(0, 0);
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    var pixel = original.GetPixel(x, y);
                    if (pixel.Red == bgColor.Red && pixel.Green == bgColor.Green && pixel.Blue == bgColor.Blue)
                    {
                        transparent.SetPixel(x, y, SKColors.Transparent);
                    }
                    else
                    {
                        transparent.SetPixel(x, y, pixel);
                    }
                }
            }
        }

        var w = transparent.Width;
        transparent.Dispose();
        return w;
    }

    [Benchmark]
    public int BdnMakeTransparent_New()
    {
        var transparent = OcrSubtitleBdn.MakeTransparent(_bdnBitmap);
        var w = transparent.Width;
        transparent.Dispose();
        return w;
    }

    // ---------------------------------------------------------------- 8: Paddle transparent -> black

    [Benchmark]
    public int PaddleTransparentBlack_Old()
    {
        // Replica of the old Pixels get/set version (allocates + copies the image twice).
        var workingBitmap = _paddleBitmapOld;
        var colors = workingBitmap.Pixels;
        var blackOpaque = new SKColor(0, 0, 0, 255);
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].Alpha < 100)
            {
                colors[i] = blackOpaque;
            }
        }

        workingBitmap.Pixels = colors;
        return colors.Length;
    }

    [Benchmark]
    public int PaddleTransparentBlack_New()
    {
        var result = PaddleOcr.MakeTransparentBlack(_paddleBitmapNew);
        return result.Width;
    }

    // ---------------------------------------------------------------- 9: ink percent

    [Benchmark]
    public double InkPercent_Old()
    {
        // Replica of the old per-pixel GetPixel version.
        var nbmp = _inkBitmap;
        long total = (long)nbmp.Width * nbmp.Height;
        long ink = 0;
        for (var y = 0; y < nbmp.Height; y++)
        {
            for (var x = 0; x < nbmp.Width; x++)
            {
                var c = nbmp.GetPixel(x, y);
                if (c.Alpha > 0 && c.Red < 128 && c.Green < 128 && c.Blue < 128)
                {
                    ink++;
                }
            }
        }

        return ink * 100.0 / total;
    }

    [Benchmark]
    public double InkPercent_New() => TesseractOcr.GetInkPercent(_inkBitmap);

    // ---------------------------------------------------------------- 10: invert colors

    [Benchmark]
    public int InvertColors_Old()
    {
        // Replica of the old unpack/invert/repack loop.
        var bitmap = _invertBitmap;
        var inverted = new SKBitmap(bitmap.Width, bitmap.Height);
        unsafe
        {
            var srcPixels = (uint*)bitmap.GetPixels().ToPointer();
            var dstPixels = (uint*)inverted.GetPixels().ToPointer();
            var totalPixels = bitmap.Width * bitmap.Height;
            for (var i = 0; i < totalPixels; i++)
            {
                var pixel = srcPixels[i];
                var a = (pixel >> 24) & 0xFF;
                var r = (pixel >> 16) & 0xFF;
                var g = (pixel >> 8) & 0xFF;
                var b = pixel & 0xFF;
                r = 255 - r;
                g = 255 - g;
                b = 255 - b;
                dstPixels[i] = (a << 24) | (r << 16) | (g << 8) | b;
            }
        }

        var w = inverted.Width;
        inverted.Dispose();
        return w;
    }

    [Benchmark]
    public int InvertColors_New()
    {
        var inverted = PreProcessingSettings.InvertColors(_invertBitmap);
        var w = inverted.Width;
        inverted.Dispose();
        return w;
    }
}
