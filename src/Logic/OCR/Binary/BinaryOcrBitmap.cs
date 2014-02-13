using System;
using System.Drawing;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.OCR.Binary
{
    public class BinaryOcrBitmap
    {

        //File format:
        //-------------
        //2bytes=width
        //2bytes=height
        //2bytes=numberOfColoredPixels
        //1byte=flags (1 bit = italic, next 7 bits = ExpandCount)
        //4bytes=hash
        //1bytes=text len
        //text len bytes=text (UTF-8)
        //w*h bytes / 8=pixels as bits(byte aligned)

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int NumberOfColoredPixels { get; private set; }
        public UInt32 Hash { get; private set; }
        private byte[] _colors;
        public bool Italic { get; set; }
        public int ExpandCount { get; set; }
        public bool LoadedOK { get; private set; }
        public string Text { get; set; }

        public BinaryOcrBitmap(Stream stream)
        {
            try
            {
                byte[] buffer = new byte[12];
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read < buffer.Length)
                {
                    LoadedOK = false;
                    return;
                }
                Width = buffer[0] << 8 | buffer[1];
                Height = buffer[2] << 8 | buffer[3];
                NumberOfColoredPixels = buffer[4] << 8 | buffer[5];
                Italic = (buffer[6] & Nikse.SubtitleEdit.Logic.VobSub.Helper.B10000000) > 0;
                ExpandCount = buffer[6] & Nikse.SubtitleEdit.Logic.VobSub.Helper.B01111111;
                Hash = (uint)(buffer[7] << 24 | buffer[8] << 16 | buffer[9] << 8 | buffer[10]);
                int textLen = buffer[11];
                if (textLen > 0)
                {
                    buffer = new byte[textLen];
                    stream.Read(buffer, 0, buffer.Length);
                    Text = System.Text.Encoding.UTF8.GetString(buffer);
                }

                _colors = new byte[Width * Height];
                stream.Read(_colors, 0, _colors.Length);
                LoadedOK = true;
            }
            catch
            {
                LoadedOK = false;
            }
        }

        public BinaryOcrBitmap(NikseBitmap nbmp)
        {
            InitializeViaNikseBmp(nbmp);
        }

        public BinaryOcrBitmap(NikseBitmap nbmp, bool italic, int expandCount, string text)
        {
            InitializeViaNikseBmp(nbmp);
            Italic = italic;
            ExpandCount = expandCount;
            Text = text;
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
                    this.SetPixel(x, y, nbmp.GetPixel(x, y));
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
            WriteInt16(stream, (short)Width);
            WriteInt16(stream, (short)Height);

            WriteInt16(stream, (short)NumberOfColoredPixels);
            
            byte flags = (byte)(ExpandCount & Nikse.SubtitleEdit.Logic.VobSub.Helper.B01111111);
            if (Italic)
                flags = (byte)(flags + Nikse.SubtitleEdit.Logic.VobSub.Helper.B10000000);
            stream.WriteByte(flags);

            WriteInt32(stream, (UInt32)Hash);

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

        private int ReadInt16(Stream stream)
        {
            byte b0 = (byte)stream.ReadByte();
            byte b1 = (byte)stream.ReadByte();
            return b0 << 8 | b1;
        }

        private void WriteInt16(Stream stream, short val)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)((val & 0xFF00) >> 8);
            buffer[1] = (byte)(val & 0x00FF);
            stream.Write(buffer, 0, buffer.Length);
        }

        private void WriteInt32(Stream stream, UInt32 val)
        {
            System.ComponentModel.ByteConverter bc = new System.ComponentModel.ByteConverter();
            
            byte[] buffer = new byte[4];
            buffer[0] = (byte)((val & 0xFF000000) >> 24);
            buffer[1] = (byte)((val & 0xFF0000) >> 16);
            buffer[2] = (byte)((val & 0xFF00) >> 8);
            buffer[3] = (byte)(val & 0xFF);
            stream.Write(buffer, 0, buffer.Length);
        }
        
        public bool GetPixel(int x, int y)
        {
            return _colors[Width * y + x] > 0;
        }

        public void SetPixel(int x, int y, Color c)
        {
            if (c.A < 100)
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
            ManagedBitmap newRectangle = new ManagedBitmap(section.Width, section.Height);

            int recty = 0;
            for (int y = section.Top; y < section.Top + section.Height; y++)
            {
                int rectx = 0;
                for (int x = section.Left; x < section.Left + section.Width; x++)
                {
                    Color c = Color.Transparent;
                    if (this.GetPixel(x, y))
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
            var nbmp = new NikseBitmap(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Color c = Color.Transparent;
                    if (this.GetPixel(x, y))
                        c = Color.White;
                    nbmp.SetPixel(x, y, c);
                }
            }
            return nbmp.GetBitmap();
        }

        internal void DrawImage(ManagedBitmap bmp, Point point)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int newY = point.Y + y;
                if (newY >= 0 && newY < Height)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int newX = point.X + x;
                        if (newX >= 0 && newX < Width)
                            this.SetPixel(newX, newY, bmp.GetPixel(x, y));
                    }
                }
            }
        }

    }
}
