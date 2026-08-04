using System;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// http://www.mpucoder.com/DVD/pes-hdr.html
    /// </summary>
    public class PacketizedElementaryStream
    {
        public const int HeaderLength = 6;
        public const int Mpeg2HeaderLength = 14;

        public uint StartCode { get; }
        public int StreamId { get; }
        public int Length { get; }
        public int ScramblingControl { get; }
        public int Priority { get; }
        public int DataAlignmentIndicator { get; }
        public int Copyright { get; }
        public int OriginalOrCopy { get; }
        public int PresentationTimestampDecodeTimestampFlags { get; }
        public int ElementaryStreamClockReferenceFlag { get; }
        public int EsRateFlag { get; }
        public int DsmTrickModeFlag { get; }
        public int AdditionalCopyInfoFlag { get; }
        public int CrcFlag { get; }
        public int ExtensionFlag { get; }
        public int HeaderDataLength { get; }

        public ulong? PresentationTimestamp { get; }
        public ulong? DecodeTimestamp { get; }

        public int? SubPictureStreamId { get; }

        public byte[] DataBuffer { get; }

        public PacketizedElementaryStream(byte[] buffer, int index)
        {
            // A truncated pack (e.g. a PES packet with declared length 0) can be too short for the fixed header
            if (index + 8 >= buffer.Length)
            {
                return;
            }

            StartCode = Helper.GetEndian(buffer, index + 0, 3);
            StreamId = buffer[index + 3];
            Length = Helper.GetEndianWord(buffer, index + 4);

            ScramblingControl = (buffer[index + 6] >> 4) & 0b00000011;
            Priority = buffer[index + 6] & 0b00001000;
            DataAlignmentIndicator = buffer[index + 6] & 0b00000100;
            Copyright = buffer[index + 6] & 0b00000010;
            OriginalOrCopy = buffer[index + 6] & 0b00000001;
            PresentationTimestampDecodeTimestampFlags = buffer[index + 7] >> 6;
            ElementaryStreamClockReferenceFlag = buffer[index + 7] & 0b00100000;
            EsRateFlag = buffer[index + 7] & 0b00010000;
            DsmTrickModeFlag = buffer[index + 7] & 0b00001000;
            AdditionalCopyInfoFlag = buffer[index + 7] & 0b00000100;
            CrcFlag = buffer[index + 7] & 0b00001000;
            ExtensionFlag = buffer[index + 7] & 0b00000010;

            HeaderDataLength = buffer[index + 8];

            if (StreamId == 0xBD && index + 9 + HeaderDataLength < buffer.Length) // 10111101 binary = 189 decimal = 0xBD hex -> private_stream_1
            {
                int id = buffer[index + 9 + HeaderDataLength];
                if (id >= 0x20 && id <= 0x40) // x3f 0r x40 ?
                {
                    SubPictureStreamId = id;
                }
            }

            // The 5-byte PTS/DTS reads below must also fit in a truncated pack
            int tempIndex = index + 9;
            if ((PresentationTimestampDecodeTimestampFlags == 0b00000010 ||
                 PresentationTimestampDecodeTimestampFlags == 0b00000011) &&
                tempIndex + 5 <= buffer.Length)
            {
                string bString = Helper.GetBinaryString(buffer, tempIndex, 5);
                bString = bString.Substring(4, 3) + bString.Substring(8, 15) + bString.Substring(24, 15);
                PresentationTimestamp = Convert.ToUInt64(bString, 2);
                tempIndex += 5;
            }
            if (PresentationTimestampDecodeTimestampFlags == 0b00000011 && tempIndex + 5 <= buffer.Length)
            {
                string bString = Helper.GetBinaryString(buffer, tempIndex, 5);
                bString = bString.Substring(4, 3) + bString.Substring(8, 15) + bString.Substring(24, 15);
                DecodeTimestamp = Convert.ToUInt64(bString, 2);
            }

            int dataIndex = index + HeaderDataLength + 24 - Mpeg2HeaderLength;
            int dataSize = Length - (4 + HeaderDataLength);

            if (dataSize < 0 || (dataSize + dataIndex > buffer.Length)) // to fix bad subs...
            {
                dataSize = buffer.Length - dataIndex;
                if (dataSize < 0)
                {
                    return;
                }
            }

            DataBuffer = new byte[dataSize + 1];
            //Buffer.BlockCopy(buffer, dataIndex, DataBuffer, 0, dataSize);
            Buffer.BlockCopy(buffer, dataIndex - 1, DataBuffer, 0, DataBuffer.Length); // why subtract one from dataIndex???
        }
    }
}
