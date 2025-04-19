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

        // byte[] is implicitly convertible to ReadOnlySpan<byte>
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
    }
}
