using Nikse.SubtitleEdit.Core.Common;
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
        public uint Hash { get; private set; }
        private byte[] _colors;
        public bool Italic { get; set; }
        public int ExpandCount { get; set; }
        public bool LoadedOk { get; }
        public string Text { get; set; }
        public List<BinaryOcrBitmap> ExpandedList { get; set; }

        public string Key => Text + "|#|" + Hash + "_" + Width + "x" + Height + "_" + NumberOfColoredPixels;

        public override string ToString()
        {
            if (Italic)
            {
                return Text + " (" + Width + "x" + Height + ", italic)";
            }

            return Text + " (" + Width + "x" + Height + ")";
        }

        public BinaryOcrBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            _colors = new byte[Width * Height];
            Hash = MurMurHash3.Hash(_colors);
            NumberOfColoredPixels = 0;
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
                Italic = (buffer[10] & 0b10000000) > 0;
                ExpandCount = buffer[10] & 0b01111111;
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
            var numberOfColoredPixels = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var alpha = nbmp.GetAlpha(x, y);
                    if (alpha < 100)
                    {
                        _colors[Width * y + x] = 0;
                    }
                    else
                    {
                        _colors[Width * y + x] = 1;
                        numberOfColoredPixels++;
                    }
                }
            }
            NumberOfColoredPixels = numberOfColoredPixels;
            Hash = MurMurHash3.Hash(_colors);
        }

        public bool AreColorsEqual(BinaryOcrBitmap other)
        {
            if (_colors.Length != other._colors.Length)
            {
                return false;
            }

            for (int i = 0; i < _colors.Length; i++)
            {
                if (_colors[i] != other._colors[i])
                {
                    return false;
                }
            }

            return true;
        }

        public void Save(Stream stream)
        {
            WriteInt16(stream, (ushort)Width);
            WriteInt16(stream, (ushort)Height);

            WriteInt16(stream, (ushort)X);
            WriteInt16(stream, (ushort)Y);

            WriteInt16(stream, (ushort)NumberOfColoredPixels);

            byte flags = (byte)(ExpandCount & 0b01111111);
            if (Italic)
            {
                flags = (byte)(flags + 0b10000000);
            }

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

        public int GetPixel(int index)
        {
            return _colors[index];
        }

        public void SetPixel(int x, int y)
        {
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
                    {
                        c = Color.White;
                    }

                    newRectangle.SetPixel(rectx, recty, c);
                    rectx++;
                }
                recty++;
            }
            return newRectangle;
        }

        public Bitmap ToOldBitmap()
        {
            return ToOldBitmap(Color.White);
        }

        public Bitmap ToOldBitmap(Color color)
        {
            if (ExpandedList != null && ExpandedList.Count > 0)
            {
                int minX = X;
                int minY = Y;
                int maxX = X + Width;
                int maxY = Y + Height;
                var list = new List<BinaryOcrBitmap> { this };
                foreach (BinaryOcrBitmap bob in ExpandedList)
                {
                    if (bob.X < minX)
                    {
                        minX = bob.X;
                    }

                    if (bob.Y < minY)
                    {
                        minY = bob.Y;
                    }

                    if (bob.X + bob.Width > maxX)
                    {
                        maxX = bob.X + bob.Width;
                    }

                    if (bob.Y + bob.Height > maxY)
                    {
                        maxY = bob.Y + bob.Height;
                    }

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
                            {
                                nbmp.SetPixel(bob.X - minX + x, bob.Y - minY + y);
                            }
                        }
                    }
                }

                return nbmp.ToOldBitmap(color); // Recursive
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
                        {
                            c = color;
                        }

                        nbmp.SetPixel(x, y, c);
                    }
                }
                return nbmp.GetBitmap();
            }
        }


        public bool IsPeriod()
        {
            if (ExpandCount > 0 || Y < 20)
            {
                return false;
            }

            if (Width == 4 && Height == 5 && NumberOfColoredPixels == 20)
            {
                return true;
            }

            if (Width == 5 && Height == 6 && NumberOfColoredPixels >= 28)
            {
                return true;
            }

            if (Width == 6 && Height == 7 && NumberOfColoredPixels >= 40)
            {
                return true;
            }

            if (Width < Height || Width < 5 || Width > 10 || Height < 3 || Height > 9)
            {
                return false;
            }

            return true;
        }

        public bool IsPeriodAtTop(int lowercaseHeight)
        {
            if (ExpandCount > 0 || Y > lowercaseHeight * 0.7)
            {
                return false;
            }

            if (Width == 4 && Height == 5 && NumberOfColoredPixels == 20)
            {
                return true;
            }

            if (Width == 5 && Height == 6 && NumberOfColoredPixels >= 28)
            {
                return true;
            }

            if (Width == 6 && Height == 7 && NumberOfColoredPixels >= 40)
            {
                return true;
            }

            if (Width < Height || Width < 5 || Width > 10 || Height < 3 || Height > 9)
            {
                return false;
            }

            return true;
        }

        public bool IsComma()
        {
            if (ExpandCount > 0 || Y < 20 || Height < Width || Width < 4 || Width > 12 || Height < 8 || Height > 15)
            {
                return false;
            }

            return true;
        }

        public bool IsApostrophe()
        {
            if (ExpandCount > 0 || Y > 10 || Height < Width - 2 || Width < 4 || Width > 12 || Height < 8 || Height > 16)
            {
                return false;
            }
            if ((double)Width * Height / NumberOfColoredPixels > 1.2)
            {
                return false;
            }
            if ((double)Height / Width < 2) // aspect ratio
            {
                return false;
            }

            return true;
        }

        public bool IsLowercaseI(out bool italic)
        {
            italic = false;
            if (ExpandCount > 0 || Y > 20 || Height < Width + 10 || Width < 3 || Width > 20 || Height < 21 || Height > 60)
            {
                return false;
            }
            if ((double)Height / Width < 2.2) // aspect ratio
            {
                return false;
            }

            if (Width > Height / 4)
            {
                if (GetPixel(1, 1) == 0 && GetPixel(2, 2) == 0 && GetPixel(Width - 1, Height - 1) == 0 && GetPixel(Width - 2, Height - 2) == 0)
                {
                    italic = true;
                }
                if (Height > 40 && (GetPixel(3, 3) == 1 || GetPixel(Width - 3, Height - 3) == 1))
                {
                    italic = false;
                }
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }
            if (transparentHorLines[0] || transparentHorLines[1])
            {
                return false;
            }

            for (int i = 0; i < Height / 2; i++)
            {
                if (transparentHorLines[Height - i - 1])
                {
                    return false;
                }
            }
            var top = Height / 7;
            for (int i = 0; i < 6; i++)
            {
                if (transparentHorLines[top + i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Detects a lowercase non-italic 'j'
        /// </summary>
        /// <returns>true if image is 'j'</returns>
        public bool IsLowercaseJ()
        {
            if (ExpandCount > 0 || Y > 20 || Height < Width * 2 || Width < 5 || Width > 25 || Height < 21 || Height > 70)
            {
                return false;
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }

            // top should be filled
            if (transparentHorLines[0] || transparentHorLines[1])
            {
                return false;
            }

            // bottom half should be filled
            for (int i = 0; i < Height / 2; i++)
            {
                if (transparentHorLines[Height - i - 1])
                {
                    return false;
                }
            }

            var top = Height / 7;
            for (int i = 0; i < 6; i++)
            {
                if (transparentHorLines[top + i])
                {
                    top = top + i;

                    // top left area should be free
                    int freeXPixels = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        if (GetPixel(x, Height / 2) != 0)
                        {
                            break;
                        }
                        freeXPixels++;
                    }
                    if (freeXPixels < 3 || freeXPixels > Width * 0.67)
                    {
                        return false;
                    }
                    for (int y = top; y < Height * 0.67; y++)
                    {
                        for (int x = 0; x < freeXPixels; x++)
                        {
                            if (GetPixel(x, y) != 0)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }

            return false;
        }

        public bool IsColon()
        {
            if (ExpandCount > 0 || Y < 5 || Y > 45 || Width > Height / 2 || Width < 3 || Width > 18 || Height < 14 || Height > 45)
            {
                return false;
            }

            if (NumberOfColoredPixels * 2 > Width * Height)
            {
                return false;
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }
            if (transparentHorLines[0] || transparentHorLines[1] || transparentHorLines[2])
            {
                return false;
            }

            if (transparentHorLines[Height - 1] || transparentHorLines[Height - 2] || transparentHorLines[Height - 3])
            {
                return false;
            }

            int startY = Height / 4;
            int endY = startY * 3;
            startY++;
            endY--;
            for (int y = startY; y < endY; y++)
            {
                if (!transparentHorLines[y])
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsDash()
        {
            if (ExpandCount > 0 || Y < 13 || Height * 2.3 > Width || Width < 10 || Width > 25 || Height < 3 || Height > 7)
            {
                return false;
            }

            if (NumberOfColoredPixels + 7 < Width * Height)
            {
                return false;
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < transparentHorLines.Length; i++)
            {
                if (transparentHorLines[i])
                {
                    return false;
                }
            }

            var transparentVerLines = new bool[Width];
            for (int x = 0; x < Width; x++)
            {
                transparentVerLines[x] = true;
                for (int y = 0; y < Height; y++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentVerLines[x] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < transparentVerLines.Length; i++)
            {
                if (transparentVerLines[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsExclamationMark()
        {
            if (ExpandCount > 0 || Y > 20 || Height < Width + 10 || Width < 3 || Width > 17 || Height < 21 || Height > 50)
            {
                return false;
            }

            if ((double)Height / Width < 2.3) // aspect ratio
            {
                return false;
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }
            if (transparentHorLines[Height - 1] || transparentHorLines[Height - 2])
            {
                return false;
            }

            for (int i = 0; i < Height / 2; i++)
            {
                if (transparentHorLines[i])
                {
                    return false;
                }
            }
            var bottom = Height - Height / 7;
            for (int i = 0; i < 6; i++)
            {
                if (transparentHorLines[bottom - i])
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsLowercaseL()
        {
            if (ExpandCount > 0 || Y > 20 || Height < Width + 10 || Width < 4 || Width > 17 || Height < 21 || Height > 50)
            {
                return false;
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < transparentHorLines.Length; i++)
            {
                if (transparentHorLines[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsC()
        {
            if (ExpandCount > 0 || Y > 20 || Height < Width + 1 || Width < 12 || Width > 49 || Height < 15 || Height > 55)
            {
                return false;
            }

            if (GetPixel(1, 1) != 0)
            {
                return false;
            }

            if (GetPixel(1, Height - 1) != 0)
            {
                return false;
            }

            if (GetPixel(Width - 1, 0) != 0)
            {
                return false;
            }

            if (GetPixel(Width - 2, Height - 2) != 0)
            {
                return false;
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < transparentHorLines.Length; i++)
            {
                if (transparentHorLines[i])
                {
                    return false;
                }
            }

            var transparentVerLines = new bool[Width];
            for (int x = 0; x < Width; x++)
            {
                transparentVerLines[x] = true;
                for (int y = 0; y < Height; y++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentVerLines[x] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < transparentVerLines.Length; i++)
            {
                if (transparentVerLines[i])
                {
                    return false;
                }
            }

            int halfWidth = Width / 2 - 1;
            int halfHeight = Height / 2;
            int halfHeightM1 = halfHeight--;
            for (int x = halfWidth; x < Width; x++)
            {
                if (GetPixel(x, halfHeight) != 0 || GetPixel(x, halfHeightM1) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsO()
        {
            if (ExpandCount > 0 || Y > 20 || Math.Abs(Height - Width) > (int)Math.Round(Height / 6.0) || Width < 12 || Width > 49 || Height < 15 || Height > 55)
            {
                return false;
            }

            if (GetPixel(1, 1) != 0)
            {
                return false;
            }

            if (GetPixel(1, Height - 1) != 0)
            {
                return false;
            }

            if (GetPixel(Width - 1, 0) != 0)
            {
                return false;
            }

            if (GetPixel(Width - 2, Height - 2) != 0)
            {
                return false;
            }

            var transparentHorLines = new bool[Height];
            for (int y = 0; y < Height; y++)
            {
                transparentHorLines[y] = true;
                for (int x = 0; x < Width; x++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentHorLines[y] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < transparentHorLines.Length; i++)
            {
                if (transparentHorLines[i])
                {
                    return false;
                }
            }

            var transparentVerLines = new bool[Width];
            for (int x = 0; x < Width; x++)
            {
                transparentVerLines[x] = true;
                for (int y = 0; y < Height; y++)
                {
                    if (GetPixel(x, y) != 0)
                    {
                        transparentVerLines[x] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < transparentVerLines.Length; i++)
            {
                if (transparentVerLines[i])
                {
                    return false;
                }
            }

            int halfWidth = Width / 2 - 1;
            int halfHeight = Height / 2;
            int runLength = Width / 6;
            for (int x = halfWidth - runLength; x < halfWidth + runLength; x++)
            {
                if (GetPixel(x, halfHeight - 1) != 0 ||
                    GetPixel(x, halfHeight + 0) != 0 ||
                    GetPixel(x, halfHeight + 1) != 0)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
