namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class RegionCompositionSegmentObject
    {
        public int ObjectId { get; set; }
        public int ObjectType { get; set; }
        public int ObjectProviderFlag { get; set; }
        public int ObjectHorizontalPosition { get; set; }
        public int ObjectVerticalPosition { get; set; }
        public int? ObjectForegroundPixelCode { get; set; }
        public int? ObjectBackgroundPixelCode { get; set; }
    }
}
