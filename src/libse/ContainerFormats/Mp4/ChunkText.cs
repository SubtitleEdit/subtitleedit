namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    public class ChunkText
    {
        public uint Size { get; set; }
        public string Text { get; set; }
        public byte[] Buffer { get; set; }
        public ulong Offset { get; set; }
    }
}
