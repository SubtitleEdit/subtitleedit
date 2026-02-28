using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Ocr;

public readonly struct NikseRectangle
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public int Left => X;
    public int Top => Y;
    public int Right => unchecked(X + Width);
    public int Bottom => unchecked(Y + Height);

    public NikseRectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}

public class RunLengthTwoParts
{
    public byte[] Buffer1 { get; set; }
    public byte[] Buffer2 { get; set; }
    public int Length => Buffer1.Length + Buffer2.Length;

    public RunLengthTwoParts()
    {
        Buffer1 = [];
        Buffer2 = [];
    }
}

public class NikseBitmap2
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

    public NikseBitmap2(int width, int height)
    {
        Width = width;
        Height = height;
        _bitmapData = new byte[Height * _widthX4];
    }

    public NikseBitmap2(int width, int height, byte[] bitmapData)
    {
        Width = width;
        Height = height;
        _bitmapData = bitmapData;
    }

    public NikseBitmap2(SKBitmap inputBitmap)
    {
        Width = inputBitmap.Width;
        Height = inputBitmap.Height;

        // Convert to BGRA8888 (equivalent to Format32bppArgb) if necessary
        SKBitmap? convertedBitmap = null;
        var needsDisposal = false;

        if (inputBitmap.ColorType != SKColorType.Bgra8888)
        {
            convertedBitmap = new SKBitmap(inputBitmap.Width, inputBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(convertedBitmap))
            {
                canvas.DrawBitmap(inputBitmap, 0, 0);
            }
            inputBitmap = convertedBitmap;
            needsDisposal = true;
        }

        // Get pixel data
        var stride = inputBitmap.RowBytes;
        _bitmapData = new byte[stride * Height];

        // Copy pixel data
        var pixelPtr = inputBitmap.GetPixels();
        Marshal.Copy(pixelPtr, _bitmapData, 0, _bitmapData.Length);

        if (needsDisposal && convertedBitmap != null)
        {
            convertedBitmap.Dispose();
        }
    }

    public NikseBitmap2(NikseBitmap2 input)
    {
        Width = input.Width;
        Height = input.Height;
        _bitmapData = new byte[input._bitmapData.Length];
        Buffer.BlockCopy(input._bitmapData, 0, _bitmapData, 0, _bitmapData.Length);
    }

    public void ReplaceYellowWithWhite()
    {
        var data = _bitmapData.AsSpan();
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] > 200 && // Alpha
                data[i + 2] > 199 && // Red
                data[i + 1] > 190 && // Green
                data[i] < 40) // Blue
            {
                data[i] = 255;     // B
                data[i + 1] = 255; // G
                data[i + 2] = 255; // R
            }
        }
    }

    public void ReplaceColor(int alpha, int red, int green, int blue,
        int alphaTo, int redTo, int greenTo, int blueTo)
    {
        var data = _bitmapData.AsSpan();
        var bTo = (byte)blueTo;
        var gTo = (byte)greenTo;
        var rTo = (byte)redTo;
        var aTo = (byte)alphaTo;
        
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == alpha &&
                data[i + 2] == red &&
                data[i + 1] == green &&
                data[i] == blue)
            {
                data[i] = bTo;
                data[i + 1] = gTo;
                data[i + 2] = rTo;
                data[i + 3] = aTo;
            }
        }
    }

    public void InvertColors()
    {
        var data = _bitmapData.AsSpan();
        for (var i = 0; i < data.Length; i += 4)
        {
            data[i] = (byte)~data[i];       // B
            data[i + 1] = (byte)~data[i + 1]; // G
            data[i + 2] = (byte)~data[i + 2]; // R
            // Skip alpha at i + 3
        }
    }

    public void ReplaceNonWhiteWithTransparent()
    {
        var data = _bitmapData.AsSpan();
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 2] + data[i + 1] + data[i] < 300)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 0;
                data[i + 3] = 0;
            }
        }
    }

    public void ReplaceTransparentWith(SKColor c)
    {
        var data = _bitmapData.AsSpan();
        var b = c.Blue;
        var g = c.Green;
        var r = c.Red;
        var a = c.Alpha;
        
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] < 10)
            {
                data[i] = b;
                data[i + 1] = g;
                data[i + 2] = r;
                data[i + 3] = a;
            }
        }
    }

    public void MakeOneColor(SKColor c)
    {
        var data = _bitmapData.AsSpan();
        var b = c.Blue;
        var g = c.Green;
        var r = c.Red;
        var a = c.Alpha;
        
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i] > 20)
            {
                data[i] = b;
                data[i + 1] = g;
                data[i + 2] = r;
                data[i + 3] = a;
            }
            else
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 0;
                data[i + 3] = 0;
            }
        }
    }

    private static SKColor GetOutlineColor(SKColor borderColor)
    {
        if (borderColor.Red + borderColor.Green + borderColor.Blue < 30)
        {
            return new SKColor(75, 75, 75, 200);
        }

        return new SKColor(
            red: borderColor.Red,
            green: borderColor.Green,
            blue: borderColor.Blue,
            alpha: 150
        );


        //if (borderColor.Red + borderColor.Green + borderColor.Blue < 30)
        //{
        //    return SKColor.FromArgb(200, 75, 75, 75);
        //}

        //return SKColor.FromArgb(150, borderColor.Red, borderColor.Green, borderColor.Blue);
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

        for (var i = 0; i < _bitmapData.Length; i += 4)
        {
            var smallestDiff = 10000;
            var buffer = backgroundBuffer;
            if (backgroundBuffer[3] == 0 && _bitmapData[i + 3] < 10) // transparent
            {
            }
            else
            {
                var patternDiff = Math.Abs(patternBuffer[0] - _bitmapData[i]) + Math.Abs(patternBuffer[1] - _bitmapData[i + 1]) + Math.Abs(patternBuffer[2] - _bitmapData[i + 2]) + Math.Abs(patternBuffer[3] - _bitmapData[i + 3]);
                if (patternDiff < smallestDiff)
                {
                    smallestDiff = patternDiff;
                    buffer = patternBuffer;
                }

                var emphasis1Diff = Math.Abs(emphasis1Buffer[0] - _bitmapData[i]) + Math.Abs(emphasis1Buffer[1] - _bitmapData[i + 1]) + Math.Abs(emphasis1Buffer[2] - _bitmapData[i + 2]) + Math.Abs(emphasis1Buffer[3] - _bitmapData[i + 3]);
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

                    var emphasis2Diff = Math.Abs(emphasis2Buffer[0] - _bitmapData[i]) + Math.Abs(emphasis2Buffer[1] - _bitmapData[i + 1]) + Math.Abs(emphasis2Buffer[2] - _bitmapData[i + 2]) + Math.Abs(emphasis2Buffer[3] - _bitmapData[i + 3]);
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
        var r = (int)Math.Round((pattern.Red * 2.0 + emphasis1.Red) / 3.0);
        var g = (int)Math.Round((pattern.Green * 2.0 + emphasis1.Green) / 3.0);
        var b = (int)Math.Round((pattern.Blue * 2.0 + emphasis1.Blue) / 3.0);
        var antializeColor = new SKColor((byte)r, (byte)g, (byte)b);

        for (var y = 1; y < Height - 1; y++)
        {
            for (var x = 1; x < Width - 1; x++)
            {
                if (GetPixel(x, y) == pattern)
                {
                    if (GetPixel(x - 1, y) == emphasis1 && GetPixel(x, y - 1) == emphasis1)
                    {
                        SetPixel(x, y, antializeColor);
                    }
                    else if (GetPixel(x - 1, y) == emphasis1 && GetPixel(x, y + 1) == emphasis1)
                    {
                        SetPixel(x, y, antializeColor);
                    }
                    else if (GetPixel(x + 1, y) == emphasis1 && GetPixel(x, y + 1) == emphasis1)
                    {
                        SetPixel(x, y, antializeColor);
                    }
                    else if (GetPixel(x + 1, y) == emphasis1 && GetPixel(x, y - 1) == emphasis1)
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
        var indexBufferEqual = 0;
        var indexBufferUnEqual = 0;

        _pixelAddress = -4;
        for (var y = 0; y < Height; y++)
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

            for (var x = 0; x < Width; x++)
            {
                var color = GetDvdColor(patternBuffer, emphasis1Buffer, emphasis2Buffer);

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
            var factor = count / 255;
            for (var i = 0; i < factor; i++)
            {
                WriteFourNibbles(buffer, 0xff, lastColor, ref index, indexHalfNibble);
            }

            var rest = count % 255;
            if (rest > 0)
            {
                WriteFourNibbles(buffer, rest, lastColor, ref index, indexHalfNibble);
            }
        }
    }

    private static void WriteFourNibbles(byte[] buffer, int count, int color, ref int index, bool indexHalfNibble)
    {
        var n = (count << 2) + color;
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
                var alpha = GetAlpha(x, y);
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
                var alpha = GetAlpha(x, y);
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
                    var alpha = GetAlpha(x, y);
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

        var newWidth = rightEnd - leftStart + 1;
        if (newWidth <= 0)
        {
            return 0;
        }

        var newBitmapData = new byte[newWidth * newHeight * 4];
        var index = 0;
        var newWidthX4 = 4 * newWidth;
        for (y = 0; y < newHeight; y++)
        {
            var pixelAddress = leftStart * 4 + y * _widthX4;
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
        var leftStart = 0;
        var done = false;
        var x = 0;
        int y;
        while (!done && x < Width)
        {
            y = 0;
            while (!done && y < Height)
            {
                var c = GetPixel(x, y);
                if (c != transparentColor)
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

        var rightEnd = Width - 1;
        done = false;
        x = Width - 1;
        while (!done && x >= 0)
        {
            y = 0;
            while (!done && y < Height)
            {
                var c = GetPixel(x, y);
                if (c != transparentColor)
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
        var newHeight = Height;
        if (bottom)
        {
            y = Height - 1;
            while (!done && y > 0)
            {
                x = 0;
                while (!done && x < Width)
                {
                    var c = GetPixel(x, y);
                    if (c != transparentColor)
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

        var newWidth = rightEnd - leftStart + 1;
        if (newWidth <= 0)
        {
            return 0;
        }

        var newBitmapData = new byte[newWidth * newHeight * 4];
        var index = 0;
        var newWidthX4 = 4 * newWidth;
        for (y = 0; y < newHeight; y++)
        {
            var pixelAddress = leftStart * 4 + y * _widthX4;
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
        var done = false;
        var newTop = 0;
        var y = 0;
        while (!done && y < Height)
        {
            var x = 0;
            while (!done && x < Width)
            {
                var c = GetPixel(x, y);
                if (c != transparentColor && !(c.Alpha == 0 && transparentColor.Alpha == 0))
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

        var newHeight = Height - newTop;
        var newBitmapData = new byte[newHeight * _widthX4];
        var index = 0;
        for (y = newTop; y < Height; y++)
        {
            var pixelAddress = y * _widthX4;
            Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, _widthX4);
            index += _widthX4;
        }

        Height = newHeight;
        _bitmapData = newBitmapData;
    }

    public int CropTopTransparent(int minimumMargin)
    {
        var done = false;
        var newTop = 0;
        var y = 0;
        while (!done && y < Height)
        {
            var x = 0;
            while (!done && x < Width)
            {
                var alpha = GetAlpha(x, y);
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

        var newHeight = Height - newTop;
        var newBitmapData = new byte[newHeight * _widthX4];
        var index = 0;
        for (y = newTop; y < Height; y++)
        {
            var pixelAddress = y * _widthX4;
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
        var yOffset = y * _widthX4 + 3;
        var max = yOffset + _widthX4;
        for (var pos = yOffset; pos < max; pos += 4)
        {
            if (_bitmapData[pos] > 1)
            {
                return false;
            }
        }

        return true;
    }

    public void Fill(SKColor color)
    {
        var data = _bitmapData.AsSpan();
        var b = color.Blue;
        var g = color.Green;
        var r = color.Red;
        var a = color.Alpha;
        
        for (var i = 0; i < data.Length; i += 4)
        {
            data[i] = b;
            data[i + 1] = g;
            data[i + 2] = r;
            data[i + 3] = a;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAlpha(int x, int y)
    {
        return _bitmapData[x * 4 + y * _widthX4 + 3];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAlpha(int index)
    {
        return _bitmapData[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAlpha(int x, int y, byte alpha)
    {
        _bitmapData[x * 4 + y * _widthX4 + 3] = alpha;
    }

    /// <summary>
    /// Get read-only span of the raw bitmap data (BGRA format)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> GetPixelData() => _bitmapData;

    /// <summary>
    /// Get mutable span of the raw bitmap data (BGRA format) - use with caution
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetPixelDataMutable() => _bitmapData;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SKColor GetPixel(int x, int y)
    {
        _pixelAddress = x * 4 + y * _widthX4;
        return new SKColor(_bitmapData[_pixelAddress + 3], _bitmapData[_pixelAddress + 2], _bitmapData[_pixelAddress + 1], _bitmapData[_pixelAddress]);
    }

    public byte[] GetPixelColors(int x, int y)
    {
        _pixelAddress = x * 4 + y * _widthX4;
        return new[] { _bitmapData[_pixelAddress + 3], _bitmapData[_pixelAddress + 2], _bitmapData[_pixelAddress + 1], _bitmapData[_pixelAddress] };
    }

    public SKColor GetPixelNext()
    {
        _pixelAddress += 4;
        return new SKColor(_bitmapData[_pixelAddress + 3], _bitmapData[_pixelAddress + 2], _bitmapData[_pixelAddress + 1], _bitmapData[_pixelAddress]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(int x, int y, SKColor color)
    {
        _pixelAddress = x * 4 + y * _widthX4;
        _bitmapData[_pixelAddress] = color.Blue;
        _bitmapData[_pixelAddress + 1] = color.Green;
        _bitmapData[_pixelAddress + 2] = color.Red;
        _bitmapData[_pixelAddress + 3] = color.Alpha;
    }

    public SKBitmap GetBitmap()
    {
        var bitmap = new SKBitmap(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul);

        // Get the pointer to the bitmap's pixel data
        var pixelPtr = bitmap.GetPixels();

        // Copy our bitmap data to the new bitmap
        Marshal.Copy(_bitmapData, 0, pixelPtr, _bitmapData.Length);

        return bitmap;


        //var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        //var bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        //var destination = bitmapData.Scan0;
        //Marshal.Copy(_bitmapData, 0, destination, _bitmapData.Length);
        //bitmap.UnlockBits(bitmapData);
        //return bitmap;
    }

    private static int FindBestMatch(SKColor color, List<SKColor> palette, out int maxDiff)
    {
        var smallestDiff = 1000;
        var smallestDiffIndex = -1;
        var i = 0;
        foreach (var pc in palette)
        {
            var diff = Math.Abs(pc.Alpha - color.Alpha) + Math.Abs(pc.Red - color.Red) + Math.Abs(pc.Green - color.Green) + Math.Abs(pc.Blue - color.Blue);
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

    //public SKBitmap ConvertTo8BitsPerPixel()
    //{
    //    var newBitmap = new SKBitmap(Width, Height, PixelFormat.Format8bppIndexed);
    //    var palette = new List<Color> { Color.Transparent };
    //    var bPalette = newBitmap.Palette;
    //    var entries = bPalette.Entries;
    //    for (int i = 0; i < newBitmap.Palette.Entries.Length; i++)
    //    {
    //        entries[i] = Color.Transparent;
    //    }

    //    var data = newBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
    //    var bytes = new byte[data.Height * data.Stride];
    //    Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

    //    for (int y = 0; y < Height; y++)
    //    {
    //        for (int x = 0; x < Width; x++)
    //        {
    //            var c = GetPixel(x, y);
    //            if (c.Alpha < 5)
    //            {
    //                bytes[y * data.Stride + x] = 0;
    //            }
    //            else
    //            {
    //                int index = FindBestMatch(c, palette, out var maxDiff);

    //                if (index == -1 && palette.Count < 255)
    //                {
    //                    index = palette.Count;
    //                    entries[index] = c;
    //                    palette.Add(c);
    //                    bytes[y * data.Stride + x] = (byte)index;
    //                }
    //                else if (palette.Count < 200 && maxDiff > 5)
    //                {
    //                    index = palette.Count;
    //                    entries[index] = c;
    //                    palette.Add(c);
    //                    bytes[y * data.Stride + x] = (byte)index;
    //                }
    //                else if (palette.Count < 255 && maxDiff > 15)
    //                {
    //                    index = palette.Count;
    //                    entries[index] = c;
    //                    palette.Add(c);
    //                    bytes[y * data.Stride + x] = (byte)index;
    //                }
    //                else if (index >= 0)
    //                {
    //                    bytes[y * data.Stride + x] = (byte)index;
    //                }
    //            }
    //        }
    //    }

    //    Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
    //    newBitmap.UnlockBits(data);
    //    newBitmap.Palette = bPalette;
    //    return newBitmap;
    //}

    public NikseBitmap2 CopyRectangle(NikseRectangle section)
    {
        if (section.Bottom > Height)
        {
            section = new NikseRectangle(section.Left, section.Top, section.Width, Height - section.Top);
        }

        if (section.Width + section.Left > Width)
        {
            section = new NikseRectangle(section.Left, section.Top, Width - section.Left, section.Height);
        }

        var newBitmapData = new byte[section.Width * section.Height * 4];
        var index = 0;
        var sectionWidthX4 = 4 * section.Width;
        var sectionLeftX4 = 4 * section.Left;
        for (var y = section.Top; y < section.Bottom; y++)
        {
            var pixelAddress = sectionLeftX4 + y * _widthX4;
            Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, sectionWidthX4);
            index += sectionWidthX4;
        }

        return new NikseBitmap2(section.Width, section.Height, newBitmapData);
    }

    /// <summary>
    /// Returns brightest color (not white though)
    /// </summary>
    /// <returns>Brightest color, if not found or if brightes color is white, then Color.Transparent is returned</returns>
    public SKColor GetBrightestColorWhiteIsTransparent()
    {
        var max = Width * Height - 4;
        var brightest = SKColors.Black;
        for (var i = 0; i < max; i++)
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
        var max = Width * Height - 4;
        var brightest = SKColors.Black;
        for (var i = 0; i < max; i++)
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
        if (Math.Abs(color1.Red - color2.Red) < maxDiff && Math.Abs(color1.Green - color2.Green) < maxDiff && Math.Abs(color1.Blue - color2.Blue) < maxDiff)
        {
            return true;
        }

        return false;
    }

    public void GrayScale()
    {
        var data = _bitmapData.AsSpan();
        for (var i = 0; i < data.Length; i += 4)
        {
            var medium = (int)((data[i + 2] + data[i + 1] + data[i]) * 0.5 + 2);
            var gray = (byte)Math.Min(medium, 255);
            data[i] = gray;
            data[i + 1] = gray;
            data[i + 2] = gray;
        }
    }

    /// <summary>
    /// Make pixels with some transparency completely transparent
    /// </summary>
    /// <param name="minAlpha">Min alpha value, 0=transparent, 255=fully visible</param>
    public void MakeBackgroundTransparent(int minAlpha)
    {
        var data = _bitmapData.AsSpan();
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] < minAlpha)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 0;
                data[i + 3] = 0;
            }
        }
    }

    public void MakeTwoColor(int minRgb)
    {
        var data = _bitmapData.AsSpan();
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] < 1 || data[i] + data[i + 1] + data[i + 2] < minRgb)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 0;
                data[i + 3] = 0;
            }
            else
            {
                data[i] = 255;
                data[i + 1] = 255;
                data[i + 2] = 255;
                data[i + 3] = 255;
            }
        }
    }

    public void MakeTwoColor(int minRgb, Color background, Color foreground)
    {
        var bufferBackground = new byte[4];
        bufferBackground[0] = (byte)background.B; // B
        bufferBackground[1] = (byte)background.G; // G
        bufferBackground[2] = (byte)background.R; // R
        bufferBackground[3] = 255; // A
        var bufferForeground = new byte[4];
        bufferForeground[0] = (byte)foreground.B; // B
        bufferForeground[1] = (byte)foreground.G; // G
        bufferForeground[2] = (byte)foreground.R; // R
        bufferForeground[3] = 255; // A
        for (var i = 0; i < _bitmapData.Length; i += 4)
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

        var startIndex = xStart * 4 + y * _widthX4;
        var endIndex = xEnd * 4 + y * _widthX4 + 4;
        var length = endIndex - startIndex;
        Buffer.BlockCopy(EmptyByteArray, 0, _bitmapData, startIndex, length);
    }

    public void AddTransparentLineRight()
    {
        var newWidth = Width + 1;

        var newBitmapData = new byte[newWidth * Height * 4];
        var index = 0;
        for (var y = 0; y < Height; y++)
        {
            var pixelAddress = 0 * 4 + y * _widthX4;
            Buffer.BlockCopy(_bitmapData, pixelAddress, newBitmapData, index, _widthX4);
            index += 4 * newWidth;
        }

        Width = newWidth;
        _bitmapData = newBitmapData;
        for (var y = 0; y < Height; y++)
        {
            SetPixel(Width - 1, y, SKColors.Transparent);
        }
    }

    public void AddMargin(int margin)
    {
        var newWidth = Width + margin * 2;
        var newHeight = Height + margin * 2;
        var newBitmapData = new byte[newWidth * newHeight * 4];
        var newWidthX4 = newWidth * 4;
        var marginX4 = margin * 4;

        for (var y = 0; y < Height; y++)
        {
            var pixelAddress = y * _widthX4;
            var index = marginX4 + (y + margin) * newWidthX4;
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
        var offsetDest = 0;
        for (var y = Height - 1; y >= 0; y--) // takes lines from bottom lines to top (mirrored horizontally)
        {
            for (var x = 0; x < Width; x++)
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
        var max = _width * 4 + y * _widthX4 + 3;
        for (var pos = y * _widthX4 + 3; pos < max; pos += 4)
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
        var xOffset = x * 4 + 3;
        for (var y = 0; y < Height; y++)
        {
            if (_bitmapData[xOffset + y * _widthX4] > 0)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsImageOnlyTransparent()
    {
        for (var i = 0; i < _bitmapData.Length; i += 4)
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
        var transparentBottomPixels = 0;
        for (var y = 0; y < Height; y++)
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
        var transparentPixelsRight = 0;
        for (var x = 0; x < Width; x++)
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

        var newWidth = Width;
        var widthChanged = false;
        if (Width % 2 != 0)
        {
            newWidth++;
            widthChanged = true;
        }

        var newHeight = Height;
        var heightChanged = false;
        if (Height % 2 != 0)
        {
            newHeight++;
            heightChanged = true;
        }

        var newBitmapData = new byte[newWidth * newHeight * 4];
        var newWidthX4 = 4 * newWidth;
        var index = 0;
        for (var y = 0; y < Height; y++)
        {
            var pixelAddress = y * _widthX4;
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

    public bool IsEqualTo(NikseBitmap2 bitmap)
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

        for (var i = 0; i < _bitmapData.Length; i++)
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
        var data = _bitmapData.AsSpan();
        var b = transparent.Blue;
        var g = transparent.Green;
        var r = transparent.Red;
        var a = transparent.Alpha;
        
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == 0)
            {
                data[i] = b;
                data[i + 1] = g;
                data[i + 2] = r;
                data[i + 3] = a;
            }
        }
    }

    public void ChangeBrightness(decimal factor)
    {
        if (factor > 1)
        {
            for (var i = 0; i < _bitmapData.Length; i += 4)
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
            for (var i = 0; i < _bitmapData.Length; i += 4)
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
            for (var i = 0; i < _bitmapData.Length; i += 4)
            {
                int a = _bitmapData[i + 3];
                _bitmapData[i + 3] = (byte)Math.Min(byte.MaxValue, (int)(a * factor));
            }
        }
        else
        {
            for (var i = 0; i < _bitmapData.Length; i += 4)
            {
                int a = _bitmapData[i + 3];
                _bitmapData[i + 3] = (byte)Math.Max(0, (int)(a * factor));
            }
        }
    }
}