using System;
using System.Collections.Generic;
using System.Drawing;

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
                PresentationTimeStamp = (ulong)buffer[tempIndex + 4] >> 1;
                PresentationTimeStamp += (ulong)buffer[tempIndex + 3] << 7;
                PresentationTimeStamp += (ulong)(buffer[tempIndex + 2] & Helper.B11111110) << 14;
                PresentationTimeStamp += (ulong)buffer[tempIndex + 1] << 22;
                PresentationTimeStamp += (ulong)(buffer[tempIndex + 0] & Helper.B00001110) << 29;
            }
            if (PresentationTimeStampDecodeTimeStampFlags == Helper.B00000011)
            {
                DecodeTimeStamp = (ulong)buffer[tempIndex + 4] >> 1;
                DecodeTimeStamp += (ulong)buffer[tempIndex + 3] << 7;
                DecodeTimeStamp += (ulong)(buffer[tempIndex + 2] & Helper.B11111110) << 14;
                DecodeTimeStamp += (ulong)buffer[tempIndex + 1] << 22;
                DecodeTimeStamp += (ulong)(buffer[tempIndex + 0] & Helper.B00001110) << 29;
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
            Buffer.BlockCopy(buffer, dataIndex - 1, DataBuffer, 0, DataBuffer.Length); // why subtract one from  dataIndex ?????
        }

        public bool IsDvbSubpicture
        {
            get { return SubPictureStreamId.HasValue && SubPictureStreamId.Value == 32; }
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

        public List<SubtitleSegment> SubtitleSegments { get; set; }
        public List<ClutDefinitionSegment> ClutDefinitions { get; set; }
        public List<RegionCompositionSegment> RegionCompositions { get; set; }
        public List<PageCompositionSegment> PageCompositions { get; set; }
        public List<ObjectDataSegment> ObjectDataList { get; set; }

        public void ParseSegments()
        {
            if (SubtitleSegments != null)
                return;

            SubtitleSegments = new List<SubtitleSegment>();
            ClutDefinitions = new List<ClutDefinitionSegment>();
            RegionCompositions = new List<RegionCompositionSegment>();
            PageCompositions = new List<PageCompositionSegment>();
            ObjectDataList = new List<ObjectDataSegment>();

            int index = 2;
            var ss = new SubtitleSegment(DataBuffer, index);
            ClutDefinitionSegment cds = null;
            while (ss.SyncByte == Helper.B00001111)
            {
                SubtitleSegments.Add(ss);
                if (ss.ClutDefinition != null)
                {
                    cds = ss.ClutDefinition;
                    ClutDefinitions.Add(ss.ClutDefinition);
                }
                else if (ss.RegionComposition != null)
                {
                    RegionCompositions.Add(ss.RegionComposition);
                }
                else if (ss.PageComposition != null)
                {
                    PageCompositions.Add(ss.PageComposition);
                }
                else if (ss.ObjectData != null)
                {
                    ObjectDataList.Add(ss.ObjectData);
                }

                index += 6 + ss.SegmentLength;
                if (index + 6 < DataBuffer.Length)
                    ss = new SubtitleSegment(DataBuffer, index);
                else
                    ss.SyncByte = Helper.B11111111;
            }
        }

        private ClutDefinitionSegment GetClutDefinitionSegment(ObjectDataSegment ods)
        {
            foreach (RegionCompositionSegment rcs in RegionCompositions)
            {
                foreach (RegionCompositionSegmentObject o in rcs.Objects)
                {
                    if (o.ObjectId == ods.ObjectId)
                    {
                        foreach (ClutDefinitionSegment cds in ClutDefinitions)
                        {
                            if (cds.ClutId == rcs.RegionClutId)
                                return cds;
                        }
                    }
                }
            }

            if (ClutDefinitions.Count > 0)
                return ClutDefinitions[0];

            return null; //TODO: Return default clut
        }

        public Point GetImagePosition(ObjectDataSegment ods)
        {
            if (SubtitleSegments == null)
                ParseSegments();

            var p = new Point(0, 0);

            foreach (RegionCompositionSegment rcs in RegionCompositions)
            {
                foreach (RegionCompositionSegmentObject o in rcs.Objects)
                {
                    if (o.ObjectId == ods.ObjectId)
                    {
                        foreach (PageCompositionSegment cds in PageCompositions)
                        {
                            foreach (var r in cds.Regions)
                            {
                                if (r.RegionId == rcs.RegionId)
                                {
                                    p.X = r.RegionHorizontalAddress + o.ObjectHorizontalPosition;
                                    p.Y = r.RegionVerticalAddress + o.ObjectVerticalPosition;
                                    return p;
                                }
                            }
                        }
                        p.X = o.ObjectHorizontalPosition;
                        p.Y = o.ObjectVerticalPosition;
                    }
                }
            }

            return p;
        }

        public Bitmap GetImage(ObjectDataSegment ods)
        {
            if (SubtitleSegments == null)
                ParseSegments();

            if (ods.Image != null)
                return ods.Image;

            int width = 720;
            int height = 576;

            ClutDefinitionSegment cds = GetClutDefinitionSegment(ods);
            var segments = SubtitleSegments;
            foreach (SubtitleSegment ss in segments)
            {
                if (ss.DisplayDefinition != null)
                {
                    width = ss.DisplayDefinition.DisplayWith;
                    height = ss.DisplayDefinition.DisplayHeight;
                }
            }

            if (ods == null)
                return new Bitmap(1, 1);

            ods.DecodeImage(DataBuffer, ods.BufferIndex, cds);
            return ods.Image;
        }


        public Bitmap GetImageFull()
        {
            if (SubtitleSegments == null)
                ParseSegments();

            int width = 720;
            int height = 576;

            var segments = SubtitleSegments;
            foreach (SubtitleSegment ss in segments)
            {
                if (ss.DisplayDefinition != null)
                {
                    width = ss.DisplayDefinition.DisplayWith;
                    height = ss.DisplayDefinition.DisplayHeight;
                }
            }

            var bmp = new Bitmap(width, height);
            foreach (ObjectDataSegment ods in ObjectDataList)
            {
                Bitmap odsImage = GetImage(ods);
                if (odsImage != null)
                {
                    Point odsPoint = GetImagePosition(ods);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.DrawImageUnscaled(odsImage, odsPoint);
                    }
                }
            }
            return bmp;
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
                case 0xFC: return "metadata stream";
                case 0xFD: return "extended_stream_id";
                case 0xFE: return "reserved data stream";
                case 0xFF: return "program_stream_directory";
                default: return "?";
            }
        }

        public ulong PresentationTimeStampToMilliseconds()
        {
            if (PresentationTimeStamp.HasValue)
                return (ulong)Math.Round((PresentationTimeStamp.Value + 45.0) / 90.0);
            else
                return 0;
        }

    }
}
