namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    public class TimeSegment
    {
        public uint? Duration { get; set; }
        public uint? Size { get; set; }
        public long? TimeOffset { get; set; }
        public ulong BaseMediaDecodeTime { get; set; }
    }
}