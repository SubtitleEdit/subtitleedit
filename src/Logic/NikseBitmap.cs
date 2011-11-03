using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Nikse.SubtitleEdit.Logic
{
    unsafe public class NikseBitmap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private byte[] _bitmapData;
        private int _pixelAddress = 0;

        public NikseBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            _bitmapData = new byte[Width * Height * 4];
        }

        public NikseBitmap(Bitmap inputBitmap)
        {
            Width = inputBitmap.Width;
            Height = inputBitmap.Height;

            if (inputBitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                var newBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height, PixelFormat.Format32bppArgb);
                for (int y = 0; y < inputBitmap.Height; y++)
                    for (int x = 0; x < inputBitmap.Width; x++)
                        newBitmap.SetPixel(x, y, inputBitmap.GetPixel(x, y));
                inputBitmap = newBitmap;
            }

            _bitmapData = new byte[Width * Height * 4];
            BitmapData bitmapdata = inputBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //Buffer.BlockCopy(buffer, dataIndex, DataBuffer, 0, dataSize);
            System.Runtime.InteropServices.Marshal.Copy(bitmapdata.Scan0, _bitmapData, 0, _bitmapData.Length);
            inputBitmap.UnlockBits(bitmapdata);
        }

        public void ReplaceYellowWithWhite()
        {
            byte[] buffer = new byte[3];
            buffer[0] = 255;
            buffer[1] = 255;
            buffer[2] = 255;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i+3] > 200 &&  _bitmapData[i+2] > 220 && _bitmapData[i+1] > 220 && _bitmapData[i] < 40)
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 3);
            }
        }

        public void Fill(Color color)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)color.B;
            buffer[1] = (byte)color.G;
            buffer[2] = (byte)color.R;
            buffer[3] = (byte)color.A;
            for (int i=0; i<_bitmapData.Length; i+=4)
                Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
        }

        public Color GetPixel(int x, int y)
        {
            _pixelAddress = (x * 4) + (y * 4 * Width);
            return Color.FromArgb(_bitmapData[_pixelAddress+3], _bitmapData[_pixelAddress+2], _bitmapData[_pixelAddress+1], _bitmapData[_pixelAddress]);
        }

        public Color GetPixelNext()
        {
            _pixelAddress += 4;
            return Color.FromArgb(_bitmapData[_pixelAddress+3], _bitmapData[_pixelAddress + 2], _bitmapData[_pixelAddress + 1], _bitmapData[_pixelAddress]);
        }

        public void SetPixel(int x, int y, Color color)
        {
            _pixelAddress = (x * 4) + (y * 4 * Width);
            _bitmapData[_pixelAddress] = (byte)color.B;
            _bitmapData[_pixelAddress+1] = (byte)color.G;
            _bitmapData[_pixelAddress+2] = (byte)color.R;
            _bitmapData[_pixelAddress+3] = (byte)color.A;
        }

        public void SetPixelNext(Color color)
        {
            _pixelAddress += 4;
            _bitmapData[_pixelAddress] = (byte)color.B;
            _bitmapData[_pixelAddress + 1] = (byte)color.G;
            _bitmapData[_pixelAddress + 2] = (byte)color.R;
            _bitmapData[_pixelAddress + 3] = (byte)color.A;
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
