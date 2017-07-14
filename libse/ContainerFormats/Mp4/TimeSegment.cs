namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    public class TimeSegment
    {
        public uint? Duration { get; set; }
        public uint? TimeOffset { get; set; }
        public ulong BaseMediaDecodeTime { get; set; }
    }
}