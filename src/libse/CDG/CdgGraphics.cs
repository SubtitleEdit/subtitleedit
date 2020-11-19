using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.CDG
{
    /// <summary>
    /// Based on KaraokePlayer CdgLib: https://github.com/spektor56/KaraokePlayer by spektor56.
    /// For more info about the CDG karaoke format see: https://jbum.com/cdg_revealed.html
    /// Normally comes in a file pair with an mp3/ogg audio file.
    /// </summary>
    public class CdgGraphics
    {
        public static int FullWidth => 300;
        public static int FullHeight => 216;

        public static double TimeMsFactor => 10.0 / 3.0; // each CD+G frame is 3.33 ms

        private const int ColorTableSize = 16;
        private const int TileHeight = 12;
        private const int TileWidth = 6;

        private readonly Packet[] _packets;
        private readonly int[] _colorTable = new int[ColorTableSize];
        private readonly byte[,] _pixelColors = new byte[FullHeight, FullWidth];

        private int _borderColorIndex;
        private int _horizontalOffset;
        private int _startPosition;
        private int _verticalOffset;

        public int NumberOfPackets => _packets.Length;

        public CdgGraphics(List<Packet> packets)
        {
            _packets = packets.ToArray();
        }

        public long DurationInMilliseconds => _packets.Length * 10 / 3;

        private void Reset()
        {
            Array.Clear(_pixelColors, 0, _pixelColors.Length);
            Array.Clear(_colorTable, 0, _colorTable.Length);
            _startPosition = 0;
        }

        public Bitmap ToBitmap(int packetNumber) // long timeInMilliseconds)
        {
            //duration of one packet is 1/300 seconds (1000/300ms) (4 packets per sector, 75 sectors per second)
            //p=t*3/10  t=p*10/3 t=milliseconds, p=packets
            // var endPosition = (int)(timeInMilliseconds * 3 / 10);

            var endPosition = packetNumber;
            if (endPosition < _startPosition)
            {
                Reset();
            }
            var packetsToRead = endPosition - _startPosition;

            var processedPackets = 0;
            for (var i = 0; i < packetsToRead; i++)
            {
                if (_startPosition >= _packets.Length)
                {
                    break;
                }

                if (Process(_packets[_startPosition++]))
                {
                    processedPackets++;
                }
            }

            if (processedPackets == 0)
            {
                return null;
            }

            var graphicData = GetGraphicData();
            var image = new Bitmap(FullWidth, FullHeight, PixelFormat.Format32bppArgb);
            var bitmapData = image.LockBits(new Rectangle(0, 0, FullWidth, FullHeight), ImageLockMode.WriteOnly, image.PixelFormat);
            var offset = 0;
            foreach (var colorValue in graphicData)
            {
                var color = BitConverter.GetBytes(colorValue);
                foreach (var bytes in color)
                {
                    Marshal.WriteByte(bitmapData.Scan0, offset, bytes);
                    offset++;
                }
            }
            image.UnlockBits(bitmapData);
            image.MakeTransparent(image.GetPixel(1, 1));
            return image;
        }

        private bool Process(Packet packet)
        {
            if (packet.Command != Command.Graphic)
            {
                return false;
            }

            bool hasChanges;

            switch (packet.Instruction)
            {
                case Instruction.MemoryPreset:
                    hasChanges = MemoryPreset(packet);
                    break;
                case Instruction.BorderPreset:
                    hasChanges = BorderPreset(packet);
                    break;
                case Instruction.TileBlockNormal:
                    hasChanges = TileBlock(packet, false);
                    break;
                case Instruction.ScrollPreset:
                    hasChanges = Scroll(packet, false);
                    break;
                case Instruction.ScrollCopy:
                    hasChanges = Scroll(packet, true);
                    break;
                case Instruction.LoadColorTableLower:
                    hasChanges = LoadColorTable(packet, 0);
                    break;
                case Instruction.LoadColorTableUpper:
                    hasChanges = LoadColorTable(packet, 1);
                    break;
                case Instruction.TileBlockXor:
                    hasChanges = TileBlock(packet, true);
                    break;
                default:
                    return false;
            }

            return hasChanges;
        }

        private bool MemoryPreset(Packet packet)
        {
            bool hasChanges = false;
            var color = packet.Data[0] & 0xf;
            if (_borderColorIndex != color)
            {
                hasChanges = true;
            }
            _borderColorIndex = color;
            var repeat = packet.Data[1] & 0xf;

            //we have a reliable packet stream, so the repeat command 
            //is executed only the first time
            if (repeat == 0)
            {
                //Note that this may be done before any load color table
                //commands by some CDGs. So the load color table itself
                //actual recalculates the RGB values for all pixels when
                //the color table changes.

                //Set the preset color for every pixel. Must be stored in 
                //the pixel color table indices array
                for (var rowIndex = 0; rowIndex < _pixelColors.GetLength(0); rowIndex++)
                {
                    for (var columnIndex = 0; columnIndex < _pixelColors.GetLength(1); columnIndex++)
                    {
                        if (_pixelColors[rowIndex, columnIndex] != (byte)color)
                        {
                            hasChanges = true;
                        }
                        _pixelColors[rowIndex, columnIndex] = (byte)color;
                    }
                }
            }

            return hasChanges;
        }

        private bool BorderPreset(Packet packet)
        {
            bool hasChanges = false;
            int rowIndex;
            int columnIndex;

            var color = packet.Data[0] & 0xf;
            if (_borderColorIndex != color)
            {
                hasChanges = true;
            }
            _borderColorIndex = color;

            //The border area is the area contained with a rectangle 
            //defined by (0,0,300,216) minus the interior pixels which are contained
            //within a rectangle defined by (6,12,294,204).

            for (rowIndex = 0; rowIndex < _pixelColors.GetLength(0); rowIndex++)
            {
                for (columnIndex = 0; columnIndex < 6; columnIndex++)
                {
                    if (_pixelColors[rowIndex, columnIndex] != (byte)color)
                    {
                        hasChanges = true;
                    }
                    _pixelColors[rowIndex, columnIndex] = (byte)color;
                }

                for (columnIndex = _pixelColors.GetLength(1) - 6;
                    columnIndex < _pixelColors.GetLength(1);
                    columnIndex++)
                {
                    if (_pixelColors[rowIndex, columnIndex] != (byte)color)
                    {
                        hasChanges = true;
                    }
                    _pixelColors[rowIndex, columnIndex] = (byte)color;
                }
            }

            for (columnIndex = 6; columnIndex < _pixelColors.GetLength(1) - 6; columnIndex++)
            {
                for (rowIndex = 0; rowIndex < 12; rowIndex++)
                {
                    if (_pixelColors[rowIndex, columnIndex] != (byte)color)
                    {
                        hasChanges = true;
                    }
                    _pixelColors[rowIndex, columnIndex] = (byte)color;
                }

                for (rowIndex = _pixelColors.GetLength(0) - 12; rowIndex < _pixelColors.GetLength(0); rowIndex++)
                {
                    if (_pixelColors[rowIndex, columnIndex] != (byte)color)
                    {
                        hasChanges = true;
                    }
                    _pixelColors[rowIndex, columnIndex] = (byte)color;
                }
            }

            return hasChanges;
        }

        private bool LoadColorTable(Packet packet, int table)
        {
            bool hasChanges = false;

            for (var i = 0; i < 8; i++)
            {
                //[---high byte---]   [---low byte----]
                //7 6 5 4 3 2 1 0     7 6 5 4 3 2 1 0
                //X X r r r r g g     X X g g b b b b
                var highByte = packet.Data[2 * i];
                var lowByte = packet.Data[2 * i + 1];

                var red = (highByte & 0x3f) >> 2;
                var green = ((highByte & 0x3) << 2) | ((lowByte & 0x3f) >> 4);
                var blue = lowByte & 0xf;

                //4 bit color to 8 bit color
                red *= 17;
                green *= 17;
                blue *= 17;

                var color = Color.FromArgb(red, green, blue).ToArgb();

                if (_colorTable[i + table * 8] != color)
                {
                    hasChanges = true;
                }
                _colorTable[i + table * 8] = color;
            }

            return hasChanges;
        }

        private bool TileBlock(Packet packet, bool bXor)
        {
            var color0 = packet.Data[0] & 0xf;
            var color1 = packet.Data[1] & 0xf;
            var rowIndex = (packet.Data[2] & 0x1f) * 12;
            var columnIndex = (packet.Data[3] & 0x3f) * 6;

            if (rowIndex > FullHeight - TileHeight)
            {
                return false;
            }

            if (columnIndex > FullWidth - TileWidth)
            {
                return false;
            }

            var hasChanges = false;

            //Set the pixel array for each of the pixels in the 12x6 tile.
            //Normal = Set the color to either color0 or color1 depending
            //on whether the pixel value is 0 or 1.
            //XOR = XOR the color with the color index currently there.
            for (var i = 0; i <= 11; i++)
            {
                var myByte = packet.Data[4 + i] & 0x3f;
                for (var j = 0; j <= 5; j++)
                {
                    var pixel = (myByte >> (5 - j)) & 0x1;
                    int newCol;
                    if (bXor)
                    {
                        //Tile Block XOR 
                        var xorCol = pixel == 0 ? color0 : color1;

                        //Get the color index currently at this location, and xor with it 
                        int currentColorIndex = _pixelColors[rowIndex + i, columnIndex + j];
                        newCol = currentColorIndex ^ xorCol;
                    }
                    else
                    {
                        newCol = pixel == 0 ? color0 : color1;
                    }

                    //Set the pixel with the new color. We set both the surfarray
                    //containing actual RGB values, as well as our array containing
                    //the color indexes into our color table. 
                    if (_pixelColors[rowIndex + i, columnIndex + j] != (byte)newCol)
                    {
                        hasChanges = true;
                    }
                    _pixelColors[rowIndex + i, columnIndex + j] = (byte)newCol;
                }
            }

            return hasChanges;
        }

        private bool Scroll(Packet packet, bool copy)
        {
            bool hasChanges = false;

            //Decode the scroll command parameters
            var color = packet.Data[0] & 0xf;
            var horizontalScroll = packet.Data[1] & 0x3f;
            var verticalScroll = packet.Data[2] & 0x3f;

            var horizontalScrollCommand = (horizontalScroll & 0x30) >> 4;
            var horizontalOffset = horizontalScroll & 0x7;
            var verticalScrollCommand = (verticalScroll & 0x30) >> 4;
            var verticalOffset = verticalScroll & 0xf;

            var horizontalOffsetOld = _horizontalOffset;
            var verticalOffsetOld = _verticalOffset;

            _horizontalOffset = horizontalOffset < 5 ? horizontalOffset : 5;
            _verticalOffset = verticalOffset < 11 ? verticalOffset : 11;

            if (horizontalOffsetOld != _horizontalOffset || verticalOffsetOld != _verticalOffset)
            {
                hasChanges = true;
            }

            //Scroll Vertical - Calculate number of pixels

            var verticalScrollPixels = 0;
            if (verticalScrollCommand == 2)
            {
                verticalScrollPixels = -12;
            }
            else if (verticalScrollCommand == 1)
            {
                verticalScrollPixels = 12;
            }

            //Scroll Horizontal- Calculate number of pixels

            var horizontalScrollPixels = 0;
            if (horizontalScrollCommand == 2)
            {
                horizontalScrollPixels = -6;
            }
            else if (horizontalScrollCommand == 1)
            {
                horizontalScrollPixels = 6;
            }

            if (horizontalScrollPixels == 0 && verticalScrollPixels == 0)
            {
                return true;
            }

            //Perform the actual scroll.

            var temp = new byte[FullHeight + 1, FullWidth + 1];
            var vInc = verticalScrollPixels + FullHeight;
            var hInc = horizontalScrollPixels + FullWidth;
            int rowIndex;
            int columnIndex;

            for (rowIndex = 0; rowIndex <= FullHeight - 1; rowIndex++)
            {
                for (columnIndex = 0; columnIndex <= FullWidth - 1; columnIndex++)
                {
                    temp[(rowIndex + vInc) % FullHeight, (columnIndex + hInc) % FullWidth] =
                        _pixelColors[rowIndex, columnIndex];
                }
            }

            //if copy is false, we were supposed to fill in the new pixels
            //with a new color. Go back and do that now.

            if (copy == false)
            {
                if (verticalScrollPixels > 0)
                {
                    for (columnIndex = 0; columnIndex <= FullWidth - 1; columnIndex++)
                    {
                        for (rowIndex = 0; rowIndex <= verticalScrollPixels - 1; rowIndex++)
                        {
                            temp[rowIndex, columnIndex] = (byte)color;
                        }
                    }
                }
                else if (verticalScrollPixels < 0)
                {
                    for (columnIndex = 0; columnIndex <= FullWidth - 1; columnIndex++)
                    {
                        for (rowIndex = FullHeight + verticalScrollPixels; rowIndex <= FullHeight - 1; rowIndex++)
                        {
                            temp[rowIndex, columnIndex] = (byte)color;
                        }
                    }
                }

                if (horizontalScrollPixels > 0)
                {
                    for (columnIndex = 0; columnIndex <= horizontalScrollPixels - 1; columnIndex++)
                    {
                        for (rowIndex = 0; rowIndex <= FullHeight - 1; rowIndex++)
                        {
                            temp[rowIndex, columnIndex] = (byte)color;
                        }
                    }
                }
                else if (horizontalScrollPixels < 0)
                {
                    for (columnIndex = FullWidth + horizontalScrollPixels; columnIndex <= FullWidth - 1; columnIndex++)
                    {
                        for (rowIndex = 0; rowIndex <= FullHeight - 1; rowIndex++)
                        {
                            temp[rowIndex, columnIndex] = (byte)color;
                        }
                    }
                }
            }

            //Now copy the temporary buffer back to our array

            for (rowIndex = 0; rowIndex <= FullHeight - 1; rowIndex++)
            {
                for (columnIndex = 0; columnIndex <= FullWidth - 1; columnIndex++)
                {
                    if (_pixelColors[rowIndex, columnIndex] != temp[rowIndex, columnIndex])
                    {
                        hasChanges = true;
                    }
                    _pixelColors[rowIndex, columnIndex] = temp[rowIndex, columnIndex];
                }
            }

            return hasChanges;
        }

        private int[,] GetGraphicData()
        {
            var graphicData = new int[FullHeight, FullWidth];
            for (var rowIndex = 0; rowIndex <= FullHeight - 1; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex <= FullWidth - 1; columnIndex++)
                {
                    if (rowIndex < TileHeight || rowIndex >= FullHeight - TileHeight || columnIndex < TileWidth ||
                        columnIndex >= FullWidth - TileWidth)
                    {
                        graphicData[rowIndex, columnIndex] = _colorTable[_borderColorIndex];
                    }
                    else
                    {
                        graphicData[rowIndex, columnIndex] =
                            _colorTable[_pixelColors[rowIndex + _verticalOffset, columnIndex + _horizontalOffset]];
                    }
                }
            }

            return graphicData;
        }
    }
}