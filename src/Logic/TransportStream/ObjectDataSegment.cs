using System;
using System.Collections.Generic;
using System.Drawing;
namespace Nikse.SubtitleEdit.Logic.TransportStream
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

        public const int PixelDecoding2Bit = 0x10;
        public const int PixelDecoding4Bit = 0x11;
        public const int PixelDecoding8Bit = 0x12;
        public const int MapTable2To4Bit = 0x20;
        public const int MapTable2To8Bit = 0x21;
        public const int MapTable4To8Bit = 0x22;
        public const int EndOfObjectLineCode = 0xf0;

        Random r = new Random();


        public void DecodeImage(byte[] buffer, int index, ClutDefinitionSegment cds)
        {
            Image = new Bitmap(720, 40);
            var r = new Random();
            Color c = Color.Red;
            List<int> twoBitColorLookup = new List<int> { 0, 1, 2, 3 };

            if (ObjectCodingMethod == 0)
            {
                int pixelCode = 0;
                int runLength = 0;
                TopFieldDataBlockLength = Helper.GetEndianWord(buffer, index + 3);
                BottomFieldDataBlockLength = Helper.GetEndianWord(buffer, index + 5);
                int dataType = buffer[index + 7];

                index += 8;
                int start = index;
                int x = 0;
                int y = 0;
                while (index < start + TopFieldDataBlockLength)
                {
                    if (dataType == PixelDecoding2Bit)
                    {
                        int bitIndex = 0;
                        while (index < start + TopFieldDataBlockLength && TwoBitPixelDecoding(buffer, ref index, ref pixelCode, ref runLength, ref bitIndex))
                        {
                            DrawPixels(cds, twoBitColorLookup[pixelCode], runLength, ref x, ref y);
                        }
                    }
                    else if (dataType == PixelDecoding4Bit)
                    {
                        bool startHalf = false;
                        while (FourBitPixelDecoding(buffer, ref index, ref pixelCode, ref runLength, ref startHalf))
                        {
                            DrawPixels(cds, pixelCode, runLength, ref x, ref y);
                        }
                    }
                    else if (dataType == PixelDecoding8Bit)
                    {
                        System.Windows.Forms.MessageBox.Show("8-bit pixel decoding!");
                        while (EightBitPixelDecoding(buffer, ref index, ref pixelCode, ref runLength))
                        {
                            DrawPixels(cds, pixelCode, runLength, ref x, ref y);
                        }
                    }
                    else if (dataType == MapTable2To4Bit)
                    {
                        //4 entry numbers of 4-bits each; entry number 0 first, entry number 3 last.
                        twoBitColorLookup[0] = buffer[index] >> 4;
                        twoBitColorLookup[1] = buffer[index] & Helper.B00001111;
                        index++;
                        twoBitColorLookup[2] = buffer[index] >> 4;
                        twoBitColorLookup[3] = buffer[index] & Helper.B00001111;
                        index++;
                    }
                    else if (dataType == EndOfObjectLineCode)
                    {
                        x = 0;
                        y += 2; // interlaced - skip one line
                    }
                    if (index < start + TopFieldDataBlockLength)
                    {
                        dataType = buffer[index];
                        index++;
                    }
                }

                x = 0;
                y = 1;
                index = start + TopFieldDataBlockLength;
                start = index;
                dataType = buffer[index - 1];
                while (index < start + BottomFieldDataBlockLength - 1)
                {
                    if (dataType == PixelDecoding2Bit)
                    {
                        int bitIndex = 0;
                        while (index < start + BottomFieldDataBlockLength - 1 && TwoBitPixelDecoding(buffer, ref index, ref pixelCode, ref runLength, ref bitIndex))
                        {
                            DrawPixels(cds, twoBitColorLookup[pixelCode], runLength, ref x, ref y);
                        }
                    }
                    else if (dataType == PixelDecoding4Bit)
                    {
                        bool startHalf = false;
                        while (FourBitPixelDecoding(buffer, ref index, ref pixelCode, ref runLength, ref startHalf))
                        {
                            DrawPixels(cds, pixelCode, runLength, ref x, ref y);
                        }
                    }
                    else if (dataType == PixelDecoding8Bit)
                    {
                        System.Windows.Forms.MessageBox.Show("8-bit pixel decoding!");
                        while (EightBitPixelDecoding(buffer, ref index, ref pixelCode, ref runLength))
                        {
                            DrawPixels(cds, pixelCode, runLength, ref x, ref y);
                        }
                    }
                    else if (dataType == MapTable2To4Bit)
                    {
                        //4 entry numbers of 4-bits each; entry number 0 first, entry number 3 last.
                        twoBitColorLookup[0] = buffer[index] >> 4;
                        twoBitColorLookup[1] = buffer[index] & Helper.B00001111;
                        index++;
                        twoBitColorLookup[2] = buffer[index] >> 4;
                        twoBitColorLookup[3] = buffer[index] & Helper.B00001111;
                        index++;
                    }
                    else if (dataType == EndOfObjectLineCode)
                    {
                        x = 0;
                        y += 2; // interlaced - skip one line
                    }
                    if (index < start + BottomFieldDataBlockLength)
                    {
                        dataType = buffer[index];
                        index++;
                    }
                }
            }
            else if (ObjectCodingMethod == 1)
            {
                NumberOfCodes = buffer[index + 3];
            }
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


            for (int k = 0; k < runLength; k++)
            {
                if (y < Image.Height && x < Image.Width)
                    Image.SetPixel(x, y, c);
                x++;
            }
        }

        public ObjectDataSegment(byte[] buffer, int index, ClutDefinitionSegment cds)
        {
            ObjectId = Helper.GetEndianWord(buffer, index);
            ObjectVersionNumber = buffer[index + 2] >> 4;
            ObjectCodingMethod = (buffer[index + 2] & Helper.B00001100) >> 2;
            NonModifyingColorFlag = (buffer[index + 2] & Helper.B00000010) > 0;

            DecodeImage(buffer, index, cds);
        }

        private int Next8Bits(byte[] buffer, ref int index)
        {
            int result = buffer[index];
            index++;
            return result;
        }

        private bool EightBitPixelDecoding(byte[] buffer, ref int index, ref int pixelCode, ref int runLength)
        {
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
                        runLength = nextByte & Helper.B01111111; // 1-127
                    else
                        return false;
                }
                else
                {
                    runLength = nextByte & Helper.B01111111; // 3-127
                    pixelCode = Next8Bits(buffer, ref index);
                }
            }
            return true;
        }

        private int Next4Bits(byte[] buffer, ref int index, ref bool startHalf)
        {
            int result;
            if (startHalf)
            {
                startHalf = false;
                result = buffer[index] & Helper.B00001111;
                index++;
            }
            else
            {
                startHalf = true;
                result = buffer[index] >> 4;
            }
            return result;
        }

        private bool FourBitPixelDecoding(byte[] buffer, ref int index, ref int pixelCode, ref int runLength, ref bool startHalf)
        {
            pixelCode = 0;
            int first = Next4Bits(buffer, ref index, ref startHalf);
            if (first != 0)
            {
                pixelCode = first; // Next4Bits(buffer, ref index, ref startHalf);
                runLength = 1;
            }
            else
            {
                int next1 = Next4Bits(buffer, ref index, ref startHalf);
                if ((next1 & Helper.B00001000) == 0)
                {
                    if (next1 != 0)
                    {
                        runLength = (next1 & Helper.B00000111) + 2; // 3-9
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
                else
                {
                    int next2 = Next4Bits(buffer, ref index, ref startHalf);
                    if ((next1 & Helper.B00000100) == 0)
                    {
                        runLength = (next1 & Helper.B00000011) + 4; // 4-7
                        pixelCode = next2;
                    }
                    else
                    {
                        int next3 = Next4Bits(buffer, ref index, ref startHalf);
                        if ((next1 & Helper.B00000001) == 0)
                        {
                            runLength = next2 + 9; // 9-24
                            pixelCode = next3;
                        }
                        else if (next1 == Helper.B00001111)
                        {
                            runLength = ((next2 << 4) + next3) + 25; // 25-280 
                            pixelCode = Next4Bits(buffer, ref index, ref startHalf);
                        }
                    }
                }
            }
            return true;
        }

        private int Next2Bits(byte[] buffer, ref int index, ref int bitIndex)
        {
            int result;
            if (bitIndex == 0)
            {
                bitIndex++;
                result = (buffer[index] & Helper.B11000000) >> 6;
            }
            else if (bitIndex == 1)
            {
                bitIndex++;
                result = (buffer[index] & Helper.B00110000) >> 4;
            }
            else if (bitIndex == 2)
            {
                bitIndex++;
                result = (buffer[index] & Helper.B00001100) >> 2;
            }
            else // 3 - last bit pair
            {
                bitIndex = 0;
                result = buffer[index] & Helper.B00000011;
                index++;
            }
            return result;
        }

        private bool TwoBitPixelDecoding(byte[] buffer, ref int index, ref int pixelCode, ref int runLength, ref int bitIndex)
        {
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
                    runLength = ((next & Helper.B00000001) << 2) + Next2Bits(buffer, ref index, ref bitIndex) + 3; // 3-10
                    pixelCode = Next2Bits(buffer, ref index, ref bitIndex);
                }
                else
                {
                    int next2 = Next2Bits(buffer, ref index, ref bitIndex);
                    if (next2 == Helper.B00000001)
                    {
                        runLength = 2;
                        pixelCode = 0;
                    }
                    else if (next2 == Helper.B00000010)
                    {
                        runLength = (Next2Bits(buffer, ref index, ref bitIndex) << 2) +  // 12-27 
                                     Next2Bits(buffer, ref index, ref bitIndex) + 12;
                        pixelCode = Next2Bits(buffer, ref index, ref bitIndex);
                    }
                    else if (next2 == Helper.B00000011)
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
                            index++;
                        return false; // end of 2-bit/pixel code string
                    }
                }
            }
            return true;
        }

    }
}
