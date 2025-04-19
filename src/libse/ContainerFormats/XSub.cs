using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    public class XSub
    {
        public TimeCode Start { get; set; }
        public TimeCode End { get; set; }
        public int Width { get; }
        public int Height { get; }

        private readonly byte[] _colorBuffer;
        private readonly byte[] _rleBuffer;

        public XSub(string timeCode, int width, int height, byte[] colors, byte[] rle)
        {
            Start = DecodeTimeCode(timeCode.Substring(0, 13));
            End = DecodeTimeCode(timeCode.Substring(13, 12));
            Width = width;
            Height = height;
            _colorBuffer = colors;
            _rleBuffer = rle;
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            var parts = timeCode.Split(new[] { ':', ';', '.', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }

        private static void GenerateBitmap(FastBitmap bmp, byte[] buf, List<SKColor> fourColors)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int nibbleOffset = 0;
            var nibbleEnd = buf.Length * 2;
            var x = 0;
            var y = 0;
            for (; ; )
            {
                if (nibbleOffset >= nibbleEnd)
                {
                    return;
                }

                var v = GetNibble(buf, nibbleOffset++);
                if (v < 0x4)
                {
                    v = (v << 4) | GetNibble(buf, nibbleOffset++);
                    if (v < 0x10)
                    {
                        v = (v << 4) | GetNibble(buf, nibbleOffset++);
                        if (v < 0x040)
                        {
                            v = (v << 4) | GetNibble(buf, nibbleOffset++);
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
                    var c = fourColors[color];
                    bmp.SetPixel(x, y, c, len);
                }

                x += len;
                if (x >= w)
                {
                    y++;
                    if (y >= h)
                    {
                        break;
                    }

                    x = 0;
                    nibbleOffset += (nibbleOffset & 1);
                }
            }
        }

        private static int GetNibble(byte[] buf, int nibbleOffset)
        {
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

            var fastBmp = new FastBitmap(bmp);
            fastBmp.LockImage();
            GenerateBitmap(fastBmp, _rleBuffer, fourColors);
            fastBmp.UnlockImage();
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
