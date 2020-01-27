namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
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
        public bool IsValid { get; set; }

        public ClutDefinitionSegment ClutDefinition;
        public ObjectDataSegment ObjectData;
        public DisplayDefinitionSegment DisplayDefinition;
        public PageCompositionSegment PageComposition;
        public RegionCompositionSegment RegionComposition;

        public SubtitleSegment(byte[] buffer, int index)
        {
            if (buffer == null || buffer.Length < 7)
            {
                return;
            }

            SyncByte = buffer[index];
            SegmentType = buffer[index + 1];
            PageId = Helper.GetEndianWord(buffer, index + 2);
            SegmentLength = Helper.GetEndianWord(buffer, index + 4);

            if (buffer.Length - 6 < SegmentLength)
            {
                return;
            }

            if (index + 6 + SegmentLength > buffer.Length)
            {
                return;
            }

            IsValid = true;

            switch (SegmentType)
            {
                case PageCompositionSegment:
                    PageComposition = new PageCompositionSegment(buffer, index + 6, SegmentLength - 2);
                    break;
                case RegionCompositionSegment:
                    RegionComposition = new RegionCompositionSegment(buffer, index + 6, SegmentLength - 10);
                    break;
                case ClutDefinitionSegment:
                    ClutDefinition = new ClutDefinitionSegment(buffer, index + 6, SegmentLength);
                    break;
                case ObjectDataSegment:
                    ObjectData = new ObjectDataSegment(buffer, index + 6);
                    break;
                case DisplayDefinitionSegment:
                    DisplayDefinition = new DisplayDefinitionSegment(buffer, index + 6);
                    break;
                case EndOfDisplaySetSegment:
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
