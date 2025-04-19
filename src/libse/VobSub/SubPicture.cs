using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;

public class SubPicture
{
    private readonly byte[] _data;

    public SubPicture(byte[] data)
    {
        _data = data;
    }

    public SKBitmap GenerateBitmap(Rectangle imageDisplayArea, int imageTopFieldDataAddress, int imageBottomFieldDataAddress, List<Color> fourColors, bool crop)
    {
        if (imageDisplayArea.Width <= 0 || imageDisplayArea.Height <= 0)
            return new SKBitmap(1, 1);

        var width = imageDisplayArea.Width + 1;
        var height = imageDisplayArea.Height + 1;
        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var canvas = new SKCanvas(bitmap);

        var bgColor = ToSKColor(fourColors[0]);
        if (bgColor != SKColors.Transparent)
        {
            canvas.Clear(bgColor);
        }

        var fastBmp = new FastSkiaBitmap(bitmap);
        GenerateBitmap(_data, fastBmp, 0, imageTopFieldDataAddress, fourColors, 2);
        GenerateBitmap(_data, fastBmp, 1, imageBottomFieldDataAddress, fourColors, 2);

        return CropBitmapAndUnlock(fastBmp, bgColor, crop);
    }

    private void GenerateBitmap(byte[] pixelData, FastSkiaBitmap bmp, int field, int imageDataAddress, List<Color> fourColors, int pixelDepth)
    {
        int width = bmp.Width;
        int height = bmp.Height;
        int pitch = width;
        int x = 0;
        int y = field;
        int idx = imageDataAddress;
        int colorIdx = 0;

        while (idx + 1 < pixelData.Length)
        {
            int b1 = pixelData[idx++];
            int b2 = pixelData[idx++];

            int count = (b1 & 0x3F);
            bool isTwoByte = (b1 & 0x40) != 0;
            bool isEndOfLine = (b1 & 0x80) != 0;

            if (isTwoByte && idx < pixelData.Length)
            {
                count = ((count << 8) | pixelData[idx++]);
            }

            SKColor color = ToSKColor(fourColors[b2 & 0x03]);

            for (int i = 0; i < count; i++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    bmp.SetPixel(x, y, color);
                }
                x++;
            }

            if (isEndOfLine)
            {
                y += 2;
                x = 0;
            }
        }
    }

    private static SKBitmap CropBitmapAndUnlock(FastSkiaBitmap bmp, SKColor backgroundColor, bool crop)
    {
        int minX = 0, maxX = bmp.Width - 1;
        int minY = 0, maxY = bmp.Height - 1;

        if (crop)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                bool isBg = true;
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (!IsBackgroundColor(bmp.GetPixel(x, y)))
                    {
                        isBg = false;
                        break;
                    }
                }
                if (!isBg)
                {
                    minY = Math.Max(0, y - 3);
                    break;
                }
            }

            for (int y = bmp.Height - 1; y >= minY; y--)
            {
                bool isBg = true;
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (!IsBackgroundColor(bmp.GetPixel(x, y)))
                    {
                        isBg = false;
                        break;
                    }
                }
                if (!isBg)
                {
                    maxY = Math.Min(bmp.Height - 1, y + 7);
                    break;
                }
            }

            for (int x = 0; x < bmp.Width; x++)
            {
                bool isBg = true;
                for (int y = minY; y <= maxY; y++)
                {
                    if (!IsBackgroundColor(bmp.GetPixel(x, y)))
                    {
                        isBg = false;
                        break;
                    }
                }
                if (!isBg)
                {
                    minX = Math.Max(0, x - 3);
                    break;
                }
            }

            for (int x = bmp.Width - 1; x >= minX; x--)
            {
                bool isBg = true;
                for (int y = minY; y <= maxY; y++)
                {
                    if (!IsBackgroundColor(bmp.GetPixel(x, y)))
                    {
                        isBg = false;
                        break;
                    }
                }
                if (!isBg)
                {
                    maxX = Math.Min(bmp.Width - 1, x + 7);
                    break;
                }
            }
        }

        var cropped = new SKBitmap(maxX - minX + 1, maxY - minY + 1);
        using (var canvas = new SKCanvas(cropped))
        {
            canvas.DrawBitmap(bmp.Bitmap, new SKRect(minX, minY, maxX + 1, maxY + 1), new SKRect(0, 0, cropped.Width, cropped.Height));
            return cropped;
        }
    }

    private static bool IsBackgroundColor(SKColor c) => c.Alpha < 2;

    private static SKColor ToSKColor(Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}

public class FastSkiaBitmap
{
    public SKBitmap Bitmap { get; }
    public int Width => Bitmap.Width;
    public int Height => Bitmap.Height;

    public FastSkiaBitmap(SKBitmap bitmap)
    {
        Bitmap = bitmap;
    }

    public SKColor GetPixel(int x, int y) => Bitmap.GetPixel(x, y);

    public void SetPixel(int x, int y, SKColor color)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            Bitmap.SetPixel(x, y, color);
    }
}
