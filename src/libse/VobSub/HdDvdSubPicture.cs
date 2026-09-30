using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// One HD-DVD sub picture from an HD-DVD .sup (see <see cref="HdDvdSupParser"/>).
    /// Like a DVD SPU, but with 32-bit offsets, a 256 entry Y/Cr/Cb palette plus a separate
    /// alpha table, and a run-length code that mixes 2-bit and 8-bit colour indexes.
    /// </summary>
    public class HdDvdSubPicture
    {
        private readonly byte[] _data;
        private readonly int _paletteOffset;
        private readonly int _alphaOffset;
        private readonly int _evenFieldOffset;
        private readonly int _oddFieldOffset;
        private readonly int _pixelDataEnd;

        public TimeSpan StartTime { get; internal set; }
        public TimeSpan EndTime { get; internal set; }

        /// <summary>
        /// False when the packet had no "stop display" command - the picture then stays up until
        /// the next one replaces it, and <see cref="HdDvdSupParser"/> fills <see cref="EndTime"/> in.
        /// </summary>
        public bool HasStopDisplay { get; }

        public bool Forced { get; }
        public SKRectI ImageDisplayArea { get; }

        /// <summary>
        /// Screen position of the bitmap <see cref="GetBitmap"/> returns with cropping on (the
        /// default). Only meaningful after <see cref="GetBitmap"/> has run.
        /// </summary>
        public SKPointI ImagePosition { get; private set; }

        /// <param name="data">The sub picture unit, starting right after the 10 byte "SP" + PTS header.</param>
        /// <param name="presentationTimeStamp">PTS from the "SP" header (90 kHz).</param>
        internal HdDvdSubPicture(byte[] data, long presentationTimeStamp)
        {
            _data = data;
            // 2 bytes zero, 4 bytes unit size, 4 bytes offset of the first control sequence
            // (all offsets are from the start of the unit). Pixel data runs up to that sequence.
            _pixelDataEnd = (int)ReadUInt32(6);
            var sequenceOffset = _pixelDataEnd;
            var startDelay = -1;
            var minAlphaSum = int.MaxValue;
            var visited = 0;

            // Display control sequences: 2 bytes delay, 4 bytes offset of the next sequence
            // (pointing at itself on the last one), then commands up to 0xff.
            while (sequenceOffset + 6 <= data.Length && visited++ < 16)
            {
                var delay = (data[sequenceOffset] << 8) | data[sequenceOffset + 1];
                var nextSequenceOffset = (int)ReadUInt32(sequenceOffset + 2);
                if (startDelay < 0)
                {
                    startDelay = delay;
                }

                var i = sequenceOffset + 6;
                var done = false;
                while (!done && i < data.Length)
                {
                    switch (data[i++])
                    {
                        case 0x00: // forced start display
                            Forced = true;
                            break;
                        case 0x01: // start display
                            break;
                        case 0x02: // stop display
                            if (!HasStopDisplay)
                            {
                                HasStopDisplay = true;
                                EndTime = PtsToTimeSpan(presentationTimeStamp + delay * 1024L);
                            }

                            break;
                        case 0x83: // palette: 256 x (Y, Cr, Cb)
                            if (_paletteOffset == 0)
                            {
                                _paletteOffset = i;
                            }

                            i += 768;
                            break;
                        case 0x84: // alpha: 256 x transparency (0 = opaque)
                            // Fades repeat the table per step - keep the most opaque one.
                            if (i + 256 <= data.Length)
                            {
                                var sum = 0;
                                for (var k = i; k < i + 256; k++)
                                {
                                    sum += data[k];
                                }

                                if (sum < minAlphaSum)
                                {
                                    minAlphaSum = sum;
                                    _alphaOffset = i;
                                }
                            }

                            i += 256;
                            break;
                        case 0x85: // display area: 12 bits each for x1, x2, y1, y2 (inclusive)
                            if (i + 6 <= data.Length)
                            {
                                var x1 = (data[i] << 4) | (data[i + 1] >> 4);
                                var x2 = ((data[i + 1] & 0x0f) << 8) | data[i + 2];
                                var y1 = (data[i + 3] << 4) | (data[i + 4] >> 4);
                                var y2 = ((data[i + 4] & 0x0f) << 8) | data[i + 5];
                                ImageDisplayArea = new SKRectI(x1, y1, x2 + 1, y2 + 1);
                            }

                            i += 6;
                            break;
                        case 0x86: // pixel data offsets of the even (top) and odd (bottom) fields
                            if (i + 8 <= data.Length)
                            {
                                _evenFieldOffset = (int)ReadUInt32(i);
                                _oddFieldOffset = (int)ReadUInt32(i + 4);
                            }

                            i += 8;
                            break;
                        default: // 0xff = end of sequence (anything else is unknown - stop too)
                            done = true;
                            break;
                    }
                }

                if (nextSequenceOffset <= sequenceOffset)
                {
                    break;
                }

                sequenceOffset = nextSequenceOffset;
            }

            StartTime = PtsToTimeSpan(presentationTimeStamp + Math.Max(0, startDelay) * 1024L);
            if (!HasStopDisplay)
            {
                EndTime = StartTime;
            }
        }

        public bool HasImage =>
            _paletteOffset > 0 && _paletteOffset + 768 <= _data.Length &&
            _alphaOffset > 0 && _alphaOffset + 256 <= _data.Length &&
            ImageDisplayArea.Width > 0 && ImageDisplayArea.Height > 0 &&
            _evenFieldOffset > 0 && _oddFieldOffset > _evenFieldOffset && _pixelDataEnd >= _oddFieldOffset && _pixelDataEnd <= _data.Length;

        /// <summary>
        /// Decodes the picture in its own palette colours. With <paramref name="crop"/> the bitmap is
        /// cut to the visible pixels and <see cref="ImagePosition"/> says where it sits on screen.
        /// </summary>
        public SKBitmap GetBitmap(bool crop = true)
        {
            ImagePosition = new SKPointI(ImageDisplayArea.Left, ImageDisplayArea.Top);
            if (!HasImage)
            {
                return new SKBitmap(1, 1);
            }

            var width = ImageDisplayArea.Width;
            var height = ImageDisplayArea.Height;
            var indexes = new byte[width * height];
            DecodeField(_evenFieldOffset, _oddFieldOffset, indexes, 0, width, height);
            DecodeField(_oddFieldOffset, _pixelDataEnd, indexes, 1, width, height);

            var colors = new SKColor[256];
            for (var c = 0; c < 256; c++)
            {
                var alpha = 255 - _data[_alphaOffset + c];
                if (alpha == 0)
                {
                    continue;
                }

                var p = _paletteOffset + c * 3;
                BluRaySupPalette.YCbCr2Rgb(_data[p], _data[p + 2], _data[p + 1], false, out var r, out var g, out var b);
                colors[c] = new SKColor((byte)r, (byte)g, (byte)b, (byte)alpha);
            }

            var left = 0;
            var top = 0;
            var right = width - 1;
            var bottom = height - 1;
            if (crop)
            {
                left = width;
                top = height;
                right = -1;
                bottom = -1;
                for (var y = 0; y < height; y++)
                {
                    var row = y * width;
                    for (var x = 0; x < width; x++)
                    {
                        if (colors[indexes[row + x]].Alpha != 0)
                        {
                            if (x < left) left = x;
                            if (x > right) right = x;
                            if (y < top) top = y;
                            bottom = y;
                        }
                    }
                }

                if (right < 0)
                {
                    return new SKBitmap(1, 1);
                }
            }

            var outWidth = right - left + 1;
            var outHeight = bottom - top + 1;
            var bitmap = new SKBitmap(new SKImageInfo(outWidth, outHeight, SKColorType.Bgra8888, SKAlphaType.Unpremul));
            var pixels = bitmap.GetPixels();
            if (pixels == IntPtr.Zero)
            {
                return bitmap;
            }

            unsafe
            {
                var rowBytes = bitmap.RowBytes;
                var basePtr = (byte*)pixels.ToPointer();
                for (var y = 0; y < outHeight; y++)
                {
                    var line = basePtr + (long)y * rowBytes;
                    var row = (y + top) * width + left;
                    for (var x = 0; x < outWidth; x++)
                    {
                        var color = colors[indexes[row + x]];
                        line[x * 4] = color.Blue;
                        line[x * 4 + 1] = color.Green;
                        line[x * 4 + 2] = color.Red;
                        line[x * 4 + 3] = color.Alpha;
                    }
                }
            }

            ImagePosition = new SKPointI(ImageDisplayArea.Left + left, ImageDisplayArea.Top + top);
            return bitmap;
        }

        /// <summary>
        /// Decodes one interlaced field into every other row of <paramref name="indexes"/>.
        /// Each code: 1 bit run flag, 1 bit colour size (0 = 2 bits, 1 = 8 bits), the colour,
        /// then for runs 1 bit length size (0 = 3 bits + 2, 1 = 7 bits + 9, where 7 bits of 0
        /// means "to end of line"). Lines are byte aligned.
        /// </summary>
        private void DecodeField(int start, int end, byte[] indexes, int firstRow, int width, int height)
        {
            var bitPosition = (long)start * 8;
            var bitEnd = (long)end * 8;
            var y = firstRow;
            var x = 0;
            while (y < height && bitPosition < bitEnd)
            {
                var isRun = ReadBits(ref bitPosition, 1) == 1;
                var color = ReadBits(ref bitPosition, 1) == 1 ? ReadBits(ref bitPosition, 8) : ReadBits(ref bitPosition, 2);
                var count = 1;
                if (isRun)
                {
                    if (ReadBits(ref bitPosition, 1) == 1)
                    {
                        count = ReadBits(ref bitPosition, 7);
                        count = count == 0 ? width - x : count + 9;
                    }
                    else
                    {
                        count = ReadBits(ref bitPosition, 3) + 2;
                    }
                }

                // A run can spill over into the next line of the field.
                while (count > 0 && y < height)
                {
                    var n = Math.Min(count, width - x);
                    indexes.AsSpan(y * width + x, n).Fill((byte)color);
                    count -= n;
                    x += n;
                    if (x == width)
                    {
                        x = 0;
                        y += 2;
                        if (count == 0)
                        {
                            bitPosition = (bitPosition + 7) & ~7L;
                        }
                    }
                }
            }
        }

        private int ReadBits(ref long bitPosition, int count)
        {
            var value = 0;
            for (var i = 0; i < count; i++)
            {
                var byteIndex = (int)(bitPosition >> 3);
                var bit = byteIndex < _data.Length ? (_data[byteIndex] >> (7 - (int)(bitPosition & 7))) & 1 : 0;
                value = (value << 1) | bit;
                bitPosition++;
            }

            return value;
        }

        private uint ReadUInt32(int index)
        {
            if (index < 0 || index + 4 > _data.Length)
            {
                return 0;
            }

            return (uint)((_data[index] << 24) | (_data[index + 1] << 16) | (_data[index + 2] << 8) | _data[index + 3]);
        }

        private static TimeSpan PtsToTimeSpan(long pts) => TimeSpan.FromMilliseconds(Math.Round(pts / 90.0));
    }
}
