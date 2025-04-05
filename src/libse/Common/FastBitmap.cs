//Downloaded from Visual C# Kicks - http://www.vcskicks.com/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Nikse.SubtitleEdit.Core.Common
{
    public unsafe class FastBitmap
    {
        public struct PixelData
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;

            public PixelData(Color c)
            {
                Alpha = c.A;
                Red = c.R;
                Green = c.G;
                Blue = c.B;
            }

            public override string ToString()
            {
                return $"({Alpha}, {Red}, {Green}, {Blue})";
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }

        private readonly Bitmap _workingBitmap;
        private int _width;
        private BitmapData _bitmapData;
        private byte* _pBase = null;

        public FastBitmap(Bitmap inputBitmap)
        {
            _workingBitmap = inputBitmap;

            Width = inputBitmap.Width;
            Height = inputBitmap.Height;
        }

        public void LockImage()
        {
            var bounds = new Rectangle(Point.Empty, _workingBitmap.Size);

            _width = bounds.Width * sizeof(PixelData);
            if (_width % 4 != 0)
            {
                _width = 4 * (_width / 4 + 1);
            }

            //Lock Image
            _bitmapData = _workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            _pBase = (Byte*)_bitmapData.Scan0.ToPointer();
        }

        private PixelData* _pixelData = null;

        public Color GetPixel(int x, int y)
        {
            _pixelData = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            return Color.FromArgb(_pixelData->Alpha, _pixelData->Red, _pixelData->Green, _pixelData->Blue);
        }

        public Color GetPixelNext()
        {
            _pixelData++;
            return Color.FromArgb(_pixelData->Alpha, _pixelData->Red, _pixelData->Green, _pixelData->Blue);
        }

        public void SetPixel(int x, int y, Color color)
        {
            var data = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            data->Alpha = color.A;
            data->Red = color.R;
            data->Green = color.G;
            data->Blue = color.B;
        }

        public void SetPixel(int x, int y, PixelData color)
        {
            var data = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            *data = color;
        }

        public void SetPixel(int x, int y, Color color, int length)
        {
            var data = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            for (int i = 0; i < length; i++)
            {
                data->Alpha = color.A;
                data->Red = color.R;
                data->Green = color.G;
                data->Blue = color.B;
                data++;
            }
        }

        public Bitmap GetBitmap()
        {
            return _workingBitmap;
        }

        public void UnlockImage()
        {
            _workingBitmap.UnlockBits(_bitmapData);
            _bitmapData = null;
            _pBase = null;
        }

        public static PixelData[] ConvertByteArrayToPixelData(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                throw new ArgumentNullException(nameof(byteArray), "Byte array cannot be null or empty.");
            }

            try
            {
                using (var ms = new MemoryStream(byteArray))
                {
                    using (var bitmap = new Bitmap(ms))
                    {
                        var sampleCount = 256;
                        var pixelData = new PixelData[sampleCount];

                        var imageWidth = bitmap.Width;

                        for (var i = 0; i < sampleCount; i++)
                        {
                            var pixelX = (int)((double)i / (sampleCount - 1) * (imageWidth - 1));
                            pixelX = Math.Max(0, Math.Min(pixelX, imageWidth - 1));

                            var sampledColor = bitmap.GetPixel(pixelX, 0); // Sample from the first row.
                            pixelData[i] = new PixelData(sampledColor);
                        }

                        return pixelData;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting byte array to PixelData: {ex.Message}");
                return null; // Handle the error as needed.
            }
        }
    }
}
