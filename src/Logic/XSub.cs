﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Logic
{
    public class XSub
    {
        public TimeCode Start { get; set; }
        public TimeCode End { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private byte[] colorBuffer;
        private byte[] rleBuffer;

        public XSub(string timeCode, int width, int height, byte[] colors, byte[] rle)
        {
            Start = DecodeTimeCode(timeCode.Substring(0, 13));
            End = DecodeTimeCode(timeCode.Substring(13, 12));
            Width = width;
            Height = height;
            colorBuffer = colors;
            rleBuffer = rle;
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            var parts = timeCode.Split(new[] { ':', ';', '.', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }

        private static int GenerateBitmap(FastBitmap bmp, byte[] buf, List<Color> fourColors)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int nibbleOffset = 0;
            var nibble_end = buf.Length * 2;
            var x = 0;
            var y = 0;
            for (; ; )
            {
                if (nibbleOffset >= nibble_end)
                    return -1;
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
                if (len > (w - x))
                    len = (w - x);

                var color = v & 0x03;
                if (color > 0)
                {
                    Color c = fourColors[color];
                    bmp.SetPixel(x, y, c, len);
                }

                x += len;
                if (x >= w)
                {
                    y++;
                    if (y >= h)
                        break;
                    x = 0;
                    nibbleOffset += (nibbleOffset & 1);
                }
            }
            return 0;
        }

        private static int GetNibble(byte[] buf, int nibble_offset)
        {
            return (buf[nibble_offset >> 1] >> ((1 - (nibble_offset & 1)) << 2)) & 0xf;
        }

        public Bitmap GetImage(Color background, Color pattern, Color emphasis1, Color emphasis2)
        {
            var fourColors = new List<Color> { background, pattern, emphasis1, emphasis2 };
            var bmp = new Bitmap(Width, Height);
            if (fourColors[0] != Color.Transparent)
            {
                using (var gr = Graphics.FromImage(bmp))
                {
                    gr.FillRectangle(new SolidBrush(fourColors[0]), new Rectangle(0, 0, bmp.Width, bmp.Height));
                }
            }
            var fastBmp = new FastBitmap(bmp);
            fastBmp.LockImage();
            GenerateBitmap(fastBmp, rleBuffer, fourColors);
            fastBmp.UnlockImage();
            return bmp;
        }

        private Color GetColor(int start)
        {
            return Color.FromArgb(colorBuffer[start], colorBuffer[start + 1], colorBuffer[start + 2]);
        }

        public Bitmap GetImage()
        {
            return GetImage(Color.Transparent, GetColor(3), GetColor(6), GetColor(9));
        }

    }
}
