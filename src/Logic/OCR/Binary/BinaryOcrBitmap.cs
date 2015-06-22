using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Ocr.Binary
{
    public class BinaryOcrBitmap
    {
        //File format:
        //-------------
        //2bytes=width
        //2bytes=height
        //2bytes=x
        //2bytes=y
        //2bytes=numberOfColoredPixels
        //1byte=flags (1 bit = italic, next 7 bits = ExpandCount)
        //4bytes=hash
        //1bytes=text len
        //text len bytes=text (UTF-8)
        //w*h bytes / 8=pixels as bits(byte aligned)

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int NumberOfColoredPixels { get; private set; }
        public UInt32 Hash { get; private set; }
        private byte[] _colors;
        public bool Italic { get; set; }
        public int ExpandCount { get; set; }
        public bool LoadedOk { get; private set; }
        public string Text { get; set; }
        public List<BinaryOcrBitmap> ExpandedList { get; set; }

        public string Key
        {
            get
            {
                return Text + "|#|" + Hash + "_" + Width + "x" + Height + "_" + NumberOfColoredPixels;
            }
        }

        public override string ToString()
        {
            if (Italic)
                return Text + " (" + Width + "x" + Height + ", italic)";
            return Text + " (" + Width + "x" + Height + ")";
        }

        public BinaryOcrBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            _colors = new byte[Width * Height];
            Hash = MurMurHash3.Hash(_colors);
            CalcuateNumberOfColoredPixels();
        }

        public BinaryOcrBitmap(Stream stream)
        {
            try
            {
                var buffer = new byte[16];
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read < buffer.Length)
                {
                    LoadedOk = false;
                    return;
                }
                Width = buffer[0] << 8 | buffer[1];
                Height = buffer[2] << 8 | buffer[3];
                X = buffer[4] << 8 | buffer[5];
                Y = buffer[6] << 8 | buffer[7];
                NumberOfColoredPixels = buffer[8] << 8 | buffer[9];
                Italic = (buffer[10] & VobSub.Helper.B10000000) > 0;
                ExpandCount = buffer[10] & VobSub.Helper.B01111111;
                Hash = (uint)(buffer[11] << 24 | buffer[12] << 16 | buffer[13] << 8 | buffer[14]);
                int textLen = buffer[15];
                if (textLen > 0)
                {
                    buffer = new byte[textLen];
                    stream.Read(buffer, 0, buffer.Length);
                    Text = System.Text.Encoding.UTF8.GetString(buffer);
                }

                _colors = new byte[Width * Height];
                stream.Read(_colors, 0, _colors.Length);
                LoadedOk = true;
            }
            catch
            {
                LoadedOk = false;
            }
        }

        public BinaryOcrBitmap(NikseBitmap nbmp)
        {
            InitializeViaNikseBmp(nbmp);
        }

        public BinaryOcrBitmap(NikseBitmap nbmp, bool italic, int expandCount, string text, int x, int y)
        {
            InitializeViaNikseBmp(nbmp);
            Italic = italic;
            ExpandCount = expandCount;
            Text = text;
            X = x;
            Y = y;
        }

        private void InitializeViaNikseBmp(NikseBitmap nbmp)
        {
            Width = nbmp.Width;
            Height = nbmp.Height;
            _colors = new byte[Width * Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetPixelViaAlpha(x, y, nbmp.GetAlpha(x, y));
                }
            }
            Hash = MurMurHash3.Hash(_colors);
            CalcuateNumberOfColoredPixels();
        }

        private void CalcuateNumberOfColoredPixels()
        {
            NumberOfColoredPixels = 0;
            for (int i = 0; i < _colors.Length; i++)
            {
                if (_colors[i] > 0)
                    NumberOfColoredPixels++;
            }
        }

        public void Save(Stream stream)
        {
            WriteInt16(stream, (ushort)Width);
            WriteInt16(stream, (ushort)Height);

            WriteInt16(stream, (ushort)X);
            WriteInt16(stream, (ushort)Y);

            WriteInt16(stream, (ushort)NumberOfColoredPixels);

            byte flags = (byte)(ExpandCount & VobSub.Helper.B01111111);
            if (Italic)
                flags = (byte)(flags + VobSub.Helper.B10000000);
            stream.WriteByte(flags);

            WriteInt32(stream, Hash);

            if (Text == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                var textBuffer = System.Text.Encoding.UTF8.GetBytes(Text);
                stream.WriteByte((byte)textBuffer.Length);
                stream.Write(textBuffer, 0, textBuffer.Length);
            }

            stream.Write(_colors, 0, _colors.Length);
        }

        private static void WriteInt16(Stream stream, ushort val)
        {
            var buffer = new byte[2];
            buffer[0] = (byte)((val & 0xFF00) >> 8);
            buffer[1] = (byte)(val & 0x00FF);
            stream.Write(buffer, 0, buffer.Length);
        }

        private static void WriteInt32(Stream stream, UInt32 val)
        {
            var buffer = new byte[4];
            buffer[0] = (byte)((val & 0xFF000000) >> 24);
            buffer[1] = (byte)((val & 0xFF0000) >> 16);
            buffer[2] = (byte)((val & 0xFF00) >> 8);
            buffer[3] = (byte)(val & 0xFF);
            stream.Write(buffer, 0, buffer.Length);
        }

        public int GetPixel(int x, int y)
        {
            return _colors[Width * y + x];
        }

        public void SetPixel(int x, int y, int c)
        {
            _colors[Width * y + x] = (byte)c;
        }

        public void SetPixel(int x, int y, Color c)
        {
            if (c.A < 100)
                _colors[Width * y + x] = 0;
            else
                _colors[Width * y + x] = 1;
        }

        public void SetPixelViaAlpha(int x, int y, int alpha)
        {
            if (alpha < 100)
                _colors[Width * y + x] = 0;
            else
                _colors[Width * y + x] = 1;
        }

        /// <summary>
        /// Copies a rectangle from the bitmap to a new bitmap
        /// </summary>
        /// <param name="section">Source rectangle</param>
        /// <returns>Rectangle from current image as new bitmap</returns>
        public ManagedBitmap GetRectangle(Rectangle section)
        {
            var newRectangle = new ManagedBitmap(section.Width, section.Height);

            int recty = 0;
            for (int y = section.Top; y < section.Top + section.Height; y++)
            {
                int rectx = 0;
                for (int x = section.Left; x < section.Left + section.Width; x++)
                {
                    Color c = Color.Transparent;
                    if (GetPixel(x, y) > 0)
                        c = Color.White;
                    newRectangle.SetPixel(rectx, recty, c);
                    rectx++;
                }
                recty++;
            }
            return newRectangle;
        }

        public Bitmap ToOldBitmap()
        {
            if (ExpandedList != null && ExpandedList.Count > 0)
            {
                int minX = X;
                int minY = Y;
                int maxX = X + Width;
                int maxY = Y + Height;
                var list = new List<BinaryOcrBitmap>();
                list.Add(this);
                foreach (BinaryOcrBitmap bob in ExpandedList)
                {
                    if (bob.X < minX)
                        minX = bob.X;
                    if (bob.Y < minY)
                        minY = bob.Y;
                    if (bob.X + bob.Width > maxX)
                        maxX = bob.X + bob.Width;
                    if (bob.Y + bob.Height > maxY)
                        maxY = bob.Y + bob.Height;
                    list.Add(bob);
                }
                var nbmp = new BinaryOcrBitmap(maxX - minX, maxY - minY);
                foreach (BinaryOcrBitmap bob in list)
                {
                    for (int y = 0; y < bob.Height; y++)
                    {
                        for (int x = 0; x < bob.Width; x++)
                        {
                            int c = bob.GetPixel(x, y);
                            if (c > 0)
                                nbmp.SetPixel(bob.X - minX + x, bob.Y - minY + y, 1);
                        }
                    }
                }

                return nbmp.ToOldBitmap(); // Resursive
            }
            else
            {
                var nbmp = new NikseBitmap(Width, Height);
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Color c = Color.Transparent;
                        if (GetPixel(x, y) > 0)
                            c = Color.White;
                        nbmp.SetPixel(x, y, c);
                    }
                }
                return nbmp.GetBitmap();
            }
        }

    }
}