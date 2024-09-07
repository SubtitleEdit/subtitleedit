﻿using System;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ObjectDataSegment
    {
        public int ObjectId { get; set; }
        public int ObjectVersionNumber { get; set; }

        /// <summary>
        /// 0x00 coding of pixels, 0x01 coded as a string of characters
        /// </summary>
        public int ObjectCodingMethod { get; set; }

        public bool NonModifyingColorFlag { get; set; }

        public int TopFieldDataBlockLength { get; set; }
        public int BottomFieldDataBlockLength { get; set; }

        public int NumberOfCodes { get; set; }

        public List<string> FirstDataTypes = new List<string>();
        public Bitmap Image { get; set; }
        private FastBitmap _fastImage;

        public static int PixelDecoding2Bit => 0x10;
        public static int PixelDecoding4Bit => 0x11;
        public static int PixelDecoding8Bit => 0x12;
        public static int MapTable2To4Bit => 0x20;
        public static int MapTable2To8Bit => 0x21;
        public static int MapTable4To8Bit => 0x22;
        public static int EndOfObjectLineCode => 0xf0;

        public int BufferIndex { get; }

        public ObjectDataSegment(byte[] buffer, int index)
        {
            ObjectId = Helper.GetEndianWord(buffer, index);
            ObjectVersionNumber = buffer[index + 2] >> 4;
            ObjectCodingMethod = (buffer[index + 2] & 0b00001100) >> 2;
            NonModifyingColorFlag = (buffer[index + 2] & 0b00000010) > 0;
            TopFieldDataBlockLength = Helper.GetEndianWord(buffer, index + 3);
            BottomFieldDataBlockLength = Helper.GetEndianWord(buffer, index + 5);
            BufferIndex = index;
        }

        public void DecodeImage(byte[] buffer, int index, ClutDefinitionSegment cds)
        {
            if (ObjectCodingMethod == 0)
            {
                var twoToFourBitColorLookup = new List<int> { 0, 1, 2, 3 };
                var twoToEightBitColorLookup = new List<int> { 0, 1, 2, 3 };
                var fourToEightBitColorLookup = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

                int pixelCode = 0;
                int runLength = 0;
                int dataType = buffer[index + 7];
                int length = TopFieldDataBlockLength;

                if (length + index + 7 > buffer.Length) // check if buffer is large enough
                {
                    Image = new Bitmap(1, 1);
                    return;
                }

                index += 8;
                int start = index;
                int x = 0;
                int y = 0;

                // Pre-decoding to determine image size
                int width = 0;
                while (index < start + TopFieldDataBlockLength)
                {
                    index = CalculateSize(buffer, index, ref dataType, start, ref x, ref y, length, ref runLength, ref width);
                }
                if (width > 2000)
                {
                    width = 2000;
                }

                if (y > 500)
                {
                    y = 500;
                }

                Image = new Bitmap(Math.Max(1, width), y + 1);
                _fastImage = new FastBitmap(Image);
                _fastImage.LockImage();

                x = 0;
                y = 0;
                index = start;
                while (index < start + TopFieldDataBlockLength && index < buffer.Length)
                {
                    index = ProcessDataType(buffer, index, cds, ref dataType, start, twoToFourBitColorLookup, fourToEightBitColorLookup, twoToEightBitColorLookup, ref x, ref y, length, ref pixelCode, ref runLength);
                }

                x = 0;
                y = 1;
                if (BottomFieldDataBlockLength == 0)
                {
                    index = start;
                }
                else
                {
                    length = BottomFieldDataBlockLength;
                    index = start + TopFieldDataBlockLength;
                    start = index;
                }
                dataType = buffer[index - 1];
                while (index < start + BottomFieldDataBlockLength - 1 && index < buffer.Length)
                {
                    index = ProcessDataType(buffer, index, cds, ref dataType, start, twoToFourBitColorLookup, fourToEightBitColorLookup, twoToEightBitColorLookup, ref x, ref y, length, ref pixelCode, ref runLength);
                }
                _fastImage.UnlockImage();
            }
            else if (ObjectCodingMethod == 1)
            {
                Image = new Bitmap(1, 1);
                NumberOfCodes = buffer[index + 3];
            }
        }

        public Position FindPosition(byte[] buffer, int index, ClutDefinitionSegment cds)
        {
            var pos = new Position(int.MaxValue, int.MaxValue);

            if (ObjectCodingMethod == 0)
            {
                var twoToFourBitColorLookup = new List<int> { 0, 1, 2, 3 };
                var twoToEightBitColorLookup = new List<int> { 0, 1, 2, 3 };
                var fourToEightBitColorLookup = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

                var pixelCode = 0;
                var runLength = 0;
                var dataType = (int)buffer[index + 7];
                var length = TopFieldDataBlockLength;

                if (length + index + 7 > buffer.Length) // check if buffer is large enough
                {
                    return null;
                }

                index += 8;
                var start = index;
                var x = 0;
                var y = 0;

                // Pre-decoding to determine image size
                var width = 0;
                while (index < start + TopFieldDataBlockLength)
                {
                    index = CalculateSize(buffer, index, ref dataType, start, ref x, ref y, length, ref runLength, ref width);
                }
                if (width > 2000)
                {
                    width = 2000;
                }

                if (y > 500)
                {
                    y = 500;
                }

                var height = y + 1;
                x = 0;
                y = 0;
                index = start;
                while (index < start + TopFieldDataBlockLength && index < buffer.Length)
                {
                    index = ProcessDataTypeForPosition(buffer, index, cds, ref dataType, start, twoToFourBitColorLookup, fourToEightBitColorLookup, twoToEightBitColorLookup, ref x, ref y, length, ref pixelCode, ref runLength, width, height, out Position subPos);
                    if (subPos.Left < pos.Left)
                    {
                        pos.Left = subPos.Left;
                    }
                    if (subPos.Top < pos.Top)
                    {
                        pos.Top = subPos.Top;
                    }
                }

                x = 0;
                y = 1;
                if (BottomFieldDataBlockLength == 0)
                {
                    index = start;
                }
                else
                {
                    length = BottomFieldDataBlockLength;
                    index = start + TopFieldDataBlockLength;
                    start = index;
                }
                dataType = buffer[index - 1];
                while (index < start + BottomFieldDataBlockLength - 1 && index < buffer.Length)
                {
                    index = ProcessDataTypeForPosition(buffer, index, cds, ref dataType, start, twoToFourBitColorLookup, fourToEightBitColorLookup, twoToEightBitColorLookup, ref x, ref y, length, ref pixelCode, ref runLength, width, height, out Position subPos);
                    if (subPos.Left < pos.Left)
                    {
                        pos.Left = subPos.Left;
                    }
                    if (subPos.Top < pos.Top)
                    {
                        pos.Top = subPos.Top;
                    }
                }
            }
            else if (ObjectCodingMethod == 1)
            {
                return null;
            }

            return new Position(pos.Left == int.MaxValue ? 0 : pos.Left, pos.Top == int.MaxValue ? 0 : pos.Top);
        }

        private int ProcessDataType(byte[] buffer, int index, ClutDefinitionSegment cds, ref int dataType, int start,
                                    List<int> twoToFourBitColorLookup, List<int> fourToEightBitColorLookup, List<int> twoToEightBitColorLookup,
                                    ref int x, ref int y, int length, ref int pixelCode, ref int runLength)
        {
            if (dataType == PixelDecoding2Bit)
            {
                var bitIndex = 0;
                while (index < start + length - 1 && index < buffer.Length && TwoBitPixelDecoding(buffer, ref index, ref bitIndex, out pixelCode, out runLength))
                {
                    DrawPixels(cds, twoToFourBitColorLookup[pixelCode], runLength, ref x, ref y);
                }
            }
            else if (dataType == PixelDecoding4Bit)
            {
                var startHalf = false;
                while (index < start + length - 1 && index < buffer.Length && FourBitPixelDecoding(buffer, ref index, ref startHalf, out pixelCode, out runLength))
                {
                    DrawPixels(cds, fourToEightBitColorLookup[pixelCode], runLength, ref x, ref y);
                }
            }
            else if (dataType == PixelDecoding8Bit)
            {
                while (index < start + length - 1 && index < buffer.Length && EightBitPixelDecoding(buffer, ref index, out pixelCode, out runLength))
                {
                    DrawPixels(cds, pixelCode, runLength, ref x, ref y);
                }
            }
            else if (dataType == MapTable2To4Bit)
            {
                //4 entry numbers of 4-bits each; entry number 0 first, entry number 3 last
                twoToFourBitColorLookup[0] = buffer[index] >> 4;
                twoToFourBitColorLookup[1] = buffer[index] & 0b00001111;
                index++;
                twoToFourBitColorLookup[2] = buffer[index] >> 4;
                twoToFourBitColorLookup[3] = buffer[index] & 0b00001111;
                index++;
            }
            else if (dataType == MapTable2To8Bit)
            {
                //4 entry numbers of 8-bits each; entry number 0 first, entry number 3 last
                twoToEightBitColorLookup[0] = buffer[index];
                index++;
                twoToEightBitColorLookup[1] = buffer[index];
                index++;
                twoToEightBitColorLookup[2] = buffer[index];
                index++;
                twoToEightBitColorLookup[3] = buffer[index];
                index++;
            }
            else if (dataType == MapTable4To8Bit)
            {
                // 16 entry numbers of 8-bits each
                for (int k = 0; k < 16; k++)
                {
                    if (index < buffer.Length)
                    {
                        fourToEightBitColorLookup[k] = buffer[index];
                        index++;
                    }
                }
            }
            else if (dataType == EndOfObjectLineCode)
            {
                x = 0;
                y += 2; // interlaced - skip one line
            }

            if (index < start + length && index < buffer.Length)
            {
                dataType = buffer[index];
                index++;
            }

            return index;
        }

        private int ProcessDataTypeForPosition(byte[] buffer, int index, ClutDefinitionSegment cds, ref int dataType, int start,
                                    List<int> twoToFourBitColorLookup, List<int> fourToEightBitColorLookup, List<int> twoToEightBitColorLookup,
                                    ref int x, ref int y, int length, ref int pixelCode, ref int runLength, int width, int height, out Position pos)
        {
            pos = new Position(int.MaxValue, int.MaxValue);

            if (dataType == PixelDecoding2Bit)
            {
                var bitIndex = 0;
                while (index < start + length - 1 && index < buffer.Length && TwoBitPixelDecoding(buffer, ref index, ref bitIndex, out pixelCode, out runLength))
                {
                    var subPos = GetFirstPixelPosition(cds, twoToFourBitColorLookup[pixelCode], runLength, ref x, ref y, width, height);
                    if (subPos.Left < pos.Left)
                    {
                        pos.Left = subPos.Left;
                    }
                    if (subPos.Top < pos.Top)
                    {
                        pos.Top = subPos.Top;
                    }
                }
            }
            else if (dataType == PixelDecoding4Bit)
            {
                var startHalf = false;
                while (index < start + length - 1 && index < buffer.Length && FourBitPixelDecoding(buffer, ref index, ref startHalf, out pixelCode, out runLength))
                {
                    var subPos = GetFirstPixelPosition(cds, fourToEightBitColorLookup[pixelCode], runLength, ref x, ref y, width, height);
                    if (subPos.Left < pos.Left)
                    {
                        pos.Left = subPos.Left;
                    }
                    if (subPos.Top < pos.Top)
                    {
                        pos.Top = subPos.Top;
                    }
                }
            }
            else if (dataType == PixelDecoding8Bit)
            {
                while (index < start + length - 1 && index < buffer.Length && EightBitPixelDecoding(buffer, ref index, out pixelCode, out runLength))
                {
                    var subPos = GetFirstPixelPosition(cds, pixelCode, runLength, ref x, ref y, width, height);
                    if (subPos.Left < pos.Left)
                    {
                        pos.Left = subPos.Left;
                    }
                    if (subPos.Top < pos.Top)
                    {
                        pos.Top = subPos.Top;
                    }
                }
            }
            else if (dataType == MapTable2To4Bit)
            {
                //4 entry numbers of 4-bits each; entry number 0 first, entry number 3 last
                twoToFourBitColorLookup[0] = buffer[index] >> 4;
                twoToFourBitColorLookup[1] = buffer[index] & 0b00001111;
                index++;
                twoToFourBitColorLookup[2] = buffer[index] >> 4;
                twoToFourBitColorLookup[3] = buffer[index] & 0b00001111;
                index++;

                pos.Left = x;
                pos.Top = y;
            }
            else if (dataType == MapTable2To8Bit)
            {
                //4 entry numbers of 8-bits each; entry number 0 first, entry number 3 last
                twoToEightBitColorLookup[0] = buffer[index];
                index++;
                twoToEightBitColorLookup[1] = buffer[index];
                index++;
                twoToEightBitColorLookup[2] = buffer[index];
                index++;
                twoToEightBitColorLookup[3] = buffer[index];
                index++;

                pos.Left = x;
                pos.Top = y;
            }
            else if (dataType == MapTable4To8Bit)
            {
                // 16 entry numbers of 8-bits each
                for (var k = 0; k < 16; k++)
                {
                    if (index < buffer.Length)
                    {
                        fourToEightBitColorLookup[k] = buffer[index];
                        index++;
                    }
                }

                pos.Left = x;
                pos.Top = y;
            }
            else if (dataType == EndOfObjectLineCode)
            {
                x = 0;
                y += 2; // interlaced - skip one line
            }

            if (index < start + length && index < buffer.Length)
            {
                dataType = buffer[index];
                index++;
            }

            return index;
        }

        private static int CalculateSize(byte[] buffer, int index, ref int dataType, int start, ref int x, ref int y, int length, ref int runLength, ref int width)
        {
            if (dataType == PixelDecoding2Bit)
            {
                int bitIndex = 0;
                while (index < start + length - 1 && index < buffer.Length && TwoBitPixelDecoding(buffer, ref index, ref bitIndex, out _, out runLength))
                {
                    x += runLength;
                }
            }
            else if (dataType == PixelDecoding4Bit)
            {
                bool startHalf = false;
                while (index < start + length - 1 && index < buffer.Length && FourBitPixelDecoding(buffer, ref index, ref startHalf, out _, out runLength))
                {
                    x += runLength;
                }
            }
            else if (dataType == PixelDecoding8Bit)
            {
                while (index < start + length - 1 && index < buffer.Length && EightBitPixelDecoding(buffer, ref index, out _, out runLength))
                {
                    x += runLength;
                }
            }
            else if (dataType == MapTable2To4Bit)
            {
                index += 2;
            }
            else if (dataType == MapTable2To8Bit)
            {
                index += 4;
            }
            else if (dataType == MapTable4To8Bit)
            {
                index += 16;
            }
            else if (dataType == EndOfObjectLineCode)
            {
                x = 0;
                y += 2; // interlaced - skip one line
            }

            if (index < start + length && index < buffer.Length)
            {
                dataType = buffer[index];
                index++;
            }
            if (x > width)
            {
                width = x;
            }

            return index;
        }

        private void DrawPixels(ClutDefinitionSegment cds, int pixelCode, int runLength, ref int x, ref int y)
        {
            var c = Color.Red;
            if (cds != null)
            {
                foreach (var item in cds.Entries)
                {
                    if (item.ClutEntryId == pixelCode)
                    {
                        c = item.GetColor();
                        break;
                    }
                }
            }

            for (var k = 0; k < runLength; k++)
            {
                if (y < _fastImage.Height && x < _fastImage.Width)
                {
                    _fastImage.SetPixel(x, y, c);
                }

                x++;
            }
        }

        private Position GetFirstPixelPosition(ClutDefinitionSegment cds, int pixelCode, int runLength, ref int x, ref int y, int width, int height)
        {
            var c = Color.Red;
            if (cds != null)
            {
                foreach (var item in cds.Entries)
                {
                    if (item.ClutEntryId == pixelCode)
                    {
                        c = item.GetColor();
                        break;
                    }
                }
            }

            for (var k = 0; k < runLength; k++)
            {
                if (y < height && x < width)
                {
                    if (c.A > 0)
                    {
                        return new Position(x, y);
                    }
                }

                x++;
            }

            return new Position(int.MaxValue, int.MaxValue);
        }

        private static int Next8Bits(byte[] buffer, ref int index)
        {
            int result = buffer[index];
            index++;
            return result;
        }

        private static bool EightBitPixelDecoding(byte[] buffer, ref int index, out int pixelCode, out int runLength)
        {
            pixelCode = 0;
            runLength = 1;
            int firstByte = Next8Bits(buffer, ref index);
            if (firstByte != 0)
            {
                runLength = 1;
                pixelCode = firstByte;
            }
            else
            {
                int nextByte = Next8Bits(buffer, ref index);
                if (nextByte >> 7 == 0)
                {
                    if (nextByte != 0)
                    {
                        runLength = nextByte & 0b01111111; // 1-127
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    runLength = nextByte & 0b01111111; // 3-127
                    pixelCode = Next8Bits(buffer, ref index);
                }
            }
            return true;
        }

        private static int Next4Bits(byte[] buffer, ref int index, ref bool startHalf)
        {
            if (index >= buffer.Length)
            {
                return 0;
            }

            int result;
            if (startHalf)
            {
                startHalf = false;
                result = buffer[index] & 0b00001111;
                index++;
            }
            else
            {
                startHalf = true;
                result = buffer[index] >> 4;
            }

            return result;
        }

        private static bool FourBitPixelDecoding(byte[] buffer, ref int index, ref bool startHalf, out int pixelCode, out int runLength)
        {
            pixelCode = 0;
            runLength = 1;
            int first = Next4Bits(buffer, ref index, ref startHalf);
            if (first != 0)
            {
                pixelCode = first;
                runLength = 1;
            }
            else
            {
                var next1 = Next4Bits(buffer, ref index, ref startHalf);
                if ((next1 & 0b00001000) == 0)
                {
                    if (next1 != 0)
                    {
                        runLength = (next1 & 0b00000111) + 2; // 3-9
                    }
                    else
                    {
                        if (startHalf)
                        {
                            startHalf = false;
                            index++;
                        }
                        return false;
                    }
                }
                else if (next1 == 0b00001100)
                {
                    runLength = 1;
                    pixelCode = 0;
                }
                else if (next1 == 0b00001101)
                {
                    runLength = 2;
                    pixelCode = 0;
                }
                else
                {
                    var next2 = Next4Bits(buffer, ref index, ref startHalf);
                    if ((next1 & 0b00000100) == 0)
                    {
                        runLength = (next1 & 0b00000011) + 4; // 4-7
                        pixelCode = next2;
                    }
                    else
                    {
                        var next3 = Next4Bits(buffer, ref index, ref startHalf);
                        if ((next1 & 0b00000001) == 0)
                        {
                            runLength = next2 + 9; // 9-24
                            pixelCode = next3;
                        }
                        else if (next1 == 0b00001111)
                        {
                            runLength = ((next2 << 4) + next3) + 25; // 25-280
                            pixelCode = Next4Bits(buffer, ref index, ref startHalf);
                        }
                    }
                }
            }

            return true;
        }

        private static int Next2Bits(byte[] buffer, ref int index, ref int bitIndex)
        {
            int result;
            if (bitIndex == 0)
            {
                bitIndex++;
                result = (buffer[index] & 0b11000000) >> 6;
            }
            else if (bitIndex == 1)
            {
                bitIndex++;
                result = (buffer[index] & 0b00110000) >> 4;
            }
            else if (bitIndex == 2)
            {
                bitIndex++;
                result = (buffer[index] & 0b00001100) >> 2;
            }
            else // 3 - last bit pair
            {
                bitIndex = 0;
                result = buffer[index] & 0b00000011;
                index++;
            }
            return result;
        }

        private static bool TwoBitPixelDecoding(byte[] buffer, ref int index, ref int bitIndex, out int pixelCode, out int runLength)
        {
            runLength = 1;
            pixelCode = 0;
            int first = Next2Bits(buffer, ref index, ref bitIndex);
            if (first != 0)
            {
                runLength = 1;
                pixelCode = first;
            }
            else
            {
                int next = Next2Bits(buffer, ref index, ref bitIndex);
                if (next == 1)
                {
                    runLength = 1;
                    pixelCode = 0;
                }
                else if (next > 1)
                {
                    runLength = ((next & 0b00000001) << 2) + Next2Bits(buffer, ref index, ref bitIndex) + 3; // 3-10
                    pixelCode = Next2Bits(buffer, ref index, ref bitIndex);
                }
                else
                {
                    int next2 = Next2Bits(buffer, ref index, ref bitIndex);
                    if (next2 == 0b00000001)
                    {
                        runLength = 2;
                        pixelCode = 0;
                    }
                    else if (next2 == 0b00000010)
                    {
                        runLength = (Next2Bits(buffer, ref index, ref bitIndex) << 2) +  // 12-27
                                     Next2Bits(buffer, ref index, ref bitIndex) + 12;
                        pixelCode = Next2Bits(buffer, ref index, ref bitIndex);
                    }
                    else if (next2 == 0b00000011)
                    {
                        runLength = (Next2Bits(buffer, ref index, ref bitIndex) << 6) + // 29 - 284
                                    (Next2Bits(buffer, ref index, ref bitIndex) << 4) +
                                    (Next2Bits(buffer, ref index, ref bitIndex) << 2) +
                                        Next2Bits(buffer, ref index, ref bitIndex) + 29;

                        pixelCode = Next2Bits(buffer, ref index, ref bitIndex);
                    }
                    else
                    {
                        if (bitIndex != 0)
                        {
                            index++;
                        }

                        return false; // end of 2-bit/pixel code string
                    }
                }
            }
            return true;
        }

    }
}
