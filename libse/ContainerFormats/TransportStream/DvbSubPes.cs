using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class DvbSubPes
    {
        public const int HeaderLength = 6;
        public const int Mpeg2HeaderLength = 14;
        public const int DefaultScreenWidth = 720;
        public const int DefaultScreenHeight = 576;

        public int Length { get; }
        public ulong? PresentationTimestamp { get; }
        public ulong? DecodeTimestamp { get; }
        public int? SubPictureStreamId { get; }
        public uint StartCode { get; }
        public int StreamId { get; }
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

        private readonly byte[] _dataBuffer;

        public DvbSubPes(byte[] buffer, int index)
        {
            if (buffer.Length < index + 9)
            {
                return;
            }

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

            if (buffer.Length <= index + 9 + HeaderDataLength)
            {
                return;
            }

            if (StreamId == 0xBD) // 10111101 binary = 189 decimal = 0xBD hex -> private_stream_1
            {
                int id = buffer[index + 9 + HeaderDataLength];
                if (id >= 0x20 && id <= 0x40) // x3f 0r x40 ?
                {
                    SubPictureStreamId = id;
                }
            }
            if (index + 9 + 4 < buffer.Length)
            {
                int tempIndex = index + 9;
                if (PresentationTimestampDecodeTimestampFlags == Helper.B00000010 || PresentationTimestampDecodeTimestampFlags == Helper.B00000011)
                {
                    PresentationTimestamp = (ulong)buffer[tempIndex + 4] >> 1;
                    PresentationTimestamp += (ulong)buffer[tempIndex + 3] << 7;
                    PresentationTimestamp += (ulong)(buffer[tempIndex + 2] & Helper.B11111110) << 14;
                    PresentationTimestamp += (ulong)buffer[tempIndex + 1] << 22;
                    PresentationTimestamp += (ulong)(buffer[tempIndex + 0] & Helper.B00001110) << 29;
                }
                if (PresentationTimestampDecodeTimestampFlags == Helper.B00000011)
                {
                    DecodeTimestamp = (ulong)buffer[tempIndex + 4] >> 1;
                    DecodeTimestamp += (ulong)buffer[tempIndex + 3] << 7;
                    DecodeTimestamp += (ulong)(buffer[tempIndex + 2] & Helper.B11111110) << 14;
                    DecodeTimestamp += (ulong)buffer[tempIndex + 1] << 22;
                    DecodeTimestamp += (ulong)(buffer[tempIndex + 0] & Helper.B00001110) << 29;
                }
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

            _dataBuffer = new byte[dataSize + 1];
            Buffer.BlockCopy(buffer, dataIndex - 1, _dataBuffer, 0, _dataBuffer.Length); // why subtract one from dataIndex???
        }

        public List<int> PrepareTeletext()
        {
            var pages = new List<int>();
            var i = 1;
            while (i <= _dataBuffer.Length - 6)
            {
                var dataUnitId = _dataBuffer[i++];
                var dataUnitLen = _dataBuffer[i++];
                if (dataUnitId == (int)Teletext.DataUnitT.DataUnitEbuTeletextNonSubtitle || dataUnitId == (int)Teletext.DataUnitT.DataUnitEbuTeletextSubtitle)
                {
                    if (dataUnitLen == 44) // teletext payload has always size 44 bytes
                    {
                        // reverse endianness (via lookup table), ETS 300 706, chapter 7.1
                        for (var j = 0; j < dataUnitLen; j++)
                        {
                            _dataBuffer[i + j] = TeletextHamming.Reverse8[_dataBuffer[i + j]];
                        }
                        var pageNumber = Teletext.GetPageNumber(new Teletext.TeletextPacketPayload(_dataBuffer, i));
                        if (!pages.Contains(pageNumber) && pageNumber > 0)
                        {
                            pages.Add(pageNumber);
                        }
                    }
                }
                i += dataUnitLen;
            }
            return pages;
        }


        public Dictionary<int, Paragraph> GetTeletext(TeletextRunSettings teletextRunSettings, int pageNumber, int pageNumberBcd)
        {
            var timestamp = PresentationTimestamp.HasValue ? PresentationTimestamp.Value / 90 : 40;

            // do not allow timestamp to go back - treat lower timestamp as a reset/overflow
            var lastTimestamp = teletextRunSettings.GetLastTimestamp(pageNumber);
            teletextRunSettings.SetLastTimestamp(pageNumber, timestamp);
            timestamp += teletextRunSettings.GetAddTimestamp(pageNumber);
            if (lastTimestamp > 0 && lastTimestamp > timestamp)
            {
                teletextRunSettings.SetAddTimestamp(pageNumber, lastTimestamp);
            }

            // offset all time codes if first timestamp in ts file is > 1 sec
            timestamp = teletextRunSettings.SubtractStartMs(timestamp);

            if (timestamp < 40)
            {
                timestamp = 40; // Teletext.cs will subtract 40 ms (1 frame @25 fps) and this value must not be below 0
            }

            var teletextPages = new Dictionary<int, Paragraph>();
            var i = 1;
            while (i <= _dataBuffer.Length - 6)
            {
                var dataUnitId = _dataBuffer[i++];
                var dataUnitLen = _dataBuffer[i++];
                if (dataUnitId == (int)Teletext.DataUnitT.DataUnitEbuTeletextNonSubtitle || dataUnitId == (int)Teletext.DataUnitT.DataUnitEbuTeletextSubtitle)
                {
                    if (dataUnitLen == 44) // teletext payload has always size 44 bytes
                    {
                        Teletext.ProcessTelxPacket((Teletext.DataUnitT)dataUnitId, new Teletext.TeletextPacketPayload(_dataBuffer, i), timestamp, teletextRunSettings, pageNumberBcd, pageNumber);
                    }
                }
                i += dataUnitLen;
            }
            if (teletextRunSettings.PageNumberAndParagraph.ContainsKey(pageNumber) && teletextRunSettings.PageNumberAndParagraph[pageNumber] != null)
            {
                if (teletextPages.ContainsKey(pageNumber))
                {
                    teletextPages[pageNumber] = teletextRunSettings.PageNumberAndParagraph[pageNumber];
                }
                else
                {
                    teletextPages.Add(pageNumber, teletextRunSettings.PageNumberAndParagraph[pageNumber]);
                }
            }
            teletextRunSettings.PageNumberAndParagraph.Clear();
            return teletextPages;
        }

        public DvbSubPes(int index, byte[] buffer)
        {
            int start = index;
            Length = index + 1;

            if (index + 9 >= buffer.Length)
            {
                return;
            }

            if (buffer[0 + index] != 0x20)
            {
                return;
            }

            if (buffer[1 + index] != 0)
            {
                return;
            }

            SubtitleSegments = new List<SubtitleSegment>();
            ClutDefinitions = new List<ClutDefinitionSegment>();
            RegionCompositions = new List<RegionCompositionSegment>();
            PageCompositions = new List<PageCompositionSegment>();
            ObjectDataList = new List<ObjectDataSegment>();

            // Find length of segments
            index = start + 2;
            var ss = new SubtitleSegment(buffer, index);
            while (ss.SyncByte == Helper.B00001111)
            {
                SubtitleSegments.Add(ss);
                index += 6 + ss.SegmentLength;
                if (index + 6 < buffer.Length)
                {
                    ss = new SubtitleSegment(buffer, index);
                }
                else
                {
                    ss.SyncByte = Helper.B11111111;
                }
            }
            Length = index;
            int size = index - start;
            _dataBuffer = new byte[size];
            Buffer.BlockCopy(buffer, start, _dataBuffer, 0, _dataBuffer.Length);

            // Parse segments
            index = 2;
            ss = new SubtitleSegment(_dataBuffer, index);
            while (ss.SyncByte == Helper.B00001111)
            {
                SubtitleSegments.Add(ss);
                if (ss.ClutDefinition != null)
                {
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
                if (index + 6 < _dataBuffer.Length)
                {
                    ss = new SubtitleSegment(_dataBuffer, index);
                }
                else
                {
                    ss.SyncByte = Helper.B11111111;
                }
            }
        }

        public bool IsDvbSubPicture => SubPictureStreamId.HasValue && SubPictureStreamId.Value == 32;

        public bool IsTeletext => DataIdentifier == 16;

        public int DataIdentifier
        {
            get
            {
                if (_dataBuffer == null || _dataBuffer.Length < 2)
                {
                    return 0;
                }

                return _dataBuffer[0];
            }
        }

        public int SubtitleStreamId
        {
            get
            {
                if (_dataBuffer == null || _dataBuffer.Length < 2)
                {
                    return 0;
                }

                return _dataBuffer[1];
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
            {
                return;
            }

            SubtitleSegments = new List<SubtitleSegment>();
            ClutDefinitions = new List<ClutDefinitionSegment>();
            RegionCompositions = new List<RegionCompositionSegment>();
            PageCompositions = new List<PageCompositionSegment>();
            ObjectDataList = new List<ObjectDataSegment>();

            int index = 2;
            var ss = new SubtitleSegment(_dataBuffer, index);
            while (ss.SyncByte == Helper.B00001111)
            {
                SubtitleSegments.Add(ss);
                if (ss.ClutDefinition != null)
                {
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
                if (index + 6 < _dataBuffer.Length)
                {
                    ss = new SubtitleSegment(_dataBuffer, index);
                }
                else
                {
                    ss.SyncByte = Helper.B11111111;
                }
            }
        }

        private ClutDefinitionSegment GetClutDefinitionSegment(ObjectDataSegment ods)
        {
            foreach (var rcs in RegionCompositions)
            {
                foreach (var o in rcs.Objects)
                {
                    if (o.ObjectId == ods.ObjectId)
                    {
                        foreach (var cds in ClutDefinitions)
                        {
                            if (cds.ClutId == rcs.RegionClutId)
                            {
                                return cds;
                            }
                        }
                    }
                }
            }

            if (ClutDefinitions.Count > 0)
            {
                return ClutDefinitions[0];
            }

            return null; // TODO: Return default clut
        }

        public Point GetImagePosition(ObjectDataSegment ods)
        {
            if (SubtitleSegments == null)
            {
                ParseSegments();
            }

            var p = new Point(0, 0);

            foreach (var rcs in RegionCompositions)
            {
                foreach (var o in rcs.Objects)
                {
                    if (o.ObjectId == ods.ObjectId)
                    {
                        foreach (var cds in PageCompositions)
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
            {
                ParseSegments();
            }

            if (ods.Image != null)
            {
                return ods.Image;
            }

            var cds = GetClutDefinitionSegment(ods);
            ods.DecodeImage(_dataBuffer, ods.BufferIndex, cds);
            return ods.Image;
        }

        public Bitmap GetImageFull()
        {
            if (SubtitleSegments == null)
            {
                ParseSegments();
            }

            int width = DefaultScreenWidth;
            int height = DefaultScreenHeight;

            var segments = SubtitleSegments;
            foreach (var ss in segments)
            {
                if (ss.DisplayDefinition != null)
                {
                    width = ss.DisplayDefinition.DisplayWith;
                    height = ss.DisplayDefinition.DisplayHeight;
                }
            }

            var bmp = new Bitmap(width, height);
            foreach (var ods in ObjectDataList)
            {
                var odsImage = GetImage(ods);
                if (odsImage != null)
                {
                    var odsPoint = GetImagePosition(ods);
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
            {
                return "ISO/IEC 13818-3 or ISO/IEC 11172-3 or ISO/IEC 13818-7 or ISO/IEC 14496-3 audio stream number " + (streamId & 0x1F).ToString("X4");
            }

            if (0xE0 <= streamId && streamId < 0xF0)
            {
                return "ITU-T Rec. H.262 | ISO/IEC 13818-2 or ISO/IEC 11172-2 or ISO/IEC 14496-2 video stream number " + (streamId & 0x0F).ToString("X4");
            }

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

        public ulong PresentationTimestampToMilliseconds()
        {
            if (PresentationTimestamp.HasValue)
            {
                return (ulong)Math.Round(PresentationTimestamp.Value / 90.0);
            }

            return 0;
        }

        public void WriteToStream(Stream stream)
        {
            stream.Write(_dataBuffer, 0, _dataBuffer.Length);
        }

    }
}
