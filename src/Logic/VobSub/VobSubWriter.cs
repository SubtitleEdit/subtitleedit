using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.VobSub
{
    public class VobSubWriter
    {
        /// <summary>
        /// 14 bytes Mpeg 2 pack header
        /// </summary>
        private static readonly byte[] Mpeg2PackHeaderBuffer =
        {
            0x00, 0x00, 0x01,       // Start code
            0xba,                   // MPEG-2 Pack ID
            0x44, 0x02, 0xc4, 0x82, // System clock reference
            0x04, 0xa9,
            0x01, 0x89, 0xc3,       // Program mux rate
            0xf8                    // stuffing byte
        };

        /// <summary>
        /// 19 bytes packetized elementary stream header (PES)
        /// </summary>
        private static readonly byte[] PacketizedElementaryStreamHeaderBufferFirst = 
        {						// header only in first packet
		    0x00, 0x00, 0x01, 0xbd,						// 0: 0x000001bd - sub ID
		    0x00, 0x00,									// 4: packet length
		    0x81, 0x80, 								// 6:  packet type
		    0x05,										// 8:  PTS length
		    0x00, 0x0, 0x00, 0x00, 0x00,				// 9:  PTS
		    0x20,										// 14: stream ID
		    0x00, 0x00,									// 15: Subpicture size in bytes
		    0x00, 0x00,									// 17: offset to control header
	    };

        /// <summary>
        /// 10 bytes packetized elementary stream header (PES)
        /// </summary>
	    private static byte[] PacketizedElementaryStreamHeaderBufferNext = 
        {						// header in following packets
		    0x00, 0x00, 0x01, 0xbd,						// 0: 0x000001bd - sub ID
		    0x00, 0x00,									// 4: packet length
		    0x81, 0x00, 								// 6: packet type
		    0x00,										// 8: PTS length = 0
		    0x20										// 9: Stream ID
	    };

        private static readonly byte[] ControlHeader = 
        {
    		0x00,													//  dummy byte (for shifting when forced)
	    	0x00, 0x00,												//  0: offset to end sequence
		    0x01,													//  2: CMD 1: start displaying
		    0x03, 0x32, 0x10,										//  3: CMD 3: Palette Info
		    0x04, 0xff, 0xff,							            //  6: CMD 4: Alpha Info
		    0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,				//  9: CMD 5: sub position
		    0x06, 0x00, 0x00, 0x00, 0x00,							// 16: CMD 6: rle offsets
		    0xff,												    // 21: End of control header
		    0x00, 0x00,												// 22: display duration in 90kHz/1024
		    0x00, 0x00,												// 24: offset to end sequence (again)
		    0x02, 0xff,										        // 26: CMD 2: stop displaying
	    };

        /// <summary>
        /// Create the binary stream representation of one caption
        /// </summary>
        /// <param name="p">Paragraph</param>
        /// <param name="bmp">Bitmap</param>
        /// <returns>byte array with vobsub caption</returns>
        private byte[] createSubFrame(Paragraph paragraph, Bitmap bmp)
        {
            const byte emphasis2 = 3;
            const byte emphasis1 = 2;
            const byte pattern = 1;
            const byte background = 0;

            var nbmp = new NikseBitmap(bmp);
            nbmp.ConverToFourColors(_background, _pattern, _emphasis1, _emphasis2);
            var twoPartBuffer = nbmp.RunLengthEncodeForDvd(_background, _pattern, _emphasis1, _emphasis2);

            int forcedOfs;
            int controlHeaderLen;
            if (paragraph.Forced)
            {
                forcedOfs = 0;
                ControlHeader[2] = 0x01; // display
                ControlHeader[3] = 0x00; // forced
                controlHeaderLen = ControlHeader.Length;
            }
            else
            {
                forcedOfs = 1;
                ControlHeader[2] = 0x00; // part of offset
                ControlHeader[3] = 0x01; // display
                controlHeaderLen = ControlHeader.Length - 1;
            }


            // fill out all info but the offets (determined later)

            /* header - contains PTM */
            int ptm = (int)paragraph.StartTime.TotalMilliseconds; // should be end time, but STC writes start time?
            PacketizedElementaryStreamHeaderBufferFirst[9] = (byte)(((ptm >> 29) & 0x0E) | 0x21);
            PacketizedElementaryStreamHeaderBufferFirst[10] = (byte)(ptm >> 22);
            PacketizedElementaryStreamHeaderBufferFirst[11] = (byte)((ptm >> 14) | 1);
            PacketizedElementaryStreamHeaderBufferFirst[12] = (byte)(ptm >> 7);
            PacketizedElementaryStreamHeaderBufferFirst[13] = (byte)(ptm * 2 + 1);

            // create control header 
            // palette (store reversed) 
            ControlHeader[1 + 4] = (byte)(((emphasis2 & 0xf) << 4) | (emphasis1 & 0x0f));
            ControlHeader[1 + 5] = (byte)(((pattern & 0xf) << 4) | (background & 0x0f));

            // alpha (store reversed) 
            ControlHeader[1 + 7] = (byte)(((_emphasis2.A & 0xf) << 4) | (_emphasis1.A & 0x0f));
            ControlHeader[1 + 8] = (byte)(((_pattern.A & 0xf) << 4) | (_background.A & 0x0f));

            // coordinates of subtitle
            // Write 6 bytes of area - starting X, ending X, starting Y, ending Y, each 12 bits
            ushort startX = (ushort)((_screenWidth - nbmp.Width) / 2);
            ushort endX = (ushort)(startX + nbmp.Width - 1);
            ushort startY = (ushort)(_screenHeight - nbmp.Height - _bottomMargin);
            ushort endY = (ushort)(startY + nbmp.Height - 1);
            var stream = new MemoryStream(6);
            stream.Position = 0;
            WriteEndianWord((ushort)(startX << 4 | endX >> 8), stream); // 16 - 12 start x + 4 end x
            WriteEndianWord((ushort)(endX << 8 | startY >> 4), stream); // 16 - 8 endx + 8 starty
            WriteEndianWord((ushort)(startY << 12 | endY), stream);     // 16 - 4 start y + 12 end y
            var buffer = stream.GetBuffer();
            ControlHeader[1 + 10] = buffer[0];
            ControlHeader[1 + 11] = buffer[1];
            ControlHeader[1 + 12] = buffer[2];
            ControlHeader[1 + 13] = buffer[3];
            ControlHeader[1 + 14] = buffer[4];
            ControlHeader[1 + 15] = buffer[5];

            // offset to even lines in rle buffer 
            ControlHeader[1 + 17] = 0x00; // 2 bytes subpicture size and 2 bytes control header ofs
            ControlHeader[1 + 18] = 0x04; // note: SubtitleCreator uses 6 and adds 0x0000 in between

            // offset to odd lines in rle buffer
            int tmp = twoPartBuffer.Buffer1.Length + ControlHeader[1 + 18];
            ControlHeader[1 + 19] = (byte)((tmp >> 8) & 0xff);
            ControlHeader[1 + 20] = (byte)(tmp & 0xff);

            // display duration in frames
            tmp = Convert.ToInt32(paragraph.Duration.TotalMilliseconds * 90.0 - 1023) >> 10;
            ControlHeader[1 + 22] = (byte)((tmp >> 8) & 0xff);
            ControlHeader[1 + 23] = (byte)(tmp & 0xff);

            // offset to end sequence - 22 is the offset of the end sequence
            tmp = twoPartBuffer.Length + 22 + (paragraph.Forced ? 1 : 0) + 4;
            ControlHeader[forcedOfs + 0] = (byte)((tmp >> 8) & 0xff);
            ControlHeader[forcedOfs + 1] = (byte)(tmp & 0xff);
            ControlHeader[1 + 24] = (byte)((tmp >> 8) & 0xff);
            ControlHeader[1 + 25] = (byte)(tmp & 0xff);

            // subpicture size
            tmp = twoPartBuffer.Length + 4 + controlHeaderLen;
            PacketizedElementaryStreamHeaderBufferFirst[15] = (byte)(tmp >> 8);
            PacketizedElementaryStreamHeaderBufferFirst[16] = (byte)tmp;

            // offset to control buffer - 2 is the size of the offset
            tmp = twoPartBuffer.Length + 2;
            PacketizedElementaryStreamHeaderBufferFirst[17] = (byte)(tmp >> 8);
            PacketizedElementaryStreamHeaderBufferFirst[18] = (byte)tmp;

            // only 0x800 bytes can be written per packet. If a packet
            // is larger, it has to be split into fragments <= 0x800 bytes
            int sizeRLE = twoPartBuffer.Length;
            int bufSize = Mpeg2PackHeaderBuffer.Length + PacketizedElementaryStreamHeaderBufferFirst.Length + controlHeaderLen + sizeRLE;
            int numAdditionalPackets = 0;
            if (bufSize > 0x800)
            {
                numAdditionalPackets = 1;
                int remainingRLEsize = sizeRLE - (0x800 - Mpeg2PackHeaderBuffer.Length - PacketizedElementaryStreamHeaderBufferFirst.Length);
                while (remainingRLEsize > (0x800 - Mpeg2PackHeaderBuffer.Length - PacketizedElementaryStreamHeaderBufferNext.Length - controlHeaderLen))
                {
                    remainingRLEsize -= (0x800 - Mpeg2PackHeaderBuffer.Length - PacketizedElementaryStreamHeaderBufferNext.Length);
                    bufSize += Mpeg2PackHeaderBuffer.Length + PacketizedElementaryStreamHeaderBufferNext.Length;
                    numAdditionalPackets++;
                }
                // packet length of the 1st packet should be the maximum size
                tmp = 0x800 - Mpeg2PackHeaderBuffer.Length - 6;
            }
            else
            {
                tmp = (bufSize - Mpeg2PackHeaderBuffer.Length - 6);
            }

            // allocate and fill buffer
            byte[] buf = new byte[(1 + numAdditionalPackets) * 0x800];
            int stuffingBytes;
            int diff = buf.Length - bufSize;
            if (diff > 0 && diff < 6)
                stuffingBytes = diff;
            else
                stuffingBytes = 0;

            int ofs = 0;
            for (int i = 0; i < Mpeg2PackHeaderBuffer.Length; i++)
                buf[ofs++] = Mpeg2PackHeaderBuffer[i];

            // set packet length
            tmp += stuffingBytes;
            PacketizedElementaryStreamHeaderBufferFirst[4] = (byte)(tmp >> 8);
            PacketizedElementaryStreamHeaderBufferFirst[5] = (byte)tmp;

            // set pts length
            PacketizedElementaryStreamHeaderBufferFirst[8] = (byte)(5 + stuffingBytes);

            // write header and use pts for stuffing bytes (if needed)
            for (int i = 0; i < 14; i++)
                buf[ofs++] = PacketizedElementaryStreamHeaderBufferFirst[i];
            for (int i = 0; i < stuffingBytes; i++)
                buf[ofs++] = (byte)0xff;
            for (int i = 14; i < PacketizedElementaryStreamHeaderBufferFirst.Length; i++)
                buf[ofs++] = PacketizedElementaryStreamHeaderBufferFirst[i];

            // write (first part of) RLE buffer
            tmp = sizeRLE;
            if (numAdditionalPackets > 0)
            {
                tmp = (0x800 - Mpeg2PackHeaderBuffer.Length - stuffingBytes - PacketizedElementaryStreamHeaderBufferFirst.Length);
                if (tmp > sizeRLE) // can only happen in 1st buffer
                    tmp = sizeRLE;
            }
            for (int i = 0; i < tmp; i++)
            {
                if (i < twoPartBuffer.Buffer1.Length)
                    buf[ofs++] = twoPartBuffer.Buffer1[i];
                else
                    buf[ofs++] = twoPartBuffer.Buffer2[i - twoPartBuffer.Buffer1.Length];
            }
            int ofsRLE = tmp;

            // fill gap in first packet with (parts of) control header
            // only if the control header is split over two packets
            int controlHeaderWritten = 0;
            if (numAdditionalPackets == 1 && ofs < 0x800)
            {
                for (; ofs < 0x800; ofs++)
                    buf[ofs] = ControlHeader[forcedOfs + (controlHeaderWritten++)];
            }

            // write additional packets
            for (int p = 0; p < numAdditionalPackets; p++)
            {
                int rleSizeLeft;
                if (p == numAdditionalPackets - 1)
                {
                    // last loop
                    rleSizeLeft = sizeRLE - ofsRLE;
                    tmp = PacketizedElementaryStreamHeaderBufferNext.Length + (controlHeaderLen - controlHeaderWritten) + (sizeRLE - ofsRLE) - 6;
                }
                else
                {
                    tmp = 0x800 - Mpeg2PackHeaderBuffer.Length - 6;
                    rleSizeLeft = (0x800 - Mpeg2PackHeaderBuffer.Length - PacketizedElementaryStreamHeaderBufferNext.Length);
                    // now, again, it could happen that the RLE buffer runs out before the last package
                    if (rleSizeLeft > (sizeRLE - ofsRLE))
                        rleSizeLeft = sizeRLE - ofsRLE;
                }
                // copy packet headers
                Mpeg2PackHeaderBuffer[13] = (byte)(0xf8);
                for (int i = 0; i < Mpeg2PackHeaderBuffer.Length; i++)
                    buf[ofs++] = Mpeg2PackHeaderBuffer[i];

                // set packet length
                PacketizedElementaryStreamHeaderBufferNext[4] = (byte)(tmp >> 8);
                PacketizedElementaryStreamHeaderBufferNext[5] = (byte)tmp;
                for (int i = 0; i < PacketizedElementaryStreamHeaderBufferNext.Length; i++)
                    buf[ofs++] = PacketizedElementaryStreamHeaderBufferNext[i];

                // copy RLE buffer
                for (int i = ofsRLE; i < ofsRLE + rleSizeLeft; i++)
                {
                    if (i < twoPartBuffer.Buffer1.Length)
                        buf[ofs++] = twoPartBuffer.Buffer1[i];
                    else
                        buf[ofs++] = twoPartBuffer.Buffer2[i - twoPartBuffer.Buffer1.Length];
                }
                ofsRLE += rleSizeLeft;
                // fill possible gap in all but last package with (parts of) control header
                // only if the control header is split over two packets
                // this can only happen in the package before the last one though
                if (p != numAdditionalPackets - 1)
                    for (; ofs < (p + 2) * 0x800; ofs++)
                        buf[ofs] = ControlHeader[forcedOfs + (controlHeaderWritten++)];
            }

            // write (rest of) control header
            for (int i = controlHeaderWritten; i < controlHeaderLen; i++)
                buf[ofs++] = ControlHeader[forcedOfs + i];

            // fill rest of last packet with padding bytes
            diff = buf.Length - ofs;
            if (diff >= 6)
            {
                diff -= 6;
                buf[ofs++] = 0x00;
                buf[ofs++] = 0x00;
                buf[ofs++] = 0x01;
                buf[ofs++] = 0xbe;
                buf[ofs++] = (byte)(diff >> 8);
                buf[ofs++] = (byte)diff;
                for (; ofs < buf.Length; ofs++)
                    buf[ofs] = 0xff;
            } // else should never happen due to stuffing bytes

            return buf;
        }

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
      
        public void WriteParagraph(Paragraph p, Bitmap bmp)
        {
            // timestamp: 00:00:33:900, filepos: 000000000
            _idx.AppendLine(string.Format("timestamp: {0:00}:{1:00}:{2:00}:{3:000}, filepos: {4}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds, _subFile.Position.ToString("X").PadLeft(9, '0').ToLower()));
            var subBuf = createSubFrame(p, bmp);
            _subFile.Write(subBuf, 0, subBuf.Length);
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
# alt: English");
            return sb;
        }

        private static string ToHexColor(Color c)
        {
            return (c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2")).ToLower();
        }

    }
}