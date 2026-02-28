using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 3)]
public class MakeOneColorBenchmark
{
    private SKBitmap _testBitmap = null!;
    private byte[] _pngBytes = null!;
    private SKColor _targetColor;

    [Params(32, 64, 128)]
    public int BitmapSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _targetColor = new SKColor(200, 200, 200);
        _testBitmap = CreateTestBitmap(BitmapSize, BitmapSize);

        using var image = SKImage.FromBitmap(_testBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        _pngBytes = data.ToArray();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _testBitmap?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public SKBitmap PixelOnly_GetSetPixel()
    {
        return MakeOneColorGetSetPixel(_testBitmap);
    }

    [Benchmark]
    public SKBitmap PixelOnly_UnsafePointers()
    {
        return MakeOneColorUnsafe(_testBitmap);
    }

    [Benchmark]
    public SKBitmap FullPipeline_PngRoundTrip()
    {
        return MakeOneColorFullPipelineOld(_pngBytes);
    }

    [Benchmark]
    public SKBitmap FullPipeline_DirectDecode()
    {
        return MakeOneColorFullPipelineNew(_pngBytes);
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
                    byte intensity = (byte)((x + y) % 256);
                    uint pixel = (255u << 24) | ((uint)intensity << 16) | ((uint)intensity << 8) | intensity;
                    row[x] = pixel;
                }
            }
        }

        return bitmap;
    }

    // Old pixel loop: GetPixel/SetPixel
    private SKBitmap MakeOneColorGetSetPixel(SKBitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                var intensity = (pixel.Red * 0.299 + pixel.Green * 0.587 + pixel.Blue * 0.114) / 255.0;
                var newRed = (byte)(_targetColor.Red * intensity);
                var newGreen = (byte)(_targetColor.Green * intensity);
                var newBlue = (byte)(_targetColor.Blue * intensity);
                result.SetPixel(x, y, new SKColor(newRed, newGreen, newBlue, pixel.Alpha));
            }
        }

        return result;
    }

    // Optimized pixel loop: unsafe pointers
    private SKBitmap MakeOneColorUnsafe(SKBitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            byte* srcBase = (byte*)bitmap.GetPixels();
            byte* dstBase = (byte*)result.GetPixels();
            int srcStride = bitmap.RowBytes;
            int dstStride = result.RowBytes;

            for (int y = 0; y < height; y++)
            {
                uint* srcRow = (uint*)(srcBase + y * srcStride);
                uint* dstRow = (uint*)(dstBase + y * dstStride);

                for (int x = 0; x < width; x++)
                {
                    uint pixel = srcRow[x];
                    byte b = (byte)(pixel & 0xFF);
                    byte g = (byte)((pixel >> 8) & 0xFF);
                    byte r = (byte)((pixel >> 16) & 0xFF);
                    byte a = (byte)(pixel >> 24);

                    double intensity = (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;

                    byte newR = (byte)(_targetColor.Red * intensity);
                    byte newG = (byte)(_targetColor.Green * intensity);
                    byte newB = (byte)(_targetColor.Blue * intensity);

                    dstRow[x] = (uint)(a << 24) | (uint)(newR << 16) | (uint)(newG << 8) | newB;
                }
            }
        }

        return result;
    }

    // Old full pipeline: PNG decode → PNG encode (ToSkBitmap) → GetPixel/SetPixel → PNG encode (ToAvaloniaBitmap)
    private SKBitmap MakeOneColorFullPipelineOld(byte[] pngBytes)
    {
        // Simulate ToSkBitmap: PNG decode
        using var ms = new MemoryStream(pngBytes);
        using var skData = SKData.CreateCopy(pngBytes);
        using var skImage = SKImage.FromEncodedData(skData);
        var decoded = new SKBitmap(skImage!.Width, skImage.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(decoded))
        {
            canvas.DrawImage(skImage, 0, 0);
        }

        // GetPixel/SetPixel processing
        var result = MakeOneColorGetSetPixel(decoded);
        decoded.Dispose();

        // Simulate ToAvaloniaBitmap: PNG encode (just encode to measure the cost)
        using var outImage = SKImage.FromBitmap(result);
        using var outData = outImage.Encode(SKEncodedImageFormat.Png, 100);
        _ = outData.ToArray();

        return result;
    }

    // New full pipeline: SKBitmap.Decode → unsafe pointers → direct pixel copy out
    private unsafe SKBitmap MakeOneColorFullPipelineNew(byte[] pngBytes)
    {
        // Direct decode (simulates SKBitmap.Decode(filePath))
        using var decoded = SKBitmap.Decode(pngBytes);

        // Unsafe pointer processing
        var result = MakeOneColorUnsafe(decoded);

        // Simulate ToAvaloniaBitmap optimized path: direct pixel copy (no PNG)
        var width = result.Width;
        var height = result.Height;
        var output = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        byte* src = (byte*)result.GetPixels();
        byte* dst = (byte*)output.GetPixels();
        Buffer.MemoryCopy(src, dst, output.ByteCount, result.RowBytes * height);

        result.Dispose();
        return output;
    }
}
