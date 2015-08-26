using System;

namespace Nikse.SubtitleEdit.Core.TransportStream
{
    /// <summary>
    /// http://www.mpucoder.com/DVD/pes-hdr.html
    /// </summary>
    public class PacketizedElementaryStream
    {
        public const int HeaderLength = 6;
        public const int Mpeg2HeaderLength = 14;

        public readonly UInt32 StartCode;
        public readonly int StreamId;
        public readonly int Length;
        public readonly int ScramblingControl;
        public readonly int Priority;
        public readonly int DataAlignmentIndicator;
        public readonly int Copyright;
        public readonly int OriginalOrCopy;
        public readonly int PresentationTimestampDecodeTimestampFlags;
        public readonly int ElementaryStreamClockReferenceFlag;
        public readonly int EsRateFlag;
        public readonly int DsmTrickModeFlag;
        public readonly int AdditionalCopyInfoFlag;
        public readonly int CrcFlag;
        public readonly int ExtensionFlag;
        public readonly int HeaderDataLength;

        public readonly UInt64? PresentationTimestamp;
        public readonly UInt64? DecodeTimestamp;

        public readonly int? SubPictureStreamId;

        public readonly byte[] DataBuffer;

        public PacketizedElementaryStream(byte[] buffer, int index)
        {
            StartCode = Helper.GetEndian(buffer, index + 0, 3);
            StreamId = buffer[index + 3];
            Length = Helper.GetEndianWord(buffer, index + 4);

            ScramblingControl = (buffer[index + 6] >> 4) & Helper.B00000011;
            Priority = buffer[index + 6] & Helper.B00001000;
            DataAlignmentIndicator = buffer[index + 6] & Helper.B00000100;
            Copyright = buffer[index + 6] & Helper.B00000010;
            OriginalOrCopy = buffer[index + 6] & Helper.B00000001;
            PresentationTimestampDecodeTimestampFlags = buffer[index + 7] >> 6;
            ElementaryStreamClockReferenceFlag = buffer[index + 7] & Helper.B00100000;
            EsRateFlag = buffer[index + 7] & Helper.B00010000;
            DsmTrickModeFlag = buffer[index + 7] & Helper.B00001000;
            AdditionalCopyInfoFlag = buffer[index + 7] & Helper.B00000100;
            CrcFlag = buffer[index + 7] & Helper.B00001000;
            ExtensionFlag = buffer[index + 7] & Helper.B00000010;

            HeaderDataLength = buffer[index + 8];

            if (StreamId == 0xBD) // 10111101 binary = 189 decimal = 0xBD hex -> private_stream_1
            {
                int id = buffer[index + 9 + HeaderDataLength];
                if (id >= 0x20 && id <= 0x40) // x3f 0r x40 ?
                    SubPictureStreamId = id;
            }

            int tempIndex = index + 9;
            if (PresentationTimestampDecodeTimestampFlags == Helper.B00000010 ||
                PresentationTimestampDecodeTimestampFlags == Helper.B00000011)
            {
                string bString = Helper.GetBinaryString(buffer, tempIndex, 5);
                bString = bString.Substring(4, 3) + bString.Substring(8, 15) + bString.Substring(24, 15);
                PresentationTimestamp = Convert.ToUInt64(bString, 2);
                tempIndex += 5;
            }
            if (PresentationTimestampDecodeTimestampFlags == Helper.B00000011)
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
                    return;
            }

            DataBuffer = new byte[dataSize+1];
            //Buffer.BlockCopy(buffer, dataIndex, DataBuffer, 0, dataSize);
            Buffer.BlockCopy(buffer, dataIndex - 1, DataBuffer, 0, DataBuffer.Length); // why subtract one from dataIndex???
        }
    }
}
