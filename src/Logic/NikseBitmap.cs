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

        /// <summary>
        /// Convert a x-color image to four colors, for e.g. dvd sub pictures.
        /// Colors CAN be in any order but should not...
        /// </summary>
        /// <param name="background">Background color</param>
        /// <param name="pattern">Pattern color, normally white or yellow</param>
        /// <param name="emphasis1">Emphasis 1, normally black or near black (border)</param>
        /// <param name="emphasis2">Emphasis 1, normally black or near black (anti-alias)</param>
        public void ConverToFourColors(Color background, Color pattern, Color emphasis1, Color emphasis2)
        {
            byte[] backgroundBuffer = new byte[4];
            backgroundBuffer[0] = (byte)background.B;
            backgroundBuffer[1] = (byte)background.G;
            backgroundBuffer[2] = (byte)background.R;
            backgroundBuffer[3] = (byte)background.A;

            byte[] patternBuffer = new byte[4];
            patternBuffer[0] = (byte)pattern.B;
            patternBuffer[1] = (byte)pattern.G;
            patternBuffer[2] = (byte)pattern.R;
            patternBuffer[3] = (byte)pattern.A;

            byte[] emphasis1Buffer = new byte[4];
            emphasis1Buffer[0] = (byte)emphasis1.B;
            emphasis1Buffer[1] = (byte)emphasis1.G;
            emphasis1Buffer[2] = (byte)emphasis1.R;
            emphasis1Buffer[3] = (byte)emphasis1.A;

            byte[] emphasis2Buffer = new byte[4];
            emphasis2Buffer[0] = (byte)emphasis2.B;
            emphasis2Buffer[1] = (byte)emphasis2.G;
            emphasis2Buffer[2] = (byte)emphasis2.R;
            emphasis2Buffer[3] = (byte)emphasis2.A;

            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                int smallestDiff = 10000; // Math.Abs(backgroundBuffer[0] - _bitmapData[i]) + Math.Abs(backgroundBuffer[1] - _bitmapData[i + 1]) + Math.Abs(backgroundBuffer[2] - _bitmapData[i + 2]) + Math.Abs(backgroundBuffer[3] - _bitmapData[i + 3]);
                byte[] buffer = backgroundBuffer;
                if (backgroundBuffer[3] == 0 && _bitmapData[i+3] < 10) // transparent
                    smallestDiff = 0;

                int patternDiff = Math.Abs(patternBuffer[0] - _bitmapData[i]) + Math.Abs(patternBuffer[1] - _bitmapData[i + 1]) + Math.Abs(patternBuffer[2] - _bitmapData[i + 2]) + Math.Abs(patternBuffer[3] - _bitmapData[i + 3]);
                if (patternDiff < smallestDiff)
                {
                    smallestDiff = patternDiff;
                    buffer = patternBuffer;
                }

                int emphasis1Diff = Math.Abs(emphasis1Buffer[0] - _bitmapData[i]) + Math.Abs(emphasis1Buffer[1] - _bitmapData[i + 1]) + Math.Abs(emphasis1Buffer[2] - _bitmapData[i + 2]) + Math.Abs(emphasis1Buffer[3] - _bitmapData[i + 3]);
                if (emphasis1Diff < smallestDiff)
                {
                    smallestDiff = emphasis1Diff;
                    buffer = emphasis1Buffer;
                }

                int emphasis2Diff = Math.Abs(emphasis2Buffer[0] - _bitmapData[i]) + Math.Abs(emphasis2Buffer[1] - _bitmapData[i + 1]) + Math.Abs(emphasis2Buffer[2] - _bitmapData[i + 2]) + Math.Abs(emphasis2Buffer[3] - _bitmapData[i + 3]);
                if (emphasis2Diff < smallestDiff)
                {
                    smallestDiff = emphasis2Diff;
                    buffer = emphasis2Buffer;
                }
                else if (_bitmapData[i + 3] >= 10 && _bitmapData[i + 3] < 90) // anti-alias
                {
                    smallestDiff = emphasis2Diff;
                    buffer = emphasis2Buffer;
                }

                Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
            }
        }

        public byte[] RunLengthEncodeForDvd(Color background, Color pattern, Color emphasis1, Color emphasis2)
        {
            byte[] bufferEqual = new byte[Width * Height];
            byte[] bufferUnEqual = new byte[Width * Height];
            int indexBufferEqual = 0;
            int indexBufferUnEqual = 0;
            bool indexHalfNibble = false;
            int lastColor = -1;
            int count = -1;

            _pixelAddress = -4;
            for (int y = 0; y < Height; y++)
            {
                int index;
                byte[] buffer;
                if (y % 2 == 0)
                {
                    index = indexBufferEqual;
                    buffer = bufferEqual;
                }
                else
                {
                    index = indexBufferUnEqual;
                    buffer = bufferUnEqual;
                }
                indexHalfNibble = false;
                lastColor = -1;
                count = 0;

                for (int x = 0; x < Width; x++)
                {
                    int color = GetDvdColor(x, y, background, pattern, emphasis1, emphasis2);

                    if (lastColor == -1)
                    {
                        lastColor = color;
                        count = 1;
                    }
                    else if (lastColor == color)
                    {
                        count++;
                    }
                    else
                    {
                        WriteRLE(ref indexHalfNibble, lastColor, count, ref index, buffer);
                        lastColor = color;
                        count = 1;
                    }
                }
                if (count > 0)
                    WriteRLE(ref indexHalfNibble, lastColor, count, ref index, buffer);

                if (indexHalfNibble)
                    index++;

                if (y % 2 == 0)
                {
                    indexBufferEqual = index;
                    bufferEqual = buffer;
                }
                else
                {
                    indexBufferUnEqual = index;
                    bufferUnEqual = buffer;
                }
            }

            byte[] result = new byte[indexBufferEqual + indexBufferUnEqual];
            Buffer.BlockCopy(bufferEqual, 0, result, 0, indexBufferEqual);
            Buffer.BlockCopy(bufferUnEqual, 0, result, indexBufferEqual, indexBufferUnEqual);

            return result;
        }

        private void WriteRLE(ref bool indexHalfNibble, int lastColor, int count, ref int index, byte[] buffer)
        {
            if (count <= Nikse.SubtitleEdit.Logic.VobSub.Helper.B00000011) // 1-3 repetitions
            {
                WriteOneNibble(buffer, count, lastColor, ref index, ref indexHalfNibble);
            }
            else if (count <= Nikse.SubtitleEdit.Logic.VobSub.Helper.B00001111) // 4-15 repetitions
            {
                WriteTwoNibbles(buffer, count, lastColor, ref index, indexHalfNibble);
            }
            else if (count <= Nikse.SubtitleEdit.Logic.VobSub.Helper.B00111111) // 4-15 repetitions
            {
                WriteThreeNibbles(buffer, count, lastColor, ref index, ref indexHalfNibble); // 16-63 repetitions
            }
            else // 64-255 repetitions
            {
                int factor = count / 255;
                for (int i=0; i<factor; i++)
                    WriteFourNibbles(buffer, 0xff, lastColor, ref index, indexHalfNibble);

                int rest = count % 255;
                if (rest > 0)
                    WriteFourNibbles(buffer, rest, lastColor, ref index, indexHalfNibble);
            }
        }

        private void WriteFourNibbles(byte[] buffer, int count, int color, ref int index, bool indexHalfNibble)
        {
            int n = (count << 2) + color;
            if (indexHalfNibble)
            {
                index++;
                byte firstNibble = (byte)(n >> 4);
                buffer[index] = firstNibble;
                index++;
                byte secondNibble = (byte)((n & Nikse.SubtitleEdit.Logic.VobSub.Helper.B00001111) << 4);
                buffer[index] = (byte)secondNibble;
            }
            else
            {
                byte firstNibble = (byte)(n >> 8);
                buffer[index] = firstNibble;
                index++;
                byte secondNibble = (byte)(n & Nikse.SubtitleEdit.Logic.VobSub.Helper.B11111111);
                buffer[index] = (byte)secondNibble;
            }
        }

        private void WriteThreeNibbles(byte[] buffer, int count, int color, ref int index, ref bool indexHalfNibble)
        {
            byte n = (byte)((count << 2) + color);
            if (indexHalfNibble)
            {
                index++;
                buffer[index] = n;
            }
            else
            {
                buffer[index] = (byte)(n >> 4);
                index++;
                buffer[index] = (byte)(n << 4);
            }
            indexHalfNibble = !indexHalfNibble;
        }


        private void WriteTwoNibbles(byte[] buffer, int count, int color, ref int index, bool indexHalfNibble)
        {
            byte n = (byte)((count << 2) + color);
            if (indexHalfNibble)
            {
                byte firstNibble = (byte)(n >> 4);
                buffer[index] = (byte)(buffer[index] & firstNibble);
                byte secondNibble = (byte)(n << 4);
                index++;
                buffer[index] = (byte)secondNibble;
            }
            else
            {
                buffer[index] = n;
                index++;
            }
        }

        private void WriteOneNibble(byte[] buffer, int count, int color, ref int index, ref bool indexHalfNibble)
        {
            byte n = (byte)((count << 2) + color);
            if (indexHalfNibble)
            {
                buffer[index] = (byte)(buffer[index] & n);
                index++;
            }
            else
            {
                buffer[index] = (byte)(n << 4);
            }
            indexHalfNibble = !indexHalfNibble;
        }

        private int GetDvdColor(int x, int y, Color background, Color pattern, Color emphasis1, Color emphasis2)
        {
            Color c = GetPixelNext();
            if (emphasis2 == c)
                return 3;
            else if (emphasis1 == c)
                return 2;
            if (pattern == c)
                return 1;
            return 0;
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
