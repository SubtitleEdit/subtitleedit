using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    /// <summary>
    /// A caption the way it goes into the stream: its palette and its run length encoded
    /// pixels. A few kilobytes for a line of text, against the hundreds the bitmap took - so
    /// it can be kept after the bitmap is gone, and <see cref="ToBitmap"/> brings the pixels
    /// back exactly (the palette holds the RGBA colours, not the YCbCr the stream carries).
    /// </summary>
    public class BluRaySupEncodedBitmap
    {
        /// <summary>
        /// The colours the RLE indexes, ending with a transparent entry. Index 255 is never in
        /// here - it is the transparent pixel.
        /// </summary>
        public List<SKColor> Palette { get; }

        public byte[] Rle { get; }
        public int Width { get; }
        public int Height { get; }

        public BluRaySupEncodedBitmap(List<SKColor> palette, byte[] rle, int width, int height)
        {
            Palette = palette;
            Rle = rle;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Decodes the pixels into a new bitmap the caller owns.
        /// </summary>
        public SKBitmap ToBitmap()
        {
            var bitmap = new SKBitmap(new SKImageInfo(Math.Max(1, Width), Math.Max(1, Height), SKColorType.Bgra8888, SKAlphaType.Unpremul));
            var pixels = bitmap.GetPixels();
            if (pixels == IntPtr.Zero || Width <= 0 || Height <= 0)
            {
                return bitmap;
            }

            unsafe
            {
                var span = new Span<byte>(pixels.ToPointer(), bitmap.ByteCount);
                span.Clear();
                var rowBytes = bitmap.RowBytes;
                var x = 0;
                var y = 0;
                var rle = Rle;
                for (var i = 0; i < rle.Length && y < Height;)
                {
                    var b = rle[i++];
                    int index;
                    int count;
                    if (b != 0)
                    {
                        // a single pixel of colour b
                        index = b;
                        count = 1;
                    }
                    else
                    {
                        if (i >= rle.Length)
                        {
                            break;
                        }

                        b = rle[i++];
                        if (b == 0)
                        {
                            // end of line
                            x = 0;
                            y++;
                            continue;
                        }

                        switch (b & 0xC0)
                        {
                            case 0x00:
                                // 00 xx -> xx pixels of colour 0
                                index = 0;
                                count = b;
                                break;
                            case 0x40:
                                // 00 4x xx -> xxx pixels of colour 0
                                if (i >= rle.Length)
                                {
                                    return bitmap;
                                }

                                index = 0;
                                count = ((b & 0x3f) << 8) | rle[i++];
                                break;
                            case 0x80:
                                // 00 8x cc -> x pixels of colour cc
                                if (i >= rle.Length)
                                {
                                    return bitmap;
                                }

                                count = b & 0x3f;
                                index = rle[i++];
                                break;
                            default:
                                // 00 cx xx cc -> xxx pixels of colour cc
                                if (i + 1 >= rle.Length)
                                {
                                    return bitmap;
                                }

                                count = ((b & 0x3f) << 8) | rle[i++];
                                index = rle[i++];
                                break;
                        }
                    }

                    var color = index < Palette.Count ? Palette[index] : SKColors.Transparent;
                    for (var n = 0; n < count && x < Width; n++, x++)
                    {
                        var offset = y * rowBytes + x * 4;
                        span[offset] = color.Blue;
                        span[offset + 1] = color.Green;
                        span[offset + 2] = color.Red;
                        span[offset + 3] = color.Alpha;
                    }
                }
            }

            return bitmap;
        }
    }
}
