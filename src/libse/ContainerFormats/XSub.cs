using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    /// <summary>
    /// One XSUB (DivX/"DXSB") subtitle event: a run-length encoded 4-colour bitmap plus its
    /// display rectangle and time codes. XSUB is the subtitle format found in .avi/.divx files.
    /// </summary>
    public class XSub
    {
        public TimeCode Start { get; set; }
        public TimeCode End { get; set; }
        public int Width { get; }
        public int Height { get; }

        /// <summary>Left edge of the bitmap in the video frame (from the packet header).</summary>
        public int Left { get; }

        /// <summary>Top edge of the bitmap in the video frame (from the packet header).</summary>
        public int Top { get; }

        private readonly byte[] _colorBuffer;
        private readonly byte[] _rleBuffer;

        /// <summary>
        /// Byte offset inside <see cref="_rleBuffer"/> where the second (odd lines) field starts,
        /// or a non-positive value when the data is not split into fields. Like DVD subpictures,
        /// an XSUB bitmap is stored as two interlaced fields: the first holds the even scan lines,
        /// the second the odd ones. Decoding the buffer as one sequential run renders the even
        /// lines into the top half of the bitmap and the odd lines into the bottom half.
        /// </summary>
        private readonly int _secondFieldOffset;

        public XSub(TimeCode start, TimeCode end, int width, int height, int left, int top, byte[] colors, byte[] rle, int secondFieldOffset)
        {
            Start = start;
            End = end;
            Width = width;
            Height = height;
            Left = left;
            Top = top;
            _colorBuffer = colors;
            _rleBuffer = rle;
            _secondFieldOffset = secondFieldOffset;
        }

        /// <summary>
        /// Decodes a run-length encoded field into every <paramref name="rowStep"/>'th row,
        /// starting at <paramref name="firstRow"/>. The encoding is the DVD subpicture one:
        /// a value is one to four nibbles, its low two bits are the colour index and the rest
        /// the run length; a run of length 0 fills the rest of the line, and each line ends on
        /// a byte boundary.
        /// </summary>
        private static unsafe void DecodeField(SKBitmap bmp, byte[] buf, int startNibble, int endNibble, int firstRow, int rowStep, uint[] colorValues)
        {
            var w = bmp.Width;
            var h = bmp.Height;
            var pixels = (uint*)bmp.GetPixels().ToPointer();
            var stride = bmp.RowBytes / 4; // Width in pixels

            var nibbleOffset = startNibble;
            var x = 0;
            var y = firstRow;

            while (y < h && nibbleOffset < endNibble)
            {
                var v = GetNibble(buf, nibbleOffset++, endNibble);
                if (v < 0x4)
                {
                    v = (v << 4) | GetNibble(buf, nibbleOffset++, endNibble);
                    if (v < 0x10)
                    {
                        v = (v << 4) | GetNibble(buf, nibbleOffset++, endNibble);
                        if (v < 0x040)
                        {
                            v = (v << 4) | GetNibble(buf, nibbleOffset++, endNibble);
                            if (v < 4)
                            {
                                v |= (w - x) << 2;
                            }
                        }
                    }
                }

                var len = v >> 2;
                if (len > w - x)
                {
                    len = w - x;
                }

                var color = v & 0x03;
                if (color > 0)
                {
                    var colorValue = colorValues[color];
                    var pixelIndex = y * stride + x;
                    for (var i = 0; i < len; i++)
                    {
                        pixels[pixelIndex++] = colorValue;
                    }
                }

                x += len;
                if (x >= w)
                {
                    y += rowStep;
                    x = 0;
                    nibbleOffset += nibbleOffset & 1; // lines are byte aligned
                }
            }
        }

        /// <summary>
        /// Nibble at <paramref name="nibbleOffset"/>, or 0 past <paramref name="endNibble"/> -
        /// a truncated packet must not throw while a run is being read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNibble(byte[] buf, int nibbleOffset, int endNibble)
        {
            if (nibbleOffset >= endNibble)
            {
                return 0;
            }

            return (buf[nibbleOffset >> 1] >> ((1 - (nibbleOffset & 1)) << 2)) & 0xf;
        }

        public SKBitmap GetImage(SKColor background, SKColor pattern, SKColor emphasis1, SKColor emphasis2)
        {
            var fourColors = new List<SKColor> { background, pattern, emphasis1, emphasis2 };
            var bmp = new SKBitmap(Width, Height);

            // If background isn't transparent, fill the bitmap with it
            if (fourColors[0].Alpha != 0)
            {
                using (var canvas = new SKCanvas(bmp))
                {
                    using (var paint = new SKPaint { Color = fourColors[0] })
                    {
                        canvas.DrawRect(0, 0, bmp.Width, bmp.Height, paint);
                    }
                }
            }

            // Pre-convert colors to uint for faster pixel writing
            var colorValues = new uint[4];
            for (var i = 0; i < 4; i++)
            {
                var c = fourColors[i];
                colorValues[i] = (uint)((c.Alpha << 24) | (c.Red << 16) | (c.Green << 8) | c.Blue);
            }

            var endNibble = _rleBuffer.Length * 2;
            if (_secondFieldOffset > 0 && _secondFieldOffset < _rleBuffer.Length)
            {
                DecodeField(bmp, _rleBuffer, 0, _secondFieldOffset * 2, 0, 2, colorValues);
                DecodeField(bmp, _rleBuffer, _secondFieldOffset * 2, endNibble, 1, 2, colorValues);
            }
            else
            {
                // No field split declared - decode as a single run of consecutive lines.
                DecodeField(bmp, _rleBuffer, 0, endNibble, 0, 1, colorValues);
            }

            bmp.NotifyPixelsChanged();
            return bmp;
        }

        private SKColor GetColor(int start)
        {
            return new SKColor(_colorBuffer[start], _colorBuffer[start + 1], _colorBuffer[start + 2]);
        }

        public SKBitmap GetImage()
        {
            return GetImage(SKColors.Transparent, GetColor(3), GetColor(6), GetColor(9));
        }
    }
}
