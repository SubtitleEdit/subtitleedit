namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ManzanitaDataIndex
    {
        public ulong Pts { get; set; }
        public long Offset { get; set; }
        public long Length { get; set; }
        public bool AcquisitionPoint { get; set; }
    }
}
