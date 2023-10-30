namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class DataOutput
    {
        public int Channel { get; set; }
        public bool Roll { get; set; }
        public int End { get; set; }
        public int Start { get; set; }
        public SerializedRow[] Screen { get; set; }
    }
}
