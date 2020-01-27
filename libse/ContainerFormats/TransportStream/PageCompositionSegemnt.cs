using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class PageCompositionSegment
    {
        public int PageTimeOut { get; set; }
        public int PageVersionNumber { get; set; }
        public int PageState { get; set; }
        public List<PageCompositionSegmentRegion> Regions = new List<PageCompositionSegmentRegion>();

        public PageCompositionSegment(byte[] buffer, int index, int regionLength)
        {
            PageTimeOut = buffer[index];
            PageVersionNumber = buffer[index + 1] >> 4;
            PageState = (buffer[index + 1] & Helper.B00001100) >> 2;
            Regions = new List<PageCompositionSegmentRegion>();
            int i = 0;
            while (i < regionLength && i + index < buffer.Length)
            {
                var rcsr = new PageCompositionSegmentRegion();
                rcsr.RegionId = buffer[i + index + 2];
                i += 2;
                rcsr.RegionHorizontalAddress = Helper.GetEndianWord(buffer, i + index + 2);
                i += 2;
                rcsr.RegionVerticalAddress = Helper.GetEndianWord(buffer, i + index + 2);
                i += 2;
                Regions.Add(rcsr);
            }
        }
    }

}
