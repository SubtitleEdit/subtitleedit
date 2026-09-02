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

            if (_workingBitmap.ColorType != SKColorType.Bgra8888)
            {
                // PixelData writes raw B,G,R,A bytes; on any other layout (e.g. the
                // Rgba8888 platform default on macOS/Linux) red and blue would silently
                // swap. Callers must create their bitmaps as Bgra8888.
                throw new InvalidOperationException(
                    $"FastBitmap requires a Bgra8888 bitmap, got {_workingBitmap.ColorType}.");
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
            if (length <= 0)
            {
                return;
            }

            // Bgra8888 in memory is B,G,R,A at increasing addresses; as a little-endian
            // uint that is B | G<<8 | R<<16 | A<<24. A span Fill vectorizes the run.
            var value = color.Blue | ((uint)color.Green << 8) | ((uint)color.Red << 16) | ((uint)color.Alpha << 24);
            var data = (uint*)(_pBase + y * _width + x * sizeof(PixelData));
            new Span<uint>(data, length).Fill(value);
        }

        /// <summary>
        /// True when every pixel in row <paramref name="y"/> has alpha below
        /// <paramref name="alphaLimit"/>. Reads only the alpha byte of each pixel -
        /// used by crop scans that would otherwise build an SKColor per pixel.
        /// </summary>
        public bool IsRowTransparent(int y, byte alphaLimit)
        {
            var p = _pBase + y * _width + 3;
            for (var x = 0; x < Width; x++, p += sizeof(PixelData))
            {
                if (*p >= alphaLimit)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True when every pixel in column <paramref name="x"/> from row
        /// <paramref name="startY"/> down has alpha below <paramref name="alphaLimit"/>.
        /// </summary>
        public bool IsColumnTransparent(int x, int startY, byte alphaLimit)
        {
            var p = _pBase + startY * _width + x * sizeof(PixelData) + 3;
            for (var y = startY; y < Height; y++, p += _width)
            {
                if (*p >= alphaLimit)
                {
                    return false;
                }
            }

            return true;
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
