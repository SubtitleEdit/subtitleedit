using System;
using System.Drawing;
using System.Drawing.Imaging;
using Nikse.SubtitleEdit.Logic.VobSub;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic
{
    public class RunLengthTwoParts
    {
        public byte[] Buffer1 { get; set; }
        public byte[] Buffer2 {get; set;}
        public int Length { get { return Buffer1.Length + Buffer2.Length;  } }
    }

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

        public NikseBitmap(int width, int height, byte[] bitmapData)
        {
            Width = width;
            Height = height;
            _bitmapData = bitmapData;
        }

        public NikseBitmap(Bitmap inputBitmap)
        {
            if (inputBitmap == null)
                return;

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

        public void ReplaceNotDarkWithWhite()
        {
            byte[] buffer = new byte[3];
            buffer[0] = 255;
            buffer[1] = 255;
            buffer[2] = 255;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] > 200 && // Alpha
                    _bitmapData[i + 2] + _bitmapData[i + 1] + _bitmapData[i] > 200)
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 3);
            }
        }

        public void ReplaceYellowWithWhite()
        {
            byte[] buffer = new byte[3];
            buffer[0] = 255;
            buffer[1] = 255;
            buffer[2] = 255;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] > 200 && // Alpha
                    _bitmapData[i + 2] > 199 && // Red
                    _bitmapData[i + 1] > 190 && // Green
                    _bitmapData[i] < 40) // Blue
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 3);
            }
        }

        public void ReplaceNonWhiteWithTransparent()
        {
            byte[] buffer = new byte[4];
            buffer[0] = 0; // B
            buffer[1] = 0; // G
            buffer[2] = 0; // R
            buffer[3] = 0; // A
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 2] + _bitmapData[i + 1] + _bitmapData[i] < 300)
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
            }
        }

        public void ReplaceTransparentWith(Color c)
        {
            byte[] buffer = new byte[4];
            buffer[0] = c.B;
            buffer[1] = c.G;
            buffer[2] = c.R;
            buffer[3] = c.A;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] < 10)
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
            }
        }

        public void MakeOneColor(Color c)
        {
            byte[] buffer = new byte[4];
            buffer[0] = c.B;
            buffer[1] = c.G;
            buffer[2] = c.R;
            buffer[3] = c.A;

            byte[] bufferTransparent = new byte[4];
            bufferTransparent[0] = 0;
            bufferTransparent[1] = 0;
            bufferTransparent[2] = 0;
            bufferTransparent[3] = 0;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i] > 20)
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
                else
                    Buffer.BlockCopy(bufferTransparent, 0, _bitmapData, i, 4);
            }
        }

        public int MakeOneColorRemoverOthers(Color c1, Color c2, int maxDif)
        {
            byte[] buffer1 = new byte[4];
            buffer1[0] = c1.B;
            buffer1[1] = c1.G;
            buffer1[2] = c1.R;
            buffer1[3] = c1.A;

            byte[] buffer2 = new byte[4];
            buffer2[0] = c2.B;
            buffer2[1] = c2.G;
            buffer2[2] = c2.R;
            buffer2[3] = c2.A;

            byte[] bufferTransparent = new byte[4];
            bufferTransparent[0] = 0;
            bufferTransparent[1] = 0;
            bufferTransparent[2] = 0;
            bufferTransparent[3] = 0;
            int count = 0;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i+3] > 20)
                {
                    if ((Math.Abs(buffer1[0] - _bitmapData[i]) < maxDif &&
                        Math.Abs(buffer1[1] - _bitmapData[i + 1]) < maxDif &&
                        Math.Abs(buffer1[2] - _bitmapData[i + 2]) < maxDif) ||
                        (Math.Abs(buffer2[0] - _bitmapData[i]) < maxDif &&
                        Math.Abs(buffer2[1] - _bitmapData[i + 1]) < maxDif &&
                        Math.Abs(buffer2[2] - _bitmapData[i + 2]) < maxDif))
                    {
                        count++;
                    }
                    else
                    {
                        Buffer.BlockCopy(bufferTransparent, 0, _bitmapData, i, 4);
                    }
                }
                else
                {
                    Buffer.BlockCopy(bufferTransparent, 0, _bitmapData, i, 4);
                }
            }
            return count;
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
                int smallestDiff = 10000;
                byte[] buffer = backgroundBuffer;
                if (backgroundBuffer[3] == 0 && _bitmapData[i + 3] < 10) // transparent
                {
                    smallestDiff = 0;
                }
                else
                {
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
                }
                Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
            }
        }

        public RunLengthTwoParts RunLengthEncodeForDvd(Color background, Color pattern, Color emphasis1, Color emphasis2)
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
                    int color = GetDvdColor(x, y, backgroundBuffer, patternBuffer, emphasis1Buffer, emphasis2Buffer);

                    if (lastColor == -1)
                    {
                        lastColor = color;
                        count = 1;
                    }
                    else if (lastColor == color && count < 64) // only allow up to 63 run-length (for SubtitleCreator compatibility)
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

            var twoParts = new RunLengthTwoParts();
            twoParts.Buffer1 = new byte[indexBufferEqual];
            Buffer.BlockCopy(bufferEqual, 0, twoParts.Buffer1, 0, indexBufferEqual);
            twoParts.Buffer2 = new byte[indexBufferUnEqual+2];
            Buffer.BlockCopy(bufferUnEqual, 0, twoParts.Buffer2, 0, indexBufferUnEqual);
            return twoParts;
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
                index++;
            }
        }

        private void WriteThreeNibbles(byte[] buffer, int count, int color, ref int index, ref bool indexHalfNibble)
        {
            //Value     Bits   n=length, c=color
            //16-63     12     0 0 0 0 n n n n n n c c           (one and a half byte)
            ushort n = (ushort)((count << 2) + color);
            if (indexHalfNibble)
            {
                index++; // there should already zeroes in last nibble
                buffer[index] = (byte)n;
                index++;
            }
            else
            {
                buffer[index] = (byte)(n >> 4);
                index++;
                buffer[index] = (byte)((n & Helper.B00011111) << 4);
            }
            indexHalfNibble = !indexHalfNibble;
        }


        private void WriteTwoNibbles(byte[] buffer, int count, int color, ref int index, bool indexHalfNibble)
        {
            //Value      Bits   n=length, c=color
            //4-15       8      0 0 n n n n c c                   (one byte)
            byte n = (byte)((count << 2) + color);
            if (indexHalfNibble)
            {
                byte firstNibble = (byte)(n >> 4);
                buffer[index] = (byte)(buffer[index] | firstNibble);
                byte secondNibble = (byte)((n & Helper.B00001111) << 4);
                index++;
                buffer[index] = secondNibble;
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
                buffer[index] = (byte)(buffer[index] | n);
                index++;
            }
            else
            {
                buffer[index] = (byte)(n << 4);
            }
            indexHalfNibble = !indexHalfNibble;
        }

        private int GetDvdColor(int x, int y, byte[] background, byte[] pattern, byte[] emphasis1, byte[] emphasis2)
        {
            _pixelAddress += 4;
            int a = _bitmapData[_pixelAddress + 3];
            int r = _bitmapData[_pixelAddress + 2];
            int g = _bitmapData[_pixelAddress + 1];
            int b = _bitmapData[_pixelAddress];

            if (pattern[0] == _bitmapData[_pixelAddress] && pattern[1] == _bitmapData[_pixelAddress + 1] && pattern[2] == _bitmapData[_pixelAddress + 2] && pattern[3] == _bitmapData[_pixelAddress + 3])
                return 1;
            if (emphasis1[0] == _bitmapData[_pixelAddress] && emphasis1[1] == _bitmapData[_pixelAddress + 1] && emphasis1[2] == _bitmapData[_pixelAddress + 2] && emphasis1[3] == _bitmapData[_pixelAddress + 3])
                return 2;
            if (emphasis2[0] == _bitmapData[_pixelAddress] && emphasis2[1] == _bitmapData[_pixelAddress + 1] && emphasis2[2] == _bitmapData[_pixelAddress + 2] && emphasis2[3] == _bitmapData[_pixelAddress + 3])
                return 3;
            return 0;
        }

        public void CropTransparentSidesAndBottom(int maximumCropping, bool bottom)
        {
            int leftStart = 0;
            bool done = false;
            int x = 0;
            int y = 0;
            while (!done && x < Width)
            {
                y = 0;
                while (!done && y < Height)
                {
                    int alpha = GetAlpha(x, y);
                    if (alpha != 0)
                    {
                        done = true;
                        leftStart = x;
                        leftStart -= maximumCropping;
                        if (leftStart < 0)
                            leftStart = 0;
                    }
                    y++;
                }
                x++;
            }

            int rightEnd = Width-1;
            done = false;
            x = Width - 1;
            while (!done && x >= 0)
            {
                y = 0;
                while (!done && y < Height)
                {
                    int alpha = GetAlpha(x, y);
                    if (alpha != 0)
                    {
                        done = true;
                        rightEnd = x;
                        rightEnd += maximumCropping;
                        if (rightEnd >= Width)
                            rightEnd = Width-1;
                    }
                    y++;
                }
                x--;
            }

            //crop bottom
            done = false;
            int newHeight = Height;
            if (bottom)
            {
                y = Height - 1;
                while (!done && y > 0)
                {
                    x = 0;
                    while (!done && x < Width)
                    {
                        int alpha = GetAlpha(x, y);
                        if (alpha != 0)
                        {
                            done = true;
                            newHeight = y + maximumCropping;
                            if (newHeight > Height)
                                newHeight = Height;
                        }
                        x++;
                    }
                    y--;
                }
            }

            if (leftStart < 2 && rightEnd >= Width - 3)
                return;

            int newWidth = rightEnd - leftStart + 1;
            if (newWidth <= 0)
                return;

            var newBitmapData = new byte[newWidth * newHeight * 4];
            int index = 0;
            for (y = 0; y < newHeight; y++)
            {
                int pixelAddress = (leftStart * 4) + (y * 4 * Width);
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, 4 * newWidth);
                index += 4 * newWidth;
            }
            Width = newWidth;
            Height = newHeight;
            _bitmapData = newBitmapData;
        }

        public void CropSidesAndBottom(int maximumCropping, Color transparentColor, bool bottom)
        {
            int leftStart = 0;
            bool done = false;
            int x = 0;
            int y = 0;
            while (!done && x < Width)
            {
                y = 0;
                while (!done && y < Height)
                {
                    Color c = GetPixel(x, y);
                    if (c != transparentColor)
                    {
                        done = true;
                        leftStart = x;
                        leftStart -= maximumCropping;
                        if (leftStart < 0)
                            leftStart = 0;
                    }
                    y++;
                }
                x++;
            }

            int rightEnd = Width - 1;
            done = false;
            x = Width - 1;
            while (!done && x >= 0)
            {
                y = 0;
                while (!done && y < Height)
                {
                    Color c = GetPixel(x, y);
                    if (c != transparentColor)
                    {
                        done = true;
                        rightEnd = x;
                        rightEnd += maximumCropping;
                        if (rightEnd >= Width)
                            rightEnd = Width - 1;
                    }
                    y++;
                }
                x--;
            }

            //crop bottom
            done = false;
            int newHeight = Height;
            if (bottom)
            {
                y = Height - 1;
                while (!done && y > 0)
                {
                    x = 0;
                    while (!done && x < Width)
                    {
                        Color c = GetPixel(x, y);
                        if (c != transparentColor)
                        {
                            done = true;
                            newHeight = y + maximumCropping;
                            if (newHeight > Height)
                                newHeight = Height;
                        }
                        x++;
                    }
                    y--;
                }
            }

            if (leftStart < 2 && rightEnd >= Width - 3)
                return;

            int newWidth = rightEnd - leftStart + 1;
            if (newWidth <= 0)
                return;

            var newBitmapData = new byte[newWidth * newHeight * 4];
            int index = 0;
            for (y = 0; y < newHeight; y++)
            {
                int pixelAddress = (leftStart * 4) + (y * 4 * Width);
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, 4 * newWidth);
                index += 4 * newWidth;
            }
            Width = newWidth;
            Height = newHeight;
            _bitmapData = newBitmapData;
        }

        public void CropTop(int maximumCropping, Color transparentColor)
        {
            bool done = false;
            int newTop = 0;
            int y = 0;
            int x = 0;
            while (!done && y < Height)
            {
                x = 0;
                while (!done && x < Width)
                {
                    Color c = GetPixel(x, y);
                    if (c != transparentColor && !(c.A == 0 && transparentColor.A == 0))
                    {
                        done = true;
                        newTop = y - maximumCropping;
                        if (newTop < 0)
                            newTop = 0;
                    }
                    x++;
                }
                y++;
            }

            if (newTop == 0)
                return;

            int newHeight = Height - newTop;
            var newBitmapData = new byte[Width * newHeight * 4];
            int index = 0;
            for (y = newTop; y < Height; y++)
            {
                int pixelAddress = y * 4 * Width;
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, 4 * Width);
                index += 4 * Width;
            }
            Height = newHeight;
            _bitmapData = newBitmapData;
        }

        public int CropTopTransparent(int maximumCropping)
        {
            bool done = false;
            int newTop = 0;
            int y = 0;
            int x = 0;
            while (!done && y < Height)
            {
                x = 0;
                while (!done && x < Width)
                {
                    int alpha = GetAlpha(x, y);
                    if (alpha > 10)
                    {
                        done = true;
                        newTop = y - maximumCropping;
                        if (newTop < 0)
                            newTop = 0;
                    }
                    x++;
                }
                y++;
            }

            if (newTop == 0)
                return 0;

            int newHeight = Height - newTop;
            var newBitmapData = new byte[Width * newHeight * 4];
            int index = 0;
            for (y = newTop; y < Height; y++)
            {
                int pixelAddress = y * 4 * Width;
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, 4 * Width);
                index += 4 * Width;
            }
            Height = newHeight;
            _bitmapData = newBitmapData;
            return newTop;
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

        public int GetAlpha(int x, int y)
        {
            return _bitmapData[(x * 4) + (y * 4 * Width) + 3];
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


        internal int RemoveAloneColors()
        {
            int count = 0;
            byte[] bufferTransparent = new byte[4];
            bufferTransparent[0] = 0;
            bufferTransparent[1] = 0;
            bufferTransparent[2] = 0;
            bufferTransparent[3] = 0;
            for (int i = 4; i < _bitmapData.Length-4; i += 4)
            {
                if ((_bitmapData[i + 3]  > 250 && _bitmapData[i + 3 + 4] > 250) || (_bitmapData[i - 1] > 250 && _bitmapData[i + 3] > 250))
                {
                    count++;
                }
                else
                {
                    Buffer.BlockCopy(bufferTransparent, 0, _bitmapData, i, 4);
                }
            }
            return count;
        }

        private static int FindBestMatch(Color color, List<Color> palette, out int maxDiff)
        {
            int smallestDiff = 1000;
            int smallestDiffIndex = -1;
            int i = 0;
            foreach (var pc in palette)
            {
                int diff = Math.Abs(pc.A - color.A) + Math.Abs(pc.R - color.R) + Math.Abs(pc.G - color.G) + Math.Abs(pc.B - color.B);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    smallestDiffIndex = i;
                    if (smallestDiff < 4)
                    {
                        maxDiff = smallestDiff;
                        return smallestDiffIndex;
                    }
                }
                i++;
            }
            maxDiff = smallestDiff;
            return smallestDiffIndex;
        }

        public Bitmap ConverTo8BitsPerPixel()
        {
            Bitmap newBitmap = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            List<Color> palette = new List<Color>();
            palette.Add(Color.Transparent);
            ColorPalette bPalette = newBitmap.Palette;
            var entries = bPalette.Entries;
            for (int i=0; i<newBitmap.Palette.Entries.Length; i++)
                entries[i] = Color.Transparent;

            BitmapData data = newBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] bytes = new byte[data.Height * data.Stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            for (int y=0; y<Height; y++)
            {
                for (int x=0; x<Width; x++)
                {
                    Color c = GetPixel(x, y);
                    if (c.A < 5)
                    {
                        bytes[y * data.Stride + x] = 0;
                    }
                    else
                    {
                        int maxDiff;
                        int index = FindBestMatch(c, palette, out maxDiff);

                        if (index == -1 && palette.Count < 255)
                        {
                            index = palette.Count;
                            entries[index] = c;
                            palette.Add(c);
                            bytes[y * data.Stride + x] = (byte)index;
                        }
                        else if (palette.Count < 200 && maxDiff > 5)
                        {
                            index = palette.Count;
                            entries[index] = c;
                            palette.Add(c);
                            bytes[y * data.Stride + x] = (byte)index;
                        }
                        else if (palette.Count < 255 && maxDiff > 15)
                        {
                            index = palette.Count;
                            entries[index] = c;
                            palette.Add(c);
                            bytes[y * data.Stride + x] = (byte)index;
                        }
                        else if (index >= 0)
                        {
                            bytes[y * data.Stride + x] = (byte)index;
                        }
                    }
                }
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            newBitmap.UnlockBits(data);
            newBitmap.Palette = bPalette;
            return newBitmap;
        }


        public NikseBitmap CopyRectangle(Rectangle section)
        {
            if (section.Bottom > Height)
                section = new Rectangle(section.Left, section.Top, section.Width, Height - section.Top);
            if (section.Width + section.Left > Width)
                section = new Rectangle(section.Left, section.Top, Width - section.Left, section.Height);
            var newBitmapData = new byte[section.Width * section.Height * 4];
            int index = 0;
            for (int y = section.Top; y < section.Bottom; y++)
            {
                int pixelAddress = (section.Left * 4) + (y * 4 * Width);
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, 4 * section.Width);
                index += 4 * section.Width;
            }
            return new NikseBitmap(section.Width, section.Height, newBitmapData);
        }


        public Color GetBrightestColor()
        {
            int max = Width * Height - 4;
            Color brightest = Color.Black;
            Color c = GetPixel(0, 0);
            for (int i = 0; i < max; i++)
            {
                c = GetPixelNext();
                if (c.A > 220 && c.R + c.G + c.B > 200 && c.R + c.G + c.B > brightest.R + brightest.G + brightest.B)
                    brightest = c;
            }
            if (IsColorClose(Color.White, brightest, 40))
                return Color.Transparent;
            if (IsColorClose(Color.Black, brightest, 10))
                return Color.Transparent;
            return brightest;
        }       

        private bool IsColorClose(Color color1, Color color2, int maxDiff)
        {
            if (Math.Abs(color1.R - color2.R) < maxDiff && Math.Abs(color1.G - color2.G) < maxDiff && Math.Abs(color1.B - color2.B) < maxDiff)
                return true;
            return false;
        }

    }
}
