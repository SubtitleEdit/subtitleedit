using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 3)]
public class ToSkBitmapBenchmark
{
    private SKBitmap _testBitmap = null!;

    [Params(64, 128, 256)]
    public int BitmapSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _testBitmap = CreateTestBitmap(BitmapSize, BitmapSize);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _testBitmap?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public SKBitmap PngEncodeDecode()
    {
        return ToSkBitmapViaPng(_testBitmap);
    }

    [Benchmark]
    public SKBitmap DirectMemoryCopy()
    {
        return ToSkBitmapDirect(_testBitmap);
    }

    [Benchmark]
    public SKBitmap CopyPixels()
    {
        return ToSkBitmapViaCopyPixels(_testBitmap);
    }

    private SKBitmap CreateTestBitmap(int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            byte* ptr = (byte*)bitmap.GetPixels();
            int stride = bitmap.RowBytes;

            for (int y = 0; y < height; y++)
            {
                uint* row = (uint*)(ptr + y * stride);
                for (int x = 0; x < width; x++)
                {
                    byte r = (byte)((x * 255) / width);
                    byte g = (byte)((y * 255) / height);
                    byte b = (byte)(((x + y) * 255) / (width + height));
                    uint pixel = (255u << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
                    row[x] = pixel;
                }
            }
        }

        return bitmap;
    }

    // Simulates the old ToSkBitmap: PNG encode → decode → canvas draw
    private SKBitmap ToSkBitmapViaPng(SKBitmap sourceBitmap)
    {
        using var image = SKImage.FromBitmap(sourceBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Position = 0;

        using var skData = SKData.CreateCopy(ms.GetBuffer(), (nuint)ms.Length);
        using var skImage = SKImage.FromEncodedData(skData);
        if (skImage == null)
        {
            return new SKBitmap(1, 1, true);
        }

        var result = new SKBitmap(skImage.Width, skImage.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(result))
        {
            canvas.DrawImage(skImage, 0, 0);
        }

        return result;
    }

    // Simulates WriteableBitmap path: Lock() + Buffer.MemoryCopy
    private SKBitmap ToSkBitmapDirect(SKBitmap sourceBitmap)
    {
        var result = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height,
                                  SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            byte* src = (byte*)sourceBitmap.GetPixels();
            byte* dst = (byte*)result.GetPixels();
            int bytesToCopy = sourceBitmap.RowBytes * sourceBitmap.Height;

            Buffer.MemoryCopy(src, dst, result.ByteCount, bytesToCopy);
        }

        return result;
    }

    // Simulates ImmutableBitmap path: Bitmap.CopyPixels (row-by-row Unsafe.CopyBlock)
    private SKBitmap ToSkBitmapViaCopyPixels(SKBitmap sourceBitmap)
    {
        var width = sourceBitmap.Width;
        var height = sourceBitmap.Height;
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            byte* src = (byte*)sourceBitmap.GetPixels();
            byte* dst = (byte*)result.GetPixels();
            int srcRowBytes = sourceBitmap.RowBytes;
            int dstRowBytes = result.RowBytes;
            int minStride = width * 4;

            for (int y = 0; y < height; y++)
            {
                var srcAddr = src + srcRowBytes * y;
                var dstAddr = dst + dstRowBytes * y;
                Unsafe.CopyBlock(dstAddr, srcAddr, (uint)minStride);
            }
        }

        return result;
    }
}
