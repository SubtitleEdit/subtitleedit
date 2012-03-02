using System;
using System.Collections.Generic;
using System.Drawing;
using Nikse.SubtitleEdit.Logic.VobSub;

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

        private TimeCode DecodeTimeCode(string timeCode)
        {
            var parts = timeCode.Split(":;.,-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);            
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }

        public Bitmap GetImage(Color background, Color pattern, Color emphasis1, Color emphasis2)
        {
            var fourColors = new List<Color> { background, pattern, emphasis1, emphasis2 };
            var bmp = new Bitmap(Width, Height);
            if (fourColors[0] != Color.Transparent)
            {
                var gr = Graphics.FromImage(bmp);
                gr.FillRectangle(new SolidBrush(fourColors[0]), new Rectangle(0, 0, bmp.Width, bmp.Height));
                gr.Dispose();
            }
            var fastBmp = new FastBitmap(bmp);
            fastBmp.LockImage();
            SubPicture.GenerateBitmap(rleBuffer, fastBmp, 0, 0, fourColors, 1);
            fastBmp.UnlockImage();
            return bmp;
        }

        private Color GetColor(int start)
        {
            return Color.FromArgb(colorBuffer[start], colorBuffer[start+1], colorBuffer[start+2]);
        }

        public Bitmap GetImage()
        {
            return GetImage(Color.Transparent, GetColor(3), GetColor(6), GetColor(9));
        }

    }
}
