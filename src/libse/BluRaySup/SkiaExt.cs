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

        /// <summary>
        /// The smallest rectangle containing every pixel with a non-zero alpha, in absolute bitmap
        /// coordinates, or <see cref="NonTransparentBounds.IsEmpty"/> when the bitmap draws nothing.
        /// </summary>
        public struct NonTransparentBounds
        {
            public int Top { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
            public bool IsEmpty => Top > Bottom || Left > Right;
        }

        // Little-endian view of two pixels: alpha is the 4th byte of each, so the high byte of each
        // of the two packed uints.
        private const ulong AlphaMask2Pixels = 0xFF000000FF000000UL;
        private const uint AlphaMask1Pixel = 0xFF000000u;

        /// <summary>
        /// Index of the first pixel in [start, end) with a non-zero alpha, or -1 when there is none.
        /// Image export rasterises into a 4000x2000 scratch canvas and nearly all of it is empty, so
        /// the scan tests eight pixels per branch and stops at the first hit.
        /// </summary>
        private static unsafe int FirstNonTransparent(uint* row, int start, int end)
        {
            var x = start;
            for (; x + 8 <= end; x += 8)
            {
                var block = (ulong*)(row + x);
                if (((block[0] | block[1] | block[2] | block[3]) & AlphaMask2Pixels) != 0)
                {
                    break;
                }
            }

            for (; x < end; x++)
            {
                if ((row[x] & AlphaMask1Pixel) != 0)
                {
                    return x;
                }
            }

            return -1;
        }

        /// <summary>
        /// Index of the last pixel in [start, end) with a non-zero alpha, or -1 when there is none.
        /// </summary>
        private static unsafe int LastNonTransparent(uint* row, int start, int end)
        {
            var x = end;
            for (; x - 8 >= start; x -= 8)
            {
                var block = (ulong*)(row + x - 8);
                if (((block[0] | block[1] | block[2] | block[3]) & AlphaMask2Pixels) != 0)
                {
                    break;
                }
            }

            for (; x > start; x--)
            {
                if ((row[x - 1] & AlphaMask1Pixel) != 0)
                {
                    return x - 1;
                }
            }

            return -1;
        }

        public static unsafe NonTransparentBounds GetNonTransparentBounds(this SKBitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            var width = bitmap.Width;
            var height = bitmap.Height;
            var top = height;
            var left = width;
            var right = -1;
            var bottom = -1;

            var ptr = (byte*)bitmap.GetPixels().ToPointer();
            var bytesPerPixel = bitmap.BytesPerPixel;
            var rowBytes = bitmap.RowBytes;

            if (bytesPerPixel == 4 && BitConverter.IsLittleEndian)
            {
                for (var y = 0; y < height; y++)
                {
                    var row = (uint*)(ptr + y * rowBytes);
                    var firstHit = FirstNonTransparent(row, 0, width);
                    if (firstHit < 0)
                    {
                        continue;
                    }

                    if (y < top)
                    {
                        top = y;
                    }

                    bottom = y;

                    if (firstHit < left)
                    {
                        left = firstHit;
                    }

                    // Only the part of the row that could push the right edge out still needs
                    // looking at, so later rows get cheaper as the bounds settle.
                    if (right < width - 1)
                    {
                        var lastHit = LastNonTransparent(row, right + 1, width);
                        if (lastHit > right)
                        {
                            right = lastHit;
                        }
                    }
                }
            }
            else
            {
                for (var y = 0; y < height; y++)
                {
                    var row = ptr + y * rowBytes;
                    for (var x = 0; x < width; x++)
                    {
                        // Alpha is the 4th byte in RGBA/BGRA formats
                        var alpha = row[x * bytesPerPixel + 3];
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

            return new NonTransparentBounds { Top = top, Left = left, Right = right, Bottom = bottom };
        }

        /// <summary>
        /// Copies the given rectangle (inclusive bounds, absolute bitmap coordinates) into a new
        /// bitmap. Parts of the rectangle outside the bitmap come out transparent.
        /// </summary>
        public static SKBitmap CropTo(this SKBitmap bitmap, int left, int top, int right, int bottom)
        {
            var newWidth = right - left + 1;
            var newHeight = bottom - top + 1;
            var cropped = new SKBitmap(newWidth, newHeight, bitmap.ColorType, bitmap.AlphaType);

            using (var canvas = new SKCanvas(cropped))
            {
                canvas.Clear(SKColors.Transparent);
                var sourceRect = new SKRect(left, top, right + 1, bottom + 1);
                var destRect = new SKRect(0, 0, newWidth, newHeight);
                canvas.DrawBitmap(bitmap, sourceRect, destRect);
            }

            return cropped;
        }

        public static TrimResult TrimTransparentPixels(this SKBitmap bitmap)
        {
            var bounds = bitmap.GetNonTransparentBounds();

            // If the entire bitmap is transparent, return the original
            if (bounds.IsEmpty)
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

            return new TrimResult
            {
                TrimmedBitmap = bitmap.CropTo(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom),
                Top = bounds.Top,
                Left = bounds.Left,
                Right = bitmap.Width - bounds.Right - 1,
                Bottom = bitmap.Height - bounds.Bottom - 1
            };
        }
    }
}
