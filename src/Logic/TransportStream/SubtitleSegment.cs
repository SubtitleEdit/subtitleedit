using System;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{

    public class SubtitleSegment
    {
        public const int PageCompositionSegment = 0x10;
        public const int RegionCompositionSegment = 0x11;
        public const int ClutDefinitionSegment = 0x12;
        public const int ObjectDataSegment = 0x13;
        public const int DisplayDefinitionSegment = 0x14;
        public const int EndOfDisplaySetSegment = 0x80;

        public int SyncByte { get; set; }
        public int SegmentType { get; set; }
        public int PageId { get; set; }
        public int SegmentLength { get; set; }
        public byte[] DataBuffer { get; set; }
        public bool IsValid { get; set; }
        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }

        public StringBuilder sb = new StringBuilder();

        public ClutDefinitionSegment ClutDefinition;
        public ObjectDataSegment ObjectData;

        public SubtitleSegment(byte[] buffer, int index, ClutDefinitionSegment cds)
        {
            DisplayWidth = 720;
            DisplayHeight = 576;

            if (buffer == null || buffer.Length < 7)
                return;

            SyncByte = buffer[index];
            SegmentType = buffer[index + 1];
            PageId = Helper.GetEndianWord(buffer, index + 2);
            SegmentLength = Helper.GetEndianWord(buffer, index + 4);

            DataBuffer = new byte[SegmentLength];
            if (buffer.Length - 6 < DataBuffer.Length)
                return;

            if (index + 6 + DataBuffer.Length > buffer.Length)
                return;

            Buffer.BlockCopy(buffer, index + 6, DataBuffer, 0, DataBuffer.Length);
            IsValid = true;

            switch (SegmentType)
            {
                case PageCompositionSegment:
                    var pcs = new PageCompositionSegemnt(buffer, index + 6);
                    sb.AppendLine("PageTimeOut: " + pcs.PageTimeOut);
                    sb.AppendLine("PageVersionNumber: " + pcs.PageVersionNumber);
                    sb.AppendLine("PageState: " + pcs.PageState);
                    break;
                case RegionCompositionSegment:
                    var rcs = new RegionCompositionSegment(buffer, index + 6);
                    sb.AppendLine("RegionId: " + rcs.RegionId);
                    sb.AppendLine("RegionVersionNumber: " + rcs.RegionVersionNumber);
                    sb.AppendLine("RegionFillFlag: " + rcs.RegionFillFlag);
                    sb.AppendLine("RegionWidth: " + rcs.RegionWidth);
                    sb.AppendLine("RegionHeight: " + rcs.RegionHeight);
                    break;
                case ClutDefinitionSegment:
                    ClutDefinition = new ClutDefinitionSegment(buffer, index + 6, SegmentLength);
                    break;
                case ObjectDataSegment:
                    ObjectData = new ObjectDataSegment(buffer, index + 6, cds);
                    break;
                case DisplayDefinitionSegment:
                    var dds = new DisplayDefinitionSegment(buffer, index + 6);
                    sb.AppendLine("DisplayDefinitionVersionNumber: " + dds.DisplayDefinitionVersionNumber);
                    sb.AppendLine("DisplayWith: " + dds.DisplayWith);
                    sb.AppendLine("DisplayHeight: " + dds.DisplayHeight);
                    sb.AppendLine("DisplayWindowFlag: " + dds.DisplayWindowFlag);
                    DisplayWidth = dds.DisplayWith; // override default value
                    DisplayHeight = dds.DisplayHeight; // override default value
                    break;
                case EndOfDisplaySetSegment:
                    break;
                default:
                    break;
            }
        }

        public string SegmentTypeDescription
        {
            get
            {
                switch (SegmentType)
                {
                    case PageCompositionSegment: return "Page composition segment";
                    case RegionCompositionSegment: return "Region composition segment";
                    case ClutDefinitionSegment: return "CLUT definition segment";
                    case ObjectDataSegment: return "Object data segment";
                    case DisplayDefinitionSegment: return "Display definition segment";
                    case EndOfDisplaySetSegment: return "End of display set segment";
                    default: return "Unknown";
                }
            }
        }
    }

}
