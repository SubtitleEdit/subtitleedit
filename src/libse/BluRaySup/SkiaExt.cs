using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    public static class SkiaExt
    {
        public static int GetNonTransparentHeight(this SKBitmap bitmap)
        {
            var startY = 0;
            var transparentBottomPixels = 0;
            for (var y = 0; y < bitmap.Height; y++)
            {
                var isLineTransparent = bitmap.IsLineTransparent(y);
                if (startY == y && isLineTransparent)
                {
                    startY++;
                    continue;
                }

                if (isLineTransparent)
                {
                    transparentBottomPixels++;
                }
                else
                {
                    transparentBottomPixels = 0;
                }
            }

            return bitmap.Height - startY - transparentBottomPixels;
        }

        private static bool IsLineTransparent(this SKBitmap bitmap, int y)
        {
            // Validate y to ensure it's within the bounds of the bitmap
            if (y < 0 || y >= bitmap.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), "The row y is out of bounds.");
            }

            for (var x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                if (color.Alpha != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetNonTransparentWidth(this SKBitmap bitmap)
        {
            var startX = 0;
            var transparentPixelsRight = 0;
            for (var x = 0; x < bitmap.Width; x++)
            {
                var isLineTransparent = bitmap.IsVerticalLineTransparent(x);
                if (startX == x && isLineTransparent)
                {
                    startX++;
                    continue;
                }

                if (isLineTransparent)
                {
                    transparentPixelsRight++;
                }
                else
                {
                    transparentPixelsRight = 0;
                }
            }

            return bitmap.Width - startX - transparentPixelsRight;
        }

        private static bool IsVerticalLineTransparent(this SKBitmap bitmap, int x)
        {
            // Validate y to ensure it's within the bounds of the bitmap
            if (x < 0 || x >= bitmap.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "The column x is out of bounds.");
            }

            for (var y = 0; y < bitmap.Height; y++)
            {
                var color = bitmap.GetPixel(x, y);
                if (color.Alpha != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEqualTo(this SKBitmap bitmap, SKBitmap other)
        {
            if (bitmap.Width != other.Width || bitmap.Height != other.Height)
            {
                return false;
            }

            if (bitmap.Width == other.Width && bitmap.Height == other.Height && bitmap.Width == 0 && bitmap.Height == 0)
            {
                return true;
            }

            // Use SKPixmap to access the raw pixel data of both bitmaps
            using (var pixmap1 = bitmap.PeekPixels())
            using (var pixmap2 = other.PeekPixels())
            {
                // Get the total byte size of each image
                var byteSize = pixmap1.RowBytes * bitmap.Height;

                // Allocate byte arrays for the pixel data
                var pixels1 = new byte[byteSize];
                var pixels2 = new byte[byteSize];

                // Copy raw pixel data into the byte arrays
                Marshal.Copy(pixmap1.GetPixels(), pixels1, 0, byteSize);
                Marshal.Copy(pixmap2.GetPixels(), pixels2, 0, byteSize);

                return ByteArraysEqual(pixels1, pixels2);
            }
        }

        public static SKBitmap AddTransparentMargins(this SKBitmap bitmap, int marginLeft, int marginTop, int marginRight, int marginBottom)
        {
            var newWidth = bitmap.Width + marginLeft + marginRight;
            var newHeight = bitmap.Height + marginTop + marginBottom;
            var newBitmap = new SKBitmap(newWidth, newHeight, bitmap.ColorType, bitmap.AlphaType);
            using (SKCanvas canvas = new SKCanvas(newBitmap))
            {
                canvas.Clear(SKColors.Transparent); // Fill the new bitmap with transparent color
                canvas.DrawBitmap(bitmap, marginLeft, marginTop); // Draw the original bitmap at the correct position
            }

            return newBitmap;
        }

        // byte[] is implicitly convertible to ReadOnlySpan<byte>
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        public class TrimResult
        {
            public SKBitmap TrimmedBitmap { get; set; }
            public int Top { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public static TrimResult TrimTransparentPixels(this SKBitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            int width = bitmap.Width;
            int height = bitmap.Height;

            // Find the bounds of non-transparent pixels
            int top = height;
            int left = width;
            int right = -1;
            int bottom = -1;

            unsafe
            {
                byte* ptr = (byte*)bitmap.GetPixels().ToPointer();
                int bytesPerPixel = bitmap.BytesPerPixel;
                int rowBytes = bitmap.RowBytes;

                // Find top and bottom in one pass
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + (y * rowBytes);

                    for (int x = 0; x < width; x++)
                    {
                        // Alpha is the 4th byte in RGBA/BGRA formats
                        byte alpha = row[x * bytesPerPixel + 3];

                        if (alpha > 0)
                        {
                            if (y < top) top = y;
                            if (y > bottom) bottom = y;
                            if (x < left) left = x;
                            if (x > right) right = x;
                        }
                    }
                }
            }

            // If the entire bitmap is transparent, return the original
            if (top > bottom || left > right)
            {
                return new TrimResult
                {
                    TrimmedBitmap = bitmap.Copy(),
                    Top = 0,
                    Left = 0,
                    Right = 0,
                    Bottom = 0
                };
            }

            // Calculate the new dimensions
            int newWidth = right - left + 1;
            int newHeight = bottom - top + 1;

            // Create the trimmed bitmap
            SKBitmap trimmedBitmap = new SKBitmap(newWidth, newHeight, bitmap.ColorType, bitmap.AlphaType);

            using (SKCanvas canvas = new SKCanvas(trimmedBitmap))
            {
                SKRect sourceRect = new SKRect(left, top, right + 1, bottom + 1);
                SKRect destRect = new SKRect(0, 0, newWidth, newHeight);
                canvas.DrawBitmap(bitmap, sourceRect, destRect);
            }

            return new TrimResult
            {
                TrimmedBitmap = trimmedBitmap,
                Top = top,
                Left = left,
                Right = width - right - 1,
                Bottom = height - bottom - 1
            };
        }
    }
}
