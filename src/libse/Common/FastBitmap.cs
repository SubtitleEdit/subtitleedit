using System;
using System.IO;
using SkiaSharp;

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

            public PixelData(SKColor c)
            {
                Alpha = c.Alpha;
                Red = c.Red;
                Green = c.Green;
                Blue = c.Blue;
            }

            public override string ToString()
            {
                return $"({Alpha}, {Red}, {Green}, {Blue})";
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }

        private readonly SKBitmap _workingBitmap;
        private int _width;
        private IntPtr _pixelsPtr;
        private byte* _pBase = null;
        private bool _isLocked = false;

        public FastBitmap(SKBitmap inputBitmap)
        {
            _workingBitmap = inputBitmap;

            Width = inputBitmap.Width;
            Height = inputBitmap.Height;
        }

        public void LockImage()
        {
            if (_isLocked)
            {
                return;
            }

            _width = _workingBitmap.Width * sizeof(PixelData);
            if (_width % 4 != 0)
            {
                _width = 4 * (_width / 4 + 1);
            }

            // Get direct access to the pixel buffer
            _pixelsPtr = _workingBitmap.GetPixels();
            _pBase = (byte*)_pixelsPtr.ToPointer();
            _isLocked = true;
        }

        private PixelData* _pixelData = null;

        public SKColor GetPixel(int x, int y)
        {
            _pixelData = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            return new SKColor(_pixelData->Red, _pixelData->Green, _pixelData->Blue, _pixelData->Alpha);
        }

        public SKColor GetPixelNext()
        {
            _pixelData++;
            return new SKColor(_pixelData->Red, _pixelData->Green, _pixelData->Blue, _pixelData->Alpha);
        }

        public void SetPixel(int x, int y, SKColor color)
        {
            var data = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            data->Alpha = color.Alpha;
            data->Red = color.Red;
            data->Green = color.Green;
            data->Blue = color.Blue;
        }

        public void SetPixel(int x, int y, PixelData color)
        {
            var data = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            *data = color;
        }

        public void SetPixel(int x, int y, SKColor color, int length)
        {
            var data = (PixelData*)(_pBase + y * _width + x * sizeof(PixelData));
            for (int i = 0; i < length; i++)
            {
                data->Alpha = color.Alpha;
                data->Red = color.Red;
                data->Green = color.Green;
                data->Blue = color.Blue;
                data++;
            }
        }

        public SKBitmap GetBitmap()
        {
            return _workingBitmap;
        }

        public void UnlockImage()
        {
            if (!_isLocked)
            {
                return;
            }

            // Notify the bitmap that we've changed its pixels (if needed)
            _workingBitmap.NotifyPixelsChanged();
            _pixelsPtr = IntPtr.Zero;
            _pBase = null;
            _isLocked = false;
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
                    using (var bitmap = SKBitmap.Decode(ms))
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
