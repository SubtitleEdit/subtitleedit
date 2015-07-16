﻿//Downloaded from Visual C# Kicks - http://www.vcskicks.com/
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Nikse.SubtitleEdit.Logic
{
    unsafe public class FastBitmap
    {
        private struct PixelData
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;

            public override string ToString()
            {
                return "(" + Alpha + ", " + Red + ", " + Green + ", " + Blue + ")";
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }

        private readonly Bitmap _workingBitmap;
        private int _width;
        private BitmapData _bitmapData;
        private Byte* _pBase = null;

        public FastBitmap(Bitmap inputBitmap)
        {
            _workingBitmap = inputBitmap;

            if (_workingBitmap.PixelFormat != PixelFormat.Format32bppArgb &&
                Environment.OSVersion.Version.Major < 6 && Configuration.Settings.General.SubtitleFontName == Utilities.WinXP2KUnicodeFontName) // 6 == Vista/Win2008Server/Win7
            { // WinXp Fix
                var newBitmap = new Bitmap(_workingBitmap.Width, _workingBitmap.Height, PixelFormat.Format32bppArgb);
                for (int y = 0; y < _workingBitmap.Height; y++)
                    for (int x = 0; x < _workingBitmap.Width; x++)
                        newBitmap.SetPixel(x, y, _workingBitmap.GetPixel(x, y));
                _workingBitmap = newBitmap;
            }

            Width = inputBitmap.Width;
            Height = inputBitmap.Height;
        }

        public void LockImage()
        {
            var bounds = new Rectangle(Point.Empty, _workingBitmap.Size);

            _width = bounds.Width * sizeof(PixelData);
            if (_width % 4 != 0) _width = 4 * (_width / 4 + 1);

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
    }
}