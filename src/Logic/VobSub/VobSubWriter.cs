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
        private static byte[] Mpeg2PackHeaderBuffer =
        {
            0x00, 0x00, 0x01,       // Start code
            0xba,                   // MPEG-2 Pack ID
            0x44, 0x02, 0xec, 0xdf, // System clock reference
            0xfe, 0x57,
            0x01, 0x89, 0xc3,       // Program mux rate
            0xf8,                   // stuffing byte
        };

        /// <summary>
        /// 9 bytes packetized elementary stream header (PES)
        /// </summary>
        private static byte[] PacketizedElementaryStreamHeaderBufferFirst =
        {
            0x00, 0x00, 0x01,       // Start code
            0xbd,                   // bd = Private stream 1 (non MPEG audio, subpictures)
            0x00, 0x00,             // 18-19=PES packet length
            0x81,                   // 20=Flags: PES scrambling control, PES priority, data alignment indicator, copyright, original or copy
            0x81,                   // 21=Flags: PTS DTS flags, ESCR flag, ES rate flag, DSM trick mode flag, additional copy info flag, PES CRC flag, PES extension flag
            0x05,                   // 22=PES header data length
        };


        /// <summary>
        /// 9 bytes packetized elementary stream header (PES)
        /// </summary>
        private static byte[] PacketizedElementaryStreamHeaderBufferNext =
        {
            0x00, 0x00, 0x01,       // Start code
            0xbd,                   // bd = Private stream 1 (non MPEG audio, subpictures)
            0x00, 0x00,             // PES packet length
            0x81,                   // 20=Flags: PES scrambling control, PES priority, data alignment indicator, copyright, original or copy
            0x00,                   // 21=Flags: PTS DTS flags, ESCR flag, ES rate flag, DSM trick mode flag, additional copy info flag, PES CRC flag, PES extension flag
            0x00,                   // 22=PES header data length
        };

        /// <summary>
        /// 5 bytes presentation time stamp (PTS)
        /// </summary>
        private static byte[] PresentationTimeStampBuffer =
        {
            0x21,                   // 0010 3=PTS 32..30 1
            0x00, 0x01,             // 15=PTS 29..15 1
            0x00, 0x01,             // 15=PTS 14..00 1
        };

        private string _subFileName;
        private FileStream _subFile;
        StringBuilder _idx = new StringBuilder();

        public VobSubWriter(string subFileName)
        {
            _subFileName = subFileName;
            _idx = CreateIdxHeader();
            _subFile = new FileStream(subFileName, FileMode.Create);
        }

        public void WriteParagraph(Paragraph p, Bitmap bmp)
        {
            // timestamp: 00:00:33:900, filepos: 000000000
            _idx.AppendLine(string.Format("timestamp: {0:00}:{1:00}:{2:00}:{3:000}, filepos: {4}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds, _subFile.Position.ToString("X").PadLeft(9, '0')));


            // write binary vobsub file (duration + image)
            long start = _subFile.Position;
            _subFile.Write(Mpeg2PackHeaderBuffer, 0, Mpeg2PackHeaderBuffer.Length);

            NikseBitmap nbmp = new NikseBitmap(bmp);
            nbmp.ConverToFourColors(Color.Transparent, Color.White, Color.FromArgb(200, 0, 0, 0), Color.FromArgb(200, 25, 25, 25));

            var outBmp = nbmp.GetBitmap();
            outBmp.Save(@"D:\download\-_-" + p.Number.ToString() + ".bmp");
            bmp.Save(@"D:\download\-__" + p.Number.ToString() + ".bmp");
            outBmp.Dispose();

            var imageBuffer = nbmp.RunLengthEncodeForDvd(Color.Transparent, Color.White, Color.FromArgb(200, 0, 0, 0), Color.FromArgb(200, 25, 25, 25));

            // PES size
            int length = Mpeg2PackHeaderBuffer.Length + PacketizedElementaryStreamHeaderBufferFirst.Length + 10 + imageBuffer.Length;

            //block_size = 0x800 - header_size;
            //long j = (header_size - 20) + block_size;
            //SubHeader[18] = (byte)(j / 0x100);
            //SubHeader[19] = (byte)(j % 0x100);

            PacketizedElementaryStreamHeaderBufferFirst[4] = (byte)(length / 256);
            PacketizedElementaryStreamHeaderBufferFirst[5] = (byte)(length % 256);
            _subFile.Write(PacketizedElementaryStreamHeaderBufferFirst, 0, PacketizedElementaryStreamHeaderBufferFirst.Length);

            // PTS (timestamp)
            FillPTS(p.StartTime);
            _subFile.Write(PresentationTimeStampBuffer, 0, PresentationTimeStampBuffer.Length);

            _subFile.WriteByte(0x32); //sub-stream number

            if (imageBuffer.Length < 0x800 - (_subFile.Position - start))
            {
                _subFile.Write(imageBuffer, 0, imageBuffer.Length);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Too long for payload!!!");
            }

//            HeaderDataLength = buffer[index + 8];

            // language id
            //    int id = buffer[9 + HeaderDataLength];
            //    if (id >= 0x20 && id <= 0x40) // x3f 0r x40 ?
            //        SubPictureStreamId = id;
            //}


            for (long i = _subFile.Position - start; i < 0x800; i++) // 2048 packet size - pad with 0xff
                _subFile.WriteByte(0xff);
        }

        /// <summary>
        /// Write the 5 PTS bytes to buffer
        /// </summary>
        private void FillPTS(TimeCode timeCode)
        {
            string pre = "0011"; // 0011 or 0010 ?
//                pre = "0010";

            long newPts = (long)(timeCode.TotalMilliseconds); // TODO: Calculation from milliseconds
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
            System.IO.File.WriteAllText(idxFileName, _idx.ToString().Trim());
        }

        private StringBuilder CreateIdxHeader()
        {
            StringBuilder sb = new StringBuilder();
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
size: 720x480

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
palette: 000000, ffffff, 000000, 191919, 828282, 828282, 828282, ffffff, 828282, bababa, 828282, 828282, 828282, 828282, 828282, 828282

# Custom colors (transp idxs and the four colors)
custom colors: OFF, tridx: 0000, colors: 000000, 000000, 000000, 000000

# Language index in use
langidx: 0

# English
id: en, index: 0
# Decomment next line to activate alternative name in DirectVobSub / Windows Media Player 6.x
# alt: English");
            return sb;
        }

    }
}