using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class RunLengthTwoParts
    {
        public byte[] Buffer1 { get; set; }
        public byte[] Buffer2 { get; set; }
        public int Length => Buffer1.Length + Buffer2.Length;
    }

    public class NikseBitmap
    {
        private int _width;
        public int Width
        {
            get => _width;
            private set
            {
                _width = value;
                _widthX4 = _width * 4;
            }
        }

        public int Height { get; private set; }

        private byte[] _bitmapData;
        private int _pixelAddress;
        private int _widthX4;

        public NikseBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            _bitmapData = new byte[Height * _widthX4];
        }

        public NikseBitmap(int width, int height, byte[] bitmapData)
        {
            Width = width;
            Height = height;
            _bitmapData = bitmapData;
        }

        public NikseBitmap(SKBitmap inputBitmap)
        {
            if (inputBitmap == null)
            {
                return;
            }

            Width = inputBitmap.Width;
            Height = inputBitmap.Height;
            bool createdNewBitmap = false;
            if (inputBitmap.ColorType != SKColorType.Bgra8888)
            {
                var info = new SKImageInfo(inputBitmap.Width, inputBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                var newBitmap = new SKBitmap(info);
                using (var canvas = new SKCanvas(newBitmap))
                {
                    canvas.DrawBitmap(inputBitmap, 0, 0);
                }
                inputBitmap = newBitmap;
                createdNewBitmap = true;
            }

            // Copy pixel data from SKBitmap.
            int byteCount = inputBitmap.ByteCount;
            _bitmapData = new byte[byteCount];
            // GetPixels returns pointer to the underlying byte array.
            Marshal.Copy(inputBitmap.GetPixels(), _bitmapData, 0, byteCount);
            if (createdNewBitmap)
            {
                inputBitmap.Dispose();
            }
        }

        public NikseBitmap(NikseBitmap input)
        {
            Width = input.Width;
            Height = input.Height;
            _bitmapData = new byte[input._bitmapData.Length];
            Buffer.BlockCopy(input._bitmapData, 0, _bitmapData, 0, _bitmapData.Length);
        }

        public void ReplaceYellowWithWhite()
        {
            var buffer = new byte[3];
            buffer[0] = 255;
            buffer[1] = 255;
            buffer[2] = 255;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] > 200 && // Alpha
                    _bitmapData[i + 2] > 199 && // Red
                    _bitmapData[i + 1] > 190 && // Green
                    _bitmapData[i] < 40) // Blue
                {
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 3);
                }
            }
        }

        public void ReplaceColor(int alpha, int red, int green, int blue,
            int alphaTo, int redTo, int greenTo, int blueTo)
        {
            var buffer = new byte[4];
            buffer[0] = (byte)blueTo;
            buffer[1] = (byte)greenTo;
            buffer[2] = (byte)redTo;
            buffer[3] = (byte)alphaTo;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] == alpha &&
                    _bitmapData[i + 2] == red &&
                    _bitmapData[i + 1] == green &&
                    _bitmapData[i] == blue)
                {
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
                }
            }
        }

        public void InvertColors()
        {
            for (int i = 0; i < _bitmapData.Length;)
            {
                _bitmapData[i] = (byte)~_bitmapData[i];
                i++;
                _bitmapData[i] = (byte)~_bitmapData[i];
                i++;
                _bitmapData[i] = (byte)~_bitmapData[i];
                i += 2;
            }
        }

        public void ReplaceNonWhiteWithTransparent()
        {
            var buffer = new byte[4];
            buffer[0] = 0; // B
            buffer[1] = 0; // G
            buffer[2] = 0; // R
            buffer[3] = 0; // A
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 2] + _bitmapData[i + 1] + _bitmapData[i] < 300)
                {
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
                }
            }
        }

        public void ReplaceTransparentWith(SKColor c)
        {
            var buffer = new byte[4];
            buffer[0] = c.Blue;
            buffer[1] = c.Green;
            buffer[2] = c.Red;
            buffer[3] = c.Alpha;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] < 10)
                {
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
                }
            }
        }

        public void MakeOneColor(SKColor c)
        {
            var buffer = new byte[4];
            buffer[0] = c.Blue;
            buffer[1] = c.Green;
            buffer[2] = c.Red;
            buffer[3] = c.Alpha;

            var bufferTransparent = new byte[4];
            bufferTransparent[0] = 0;
            bufferTransparent[1] = 0;
            bufferTransparent[2] = 0;
            bufferTransparent[3] = 0;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                Buffer.BlockCopy(_bitmapData[i] > 20 ? buffer : bufferTransparent, 0, _bitmapData, i, 4);
            }
        }

        private static SKColor GetOutlineColor(SKColor borderColor)
        {
            if (borderColor.Red + borderColor.Green + borderColor.Blue < 30)
            {
                return new SKColor(75, 75, 75, 200);
            }

            return new SKColor(borderColor.Red, borderColor.Green, borderColor.Blue, 150);
        }

        /// <summary>
        /// Convert a x-color image to four colors, for e.g. DVD sub pictures.
        /// </summary>
        /// <param name="background">Background color</param>
        /// <param name="pattern">Pattern color, normally white or yellow</param>
        /// <param name="emphasis1">Emphasis 1, normally black or near black (border)</param>
        /// <param name="useInnerAntialize"></param>
        public SKColor ConvertToFourColors(SKColor background, SKColor pattern, SKColor emphasis1, bool useInnerAntialize)
        {
            var backgroundBuffer = new byte[4];
            backgroundBuffer[0] = background.Blue;
            backgroundBuffer[1] = background.Green;
            backgroundBuffer[2] = background.Red;
            backgroundBuffer[3] = background.Alpha;

            var patternBuffer = new byte[4];
            patternBuffer[0] = pattern.Blue;
            patternBuffer[1] = pattern.Green;
            patternBuffer[2] = pattern.Red;
            patternBuffer[3] = pattern.Alpha;

            var emphasis1Buffer = new byte[4];
            emphasis1Buffer[0] = emphasis1.Blue;
            emphasis1Buffer[1] = emphasis1.Green;
            emphasis1Buffer[2] = emphasis1.Red;
            emphasis1Buffer[3] = emphasis1.Alpha;

            var emphasis2Buffer = new byte[4];
            var emphasis2 = GetOutlineColor(emphasis1);
            if (!useInnerAntialize)
            {
                emphasis2Buffer[0] = emphasis2.Blue;
                emphasis2Buffer[1] = emphasis2.Green;
                emphasis2Buffer[2] = emphasis2.Red;
                emphasis2Buffer[3] = emphasis2.Alpha;
            }

            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                int smallestDiff = 10000;
                byte[] buffer = backgroundBuffer;
                if (backgroundBuffer[3] == 0 && _bitmapData[i + 3] < 10) // transparent
                {
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
                    if (useInnerAntialize)
                    {
                        if (emphasis1Diff - 20 < smallestDiff)
                        {
                            buffer = emphasis1Buffer;
                        }
                    }
                    else
                    {
                        if (emphasis1Diff < smallestDiff)
                        {
                            smallestDiff = emphasis1Diff;
                            buffer = emphasis1Buffer;
                        }

                        int emphasis2Diff = Math.Abs(emphasis2Buffer[0] - _bitmapData[i]) + Math.Abs(emphasis2Buffer[1] - _bitmapData[i + 1]) + Math.Abs(emphasis2Buffer[2] - _bitmapData[i + 2]) + Math.Abs(emphasis2Buffer[3] - _bitmapData[i + 3]);
                        if (emphasis2Diff < smallestDiff)
                        {
                            buffer = emphasis2Buffer;
                        }
                        else if (_bitmapData[i + 3] >= 10 && _bitmapData[i + 3] < 90) // anti-alias
                        {
                            buffer = emphasis2Buffer;
                        }
                    }
                }
                Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
            }

            if (useInnerAntialize)
            {
                return VobSubAntialize(pattern, emphasis1);
            }

            return emphasis2;
        }

        private SKColor VobSubAntialize(SKColor pattern, SKColor emphasis1)
        {
            int r = (int)Math.Round(((pattern.Red * 2.0 + emphasis1.Red) / 3.0));
            int g = (int)Math.Round(((pattern.Green * 2.0 + emphasis1.Green) / 3.0));
            int b = (int)Math.Round(((pattern.Blue * 2.0 + emphasis1.Blue) / 3.0));
            var antializeColor = new SKColor((byte)r, (byte)g, (byte)b, 255);

            for (int y = 1; y < Height - 1; y++)
            {
                for (int x = 1; x < Width - 1; x++)
                {
                    if (GetPixel(x, y).Equals(pattern))
                    {
                        if (GetPixel(x - 1, y).Equals(emphasis1) && GetPixel(x, y - 1).Equals(emphasis1))
                        {
                            SetPixel(x, y, antializeColor);
                        }
                        else if (GetPixel(x - 1, y).Equals(emphasis1) && GetPixel(x, y + 1).Equals(emphasis1))
                        {
                            SetPixel(x, y, antializeColor);
                        }
                        else if (GetPixel(x + 1, y).Equals(emphasis1) && GetPixel(x, y + 1).Equals(emphasis1))
                        {
                            SetPixel(x, y, antializeColor);
                        }
                        else if (GetPixel(x + 1, y).Equals(emphasis1) && GetPixel(x, y - 1).Equals(emphasis1))
                        {
                            SetPixel(x, y, antializeColor);
                        }
                    }
                }
            }

            return antializeColor;
        }

        public RunLengthTwoParts RunLengthEncodeForDvd(SKColor background, SKColor pattern, SKColor emphasis1, SKColor emphasis2)
        {
            var backgroundBuffer = new byte[4];
            backgroundBuffer[0] = background.Blue;
            backgroundBuffer[1] = background.Green;
            backgroundBuffer[2] = background.Red;
            backgroundBuffer[3] = background.Alpha;

            var patternBuffer = new byte[4];
            patternBuffer[0] = pattern.Blue;
            patternBuffer[1] = pattern.Green;
            patternBuffer[2] = pattern.Red;
            patternBuffer[3] = pattern.Alpha;

            var emphasis1Buffer = new byte[4];
            emphasis1Buffer[0] = emphasis1.Blue;
            emphasis1Buffer[1] = emphasis1.Green;
            emphasis1Buffer[2] = emphasis1.Red;
            emphasis1Buffer[3] = emphasis1.Alpha;

            var emphasis2Buffer = new byte[4];
            emphasis2Buffer[0] = emphasis2.Blue;
            emphasis2Buffer[1] = emphasis2.Green;
            emphasis2Buffer[2] = emphasis2.Red;
            emphasis2Buffer[3] = emphasis2.Alpha;

            var bufferEqual = new byte[Width * Height];
            var bufferUnEqual = new byte[Width * Height];
            int indexBufferEqual = 0;
            int indexBufferUnEqual = 0;

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

                var indexHalfNibble = false;
                var lastColor = -1;
                var count = 0;

                for (int x = 0; x < Width; x++)
                {
                    int color = GetDvdColor(patternBuffer, emphasis1Buffer, emphasis2Buffer);

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
                        WriteRle(ref indexHalfNibble, lastColor, count, ref index, buffer);
                        lastColor = color;
                        count = 1;
                    }
                }

                if (count > 0)
                {
                    WriteRle(ref indexHalfNibble, lastColor, count, ref index, buffer);
                }

                if (indexHalfNibble)
                {
                    index++;
                }

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

            var twoParts = new RunLengthTwoParts { Buffer1 = new byte[indexBufferEqual] };
            Buffer.BlockCopy(bufferEqual, 0, twoParts.Buffer1, 0, indexBufferEqual);
            twoParts.Buffer2 = new byte[indexBufferUnEqual + 2];
            Buffer.BlockCopy(bufferUnEqual, 0, twoParts.Buffer2, 0, indexBufferUnEqual);
            return twoParts;
        }

        private static void WriteRle(ref bool indexHalfNibble, int lastColor, int count, ref int index, byte[] buffer)
        {
            if (count <= 0b00000011) // 1-3 repetitions
            {
                WriteOneNibble(buffer, count, lastColor, ref index, ref indexHalfNibble);
            }
            else if (count <= 0b00001111) // 4-15 repetitions
            {
                WriteTwoNibbles(buffer, count, lastColor, ref index, indexHalfNibble);
            }
            else if (count <= 0b00111111) // 4-15 repetitions
            {
                WriteThreeNibbles(buffer, count, lastColor, ref index, ref indexHalfNibble); // 16-63 repetitions
            }
            else // 64-255 repetitions
            {
                int factor = count / 255;
                for (int i = 0; i < factor; i++)
                {
                    WriteFourNibbles(buffer, 0xff, lastColor, ref index, indexHalfNibble);
                }

                int rest = count % 255;
                if (rest > 0)
                {
                    WriteFourNibbles(buffer, rest, lastColor, ref index, indexHalfNibble);
                }
            }
        }

        private static void WriteFourNibbles(byte[] buffer, int count, int color, ref int index, bool indexHalfNibble)
        {
            int n = (count << 2) + color;
            if (indexHalfNibble)
            {
                index++;
                var firstNibble = (byte)(n >> 4);
                buffer[index] = firstNibble;
                index++;
                var secondNibble = (byte)((n & 0b00001111) << 4);
                buffer[index] = secondNibble;
            }
            else
            {
                var firstNibble = (byte)(n >> 8);
                buffer[index] = firstNibble;
                index++;
                var secondNibble = (byte)(n & 0b11111111);
                buffer[index] = secondNibble;
                index++;
            }
        }

        private static void WriteThreeNibbles(byte[] buffer, int count, int color, ref int index, ref bool indexHalfNibble)
        {
            //Value     Bits   n=length, c=color
            //16-63     12     0 0 0 0 n n n n n n c c           (one and a half byte)
            var n = (ushort)((count << 2) + color);
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
                buffer[index] = (byte)((n & 0b00011111) << 4);
            }

            indexHalfNibble = !indexHalfNibble;
        }

        private static void WriteTwoNibbles(byte[] buffer, int count, int color, ref int index, bool indexHalfNibble)
        {
            //Value      Bits   n=length, c=color
            //4-15       8      0 0 n n n n c c                   (one byte)
            var n = (byte)((count << 2) + color);
            if (indexHalfNibble)
            {
                var firstNibble = (byte)(n >> 4);
                buffer[index] = (byte)(buffer[index] | firstNibble);
                var secondNibble = (byte)((n & 0b00001111) << 4);
                index++;
                buffer[index] = secondNibble;
            }
            else
            {
                buffer[index] = n;
                index++;
            }
        }

        private static void WriteOneNibble(byte[] buffer, int count, int color, ref int index, ref bool indexHalfNibble)
        {
            var n = (byte)((count << 2) + color);
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

        private int GetDvdColor(byte[] pattern, byte[] emphasis1, byte[] emphasis2)
        {
            _pixelAddress += 4;
            int a = _bitmapData[_pixelAddress + 3];
            int r = _bitmapData[_pixelAddress + 2];
            int g = _bitmapData[_pixelAddress + 1];
            int b = _bitmapData[_pixelAddress];

            if (pattern[0] == b && pattern[1] == g && pattern[2] == r && pattern[3] == a)
            {
                return 1;
            }

            if (emphasis1[0] == b && emphasis1[1] == g && emphasis1[2] == r && emphasis1[3] == a)
            {
                return 2;
            }

            if (emphasis2[0] == b && emphasis2[1] == g && emphasis2[2] == r && emphasis2[3] == a)
            {
                return 3;
            }

            return 0;
        }

        public int CropTransparentSidesAndBottom(int maximumCropping, bool bottom)
        {
            var leftStart = 0;
            var done = false;
            var x = 0;
            int y;
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
                        if (leftStart > maximumCropping)
                        {
                            leftStart -= maximumCropping;
                        }

                        if (leftStart < 0)
                        {
                            leftStart = 0;
                        }
                    }

                    y++;
                }

                x++;
            }

            var rightEnd = Width - 1;
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
                        if (Width - rightEnd > maximumCropping)
                        {
                            rightEnd += maximumCropping;
                        }

                        if (rightEnd >= Width)
                        {
                            rightEnd = Width - 1;
                        }
                    }

                    y++;
                }

                x--;
            }

            //crop bottom
            done = false;
            var newHeight = Height;
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
                            newHeight = y + maximumCropping + 1;
                            if (newHeight > Height)
                            {
                                newHeight = Height;
                            }
                        }

                        x++;
                    }

                    y--;
                }
            }

            if (leftStart < 2 && rightEnd >= Width - 3)
            {
                return 0;
            }

            int newWidth = rightEnd - leftStart + 1;
            if (newWidth <= 0)
            {
                return 0;
            }

            var newBitmapData = new byte[newWidth * newHeight * 4];
            var index = 0;
            var newWidthX4 = 4 * newWidth;
            for (y = 0; y < newHeight; y++)
            {
                int pixelAddress = (leftStart * 4) + (y * _widthX4);
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, newWidthX4);
                index += newWidthX4;
            }

            Width = newWidth;
            Height = newHeight;
            _bitmapData = newBitmapData;
            return leftStart;
        }

        /// <returns>Pixels cropped left</returns>
        public int CropSidesAndBottom(int maximumCropping, SKColor transparentColor, bool bottom)
        {
            int leftStart = 0;
            bool done = false;
            int x = 0;
            int y;
            while (!done && x < Width)
            {
                y = 0;
                while (!done && y < Height)
                {
                    var c = GetPixel(x, y);
                    if (!c.Equals(transparentColor))
                    {
                        done = true;
                        leftStart = x;
                        leftStart -= maximumCropping;
                        if (leftStart < 0)
                        {
                            leftStart = 0;
                        }
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
                    var c = GetPixel(x, y);
                    if (!c.Equals(transparentColor))
                    {
                        done = true;
                        rightEnd = x;
                        rightEnd += maximumCropping;
                        if (rightEnd >= Width)
                        {
                            rightEnd = Width - 1;
                        }
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
                        var c = GetPixel(x, y);
                        if (!c.Equals(transparentColor))
                        {
                            done = true;
                            newHeight = y + maximumCropping;
                            if (newHeight > Height)
                            {
                                newHeight = Height;
                            }
                        }

                        x++;
                    }

                    y--;
                }
            }

            if (leftStart < 2 && rightEnd >= Width - 3)
            {
                return 0;
            }

            int newWidth = rightEnd - leftStart + 1;
            if (newWidth <= 0)
            {
                return 0;
            }

            var newBitmapData = new byte[newWidth * newHeight * 4];
            int index = 0;
            var newWidthX4 = 4 * newWidth;
            for (y = 0; y < newHeight; y++)
            {
                int pixelAddress = (leftStart * 4) + (y * _widthX4);
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, newWidthX4);
                index += newWidthX4;
            }

            Width = newWidth;
            Height = newHeight;
            _bitmapData = newBitmapData;
            return leftStart;
        }

        public void CropTop(int maximumCropping, SKColor transparentColor)
        {
            bool done = false;
            int newTop = 0;
            int y = 0;
            while (!done && y < Height)
            {
                var x = 0;
                while (!done && x < Width)
                {
                    var c = GetPixel(x, y);
                    if (!c.Equals(transparentColor) && !(c.Alpha == 0 && transparentColor.Alpha == 0))
                    {
                        done = true;
                        newTop = y - maximumCropping;
                        if (newTop < 0)
                        {
                            newTop = 0;
                        }
                    }

                    x++;
                }

                y++;
            }

            if (newTop == 0)
            {
                return;
            }

            int newHeight = Height - newTop;
            var newBitmapData = new byte[newHeight * _widthX4];
            int index = 0;
            for (y = newTop; y < Height; y++)
            {
                int pixelAddress = y * _widthX4;
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, _widthX4);
                index += _widthX4;
            }

            Height = newHeight;
            _bitmapData = newBitmapData;
        }

        public int CropTopTransparent(int minimumMargin)
        {
            bool done = false;
            int newTop = 0;
            int y = 0;
            while (!done && y < Height)
            {
                var x = 0;
                while (!done && x < Width)
                {
                    int alpha = GetAlpha(x, y);
                    if (alpha > 10)
                    {
                        done = true;
                        newTop = y - minimumMargin;
                        if (newTop < 0)
                        {
                            newTop = 0;
                        }
                    }

                    x++;
                }

                y++;
            }

            if (newTop == 0)
            {
                return 0;
            }

            int newHeight = Height - newTop;
            var newBitmapData = new byte[newHeight * _widthX4];
            int index = 0;
            for (y = newTop; y < Height; y++)
            {
                int pixelAddress = y * _widthX4;
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, _widthX4);
                index += _widthX4;
            }

            Height = newHeight;
            _bitmapData = newBitmapData;
            return newTop;
        }

        public int CalcTopCropping(SKColor color)
        {
            var y = 0;
            for (; y < Height; y++)
            {
                if (!IsHorizontalLineColor(y, color))
                {
                    break;
                }
            }

            return y;
        }

        public int CalcBottomCropping(SKColor color)
        {
            var y = Height - 1;
            for (; y > 0; y--)
            {
                if (!IsHorizontalLineColor(y, color))
                {
                    break;
                }
            }

            return Height - y;
        }

        public int CalcBottomTransparent()
        {
            var y = Height - 1;
            for (; y > 0; y--)
            {
                if (!IsHorizontalLineTransparent(y))
                {
                    break;
                }
            }

            return Height - y;
        }

        public int CalcLeftCropping(SKColor color)
        {
            var x = 0;
            for (; x < Width; x++)
            {
                if (!IsVerticalLineColor(x, color))
                {
                    break;
                }
            }

            return x;
        }

        public int CalcLeftCroppingTransparent()
        {
            var x = 0;
            for (; x < Width; x++)
            {
                if (!IsVerticalLineTransparent(x))
                {
                    break;
                }
            }

            return x;
        }

        public int CalcRightCropping(SKColor color)
        {
            var x = Width - 1;
            for (; x > 0; x--)
            {
                if (!IsVerticalLineColor(x, color))
                {
                    break;
                }
            }

            return Width - x;
        }

        public bool IsVerticalLineColor(int x, SKColor color)
        {
            for (var y = 0; y < Height; y++)
            {
                if (!IsColorClose(GetPixel(x, y), color, 9))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsHorizontalLineColor(int y, SKColor color)
        {
            for (var x = 0; x < Width; x++)
            {
                if (!IsColorClose(GetPixel(x, y), color, 9))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsHorizontalLineTransparent(int y)
        {
            for (var x = 0; x < Width; x++)
            {
                if (GetAlpha(x, y) > 1)
                {
                    return false;
                }
            }

            return true;
        }

        public void Fill(SKColor color)
        {
            var buffer = new byte[4];
            buffer[0] = color.Blue;
            buffer[1] = color.Green;
            buffer[2] = color.Red;
            buffer[3] = color.Alpha;
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
            }
        }

        public int GetAlpha(int x, int y)
        {
            return _bitmapData[(x * 4) + (y * _widthX4) + 3];
        }

        public int GetAlpha(int index)
        {
            return _bitmapData[index];
        }

        public SKColor GetPixel(int x, int y)
        {
            _pixelAddress = (x * 4) + (y * _widthX4);
            return new SKColor(
                _bitmapData[_pixelAddress + 2], // red
                _bitmapData[_pixelAddress + 1], // green
                _bitmapData[_pixelAddress],     // blue
                _bitmapData[_pixelAddress + 3]  // alpha
            );
        }

        public byte[] GetPixelColors(int x, int y)
        {
            _pixelAddress = (x * 4) + (y * _widthX4);
            return new[] { _bitmapData[_pixelAddress + 3], _bitmapData[_pixelAddress + 2], _bitmapData[_pixelAddress + 1], _bitmapData[_pixelAddress] };
        }

        public SKColor GetPixelNext()
        {
            _pixelAddress += 4;
            return new SKColor(
                _bitmapData[_pixelAddress + 2], // red
                _bitmapData[_pixelAddress + 1], // green
                _bitmapData[_pixelAddress],     // blue
                _bitmapData[_pixelAddress + 3]  // alpha
            );
        }

        public void SetPixel(int x, int y, SKColor color)
        {
            _pixelAddress = (x * 4) + (y * _widthX4);
            _bitmapData[_pixelAddress] = color.Blue;
            _bitmapData[_pixelAddress + 1] = color.Green;
            _bitmapData[_pixelAddress + 2] = color.Red;
            _bitmapData[_pixelAddress + 3] = color.Alpha;
        }

        public SKBitmap GetBitmap()
        {
            var skBitmap = new SKBitmap(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var pixels = skBitmap.GetPixels();
            Marshal.Copy(_bitmapData, 0, pixels, _bitmapData.Length);
            return skBitmap;
        }

        private static int FindBestMatch(SKColor color, List<SKColor> palette, out int maxDiff)
        {
            int smallestDiff = 1000;
            int smallestDiffIndex = -1;
            int i = 0;
            foreach (var pc in palette)
            {
                int diff = Math.Abs(pc.Alpha - color.Alpha) + Math.Abs(pc.Red - color.Red) + Math.Abs(pc.Green - color.Green) + Math.Abs(pc.Blue - color.Blue);
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

        public SKBitmap ConvertTo8BitsPerPixel()
        {
            var newBitmap = new SKBitmap(Width, Height, SKColorType.Gray8, SKAlphaType.Opaque);
            var palette = new List<SKColor> { SKColors.Transparent };
            var pixels = new byte[Width * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var c = GetPixel(x, y);
                    if (c.Alpha < 5)
                    {
                        pixels[y * Width + x] = 0;
                    }
                    else
                    {
                        int index = FindBestMatch(c, palette, out var maxDiff);

                        if (index == -1 && palette.Count < 255)
                        {
                            index = palette.Count;
                            palette.Add(c);
                            pixels[y * Width + x] = (byte)index;
                        }
                        else if (palette.Count < 200 && maxDiff > 5)
                        {
                            index = palette.Count;
                            palette.Add(c);
                            pixels[y * Width + x] = (byte)index;
                        }
                        else if (palette.Count < 255 && maxDiff > 15)
                        {
                            index = palette.Count;
                            palette.Add(c);
                            pixels[y * Width + x] = (byte)index;
                        }
                        else if (index >= 0)
                        {
                            pixels[y * Width + x] = (byte)index;
                        }
                    }
                }
            }

            // Copy pixels to the new bitmap
            IntPtr destPtr = newBitmap.GetPixels();
            Marshal.Copy(pixels, 0, destPtr, pixels.Length);

            return newBitmap;
        }

        public NikseBitmap CopyRectangle(SKRect section)
        {
            var rect = new SKRectI((int)section.Left, (int)section.Top, (int)section.Width, (int)section.Height);
            return CopyRectangle(rect);
        }

        public NikseBitmap CopyRectangle(SKRectI section)
        {
            if (section.Bottom > Height)
            {
                section = new SKRectI(section.Left, section.Top, section.Width, Height - section.Top);
            }

            if (section.Width + section.Left > Width)
            {
                section = new SKRectI(section.Left, section.Top, Width - section.Left, section.Height);
            }

            var newBitmapData = new byte[section.Width * section.Height * 4];
            int index = 0;
            var sectionWidthX4 = 4 * section.Width;
            var sectionLeftX4 = 4 * section.Left;
            for (int y = section.Top; y < section.Bottom; y++)
            {
                int pixelAddress = sectionLeftX4 + (y * _widthX4);
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, sectionWidthX4);
                index += sectionWidthX4;
            }

            return new NikseBitmap(section.Width, section.Height, newBitmapData);
        }

        /// <summary>
        /// Returns brightest color (not white though)
        /// </summary>
        /// <returns>Brightest color, if not found or if brightes color is white, then transparent color is returned</returns>
        public SKColor GetBrightestColorWhiteIsTransparent()
        {
            int max = Width * Height - 4;
            var brightest = SKColors.Black;
            for (int i = 0; i < max; i++)
            {
                var c = GetPixelNext();
                if (c.Alpha > 220 && c.Red + c.Green + c.Blue > 200 && c.Red + c.Green + c.Blue > brightest.Red + brightest.Green + brightest.Blue)
                {
                    brightest = c;
                }
            }

            if (IsColorClose(SKColors.White, brightest, 40))
            {
                return SKColors.Transparent;
            }

            if (IsColorClose(SKColors.Black, brightest, 10))
            {
                return SKColors.Transparent;
            }

            return brightest;
        }

        /// <summary>
        /// Returns brightest color
        /// </summary>
        /// <returns>Brightest color</returns>
        public SKColor GetBrightestColor()
        {
            int max = Width * Height - 4;
            var brightest = SKColors.Black;
            for (int i = 0; i < max; i++)
            {
                var c = GetPixelNext();
                if (c.Alpha > 220 && c.Red + c.Green + c.Blue > 200 && c.Red + c.Green + c.Blue > brightest.Red + brightest.Green + brightest.Blue)
                {
                    brightest = c;
                }
            }

            return brightest;
        }

        private static bool IsColorClose(SKColor color1, SKColor color2, int maxDiff)
        {
            if (Math.Abs(color1.Red - color2.Red) < maxDiff &&
                Math.Abs(color1.Green - color2.Green) < maxDiff &&
                Math.Abs(color1.Blue - color2.Blue) < maxDiff)
            {
                return true;
            }

            return false;
        }

        public void GrayScale()
        {
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                int medium = Convert.ToInt32((_bitmapData[i + 2] + _bitmapData[i + 1] + _bitmapData[i]) * 1.5 / 3.0 + 2);
                if (medium > byte.MaxValue)
                {
                    medium = byte.MaxValue;
                }

                _bitmapData[i + 2] = _bitmapData[i + 1] = _bitmapData[i] = (byte)medium;
            }
        }

        /// <summary>
        /// Make pixels with some transparency completely transparent
        /// </summary>
        /// <param name="minAlpha">Min alpha value, 0=transparent, 255=fully visible</param>
        public void MakeBackgroundTransparent(int minAlpha)
        {
            var buffer = new byte[4];
            buffer[0] = 0; // B
            buffer[1] = 0; // G
            buffer[2] = 0; // R
            buffer[3] = 0; // A
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] < minAlpha)
                {
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
                }
            }
        }

        public void MakeTwoColor(int minRgb)
        {
            var buffer = new byte[4];
            buffer[0] = 0; // B
            buffer[1] = 0; // G
            buffer[2] = 0; // R
            buffer[3] = 0; // A
            var bufferWhite = new byte[4];
            bufferWhite[0] = 255; // B
            bufferWhite[1] = 255; // G
            bufferWhite[2] = 255; // R
            bufferWhite[3] = 255; // A
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] < 1 || _bitmapData[i + 0] + _bitmapData[i + 1] + _bitmapData[i + 2] < minRgb)
                {
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
                }
                else
                {
                    Buffer.BlockCopy(bufferWhite, 0, _bitmapData, i, 4);
                }
            }
        }

        public void MakeTwoColor(int minRgb, SKColor background, SKColor foreground)
        {
            var bufferBackground = new byte[4];
            bufferBackground[0] = background.Blue; // B
            bufferBackground[1] = background.Green; // G
            bufferBackground[2] = background.Red; // R
            bufferBackground[3] = 255; // A
            var bufferForeground = new byte[4];
            bufferForeground[0] = foreground.Blue; // B
            bufferForeground[1] = foreground.Green; // G
            bufferForeground[2] = foreground.Red; // R
            bufferForeground[3] = 255; // A
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] < 1 || _bitmapData[i + 0] + _bitmapData[i + 1] + _bitmapData[i + 2] < minRgb)
                {
                    Buffer.BlockCopy(bufferBackground, 0, _bitmapData, i, 4);
                }
                else
                {
                    Buffer.BlockCopy(bufferForeground, 0, _bitmapData, i, 4);
                }
            }
        }

        private static readonly byte[] EmptyByteArray = new byte[100000];

        public void MakeVerticalLinePartTransparent(int xStart, int xEnd, int y)
        {
            if (xEnd > Width - 1)
            {
                xEnd = Width - 1;
            }

            if (xStart < 0)
            {
                xStart = 0;
            }

            int startIndex = (xStart * 4) + (y * _widthX4);
            int endIndex = (xEnd * 4) + (y * _widthX4) + 4;
            int length = endIndex - startIndex;
            Buffer.BlockCopy(EmptyByteArray, 0, _bitmapData, startIndex, length);
        }

        public void AddTransparentLineRight()
        {
            int newWidth = Width + 1;

            var newBitmapData = new byte[newWidth * Height * 4];
            int index = 0;
            for (int y = 0; y < Height; y++)
            {
                int pixelAddress = (0 * 4) + (y * _widthX4);
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, _widthX4);
                index += 4 * newWidth;
            }

            Width = newWidth;
            _bitmapData = newBitmapData;
            for (int y = 0; y < Height; y++)
            {
                SetPixel(Width - 1, y, SKColors.Transparent);
            }
        }

        public void AddMargin(int margin)
        {
            int newWidth = Width + margin * 2;
            int newHeight = Height + margin * 2;
            var newBitmapData = new byte[newWidth * newHeight * 4];
            var newWidthX4 = newWidth * 4;
            var marginX4 = margin * 4;

            for (int y = 0; y < Height; y++)
            {
                int pixelAddress = y * _widthX4;
                int index = marginX4 + (y + margin) * newWidthX4;
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, _widthX4);
            }

            Width = newWidth;
            Height = newHeight;
            _bitmapData = newBitmapData;
        }

        public void SaveAsTarga(string fileName)
        {
            // TGA header (18-byte fixed header)
            byte[] header =
            {
                0, // ID length (1 bytes)
                0, // no color map (1 bytes)
                2, // uncompressed, true color (1 bytes)
                0, 0, // Color map First Entry Index
                0, 0, // Color map Length
                0, // Color map Entry Size
                0, 0, 0, 0, // x and y origin
                (byte)(Width & 0x00FF),
                (byte)((Width & 0xFF00) >> 8),
                (byte)(Height & 0x00FF),
                (byte)((Height & 0xFF00) >> 8),
                32, // pixel depth - 32=32 bit bitmap
                0 // Image Descriptor
            };

            var pixels = new byte[_bitmapData.Length];
            int offsetDest = 0;
            for (int y = Height - 1; y >= 0; y--) // takes lines from bottom lines to top (mirrored horizontally)
            {
                for (int x = 0; x < Width; x++)
                {
                    var c = GetPixel(x, y);
                    pixels[offsetDest] = c.Blue;
                    pixels[offsetDest + 1] = c.Green;
                    pixels[offsetDest + 2] = c.Red;
                    pixels[offsetDest + 3] = c.Alpha;
                    offsetDest += 4;
                }
            }

            using (var fileStream = File.Create(fileName))
            {
                fileStream.Write(header, 0, header.Length);
                fileStream.Write(pixels, 0, pixels.Length);
            }
        }

        /// <summary>
        /// Horizontal line.
        /// </summary>
        public bool IsLineTransparent(int y)
        {
            int max = (_width * 4) + (y * _widthX4) + 3;
            for (int pos = y * _widthX4 + 3; pos < max; pos += 4)
            {
                if (_bitmapData[pos] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsVerticalLineTransparent(int x)
        {
            int xOffset = x * 4 + 3;
            for (int y = 0; y < Height; y++)
            {
                if (_bitmapData[xOffset + (y * _widthX4)] > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsImageOnlyTransparent()
        {
            for (int i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i] != 0) // check alpha
                {
                    return false;
                }
            }
            return true;
        }

        public int GetNonTransparentHeight()
        {
            var startY = 0;
            int transparentBottomPixels = 0;
            for (int y = 0; y < Height; y++)
            {
                var isLineTransparent = IsLineTransparent(y);
                if (startY == y && isLineTransparent)
                {
                    startY++;
                    continue;
                }

                if (isLineTransparent)
                {
                    transparentBottomPixels++;
                }
                else
                {
                    transparentBottomPixels = 0;
                }
            }

            return Height - startY - transparentBottomPixels;
        }

        public int GetNonTransparentWidth()
        {
            var startX = 0;
            int transparentPixelsRight = 0;
            for (int x = 0; x < Width; x++)
            {
                var isLineTransparent = IsVerticalLineTransparent(x);
                if (startX == x && isLineTransparent)
                {
                    startX++;
                    continue;
                }

                if (isLineTransparent)
                {
                    transparentPixelsRight++;
                }
                else
                {
                    transparentPixelsRight = 0;
                }
            }

            return Width - startX - transparentPixelsRight;
        }

        public void EnsureEvenLines(SKColor fillColor)
        {
            if (Width % 2 == 0 && Height % 2 == 0)
            {
                return;
            }

            int newWidth = Width;
            bool widthChanged = false;
            if (Width % 2 != 0)
            {
                newWidth++;
                widthChanged = true;
            }

            int newHeight = Height;
            bool heightChanged = false;
            if (Height % 2 != 0)
            {
                newHeight++;
                heightChanged = true;
            }

            var newBitmapData = new byte[newWidth * newHeight * 4];
            var newWidthX4 = 4 * newWidth;
            int index = 0;
            for (int y = 0; y < Height; y++)
            {
                int pixelAddress = y * _widthX4;
                Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, _widthX4);
                index += newWidthX4;
            }
            Width = newWidth;
            Height = newHeight;
            _bitmapData = newBitmapData;

            if (widthChanged)
            {
                for (var y = 0; y < Height; y++)
                {
                    SetPixel(Width - 1, y, fillColor);
                }
            }

            if (heightChanged)
            {
                for (var x = 0; x < Width; x++)
                {
                    SetPixel(x, Height - 1, fillColor);
                }
            }
        }

        public bool IsEqualTo(NikseBitmap bitmap)
        {
            if (Width != bitmap.Width || Height != bitmap.Height)
            {
                return false;
            }

            if (Width == bitmap.Width && Height == bitmap.Height &&
                Width == 0 && Height == 0)
            {
                return true;
            }

            for (int i = 0; i < _bitmapData.Length; i++)
            {
                if (_bitmapData[i] != bitmap._bitmapData[i])
                {
                    return false;
                }
            }

            return true;
        }

        public void SetTransparentTo(SKColor transparent)
        {
            var buffer = new byte[4];
            buffer[0] = transparent.Blue;
            buffer[1] = transparent.Green;
            buffer[2] = transparent.Red;
            buffer[3] = transparent.Alpha;
            for (var i = 0; i < _bitmapData.Length; i += 4)
            {
                if (_bitmapData[i + 3] == 0)
                {
                    Buffer.BlockCopy(buffer, 0, _bitmapData, i, 4);
                }
            }
        }

        public void ChangeBrightness(decimal factor)
        {
            if (factor > 1)
            {
                for (int i = 0; i < _bitmapData.Length; i += 4)
                {
                    int r = _bitmapData[i + 2];
                    int g = _bitmapData[i + 1];
                    int b = _bitmapData[i];
                    _bitmapData[i + 2] = (byte)Math.Min(byte.MaxValue, (int)(r * factor));
                    _bitmapData[i + 1] = (byte)Math.Min(byte.MaxValue, (int)(g * factor));
                    _bitmapData[i] = (byte)Math.Min(byte.MaxValue, (int)(b * factor));
                }
            }
            else
            {
                for (int i = 0; i < _bitmapData.Length; i += 4)
                {
                    int r = _bitmapData[i + 2];
                    int g = _bitmapData[i + 1];
                    int b = _bitmapData[i];
                    _bitmapData[i + 2] = (byte)Math.Max(0, (int)(r * factor));
                    _bitmapData[i + 1] = (byte)Math.Max(0, (int)(g * factor));
                    _bitmapData[i] = (byte)Math.Max(0, (int)(b * factor));
                }
            }
        }

        public void ChangeAlpha(decimal factor)
        {
            if (factor > 1)
            {
                for (int i = 0; i < _bitmapData.Length; i += 4)
                {
                    int a = _bitmapData[i + 3];
                    _bitmapData[i + 3] = (byte)Math.Min(byte.MaxValue, (int)(a * factor));
                }
            }
            else
            {
                for (int i = 0; i < _bitmapData.Length; i += 4)
                {
                    int a = _bitmapData[i + 3];
                    _bitmapData[i + 3] = (byte)Math.Max(0, (int)(a * factor));
                }
            }
        }
    }
}
