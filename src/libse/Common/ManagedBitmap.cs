using System.IO;
using System.IO.Compression;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class ManagedBitmap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private readonly SKColor[] _colors;
        public bool LoadedOk { get; private set; }

        public ManagedBitmap(string fileName)
        {
            try
            {
                byte[] buffer;
                using (var fd = new MemoryStream())
                using (Stream csStream = new GZipStream(File.OpenRead(fileName), CompressionMode.Decompress))
                {
                    csStream.CopyTo(fd);
                    buffer = fd.ToArray();
                }

                Width = buffer[4] << 8 | buffer[5];
                Height = buffer[6] << 8 | buffer[7];
                _colors = new SKColor[Width * Height];
                int start = 8;
                for (int i = 0; i < _colors.Length; i++)
                {
                    _colors[i] = new SKColor(buffer[start + 1], buffer[start + 2], buffer[start + 3], buffer[start]);
                    start += 4;
                }
                LoadedOk = true;
            }
            catch
            {
                LoadedOk = false;
            }
        }

        public ManagedBitmap(Stream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, buffer.Length);
            Width = buffer[4] << 8 | buffer[5];
            Height = buffer[6] << 8 | buffer[7];
            _colors = new SKColor[Width * Height];
            buffer = new byte[Width * Height * 4];
            stream.Read(buffer, 0, buffer.Length);
            int start = 0;
            for (int i = 0; i < _colors.Length; i++)
            {
                _colors[i] = new SKColor(buffer[start + 1], buffer[start + 2], buffer[start + 3], buffer[start]);
                start += 4;
            }
        }

        public ManagedBitmap(SKBitmap oldBitmap)
        {
            var nbmp = new NikseBitmap(oldBitmap);
            Width = nbmp.Width;
            Height = nbmp.Height;
            _colors = new SKColor[Width * Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetPixel(x, y, nbmp.GetPixel(x, y));
                }
            }
        }

        public ManagedBitmap(NikseBitmap nbmp)
        {
            Width = nbmp.Width;
            Height = nbmp.Height;
            _colors = new SKColor[Width * Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetPixel(x, y, nbmp.GetPixel(x, y));
                }
            }
        }

        public void Save(string fileName)
        {
            using (var outFile = new MemoryStream())
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("MBMP");
                outFile.Write(buffer, 0, buffer.Length);
                WriteInt16(outFile, (short)Width);
                WriteInt16(outFile, (short)Height);
                foreach (SKColor c in _colors)
                {
                    WriteColor(outFile, c);
                }
                buffer = outFile.ToArray();
                using (var gz = new GZipStream(new FileStream(fileName, FileMode.Create), CompressionMode.Compress, false))
                {
                    gz.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public void AppendToStream(Stream targetStream)
        {
            using (var outFile = new MemoryStream())
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("MBMP");
                outFile.Write(buffer, 0, buffer.Length);
                WriteInt16(outFile, (short)Width);
                WriteInt16(outFile, (short)Height);
                foreach (SKColor c in _colors)
                {
                    WriteColor(outFile, c);
                }
                buffer = outFile.ToArray();
                targetStream.Write(buffer, 0, buffer.Length);
            }
        }

        private static void WriteInt16(Stream stream, short val)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)((val & 0xFF00) >> 8);
            buffer[1] = (byte)(val & 0x00FF);
            stream.Write(buffer, 0, buffer.Length);
        }

        private static void WriteColor(Stream stream, SKColor c)
        {
            byte[] buffer = new byte[4];
            buffer[0] = c.Alpha;
            buffer[1] = c.Red;
            buffer[2] = c.Green;
            buffer[3] = c.Blue;
            stream.Write(buffer, 0, buffer.Length);
        }

        public ManagedBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            _colors = new SKColor[Width * Height];
        }

        public SKColor GetPixel(int x, int y)
        {
            return _colors[Width * y + x];
        }

        public void SetPixel(int x, int y, SKColor c)
        {
            _colors[Width * y + x] = c;
        }

        /// <summary>
        /// Copies a rectangle from the bitmap to a new bitmap
        /// </summary>
        /// <param name="section">Source rectangle</param>
        /// <returns>Rectangle from current image as new bitmap</returns>
        public ManagedBitmap GetRectangle(SKRectI section)
        {
            var newRectangle = new ManagedBitmap(section.Width, section.Height);

            int recty = 0;
            for (int y = section.Top; y < section.Top + section.Height; y++)
            {
                int rectx = 0;
                for (int x = section.Left; x < section.Left + section.Width; x++)
                {
                    newRectangle.SetPixel(rectx, recty, GetPixel(x, y));
                    rectx++;
                }
                recty++;
            }
            return newRectangle;
        }

        public SKBitmap ToSKBitmap()
        {
            var nbmp = new NikseBitmap(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    nbmp.SetPixel(x, y, GetPixel(x, y));
                }
            }
            return nbmp.GetBitmap();
        }
    }
}
