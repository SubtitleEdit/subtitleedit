using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.VobSub
{
    public class VobSubWriter
    {

        private class MemWriter
        {
            private byte BitPointer;
            private byte[] BitSet = { 0, 0, 0 };
            private byte[] buf;
            private long pos;
            private long size;

            public MemWriter(long Size)
            {
                buf = new byte[Size];
                size = Size;
                pos = 0;
            }

            private void Write2bits(byte val)
            {
                // Write 2 bits, when full write a byte to the memory buffer
                if (BitPointer < 3)
                {
                    BitSet[BitPointer++] = (byte)(val & 0x3);
                }
                else
                {
                    BitPointer = 0;
                    buf[pos++] = (byte)((BitSet[0] << 6) | (BitSet[1] << 4) | (BitSet[2] << 2) | (val & 0x3));
                }
            }

            private void Write2bits(bool EndOfLine)
            {
                if (EndOfLine)
                {
                    // Write all bits, and let the position pointer start at a new byte
                    switch (BitPointer)
                    {
                        case 0:
                            break;
                        case 1:
                            buf[pos++] = (byte)(BitSet[0] << 6);
                            break;
                        case 2:
                            buf[pos++] = (byte)((BitSet[0] << 6) | (BitSet[1] << 4));
                            break;
                        case 3:
                            buf[pos++] = (byte)((BitSet[0] << 6) | (BitSet[1] << 4) | (BitSet[2] << 2));
                            break;
                    }
                    BitPointer = 0;
                    // Start new line at next even position
                    //if (pos % 2 != 0)
                    //    WriteByte(0);
                }
            }

            public byte[] GetBuf()
            {
                return buf;
            }

            public long GetPosition()
            {
                return pos;
            }

            public bool GotoBegin()
            {
                pos = 0;
                return true;
            }

            public bool GotoEnd()
            {
                pos = size;
                return true;
            }

            public byte ReadByte()
            {
                return buf[pos++];
            }

            public UInt16 ReadInt16()
            {
                return (UInt16)(
                    ((UInt16)buf[pos++] << 8) |
                    ((UInt16)buf[pos++]));
            }

            public int ReadInt32()
            {
                return (
                    ((int)buf[pos++] << 24) |
                    ((int)buf[pos++] << 16) |
                    ((int)buf[pos++] << 8) |
                    ((int)buf[pos++]));
            }

            public bool Seek(long position)
            {
                if (pos < size)
                {
                    pos = Math.Max(0, position);
                    return true;
                }
                else
                    return false;
            }

            public void WriteByte(byte val)
            {
                buf[pos++] = val;
            }

            public void WriteInt16(ushort val)
            {
                buf[pos++] = (byte)((val & 0xFF00) >> 8);
                buf[pos++] = (byte)(val & 0x00FF);
            }

            public void WriteInt32(uint val)
            {
                buf[pos++] = (byte)(val & 0x000000FF);
                buf[pos++] = (byte)((val & 0x0000FF00) >> 8);
                buf[pos++] = (byte)((val & 0x00FF0000) >> 16);
                buf[pos++] = (byte)((val & 0xFF000000) >> 24);
            }

            public void WriteRLE(uint RunLength, byte Color, bool EndOfLine)
            {
                // If end of line, add a carriage (two empty bytes) after writing the last run sequence
                if (EndOfLine)
                {
                    if (Color == 0)
                    {
                        Write2bits(true);       // Flush buffer and write
                        WriteInt16(0);
                    }
                    else
                    {
                        Write2bits(0);
                        Write2bits(0);
                        Write2bits(0);
                        Write2bits(0);
                        Write2bits(0);
                        Write2bits(0);
                        Write2bits(0); // 7x
                        Write2bits(Color);
                        Write2bits(true);       // Flush buffer and write
                    }
                    return;
                }

                // Create the RLE code - use a max RL of 255
                while (RunLength > 255)
                {
                    RunLength -= 255;
                    WriteRLE(255, Color, false);
                }
                if (RunLength < 4)
                    Write2bits((byte)RunLength);
                else
                {
                    // Run length is longer than 4: Count number of 00 (2 zero bits) we need to add to the code
                    ushort Counter = 0;
                    while ((RunLength >> (++Counter * 2)) > 0)
                    {
                        // Write 00 bits
                        Write2bits((byte)0);
                    }
                    // Write RunLength
                    for (--Counter; Counter > 0; Counter--)
                    {
                        Write2bits((byte)(RunLength >> (Counter * 2)));
                    }
                    Write2bits((byte)(RunLength >> (Counter * 2)));
                }
                Write2bits((byte)Color);
            }
        }

        /// <summary>
        /// 14 bytes Mpeg 2 pack header
        /// </summary>
        private static readonly byte[] Mpeg2PackHeaderBuffer =
        {
            0x00, 0x00, 0x01,       // Start code
            0xba,                   // MPEG-2 Pack ID
            0x44, 0x02, 0xec, 0xdf, // System clock reference
            0xfe, 0x57,
            0x01, 0x89, 0xc3,       // Program mux rate
            0xf8                    // stuffing byte
        };

        /// <summary>
        /// 9 bytes packetized elementary stream header (PES)
        /// </summary>
        private static readonly byte[] PacketizedElementaryStreamHeaderBufferFirst =
        {
            0x00, 0x00, 0x01,       // Start code
            0xbd,                   // bd = Private stream 1 (non MPEG audio, subpictures)
            0x00, 0x00,             // 18-19=PES packet length
            0x81,                   // 20=Flags: PES scrambling control, PES priority, data alignment indicator, copyright, original or copy
            0x81,                   // 21=Flags: PTS DTS flags, ESCR flag, ES rate flag, DSM trick mode flag, additional copy info flag, PES CRC flag, PES extension flag
            0x08                    // 22=PES header data length
        };

        /// <summary>
        /// 9 bytes packetized elementary stream header (PES)
        /// </summary>
        private static readonly byte[] PacketizedElementaryStreamHeaderBufferNext =
        {
            0x00, 0x00, 0x01,       // Start code
            0xbd,                   // bd = Private stream 1 (non MPEG audio, subpictures)
            0x00, 0x00,             // PES packet length
            0x81,                   // 20=Flags: PES scrambling control, PES priority, data alignment indicator, copyright, original or copy
            0x00,                   // 21=Flags: PTS DTS flags, ESCR flag, ES rate flag, DSM trick mode flag, additional copy info flag, PES CRC flag, PES extension flag
            0x00                    // 22=PES header data length
        };

        /// <summary>
        /// 5 bytes presentation time stamp (PTS)
        /// </summary>
        private static readonly byte[] PresentationTimeStampBuffer =
        {
            0x21,                   // 0010 3=PTS 32..30 1
            0x00, 0x01,             // 15=PTS 29..15 1
            0x00, 0x01              // 15=PTS 14..00 1
        };

        private const int PacketizedElementaryStreamMaximumLength = 2028;

        private readonly string _subFileName;
        private FileStream _subFile;
        readonly StringBuilder _idx = new StringBuilder();
        readonly int _screenWidth = 720;
        readonly int _screenHeight = 480;
        readonly int _bottomMargin = 15;
        readonly int _languageStreamId = 32;
        Color _background = Color.Transparent;
        Color _pattern = Color.White;
        Color _emphasis1 = Color.Black;
        Color _emphasis2 = Color.FromArgb(240, Color.Black);
        readonly string _languageName = "English";
        readonly string _languageNameShort = "en";

        public VobSubWriter(string subFileName, int screenWidth, int screenHeight, int bottomMargin, int languageStreamId, Color pattern, Color emphasis1, Color emphasis2,
                            string languageName, string languageNameShort)
        {
            _subFileName = subFileName;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _bottomMargin = bottomMargin;
            _languageStreamId = languageStreamId;
            _pattern = pattern;
            _emphasis1 = emphasis1;
            _emphasis2 = emphasis2;
            _languageName = languageName;
            _languageNameShort = languageNameShort;
            _idx = CreateIdxHeader();
            _subFile = new FileStream(subFileName, FileMode.Create);
        }

        public void WriteEndianWord(int i, Stream stream)
        {
            stream.WriteByte((byte)(i / 256));
            stream.WriteByte((byte)(i % 256));
        }

        private byte[] GetSubImageBuffer(RunLengthTwoParts twoPartBuffer, NikseBitmap nbmp, Paragraph p, ContentAlignment alignment)
        {
            var ms = new MemoryStream();

            // sup picture datasize
            WriteEndianWord(twoPartBuffer.Length + 33, ms);

            // first display control sequence table address
            int startDisplayControlSequenceTableAddress = twoPartBuffer.Length + 4;
            WriteEndianWord(startDisplayControlSequenceTableAddress, ms);

            // Write image
            const int imageTopFieldDataAddress = 4;
            ms.Write(twoPartBuffer.Buffer1, 0, twoPartBuffer.Buffer1.Length);
            int imageBottomFieldDataAddress = 4 + twoPartBuffer.Buffer1.Length;
            ms.Write(twoPartBuffer.Buffer2, 0, twoPartBuffer.Buffer2.Length);

            // Write zero delay
            ms.WriteByte(0);
            ms.WriteByte(0);

            // next display control sequence table address (use current is last)
            WriteEndianWord(startDisplayControlSequenceTableAddress + 24, ms); // start of display control sequence table address

            // Control command 1 = ForcedStartDisplay
            ms.WriteByte(1);

            // Control command 3 = SetColor
            WriteColors(ms); // 3 bytes

            // Control command 4 = SetContrast
            WriteContrast(ms); // 3 bytes

            // Control command 5 = SetDisplayArea
            WriteDisplayArea(ms, nbmp, alignment); // 7 bytes

            // Control command 6 = SetPixelDataAddress
            WritePixelDataAddress(ms, imageTopFieldDataAddress, imageBottomFieldDataAddress); // 5 bytes

            // Control command exit
            ms.WriteByte(255); // 1 bytes

            // Control Sequence Table
            // Write delay - subtitle duration
            WriteEndianWord(Convert.ToInt32(p.Duration.TotalMilliseconds * 90.0 - 1023) >> 10, ms);

            // next display control sequence table address (use current is last)
            WriteEndianWord(startDisplayControlSequenceTableAddress + 24, ms); // start of display control sequence table address

            // Control command 2 = StopDisplay
            ms.WriteByte(2);

            return ms.ToArray();
        }

        public void WriteParagraph(Paragraph p, Bitmap bmp, ContentAlignment alignment) // inspired by code from SubtitleCreator
        {
            // timestamp: 00:00:33:900, filepos: 000000000
            _idx.AppendLine(string.Format("timestamp: {0:00}:{1:00}:{2:00}:{3:000}, filepos: {4}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds, _subFile.Position.ToString("X").PadLeft(9, '0').ToLower()));

            var nbmp = new NikseBitmap(bmp);
            nbmp.ConverToFourColors(_background, _pattern, _emphasis1, _emphasis2);
            var twoPartBuffer = nbmp.RunLengthEncodeForDvd(_background, _pattern, _emphasis1, _emphasis2);
            var imageBuffer = GetSubImageBuffer(twoPartBuffer, nbmp, p, alignment);

            int bufferIndex = 0;
            byte VobSubID = 32;
            var mwsub = new MemWriter(200000);
            long header_size;
            byte[] SubHeader = new byte[30];
            byte[] ts = new byte[4];
            byte[] b = new byte[4];
            long filepos = 0;

            // Lended from "Son2VobSub" by Alain Vielle and Petr Vyskocil
            // And also from Sup2VobSub by Emmel
            SubHeader[0] = 0x00; // MPEG 2 PACK HEADER
            SubHeader[1] = 0x00;
            SubHeader[2] = 0x01;
            SubHeader[3] = 0xba;
            SubHeader[4] = 0x44;
            SubHeader[5] = 0x02;
            SubHeader[6] = 0xc4;
            SubHeader[7] = 0x82;
            SubHeader[8] = 0x04;
            SubHeader[9] = 0xa9;
            SubHeader[10] = 0x01;
            SubHeader[11] = 0x89;
            SubHeader[12] = 0xc3;
            SubHeader[13] = 0xf8;

            SubHeader[14] = 0x00; // PES
            SubHeader[15] = 0x00;
            SubHeader[16] = 0x01;
            SubHeader[17] = 0xbd;

            int packetSize = imageBuffer.Length;
            long toWrite = packetSize;  // Image buffer + control sequence length
            bool header0 = true;
            long blockSize = 0;
            long paddingsize;

            while (toWrite > 0)
            {
                if (header0)
                {
                    header0 = false;

                    // This is only for first packet
                    SubHeader[20] = 0x81;   // mark as original
                    SubHeader[21] = 0x80;   // first packet: PTS
                    SubHeader[22] = 0x05;   // PES header data length

                    // PTS (90kHz):
                    //--------------
                    SubHeader[23] = (byte)((ts[3] & 0xc0) >> 5 | 0x21);
                    SubHeader[24] = (byte)((ts[3] & 0x3f) << 2 | (ts[2] & 0xc0) >> 6);
                    SubHeader[25] = (byte)((ts[2] & 0x3f) << 2 | (ts[1] & 0x80) >> 6 | 0x01);
                    SubHeader[26] = (byte)((ts[1] & 0x7f) << 1 | (ts[0] & 0x80) >> 7);
                    SubHeader[27] = (byte)((ts[0] & 0x7f) << 1 | 0x01);

                    const string pre = "0010"; // 0011 or 0010 ? (KMPlayer will not understand 0011!!!)
                    long newPts = (long)(p.StartTime.TotalSeconds * 90000.0 + 0.5);
                    string bString = Convert.ToString(newPts, 2).PadLeft(33, '0');
                    string fiveBytesString = pre + bString.Substring(0, 3) + "1" + bString.Substring(3, 15) + "1" + bString.Substring(18, 15) + "1";
                    for (int i = 0; i < 5; i++)
                    {
                        SubHeader[23 + i] = Convert.ToByte(fiveBytesString.Substring((i * 8), 8), 2);
                    }
                    SubHeader[28] = VobSubID;
                    header_size = 29;
                }
                else
                {
                    SubHeader[20] = 0x81; // mark as original
                    SubHeader[21] = 0x00; // no PTS
                    SubHeader[22] = 0x00; // header data length
                    SubHeader[23] = VobSubID;
                    header_size = 24;
                }

                if ((toWrite + header_size) <= 0x800)
                {
                    // write whole image in one 0x800 part

                    long j = (header_size - 20) + toWrite;
                    SubHeader[18] = (byte)(j / 0x100);
                    SubHeader[19] = (byte)(j % 0x100);

                    // First Write header
                    for (int x = 0; x < header_size; x++)
                        mwsub.WriteByte(SubHeader[x]);

                    // Write Image Data
                    for (int x = 0; x < toWrite; x++)
                        mwsub.WriteByte((byte)imageBuffer[bufferIndex++]);

                    // Pad remaining space
                    paddingsize = 0x800 - header_size - toWrite;
                    for (int x = 0; x < paddingsize; x++)
                        mwsub.WriteByte(0xff);

                    toWrite = 0;
                }
                else
                {
                    // write multiple parts

                    blockSize = 0x800 - header_size;
                    long j = (header_size - 20) + blockSize;
                    SubHeader[18] = (byte)(j / 0x100);
                    SubHeader[19] = (byte)(j % 0x100);

                    // First Write header
                    for (int x = 0; x < header_size; x++)
                        mwsub.WriteByte(SubHeader[x]);

                    // Write Image Data
                    for (int x = 0; x < blockSize; x++)
                        mwsub.WriteByte((byte)imageBuffer[bufferIndex++]);

                    toWrite -= blockSize;
                }
            }

            // Write whole memory stream to file
            long EndPosition = mwsub.GetPosition();
            filepos += EndPosition;
            mwsub.GotoBegin();
            _subFile.Write(mwsub.GetBuf(), 0, (int)EndPosition);
        }    

        private static void WritePesSize(int subtract, byte[] imageBuffer, byte[] writeBuffer)
        {
            int length = Mpeg2PackHeaderBuffer.Length + imageBuffer.Length - subtract;
            if (length > PacketizedElementaryStreamMaximumLength)
            {
                writeBuffer[4] = PacketizedElementaryStreamMaximumLength / 256;
                writeBuffer[5] = PacketizedElementaryStreamMaximumLength % 256;
            }
            else
            {
                writeBuffer[4] = (byte)(length / 256);
                writeBuffer[5] = (byte)(length % 256);
            }
        }

        private void WritePixelDataAddress(Stream stream, int imageTopFieldDataAddress, int imageBottomFieldDataAddress)
        {
            stream.WriteByte(6);
            WriteEndianWord(imageTopFieldDataAddress, stream);
            WriteEndianWord(imageBottomFieldDataAddress, stream);
        }

        private void WriteDisplayArea(Stream stream, NikseBitmap nbmp, ContentAlignment alignment)
        {
            stream.WriteByte(5);

            // Write 6 bytes of area - starting X, ending X, starting Y, ending Y, each 12 bits
            ushort startX = (ushort) ((_screenWidth - nbmp.Width) / 2);
            ushort startY = (ushort)(_screenHeight - nbmp.Height - _bottomMargin);

            if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
            {
                startY = (ushort)_bottomMargin;
            }
            if (alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.MiddleRight)
            {
                startY = (ushort)((_screenHeight / 2) - (nbmp.Height / 2));
            }
            if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.BottomLeft)
            {
                startX = (ushort)_bottomMargin;
            }
            if (alignment == ContentAlignment.TopRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.BottomRight)
            {
                startX = (ushort)(_screenWidth - nbmp.Width - _bottomMargin);
            }

            ushort endX = (ushort)(startX + nbmp.Width - 1);
            ushort endY = (ushort)(startY + nbmp.Height - 1);

            WriteEndianWord((ushort)(startX << 4 | endX >> 8), stream); // 16 - 12 start x + 4 end x
            WriteEndianWord((ushort)(endX << 8 | startY >> 4), stream); // 16 - 8 endx + 8 starty
            WriteEndianWord((ushort)(startY << 12 | endY), stream);     // 16 - 4 start y + 12 end y
        }

        /// <summary>
        /// Directly provides the four contrast (alpha blend) values to associate with the four pixel values. One nibble per pixel value for a total of 2 bytes. 0x0 = transparent, 0xF = opaque
        /// </summary>
        private void WriteContrast(Stream stream)
        {
            stream.WriteByte(4);
            stream.WriteByte((byte)((_emphasis2.A << 4) | _emphasis1.A)); // emphasis2 + emphasis1
            stream.WriteByte((byte)((_pattern.A << 4) | _background.A)); // pattern + background
        }

        /// <summary>
        /// provides four indices into the CLUT for the current PGC to associate with the four pixel values. One nibble per pixel value for a total of 2 bytes.
        /// </summary>
        private void WriteColors(Stream stream)
        {
            // Index to palette
            const byte emphasis2 = 3;
            const byte emphasis1 = 2;
            const byte pattern = 1;
            const byte background = 0;

            stream.WriteByte(3);
            stream.WriteByte((emphasis2 << 4) | emphasis1); // emphasis2 + emphasis1
            stream.WriteByte((pattern << 4) | background); // pattern + background
        }

        /// <summary>
        /// Write the 5 PTS bytes to buffer
        /// </summary>
        private void FillPTS(TimeCode timeCode)
        {
            const string pre = "0010"; // 0011 or 0010 ? // use 0010 - KMPlayer does not understand 0011
            long newPts = (long)(timeCode.TotalSeconds * 90000.0 + 0.5);
            string bString = Convert.ToString(newPts, 2).PadLeft(33, '0');
            string fiveBytesString = pre + bString.Substring(0, 3) + "1" + bString.Substring(3, 15) + "1" + bString.Substring(18, 15) + "1";
            for (int i = 0; i < 5; i++)
            {
                byte b = Convert.ToByte(fiveBytesString.Substring((i * 8), 8), 2);
                PresentationTimeStampBuffer[i] = b;
            }
        }

        public void CloseSubFile()
        {
            if (_subFile != null)
                _subFile.Close();
            _subFile = null;
        }

        public void WriteIdxFile()
        {
            string idxFileName = _subFileName.Substring(0, _subFileName.Length - 3) + "idx";
            File.WriteAllText(idxFileName, _idx.ToString().Trim());
        }

        private StringBuilder CreateIdxHeader()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"# VobSub index file, v7 (do not modify this line!)
#
# To repair desyncronization, you can insert gaps this way:
# (it usually happens after vob id changes)
#
#    delay: [sign]hh:mm:ss:ms
#
# Where:
#    [sign]: +, - (optional)
#    hh: hours (0 <= hh)
#    mm/ss: minutes/seconds (0 <= mm/ss <= 59)
#    ms: milliseconds (0 <= ms <= 999)
#
#    Note: You can't position a sub before the previous with a negative value.
#
# You can also modify timestamps or delete a few subs you don't like.
# Just make sure they stay in increasing order.


# Settings

# Original frame size
size: " + _screenWidth + "x" + _screenHeight + @"

# Origin, relative to the upper-left corner, can be overloaded by aligment
org: 0, 0

# Image scaling (hor,ver), origin is at the upper-left corner or at the alignment coord (x, y)
scale: 100%, 100%

# Alpha blending
alpha: 100%

# Smoothing for very blocky images (use OLD for no filtering)
smooth: OFF

# In millisecs
fadein/out: 50, 50

# Force subtitle placement relative to (org.x, org.y)
align: OFF at LEFT TOP

# For correcting non-progressive desync. (in millisecs or hh:mm:ss:ms)
# Note: Not effective in DirectVobSub, use 'delay: ... ' instead.
time offset: 0

# ON: displays only forced subtitles, OFF: shows everything
forced subs: OFF

# The original palette of the DVD
palette: 000000, " + ToHexColor(_pattern) + ", " + ToHexColor(_emphasis1) + ", " + ToHexColor(_emphasis2) + @", 828282, 828282, 828282, ffffff, 828282, bababa, 828282, 828282, 828282, 828282, 828282, 828282

# Custom colors (transp idxs and the four colors)
custom colors: OFF, tridx: 0000, colors: 000000, 000000, 000000, 000000

# Language index in use
langidx: 0

# " + _languageName + @"
id: " + _languageNameShort + @", index: 0
# Decomment next line to activate alternative name in DirectVobSub / Windows Media Player 6.x
# alt: " + _languageName + @"
# Vob/Cell ID: 1, 1 (PTS: 0)");
            return sb;
        }

        private static string ToHexColor(Color c)
        {
            return (c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2")).ToLower();
        }

    }
}