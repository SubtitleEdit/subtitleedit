using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{

    public class PageCompositionSegemnt
    {
        public int PageTimeOut { get; set; }
        public int PageVersionNumber { get; set; }
        public int PageState { get; set; }
        public List<PageCompositionSegmentRegion> Regions = new List<PageCompositionSegmentRegion>();

        public PageCompositionSegemnt(byte[] buffer, int index)
        {
            PageTimeOut = buffer[index];
            PageVersionNumber = buffer[index + 1] >> 4;
            PageState = (buffer[index + 1] & Helper.B00001100) >> 2;
        }
    }

}
