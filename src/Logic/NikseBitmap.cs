using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Nikse.SubtitleEdit.Logic
{
    unsafe public class NikseBitmap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private readonly Bitmap _workingBitmap;
        private byte[] _bitmapData;
        private int _pixelAddress = 0;

        public NikseBitmap(Bitmap inputBitmap)
        {
            _workingBitmap = inputBitmap;
            Width = _workingBitmap.Width;
            Height = _workingBitmap.Height;

            if (_workingBitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                var newBitmap = new Bitmap(_workingBitmap.Width, _workingBitmap.Height, PixelFormat.Format32bppArgb);
                for (int y = 0; y < _workingBitmap.Height; y++)
                    for (int x = 0; x < _workingBitmap.Width; x++)
                        newBitmap.SetPixel(x, y, _workingBitmap.GetPixel(x, y));
                _workingBitmap = newBitmap;
            }

            _bitmapData = new byte[Width * Height * 4];
            BitmapData bitmapdata = _workingBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //Buffer.BlockCopy(buffer, dataIndex, DataBuffer, 0, dataSize);
            System.Runtime.InteropServices.Marshal.Copy(bitmapdata.Scan0, _bitmapData, 0, _bitmapData.Length);
            _workingBitmap.UnlockBits(bitmapdata);

        }

        public Color GetPixel(int x, int y)
        {
            int _pixelAddress = x * y * 4;
            return Color.FromArgb(_bitmapData[_pixelAddress], _bitmapData[_pixelAddress+1], _bitmapData[_pixelAddress+2], _bitmapData[_pixelAddress+3]);
        }

        public Color GetPixelNext()
        {
            _pixelAddress += 4;
            return Color.FromArgb(_bitmapData[_pixelAddress], _bitmapData[_pixelAddress + 1], _bitmapData[_pixelAddress + 2], _bitmapData[_pixelAddress + 3]);
        }

        public void SetPixel(int x, int y, Color color)
        {
            int _pixelAddress = x * y * 4;
            _bitmapData[_pixelAddress] = (byte)color.A;
            _bitmapData[_pixelAddress+1] = (byte)color.R;
            _bitmapData[_pixelAddress+2] = (byte)color.G;
            _bitmapData[_pixelAddress+3] = (byte)color.B;
        }

        public Bitmap GetBitmap()
        {
            Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr destination = bitmapdata.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(_bitmapData, 0, destination, _bitmapData.Length);
            bitmap.UnlockBits(bitmapdata);
            return bitmap;
        }

    }
}
