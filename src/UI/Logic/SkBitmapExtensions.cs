using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic;

internal static class SkBitmapExtensions
{
    public static SKBitmap MakeImageBrighter(SKBitmap bitmap, float brightnessIncrease = 0.25f)
    {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint();
        var colorMatrix = new[]
        {
            1 + brightnessIncrease, 0, 0, 0, 0,
            0, 1 + brightnessIncrease, 0, 0, 0,
            0, 0, 1 + brightnessIncrease, 0, 0,
            0, 0, 0, 1, 0 // Alpha stays the same
        };

        paint.ColorFilter = SKColorFilter.CreateColorMatrix(colorMatrix);

        canvas.DrawBitmap(bitmap, 0, 0, paint);

        return bitmap;
    }

    public static unsafe SKBitmap CropTransparentColors(this SKBitmap originalBitmap, byte alphaThreshold = 0)
    {
        if (originalBitmap.Width == 0 || originalBitmap.Height == 0)
        {
            return originalBitmap;
        }

        int width = originalBitmap.Width;
        int height = originalBitmap.Height;
        int bytesPerPixel = originalBitmap.BytesPerPixel;

        // Get pointer to the pixel data
        byte* ptr = (byte*)originalBitmap.GetPixels().ToPointer();
        int rowBytes = originalBitmap.RowBytes;

        // We assume standard 32-bit (RGBA/BGRA) where Alpha is usually at index 3
        // However, to be safe across color types, we use Skia's Alpha Type.
        int alphaOffset = originalBitmap.ColorType == SKColorType.Bgra8888 ||
                          originalBitmap.ColorType == SKColorType.Rgba8888 ? 3 : 0;

        int top = 0, bottom = height - 1, left = 0, right = width - 1;

        // 1. Find Top
        bool found = false;
        for (int y = 0; y < height && !found; y++)
        {
            byte* row = ptr + (y * rowBytes);
            for (int x = 0; x < width; x++)
            {
                if (row[x * bytesPerPixel + alphaOffset] > alphaThreshold)
                {
                    top = y;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            return new SKBitmap(1, 1); // Entirely transparent
        }

        // 2. Find Bottom
        found = false;
        for (int y = height - 1; y >= top && !found; y--)
        {
            byte* row = ptr + (y * rowBytes);
            for (int x = 0; x < width; x++)
            {
                if (row[x * bytesPerPixel + alphaOffset] > alphaThreshold)
                {
                    bottom = y;
                    found = true;
                    break;
                }
            }
        }

        // 3. Find Left
        found = false;
        for (int x = 0; x < width && !found; x++)
        {
            for (int y = top; y <= bottom; y++)
            {
                byte* pixel = ptr + (y * rowBytes) + (x * bytesPerPixel);
                if (pixel[alphaOffset] > alphaThreshold)
                {
                    left = x;
                    found = true;
                    break;
                }
            }
        }

        // 4. Find Right
        found = false;
        for (int x = width - 1; x >= left && !found; x--)
        {
            for (int y = top; y <= bottom; y++)
            {
                byte* pixel = ptr + (y * rowBytes) + (x * bytesPerPixel);
                if (pixel[alphaOffset] > alphaThreshold)
                {
                    right = x;
                    found = true;
                    break;
                }
            }
        }

        // Create the cropped bitmap using ExtractSubset for better performance
        var subset = new SKRectI(left, top, right + 1, bottom + 1);
        var destination = new SKBitmap();
        originalBitmap.ExtractSubset(destination, subset);

        return destination;
    }

    public static string ToBase64String(this SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data == null ? string.Empty : Convert.ToBase64String(data.ToArray());
    }

    public static byte[] ToPngArray(this SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    // Original simple code for ToAvaloniaBitmap :
    //public static Bitmap ToAvaloniaBitmapOld(this SKBitmap skBitmap)
    //{
    //    if (skBitmap.Width == 0 || skBitmap.Height == 0)
    //    {
    //        skBitmap = new SKBitmap(1, 1, true);
    //    }

    //    using var image = SKImage.FromBitmap(skBitmap);
    //    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    //    using var stream = new MemoryStream(data.ToArray());

    //    return new Bitmap(stream);
    //}

    public static Bitmap ToAvaloniaBitmap(this SKBitmap skBitmap)
    {
        if (skBitmap.Width <= 0 || skBitmap.Height <= 0)
        {
            return new WriteableBitmap(new PixelSize(1, 1), Vector.One, PixelFormat.Bgra8888, AlphaFormat.Premul);
        }

        var bitmap = new WriteableBitmap(
            new PixelSize(skBitmap.Width, skBitmap.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using (var lockedBitmap = bitmap.Lock())
        {
            int width = skBitmap.Width;
            int height = skBitmap.Height;
            int srcStride = skBitmap.RowBytes;
            int dstStride = lockedBitmap.RowBytes;

            unsafe
            {
                byte* srcBase = (byte*)skBitmap.GetPixels();
                byte* dstBase = (byte*)lockedBitmap.Address;

                for (var y = 0; y < height; y++)
                {
                    // Pointer arithmetic is faster than repeated multiplication
                    uint* srcRow = (uint*)(srcBase + (y * srcStride));
                    uint* dstRow = (uint*)(dstBase + (y * dstStride));

                    for (var x = 0; x < width; x++)
                    {
                        uint pixel = srcRow[x];
                        uint a = pixel >> 24;

                        if (a == 255)
                        {
                            dstRow[x] = pixel; // Fully opaque, no math needed
                        }
                        else if (a == 0)
                        {
                            dstRow[x] = 0; // Fully transparent
                        }
                        else
                        {
                            // Fast Premultiply Approximation: (color * alpha + 128) >> 8
                            // We do this for R, G, and B in one block
                            uint r = (pixel >> 16) & 0xFF;
                            uint g = (pixel >> 8) & 0xFF;
                            uint b = pixel & 0xFF;
                            r = (r * a + 128) >> 8;
                            g = (g * a + 128) >> 8;
                            b = (b * a + 128) >> 8;

                            dstRow[x] = (a << 24) | (r << 16) | (g << 8) | b;
                        }
                    }
                }
            }
        }

        return bitmap;
    }

    // Original simple code for ToSkBitmap:
    //public static SKBitmap ToSkBitmapOld(this Bitmap avaloniaBitmap)
    //{
    //    using var stream = new MemoryStream();
    //    avaloniaBitmap.Save(stream);
    //    stream.Seek(0, SeekOrigin.Begin);
    //    return SKBitmap.Decode(stream);
    //}

    public static SKBitmap ToSkBitmap(this Bitmap avaloniaBitmap)
    {
        if (avaloniaBitmap is WriteableBitmap writeableBitmap)
        {
            return ToSkBitmapDirect(writeableBitmap);
        }

        if (avaloniaBitmap.Format == PixelFormat.Bgra8888)
        {
            return ToSkBitmapViaCopyPixels(avaloniaBitmap);
        }

        using var ms = new MemoryStream();
        avaloniaBitmap.Save(ms);
        using var data = SKData.CreateCopy(ms.GetBuffer(), (nuint)ms.Length);
        using var image = SKImage.FromEncodedData(data);
        if (image == null)
        {
            return new SKBitmap(1, 1, true);
        }

        var skBitmap = new SKBitmap(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(skBitmap))
        {
            canvas.DrawImage(image, 0, 0);
        }

        return skBitmap;
    }

    private static SKBitmap ToSkBitmapViaCopyPixels(Bitmap bitmap)
    {
        var width = bitmap.PixelSize.Width;
        var height = bitmap.PixelSize.Height;
        var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        bitmap.CopyPixels(
            new PixelRect(0, 0, width, height),
            skBitmap.GetPixels(),
            skBitmap.ByteCount,
            skBitmap.RowBytes);

        return skBitmap;
    }

    private static unsafe SKBitmap ToSkBitmapDirect(WriteableBitmap writeableBitmap)
    {
        using var framebuffer = writeableBitmap.Lock();
        var width = writeableBitmap.PixelSize.Width;
        var height = writeableBitmap.PixelSize.Height;
        var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        byte* src = (byte*)framebuffer.Address;
        byte* dst = (byte*)skBitmap.GetPixels();
        int bytesToCopy = framebuffer.RowBytes * height;

        Buffer.MemoryCopy(src, dst, skBitmap.ByteCount, bytesToCopy);

        return skBitmap;
    }
}