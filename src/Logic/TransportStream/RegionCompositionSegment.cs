using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.TransportStream
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

        public RegionCompositionSegment(byte[] buffer, int index)
        {
            RegionId = buffer[index];
            RegionVersionNumber = buffer[index + 1] >> 4;
            RegionFillFlag = (buffer[index + 1] & Helper.B00001000) > 0;
            RegionWidth = Helper.GetEndianWord(buffer, index + 2);
            RegionHeight = Helper.GetEndianWord(buffer, index + 4);
        }
    }

}
