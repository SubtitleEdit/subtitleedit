using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class RegionCompositionSegment
    {
        public int RegionId { get; set; }
        public int RegionVersionNumber { get; set; }
        public bool RegionFillFlag { get; set; }
        public int RegionWidth { get; set; }
        public int RegionHeight { get; set; }
        public int RegionLevelOfCompatibility { get; set; }
        public int RegionDepth { get; set; }
        public int RegionClutId { get; set; }
        public int Region8BitPixelCode { get; set; }
        public int Region4BitPixelCode { get; set; }
        public int Region2BitPixelCode { get; set; }

        public List<RegionCompositionSegmentObject> Objects = new List<RegionCompositionSegmentObject>();

        public RegionCompositionSegment(byte[] buffer, int index, int regionLength)
        {
            RegionId = buffer[index];
            RegionVersionNumber = buffer[index + 1] >> 4;
            RegionFillFlag = (buffer[index + 1] & Helper.B00001000) > 0;
            RegionWidth = Helper.GetEndianWord(buffer, index + 2);
            RegionHeight = Helper.GetEndianWord(buffer, index + 4);
            RegionLevelOfCompatibility = buffer[index + 6] >> 5;
            RegionDepth = (buffer[index + 6] & Helper.B00011100) >> 2;
            RegionClutId = buffer[index + 7];
            Region8BitPixelCode = buffer[index + 8];
            Region4BitPixelCode = buffer[index + 9] >> 4;
            Region2BitPixelCode = (buffer[index + 9] & Helper.B00001100) >> 2;
            int i = 0;
            while (i < regionLength && i + index < buffer.Length)
            {
                var rcso = new RegionCompositionSegmentObject();
                rcso.ObjectId = Helper.GetEndianWord(buffer, i + index + 10);
                i += 2;
                rcso.ObjectType = buffer[i + index + 10] >> 6;
                rcso.ObjectProviderFlag = (buffer[index + i + 10] & Helper.B00110000) >> 4;
                rcso.ObjectHorizontalPosition = ((buffer[index + i + 10] & Helper.B00001111) << 8) + buffer[index + i + 11];
                i += 2;
                rcso.ObjectVerticalPosition = ((buffer[index + i + 10] & Helper.B00001111) << 8) + buffer[index + i + 11];
                i += 2;
                if (rcso.ObjectType == 1 || rcso.ObjectType == 2)
                {
                    i += 2;
                }

                Objects.Add(rcso);
            }
        }
    }
}
