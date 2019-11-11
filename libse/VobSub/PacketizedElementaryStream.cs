using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// http://www.mpucoder.com/DVD/pes-hdr.html
    /// </summary>
    public class PacketizedElementaryStream
    {
        public const int HeaderLength = 6;

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

        private readonly byte[] _dataBuffer;

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

            if (StreamId == 0xBD)
            {
                int id = buffer[index + 9 + HeaderDataLength];
                if (id >= 0x20 && id <= 0x40) // x3f 0r x40 ?
                {
                    SubPictureStreamId = id;
                }
            }

            int tempIndex = index + 9;
            if (PresentationTimestampDecodeTimestampFlags == Helper.B00000010 ||
                PresentationTimestampDecodeTimestampFlags == Helper.B00000011)
            {
                PresentationTimestamp = (ulong)buffer[tempIndex + 4] >> 1;
                PresentationTimestamp += (ulong)buffer[tempIndex + 3] << 7;
                PresentationTimestamp += (ulong)(buffer[tempIndex + 2] & Helper.B11111110) << 14;
                PresentationTimestamp += (ulong)buffer[tempIndex + 1] << 22;
                PresentationTimestamp += (ulong)(buffer[tempIndex + 0] & Helper.B00001110) << 29;

                //string bString = Helper.GetBinaryString(buffer, tempIndex, 5);
                //bString = bString.Substring(4, 3) + bString.Substring(8, 15) + bString.Substring(24, 15);
                //PresentationTimestamp = Convert.ToUInt64(bString, 2);
                tempIndex += 5;
            }
            if (PresentationTimestampDecodeTimestampFlags == Helper.B00000011)
            {
                //string bString = Helper.GetBinaryString(buffer, tempIndex, 5);
                //bString = bString.Substring(4, 3) + bString.Substring(8, 15) + bString.Substring(24, 15);
                //DecodeTimestamp = Convert.ToUInt64(bString, 2);

                DecodeTimestamp = (ulong)buffer[tempIndex + 4] >> 1;
                DecodeTimestamp += (ulong)buffer[tempIndex + 3] << 7;
                DecodeTimestamp += (ulong)(buffer[tempIndex + 2] & Helper.B11111110) << 14;
                DecodeTimestamp += (ulong)buffer[tempIndex + 1] << 22;
                DecodeTimestamp += (ulong)(buffer[tempIndex + 0] & Helper.B00001110) << 29;
            }

            int dataIndex = index + HeaderDataLength + 24 - Mpeg2Header.Length;
            int dataSize = Length - (4 + HeaderDataLength);

            if (dataSize < 0 || (dataSize + dataIndex > buffer.Length)) // to fix bad subs...
            {
                dataSize = buffer.Length - dataIndex;
                if (dataSize < 0)
                {
                    return;
                }
            }

            _dataBuffer = new byte[dataSize];
            Buffer.BlockCopy(buffer, dataIndex, _dataBuffer, 0, dataSize);
        }

        public void WriteToStream(Stream stream)
        {
            stream.Write(_dataBuffer, 0, _dataBuffer.Length);
        }
    }
}
