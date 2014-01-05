using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{
   
    public class DvbSubPes
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
        public readonly int PresentationTimeStampDecodeTimeStampFlags;
        public readonly int ElementaryStreamClockReferenceFlag;
        public readonly int EsRateFlag;
        public readonly int DsmTrickModeFlag;
        public readonly int AdditionalCopyInfoFlag;
        public readonly int CrcFlag;
        public readonly int ExtensionFlag;
        public readonly int HeaderDataLength;        

        public readonly UInt64? PresentationTimeStamp;
        public readonly UInt64? DecodeTimeStamp;

        public readonly int? SubPictureStreamId;

        public readonly byte[] DataBuffer;        

        public DvbSubPes(byte[] buffer, int index)
        {
            if (buffer.Length < 9)
                return;

            StartCode = Helper.GetEndian(buffer, index + 0, 3);
            StreamId = buffer[index + 3];
            Length = Helper.GetEndianWord(buffer, index + 4);

            ScramblingControl = (buffer[index + 6] >> 4) & Helper.B00000011;
            Priority = buffer[index + 6] & Helper.B00001000;
            DataAlignmentIndicator = buffer[index + 6] & Helper.B00000100;
            Copyright = buffer[index + 6] & Helper.B00000010;
            OriginalOrCopy = buffer[index + 6] & Helper.B00000001;
            PresentationTimeStampDecodeTimeStampFlags = buffer[index + 7] >> 6;
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
            if (PresentationTimeStampDecodeTimeStampFlags == Helper.B00000010 ||
                PresentationTimeStampDecodeTimeStampFlags == Helper.B00000011)
            {
                string bString = Helper.GetBinaryString(buffer, tempIndex, 5);
                bString = bString.Substring(4, 3) + bString.Substring(8, 15) + bString.Substring(24, 15);
                PresentationTimeStamp = Convert.ToUInt64(bString, 2);
                tempIndex += 5;
            }
            if (PresentationTimeStampDecodeTimeStampFlags == Helper.B00000011)
            {
                string bString = Helper.GetBinaryString(buffer, tempIndex, 5);
                bString = bString.Substring(4, 3) + bString.Substring(8, 15) + bString.Substring(24, 15);
                DecodeTimeStamp = Convert.ToUInt64(bString, 2);
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
            Buffer.BlockCopy(buffer, dataIndex - 1, DataBuffer, 0, DataBuffer.Length); // why subtract one from  dataIndex ?????
        }

        public int DataIdentifier
        {
            get
            {
                if (DataBuffer == null || DataBuffer.Length < 2)
                    return 0;

                return DataBuffer[0];
            }
        }

        public int SubtitleStreamId
        {
            get
            {
                if (DataBuffer == null || DataBuffer.Length < 2)
                    return 0;

                return DataBuffer[1];
            }
        }

        public List<SubtitleSegment> SubtitleSegments
        {
            get
            {
                var list = new List<SubtitleSegment>();
                int index = 2;
                var ss = new SubtitleSegment(DataBuffer, index, null);
                ClutDefinitionSegment cds = null;
                while (ss.SyncByte == Helper.B00001111)
                {
                    list.Add(ss);
                    if (ss.ClutDefinition != null)
                        cds = ss.ClutDefinition;
                        
                    index += 6 + ss.SegmentLength;
                    if (index + 6 < DataBuffer.Length)
                        ss = new SubtitleSegment(DataBuffer, index, cds);
                    else
                        ss.SyncByte = Helper.B11111111;
                }
                return list;
            }
        }

        public static int getInt(byte[] bytes, int offset, int len, int mask)
        {
            int r = 0;
            for (int i = 0; i < len; i++)
            {
                r = (r << 8) | bytes[offset + i];
            }
            return r & mask;
        }        

        public static string GetStreamIdDescription(int streamId)
        {
            if (0xC0 <= streamId && streamId < 0xE0)
                return "ISO/IEC 13818-3 or ISO/IEC 11172-3 or ISO/IEC 13818-7 or ISO/IEC 14496-3 audio stream number " + (streamId & 0x1F).ToString("X4");

            if (0xE0 <= streamId && streamId < 0xF0)
                return "ITU-T Rec. H.262 | ISO/IEC 13818-2 or ISO/IEC 11172-2 or ISO/IEC 14496-2 video stream number " + (streamId & 0x0F).ToString("X4");

            switch (streamId)
            {
                case 0xBC: return "program_stream_map";
                case 0xBD: return "private_stream_1";
                case 0xBE: return "padding_stream";
                case 0xBF: return "private_stream_2";
                case 0xF0: return "ECM_stream";
                case 0xF1: return "EMM_stream";
                case 0xF2: return "DSMCC_stream";
                case 0xF3: return "ISO/IEC_13522_stream";
                case 0xF4: return "ITU-T Rec. H.222.1 type A";
                case 0xF5: return "ITU-T Rec. H.222.1 type B";
                case 0xF6: return "ITU-T Rec. H.222.1 type C";
                case 0xF7: return "ITU-T Rec. H.222.1 type D";
                case 0xF8: return "ITU-T Rec. H.222.1 type E";
                case 0xF9: return "ancillary_stream";
                case 0xFA: return "ISO/IEC14496-1_SL-packetized_stream";
                case 0xFB: return "ISO/IEC14496-1_FlexMux_stream";
                /* ISO/IEC 13818-1:2007/FPDAM5 */
                case 0xFC: return "metadata stream";
                case 0xFD: return "extended_stream_id";
                case 0xFE: return "reserved data stream";
                case 0xFF: return "program_stream_directory";
                default: return "?";
            }
        }

	
    }
}
